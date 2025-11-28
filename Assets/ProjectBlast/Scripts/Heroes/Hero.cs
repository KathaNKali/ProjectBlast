using UnityEngine;
using ProjectBlast.Grid;
using ProjectBlast.Data;
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
    public class Hero : MonoBehaviour
    {
        #region Inspector Fields
        
        [Header("Hero Configuration")]
        public HeroDataSO HeroData;
        
        [Header("Grid Integration")]
        public GridSlot CurrentGridSlot;
        
        [Header("Visual Feedback")]
        public Material HighlightMaterial;
        
        [Header("TDE Components (Auto-found)")]
        public Character Character;
        public Health Health;
        public CharacterHandleWeapon HandleWeapon;
        public CharacterOrientation3D Orientation3D;
        
        [Header("AI Components (Auto-found)")]
        public AIBrain AIBrain;
        public AIActionShoot3D AIActionShoot;
        public AIActionAimWeaponAtTarget3D AIActionAim;
        public AIDecisionDetectTargetRadius3D AIDecisionDetect;
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
        
        public bool IsInFiringZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Firing;
        public bool IsInActiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Active;
        public bool IsInPassiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Passive;
        public bool IsDead => _isDead;
        public bool IsAlive => !_isDead && Health != null && Health.CurrentHealth > 0;
        public bool IsOutOfAmmo => !UnlimitedAmmo && _isOutOfAmmo;
        public bool IsFunctional => IsAlive && !IsOutOfAmmo;
        public int CurrentAmmo => UnlimitedAmmo ? -1 : _currentAmmo;
        public int MaxAmmo => UnlimitedAmmo ? -1 : StartingAmmo;
        public float AmmoPercentage => UnlimitedAmmo ? 1f : (float)_currentAmmo / StartingAmmo;
        public bool IsAmmoLow => !UnlimitedAmmo && _currentAmmo <= LowAmmoThreshold && _currentAmmo > 0;
        public Transform CurrentTarget => AIBrain != null ? AIBrain.Target : null;
        public bool IsFiring => AIBrain != null && AIBrain.BrainActive && CurrentTarget != null;
        public Weapon CurrentWeapon => _currentWeapon;
        
        #endregion
        
        #region Initialization
        
        void Awake()
        {
            // Find TDE Character Components
            Character = GetComponent<Character>();
            Health = GetComponent<Health>();
            HandleWeapon = GetComponent<CharacterHandleWeapon>();
            Orientation3D = GetComponent<CharacterOrientation3D>();
            
            // Find AI Components (on child GameObject or self)
            AIBrain = GetComponentInChildren<AIBrain>();
            AIActionShoot = GetComponentInChildren<AIActionShoot3D>();
            AIActionAim = GetComponentInChildren<AIActionAimWeaponAtTarget3D>();
            AIDecisionDetect = GetComponentInChildren<AIDecisionDetectTargetRadius3D>();
            AIDecisionLOS = GetComponentInChildren<AIDecisionLineOfSightToTarget3D>();
            
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }
        
        void Start()
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
        }
        
        void OnDestroy()
        {
            if (Health != null)
            {
                Health.OnDeath -= OnHeroDeath;
                Health.OnRevive -= OnHeroRevive;
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
            
            // Disable AI Brain initially - will activate when entering Firing zone
            if (AIBrain != null)
            {
                AIBrain.BrainActive = false;
            }
            
            Debug.Log($"[Hero] {HeroName} weapon system and AI initialized. AI will activate when entering Firing zone.");
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
            
            // Configure target detection
            if (AIDecisionDetect != null)
            {
                AIDecisionDetect.Radius = DetectionRange;
                AIDecisionDetect.TargetLayerMask = TargetLayerMask;
                AIDecisionDetect.ObstacleMask = ObstacleLayerMask;
                AIDecisionDetect.TargetCheckFrequency = TargetSearchInterval;
                Debug.Log($"[Hero] {HeroName} AI Detection configured - Range: {DetectionRange}m, Scan: {TargetSearchInterval}s");
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
            if (HandleWeapon == null || weaponPrefab == null)
            {
                Debug.LogError($"[Hero] {HeroName} cannot equip weapon.");
                return;
            }
            
            // Let TDE handle instantiation through ChangeWeapon
            HandleWeapon.ChangeWeapon(weaponPrefab, weaponPrefab.WeaponName);
            
            // Get reference to the TDE-instantiated weapon
            _currentWeapon = HandleWeapon.CurrentWeapon;
            
            if (_currentWeapon == null)
            {
                Debug.LogError($"[Hero] {HeroName} weapon instantiation failed!");
                return;
            }
            
            Debug.Log($"[Hero] {HeroName} equipped weapon. AI will control shooting.");
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
            
            // Equip weapon when entering Firing zone
            if (_currentWeapon == null && WeaponPrefab != null)
            {
                EquipWeapon(WeaponPrefab);
            }
            
            // Activate AI Brain for combat
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
            
            // Unequip weapon when leaving Firing zone
            if (_currentWeapon != null)
            {
                Debug.Log($"[Hero] {HeroName} unequipping weapon.");
                Destroy(_currentWeapon.gameObject);
                _currentWeapon = null;
            }
            
            Debug.Log($"[Hero] {HeroName} stopped firing and unequipped weapon.");
        }
        
        #endregion
        
        #region Ammo System
        
        protected virtual void InitializeAmmo()
        {
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
        
        protected virtual void OnAmmoDepletion()
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
        
        #region Update (Ammo Tracking)
        
        void Update()
        {
            _displayCurrentAmmo = _currentAmmo;
            _displayCurrentTarget = CurrentTarget;
            _displayCurrentSlot = CurrentGridSlot != null ? CurrentGridSlot.GetCoordinateLabel() : "None";
            _displayCurrentZone = CurrentGridSlot != null ? CurrentGridSlot.Zone.ToString() : "None";
            
            // Track weapon firing for ammo consumption
            // Only consume ammo when weapon transitions from NOT firing to firing
            if (!UnlimitedAmmo && _currentWeapon != null && AIBrain != null && AIBrain.BrainActive)
            {
                Weapon.WeaponStates currentState = _currentWeapon.WeaponState.CurrentState;
                
                // Weapon just started firing (state transition to WeaponUse)
                if (currentState == Weapon.WeaponStates.WeaponUse && 
                    _lastWeaponState != Weapon.WeaponStates.WeaponUse &&
                    Time.time - _lastAmmoCheckTime > 0.1f) // Prevent double-consumption
                {
                    if (!ConsumeAmmo())
                    {
                        Debug.LogWarning($"[Hero] {HeroName} ran out of ammo!");
                        StopFiring();
                    }
                    _lastAmmoCheckTime = Time.time;
                }
                
                _lastWeaponState = currentState;
            }
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
