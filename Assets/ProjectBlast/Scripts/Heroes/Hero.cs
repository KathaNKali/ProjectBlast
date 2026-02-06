using UnityEngine;
using ProjectBlast.Grid;
using ProjectBlast.Data;
using ProjectBlast.AI;
using ProjectBlast.Combat;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// Base Hero class - uses TDE's AIBrain system for automatic combat behavior.
    /// Heroes use AI states to control targeting, aiming, and shooting at enemies.
    /// 
    /// TDE AI BRAIN INTEGRATION:
    /// - AIBrain: State machine controlling hero behavior (Inactive/Idle/Combat)
    /// - AIActionShoot3D: Handles weapon firing at targets
    /// - AIActionAimWeaponAtTarget3D: Aims weapon at detected targets
    /// - AIDecisionDetectTargetRadius3D: Detects enemies within range
    /// - AIDecisionLineOfSightToTarget3D: Verifies line-of-sight to targets
    /// - CharacterOrientation3D: Rotates character body toward targets
    /// 
    /// CONFIGURATION:
    /// - HeroDataSO contains all stats (range, fire rate, ammo, health)
    /// - Stats applied to AI components automatically on initialization
    /// - AI states configured in Unity Inspector for per-hero customization
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hero : MonoBehaviour, MMEventListener<MMAmmoEvent>
    {
        #region Inspector Fields
        
        [Header("Hero Configuration")]
        public HeroDataSO HeroData;
        
        [Header("Grid Integration")]
        [Tooltip("Current grid slot - automatically triggers zone change detection")]
        public GridSlot CurrentGridSlotPublic; // Public for Inspector visibility
        private GridSlot _currentGridSlot;
        
        [Header("Visual Feedback")]
        public Material HighlightMaterial;
        
        [Header("TDE Components (Auto-found)")]
        public Character Character;
        public Health Health;
        public CharacterHandleWeapon HandleWeapon;
        public CharacterOrientation3D Orientation3D;
        public HeroAmmo HeroAmmoAbility;
        
        [Header("AI Components (Auto-found)")]
        public AIBrain AIBrain;
        public AIActionShoot3D AIActionShoot;
        public AIActionAimWeaponAtTarget3D AIActionAim;
        public AIDecisionDetectTargetPriority3D AIDecisionDetectPriority; // Custom priority-based detection
        public AIDecisionLineOfSightToTarget3D AIDecisionLOS;
        
        [Header("Weapon Configuration")]
        public Transform WeaponAttachment;
        
        [Header("Read-Only Runtime Info")]
        [SerializeField] private int _displayCurrentAmmo;
        [SerializeField] private Transform _displayCurrentTarget;
        [SerializeField] private string _displayCurrentSlot;
        [SerializeField] private string _displayCurrentZone;
        
        [Header("Lifecycle Settings")]
        public float RemovalDelay = 1.5f;
        public bool DestroyOnRemoval = true;
        
        #endregion
        
        #region Properties (SO-Driven)
        
        public string HeroName => HeroData != null ? HeroData.HeroName : gameObject.name;
        public HeroClass HeroClass => HeroData != null ? HeroData.HeroClass : HeroClass.Ranged;
        public float DetectionRange => HeroData != null ? HeroData.DetectionRange : 20f;
        public float TargetSearchInterval => HeroData != null ? HeroData.TargetSearchInterval : 0.5f;
        public LayerMask TargetLayerMask => HeroData != null ? HeroData.TargetLayerMask : 0;
        public LayerMask ObstacleLayerMask => HeroData != null ? HeroData.ObstacleLayerMask : LayerManager.ObstaclesLayerMask;
        public bool UnlimitedAmmo => HeroData != null ? HeroData.UnlimitedAmmo : false;
        public int StartingAmmo => HeroData != null ? HeroData.StartingAmmo : 100;
        public int LowAmmoThreshold => HeroData != null ? HeroData.LowAmmoThreshold : 20;
        public Weapon WeaponPrefab => HeroData != null ? HeroData.DefaultWeaponPrefab : null;
        
        #endregion
        
        #region Private Fields
        
        private Material _originalMaterial;
        private Renderer _renderer;
        private bool _isDead = false;
        private Weapon _currentWeapon;
        private int _currentAmmo;
        private bool _isOutOfAmmo = false;
        private bool _isBeingRemoved = false;
        private float _lastAmmoCheckTime;
        private Weapon.WeaponStates _lastWeaponState;
        private bool _aiConfigured = false;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Current grid slot with automatic zone change detection
        /// </summary>
        public GridSlot CurrentGridSlot
        {
            get => _currentGridSlot;
            set
            {
                GridZone? oldZone = _currentGridSlot?.Zone;
                _currentGridSlot = value;
                CurrentGridSlotPublic = value; // Update Inspector field
                GridZone? newZone = _currentGridSlot?.Zone;
                
                // Detect zone change and trigger callback
                if (oldZone != newZone)
                {
                    OnZoneChanged(oldZone, newZone);
                }
            }
        }
        
        public bool IsInFiringZone => _currentGridSlot != null && _currentGridSlot.Zone == GridZone.Firing;
        public bool IsInActiveZone => _currentGridSlot != null && _currentGridSlot.Zone == GridZone.Active;
        public bool IsInPassiveZone => _currentGridSlot != null && _currentGridSlot.Zone == GridZone.Passive;
        public bool IsDead => _isDead;
        public bool IsAlive => !_isDead && Health != null && Health.CurrentHealth > 0;
        public bool IsOutOfAmmo => !UnlimitedAmmo && _isOutOfAmmo;
        public bool IsFunctional => IsAlive && !IsOutOfAmmo;
        
        // Ammo properties - now use cached value updated via events (event-driven pattern)
        public int CurrentAmmo
        {
            get
            {
                if (UnlimitedAmmo) return -1;
                return _currentAmmo; // Return cached value from MMAmmoEvent
            }
        }
        
        public int MaxAmmo => UnlimitedAmmo ? -1 : StartingAmmo;
        public float AmmoPercentage => UnlimitedAmmo ? 1f : (float)CurrentAmmo / StartingAmmo;
        public bool IsAmmoLow => !UnlimitedAmmo && CurrentAmmo <= LowAmmoThreshold && CurrentAmmo > 0;
        public Transform CurrentTarget => AIBrain != null ? AIBrain.Target : null;
        public bool IsFiring => AIBrain != null && AIBrain.BrainActive && CurrentTarget != null;
        public Weapon CurrentWeapon => _currentWeapon;
        
        #endregion
        
        #region Initialization
        
	void Awake()
	{
		// Find TDE Character Components using MMGetComponentNoAlloc (TDE performance pattern)
		// 30-50% faster than GetComponent, no GC allocations
		Character = gameObject.MMGetComponentNoAlloc<Character>();
		Health = gameObject.MMGetComponentNoAlloc<Health>();
		HandleWeapon = gameObject.MMGetComponentNoAlloc<CharacterHandleWeapon>();
		Orientation3D = gameObject.MMGetComponentNoAlloc<CharacterOrientation3D>();
		HeroAmmoAbility = gameObject.MMGetComponentNoAlloc<HeroAmmo>();
		
		// CRITICAL FIX: Add HeroAmmo component if missing
		if (HeroAmmoAbility == null)
		{
			Debug.LogWarning($"[Hero] {gameObject.name} missing HeroAmmo component - adding automatically");
			HeroAmmoAbility = gameObject.AddComponent<HeroAmmo>();
		}
		
		// Find AI Components (on child GameObject or self) - use GetComponentInChildren for hierarchy search
		AIBrain = GetComponentInChildren<AIBrain>();
		AIActionShoot = GetComponentInChildren<AIActionShoot3D>();
		AIActionAim = GetComponentInChildren<AIActionAimWeaponAtTarget3D>();
		AIDecisionDetectPriority = GetComponentInChildren<AIDecisionDetectTargetPriority3D>();
		AIDecisionLOS = GetComponentInChildren<AIDecisionLineOfSightToTarget3D>();
		
		// CRITICAL: Disable AI Brain immediately on instantiation
		// This prevents AI from activating before hero reaches Firing zone
		if (AIBrain != null)
		{
			AIBrain.BrainActive = false;
			Debug.Log($"[Hero] {gameObject.name} AIBrain disabled in Awake() - will activate when entering Firing zone");
		}
		
		_renderer = gameObject.MMGetComponentNoAlloc<Renderer>();
		if (_renderer != null)
		{
			_originalMaterial = _renderer.material;
		}
	}        void Start()
        {
            InitializeHero();
        }
        
        protected virtual void InitializeFromData()
        {
            if (HeroData == null)
            {
                Debug.LogWarning($"[Hero] InitializeFromData called but HeroData is null on {gameObject.name}");
                return;
            }
            
            HeroData.ApplyToHero(this);
            Debug.Log($"[Hero] Loaded stats from {HeroData.name}. DPS: {HeroData.DPS:F1}, Ammo Lifetime: {HeroData.AmmoLifetime:F1}s");
        }
        
        protected virtual void InitializeHero()
        {
            if (HeroData == null)
            {
                Debug.LogError($"[Hero] {gameObject.name} has no HeroDataSO assigned!");
                return;
            }
            
            InitializeFromData();
            ConfigureAI();
            
            if (Health != null)
            {
                Health.OnDeath += OnHeroDeath;
                Health.OnRevive += OnHeroRevive;
            }
            
            InitializeWeaponSystem();
            InitializeAmmo();
            
            // Register with CombatCoordinator for ammo tracking (Option B: skip unlimited ammo)
            if (!UnlimitedAmmo && CombatCoordinator.HasInstance)
            {
                CombatCoordinator.Instance.RegisterHero(gameObject, StartingAmmo);
                Debug.Log($"[Hero] {HeroName} registered with CombatCoordinator. Ammo: {StartingAmmo}");
            }
            else if (UnlimitedAmmo)
            {
                Debug.Log($"[Hero] {HeroName} has unlimited ammo - skipping CombatCoordinator registration");
            }
        }
        
        void OnDestroy()
        {
            if (Health != null)
            {
                Health.OnDeath -= OnHeroDeath;
                Health.OnRevive -= OnHeroRevive;
            }
            
            // Unregister from CombatCoordinator
            if (CombatCoordinator.HasInstance)
            {
                CombatCoordinator.Instance.UnregisterHero(gameObject);
            }
        }
        
        void OnEnable()
        {
            this.MMEventStartListening<MMAmmoEvent>();
        }
        
        void OnDisable()
        {
            this.MMEventStopListening<MMAmmoEvent>();
        }
        
        /// <summary>
        /// Handles ammo change events (TDE event-driven pattern)
        /// </summary>
        public void OnMMEvent(MMAmmoEvent ammoEvent)
        {
            // Only process events for this hero
            if (ammoEvent.Hero == gameObject)
            {
                _currentAmmo = ammoEvent.CurrentAmmo;
                _displayCurrentAmmo = _currentAmmo; // Update Inspector display
                
                // Check for low ammo warning
                if (IsAmmoLow && !_isOutOfAmmo)
                {
                    // Could trigger UI warning, sound, etc.
                }
            }
        }
        
        #endregion
        
        #region Weapon System (TDE Auto-Aim)
        
        protected virtual void InitializeWeaponSystem()
        {
            if (HandleWeapon == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} missing CharacterHandleWeapon ability.");
                return;
            }
            
            if (WeaponAttachment == null)
            {
                WeaponAttachment = transform.Find("WeaponAttachment");
                if (WeaponAttachment == null)
                {
                    GameObject attachment = new GameObject("WeaponAttachment");
                    attachment.transform.SetParent(transform);
                    attachment.transform.localPosition = Vector3.zero;
                    attachment.transform.localRotation = Quaternion.identity;
                    WeaponAttachment = attachment.transform;
                }
            }
            
            HandleWeapon.WeaponAttachment = WeaponAttachment;
            
            // Equip weapon immediately on initialization
            // This ensures weapon is ready when AIBrain activates in Firing zone
            if (WeaponPrefab != null)
            {
                Debug.Log($"[Hero] {HeroName} equipping weapon on initialization: {WeaponPrefab.WeaponName}");
                EquipWeapon(WeaponPrefab);
            }
            else
            {
                Debug.LogWarning($"[Hero] {HeroName} has no weapon prefab assigned in HeroDataSO!");
            }
            
            // Disable AI Brain initially - will activate when entering Firing zone
            if (AIBrain != null)
            {
                AIBrain.BrainActive = false;
            }
            
            Debug.Log($"[Hero] {HeroName} weapon system initialized. Weapon equipped. AI will activate when entering Firing zone.");
        }
        
        /// <summary>
        /// Configures AI components with stats from HeroDataSO
        /// </summary>
        protected virtual void ConfigureAI()
        {
            if (HeroData == null)
            {
                Debug.LogWarning($"[Hero] ConfigureAI called but HeroData is null on {gameObject.name}");
                return;
            }
            
            if (AIBrain == null)
            {
                Debug.LogError($"[Hero] {HeroName} has no AIBrain component! Hero requires AIBrain for combat behavior.");
                return;
            }
            
            // Set AIBrain owner
            AIBrain.Owner = gameObject;
            
            // Configure target detection with priority
            if (AIDecisionDetectPriority != null)
            {
                // Priority-based detection uses MMConeOfVision (configured in prefab)
                // No runtime configuration needed - radius/angle set on MMConeOfVision component
                Debug.Log($"[Hero] {HeroName} AI Priority Detection configured - Priority: {AIDecisionDetectPriority.Priority}");
            }
            else
            {
                Debug.LogWarning($"[Hero] {HeroName} has no AIDecisionDetectTargetPriority3D! Target selection may not work correctly.");
            }
            
            // Configure line-of-sight checking
            if (AIDecisionLOS != null)
            {
                AIDecisionLOS.ObstacleLayerMask = ObstacleLayerMask;
                Debug.Log($"[Hero] {HeroName} AI Line-of-Sight configured");
            }
            
            // Configure shooting action
            if (AIActionShoot != null)
            {
                AIActionShoot.TargetHandleWeaponAbility = HandleWeapon;
                AIActionShoot.AimAtTarget = true;
                AIActionShoot.ShootOffset = Vector3.up * 1.8f; // Aim at torso height
                AIActionShoot.LockVerticalAim = false;
                Debug.Log($"[Hero] {HeroName} AI Shooting configured");
            }
            
            // Configure aiming action
            if (AIActionAim != null)
            {
                AIActionAim.TargetHandleWeaponAbility = HandleWeapon;
                Debug.Log($"[Hero] {HeroName} AI Aiming configured");
            }
            
            _aiConfigured = true;
            Debug.Log($"[Hero] {HeroName} AI configuration complete. States: {AIBrain.States.Count}");
        }
        
        public virtual void EquipWeapon(Weapon weaponPrefab)
        {
            if (HandleWeapon == null)
            {
                Debug.LogError($"[Hero] {HeroName} cannot equip weapon - HandleWeapon component is null!");
                return;
            }
            
            if (weaponPrefab == null)
            {
                Debug.LogError($"[Hero] {HeroName} cannot equip weapon - weaponPrefab is null!");
                return;
            }
            
            // Ensure CharacterHandleWeapon has WeaponAttachment set
            if (HandleWeapon.WeaponAttachment == null)
            {
                if (WeaponAttachment != null)
                {
                    HandleWeapon.WeaponAttachment = WeaponAttachment;
                    Debug.Log($"[Hero] {HeroName} assigned WeaponAttachment to CharacterHandleWeapon");
                }
                else
                {
                    Debug.LogError($"[Hero] {HeroName} cannot equip weapon - WeaponAttachment is null!");
                    return;
                }
            }
            
            Debug.Log($"[Hero] {HeroName} equipping weapon: {weaponPrefab.WeaponName}");
            
            // Let TDE handle instantiation through ChangeWeapon
            HandleWeapon.ChangeWeapon(weaponPrefab, weaponPrefab.WeaponName);
            
            // Give TDE a frame to instantiate the weapon
            StartCoroutine(WaitForWeaponEquip());
        }
        
        private System.Collections.IEnumerator WaitForWeaponEquip()
        {
            yield return null; // Wait one frame
            
            // Get reference to the TDE-instantiated weapon
            _currentWeapon = HandleWeapon.CurrentWeapon;
            
            if (_currentWeapon == null)
            {
                Debug.LogError($"[Hero] {HeroName} weapon instantiation failed! HandleWeapon.CurrentWeapon is null after ChangeWeapon().");
                Debug.LogError($"[Hero] Check that: 1) WeaponAttachment exists, 2) Weapon prefab is valid, 3) CharacterHandleWeapon is properly configured");
            }
            else
            {
                Debug.Log($"[Hero] {HeroName} equipped weapon '{_currentWeapon.WeaponName}' successfully. AI will control shooting.");
                
                // Apply homing settings if enabled in HeroDataSO
                ApplyHomingSettings();
            }
        }
        
        /// <summary>
        /// Apply homing projectile settings from HeroDataSO to the equipped weapon
        /// </summary>
        protected virtual void ApplyHomingSettings()
        {
            if (HeroData == null || _currentWeapon == null) return;
            
            if (HeroData.UseHomingProjectiles)
            {
                ProjectileWeapon projectileWeapon = _currentWeapon as ProjectileWeapon;
                if (projectileWeapon != null && projectileWeapon.ObjectPooler != null)
                {
                    // Get projectile prefab from object pooler
                    MMSimpleObjectPooler simplePooler = projectileWeapon.ObjectPooler as MMSimpleObjectPooler;
                    if (simplePooler != null && simplePooler.GameObjectToPool != null)
                    {
                        HomingProjectile homingProjectile = simplePooler.GameObjectToPool.GetComponent<HomingProjectile>();
                        if (homingProjectile != null)
                        {
                            homingProjectile.TurnSpeed = HeroData.HomingTurnSpeed;
                            homingProjectile.HomingDuration = HeroData.HomingDuration;
                            
                            Debug.Log($"[Hero] {HeroName} applied homing settings: TurnSpeed={HeroData.HomingTurnSpeed}, Duration={HeroData.HomingDuration}");
                        }
                        else
                        {
                            Debug.LogWarning($"[Hero] {HeroName} has UseHomingProjectiles=true but projectile prefab doesn't have HomingProjectile component!");
                        }
                    }
                }
            }
        }
        
        public virtual void StartFiring()
        {
            if (!IsInFiringZone)
            {
                Debug.LogWarning($"[Hero] {HeroName} cannot start firing - not in Firing zone.");
                return;
            }
            
            if (IsOutOfAmmo)
            {
                Debug.LogWarning($"[Hero] {HeroName} cannot start firing - out of ammo!");
                return;
            }
            
            // Weapon should already be equipped from initialization
            // If not, try to equip it now
            if (_currentWeapon == null && WeaponPrefab != null)
            {
                Debug.LogWarning($"[Hero] {HeroName} weapon not equipped during init, equipping now...");
                EquipWeapon(WeaponPrefab);
                // Note: AIBrain will activate next frame after weapon is equipped
                StartCoroutine(ActivateAIAfterWeaponEquip());
                return;
            }
            
            // Activate AI Brain for combat
            ActivateAICombat();
        }
        
        /// <summary>
        /// Activates AI Brain for combat behavior
        /// </summary>
        private void ActivateAICombat()
        {
            if (AIBrain == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} cannot start firing - no AI Brain!");
                return;
            }
            
            if (!_aiConfigured)
            {
                Debug.LogWarning($"[Hero] {HeroName} AI not configured!");
                ConfigureAI();
            }
            
            AIBrain.BrainActive = true;
            
            // Transition to Combat state (if it exists in inspector)
            // If state doesn't exist, AI will stay in current state but active
            if (HasAIState("Combat"))
            {
                AIBrain.TransitionToState("Combat");
                Debug.Log($"[Hero] {HeroName} AI activated - transitioned to Combat state.");
            }
            else
            {
                Debug.Log($"[Hero] {HeroName} AI activated - using current state (Combat state not found).");
            }
        }
        
        /// <summary>
        /// Waits for weapon to equip, then activates AI
        /// </summary>
        private System.Collections.IEnumerator ActivateAIAfterWeaponEquip()
        {
            yield return null; // Wait one frame for weapon equip
            
            if (_currentWeapon != null)
            {
                ActivateAICombat();
            }
            else
            {
                Debug.LogError($"[Hero] {HeroName} weapon still null after equip attempt! Cannot activate AI.");
            }
        }
        
        public virtual void StopFiring()
        {
            // Deactivate AI Brain
            if (AIBrain != null)
            {
                AIBrain.BrainActive = false;
                
                // Transition to Idle or Inactive state if available
                if (HasAIState("Idle"))
                {
                    AIBrain.TransitionToState("Idle");
                    Debug.Log($"[Hero] {HeroName} AI deactivated - transitioned to Idle state.");
                }
                else if (HasAIState("Inactive"))
                {
                    AIBrain.TransitionToState("Inactive");
                    Debug.Log($"[Hero] {HeroName} AI deactivated - transitioned to Inactive state.");
                }
                else
                {
                    Debug.Log($"[Hero] {HeroName} AI deactivated.");
                }
            }
            
            // Stop shooting
            if (HandleWeapon != null) HandleWeapon.ShootStop();
            
            // Keep weapon equipped for potential re-entry to Firing zone
            // Weapon will only be destroyed on hero removal/death
            
            Debug.Log($"[Hero] {HeroName} stopped firing. Weapon kept equipped for potential re-entry.");
        }
        
        #endregion
        
        #region Ammo System
        
        protected virtual void InitializeAmmo()
        {
            // Initialize HeroAmmo ability (TDE pattern)
            if (HeroAmmoAbility != null)
            {
                HeroAmmoAbility.InitializeAmmo(StartingAmmo, UnlimitedAmmo);
                HeroAmmoAbility.LowAmmoThreshold = LowAmmoThreshold;
                Debug.Log($"[Hero] {HeroName} initialized HeroAmmo ability. Ammo: {StartingAmmo}, Unlimited: {UnlimitedAmmo}");
            }
            
            if (UnlimitedAmmo)
            {
                _currentAmmo = -1;
                _isOutOfAmmo = false;
            }
            else
            {
                _currentAmmo = StartingAmmo;
                _isOutOfAmmo = false;
            }
        }
        
        protected virtual int GetAmmoConsumptionRate()
        {
            if (_currentWeapon != null)
            {
                var weaponDataHolder = _currentWeapon.GetComponent<WeaponDataHolder>();
                if (weaponDataHolder != null && weaponDataHolder.WeaponData != null)
                {
                    return weaponDataHolder.GetAmmoPerShot();
                }
            }
            return 1;
        }
        
        public virtual bool ConsumeAmmo(int amount = -1)
        {
            if (amount <= 0) amount = GetAmmoConsumptionRate();
            if (UnlimitedAmmo) return true;
            if (_isOutOfAmmo) return false;
            
            if (_currentAmmo < amount)
            {
                _currentAmmo = 0;
                OnAmmoDepletion();
                return false;
            }
            
            _currentAmmo -= amount;
            
            if (_currentAmmo <= 0)
            {
                _currentAmmo = 0;
                OnAmmoDepletion();
                return false;
            }
            
            if (_currentAmmo == LowAmmoThreshold)
            {
                OnAmmoLow();
            }
            
            return true;
        }
        
        public virtual void AddAmmo(int amount)
        {
            if (UnlimitedAmmo) return;
            
            _currentAmmo += amount;
            if (_currentAmmo > StartingAmmo) _currentAmmo = StartingAmmo;
            
            if (_isOutOfAmmo && _currentAmmo > 0)
            {
                _isOutOfAmmo = false;
                if (IsInFiringZone) StartFiring();
            }
        }
        
        public virtual void OnAmmoDepletion()
        {
            _isOutOfAmmo = true;
            StopFiring();
            Debug.LogWarning($"[Hero] {HeroName} OUT OF AMMO!");
            StartCoroutine(RemoveFromGridAfterDelay("ammo depletion"));
        }
        
        protected virtual void OnAmmoLow()
        {
            Debug.LogWarning($"[Hero] {HeroName} ammo LOW! ({_currentAmmo} remaining)");
        }
        
        #endregion
        
        #region Update (Display & Safety Checks)
        
        void Update()
        {
            _displayCurrentAmmo = CurrentAmmo; // Now queries CombatCoordinator
            _displayCurrentTarget = CurrentTarget;
            _displayCurrentSlot = _currentGridSlot != null ? _currentGridSlot.GetCoordinateLabel() : "None";
            _displayCurrentZone = _currentGridSlot != null ? _currentGridSlot.Zone.ToString() : "None";
            
            // DEFENSIVE: Ensure AI only active in Firing zone (safety net)
            if (AIBrain != null && AIBrain.BrainActive && !IsInFiringZone)
            {
                Debug.LogWarning($"[Hero] {HeroName} AI was active outside Firing zone! Current: {_currentGridSlot?.Zone}. Deactivating...");
                StopFiring();
            }
            
            // NOTE: Ammo consumption now handled by CombatCoordinator.OnHeroFiredBullet()
            // No need to track weapon state transitions here
        }
        
        #endregion
        
        #region Debug Visualization
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            
            if (CurrentTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, CurrentTarget.position);
            }
            
            if (_currentWeapon != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(_currentWeapon.transform.position, _currentWeapon.transform.forward * 5f);
            }
        }
        
        #endregion
        
        #region Zone Management
        
        /// <summary>
        /// Called automatically when hero changes grid zones.
        /// Handles starting/stopping combat based on zone.
        /// </summary>
        protected virtual void OnZoneChanged(GridZone? oldZone, GridZone? newZone)
        {
            Debug.Log($"[Hero] {HeroName} zone changed: {oldZone?.ToString() ?? "None"} → {newZone?.ToString() ?? "None"}");
            
            // Entering Firing zone - start combat
            if (newZone == GridZone.Firing)
            {
                Debug.Log($"[Hero] {HeroName} entered Firing zone - starting combat automatically");
                StartFiring();
            }
            // Leaving Firing zone - stop combat
            else if (oldZone == GridZone.Firing && newZone != GridZone.Firing)
            {
                Debug.Log($"[Hero] {HeroName} left Firing zone - stopping combat");
                StopFiring();
            }
            // Entering non-Firing zones - ensure AI is inactive
            else if (newZone == GridZone.Active || newZone == GridZone.Passive)
            {
                EnsureAIInactive();
            }
        }
        
        /// <summary>
        /// Ensures AI Brain is inactive (safety check for non-Firing zones)
        /// </summary>
        private void EnsureAIInactive()
        {
            if (AIBrain != null && AIBrain.BrainActive)
            {
                Debug.LogWarning($"[Hero] {HeroName} AI was active in non-Firing zone! Deactivating...");
                StopFiring();
            }
        }
        
        #endregion
        
        #region AI Utilities
        
        /// <summary>
        /// Checks if AI Brain has a state with the given name
        /// </summary>
        protected virtual bool HasAIState(string stateName)
        {
            if (AIBrain == null || AIBrain.States == null) return false;
            
            foreach (var state in AIBrain.States)
            {
                if (state.StateName == stateName)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Activates idle behavior when hero is not in Firing zone
        /// </summary>
        public virtual void StartIdleBehavior()
        {
            if (AIBrain == null) return;
            
            // Optionally activate AI for idle animations/behaviors
            if (HasAIState("Idle"))
            {
                AIBrain.BrainActive = true;
                AIBrain.TransitionToState("Idle");
                Debug.Log($"[Hero] {HeroName} started idle behavior.");
            }
        }
        
        /// <summary>
        /// Deactivates idle behavior
        /// </summary>
        public virtual void StopIdleBehavior()
        {
            if (AIBrain == null) return;
            
            if (HasAIState("Inactive"))
            {
                AIBrain.TransitionToState("Inactive");
            }
            AIBrain.BrainActive = false;
            Debug.Log($"[Hero] {HeroName} stopped idle behavior.");
        }
        
        #endregion
        
        #region Grid Interaction
        
        void OnMouseDown()
        {
            if (HeroQueueManager.Instance != null && HeroQueueManager.Instance.IsAnimating)
            {
                return;
            }
            
            if (IsInActiveZone)
            {
                HeroQueueManager.Instance?.OnHeroClicked(this);
            }
        }
        
        public void Highlight()
        {
            if (_renderer != null && HighlightMaterial != null)
            {
                _renderer.material = HighlightMaterial;
            }
        }
        
        public void Unhighlight()
        {
            if (_renderer != null && _originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
        }
        
        #endregion
        
        #region Death & Lifecycle
        
        protected virtual void OnHeroDeath()
        {
            _isDead = true;
            StopFiring();
            Debug.Log($"[Hero] {HeroName} died!");
            StartCoroutine(RemoveFromGridAfterDelay("death"));
        }
        
        protected virtual void OnHeroRevive()
        {
            _isDead = false;
            Debug.Log($"[Hero] {HeroName} revived!");
        }
        
        protected virtual System.Collections.IEnumerator RemoveFromGridAfterDelay(string reason)
        {
            if (_isBeingRemoved) yield break;
            _isBeingRemoved = true;
            
            Debug.Log($"[Hero] {HeroName} will be removed in {RemovalDelay}s (reason: {reason}).");
            yield return new WaitForSeconds(RemovalDelay);
            
            if (CurrentGridSlot != null)
            {
                if (ProjectBlast.Grid.GridManager.Instance != null)
                {
                    ProjectBlast.Grid.GridManager.Instance.RemoveHero(this);
                }
                CurrentGridSlot = null;
            }
            
            if (HeroQueueManager.Instance != null)
            {
                HeroQueueManager.Instance.OnHeroRemoved(this, reason);
            }
            
            if (DestroyOnRemoval)
            {
                Destroy(gameObject);
            }
            else
            {
                transform.position = new Vector3(1000, -1000, 1000);
                gameObject.SetActive(false);
            }
        }
        
        #endregion
    }
    
    public enum HeroClass
    {
        Ranged,
        Tank,
        Support,
        AOE,
        Melee,
        Special
    }
}
