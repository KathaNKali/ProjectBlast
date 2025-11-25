using UnityEngine;
using ProjectBlast.Grid;
using ProjectBlast.Data;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// Base Hero class - represents a deployable combat unit.
    /// Heroes are placed on grids and auto-fire at enemies.
    /// 
    /// INTEGRATION WITH TDE:
    /// - Uses Character component for TDE compatibility
    /// - Health component for damage/death
    /// - Weapon system for auto-firing
    /// - MMFeedbacks for visual/audio polish
    /// 
    /// HERO LIFECYCLE:
    /// 1. Spawn in Passive Grid (queue)
    /// 2. Shift to Active Grid (ready)
    /// 3. Deploy to Firing Grid (combat)
    /// 4. Auto-fire at enemies
    /// 5. Death or ammo depletion = permanent loss
    /// 
    /// SCRIPTABLEOBJECT INTEGRATION:
    /// - Assign HeroData SO to load all stats
    /// - Inspector values used as fallback if HeroData not assigned
    /// - Call InitializeFromData() to apply SO stats
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hero : MonoBehaviour
    {
        #region Inspector Fields
        
        [Header("Hero Configuration")]
        [Tooltip("Hero data ScriptableObject (all stats loaded from here if assigned)")]
        public HeroDataSO HeroData;
        
        [Header("Grid Integration")]
        [Tooltip("Current slot this hero occupies (managed by GridManager)")]
        public GridSlot CurrentGridSlot;
        
        [Header("Visual Feedback")]
        [Tooltip("Material to apply when hero is selected/highlighted")]
        public Material HighlightMaterial;
        
        [Header("TDE Components (Auto-found)")]
        [Tooltip("TDE Character component (auto-assigned)")]
        public Character Character;
        
        [Tooltip("TDE Health component (auto-assigned)")]
        public Health Health;
        
        [Tooltip("TDE CharacterHandleWeapon ability (auto-assigned)")]
        public CharacterHandleWeapon HandleWeapon;
        
        [Header("Weapon Configuration")]
        [Tooltip("Transform where weapon will be attached (child of this GameObject)")]
        public Transform WeaponAttachment;
        
        [Header("Read-Only Runtime Info")]
        [Tooltip("Current ammo count (runtime)")]
        [SerializeField] private int _displayCurrentAmmo;
        
        [Tooltip("Is this hero currently firing? (runtime)")]
        [SerializeField] private bool _displayIsFiring;
        
        [Header("Lifecycle Settings")]
        [Tooltip("Delay before removing hero from grid after death/depletion (seconds)")]
        public float RemovalDelay = 1.5f;
        
        [Tooltip("Should hero GameObject be destroyed after removal?")]
        public bool DestroyOnRemoval = true;
        
        #endregion
        
        #region Runtime Properties (Data-Driven)
        
        /// <summary>
        /// Hero name - reads from HeroDataSO if available
        /// </summary>
        public string HeroName => HeroData != null ? HeroData.HeroName : gameObject.name;
        
        /// <summary>
        /// Hero class - reads from HeroDataSO if available
        /// </summary>
        public HeroClass HeroClass => HeroData != null ? HeroData.HeroClass : HeroClass.Ranged;
        
        /// <summary>
        /// Detection range - reads from HeroDataSO if available
        /// </summary>
        public float DetectionRange => HeroData != null ? HeroData.DetectionRange : 20f;
        
        /// <summary>
        /// Target search interval - reads from HeroDataSO if available
        /// </summary>
        public float TargetSearchInterval => HeroData != null ? HeroData.TargetSearchInterval : 0.5f;
        
        /// <summary>
        /// Fire rate - reads from HeroDataSO if available
        /// </summary>
        public float AutoFireRate => HeroData != null ? HeroData.FireRate : 2f;
        
        /// <summary>
        /// Target layer mask - reads from HeroDataSO if available
        /// </summary>
        public LayerMask TargetLayerMask => HeroData != null ? HeroData.TargetLayerMask : 0;
        
        /// <summary>
        /// Unlimited ammo - reads from HeroDataSO if available
        /// </summary>
        public bool UnlimitedAmmo => HeroData != null ? HeroData.UnlimitedAmmo : false;
        
        /// <summary>
        /// Starting ammo - reads from HeroDataSO if available
        /// </summary>
        public int StartingAmmo => HeroData != null ? HeroData.StartingAmmo : 100;
        
        /// <summary>
        /// Low ammo threshold - reads from HeroDataSO if available
        /// </summary>
        public int LowAmmoThreshold => HeroData != null ? HeroData.LowAmmoThreshold : 20;
        
        /// <summary>
        /// Weapon prefab - reads from HeroDataSO if available
        /// </summary>
        public Weapon WeaponPrefab => HeroData != null ? HeroData.DefaultWeaponPrefab : null;
        
        #endregion
        
        #region Private Fields
        
        private Material _originalMaterial;
        private Renderer _renderer;
        private bool _isDead = false;
        private Weapon _currentWeapon;
        private bool _isFiring = false;
        private int _currentAmmo;
        private bool _isOutOfAmmo = false;
        private bool _isBeingRemoved = false;
        
        #endregion
        
        /// <summary>
        /// Whether this hero is currently in the Firing zone and can attack
        /// </summary>
        public bool IsInFiringZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Firing;
        
        /// <summary>
        /// Whether this hero is in the Active zone and ready for deployment
        /// </summary>
        public bool IsInActiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Active;
        
        /// <summary>
        /// Whether this hero is in the Passive zone and waiting in queue
        /// </summary>
        public bool IsInPassiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Passive;
        
        /// <summary>
        /// Whether this hero is dead
        /// </summary>
        public bool IsDead => _isDead;
        
        /// <summary>
        /// Whether this hero is alive and functional
        /// </summary>
        public bool IsAlive => !_isDead && Health != null && Health.CurrentHealth > 0;
        
        /// <summary>
        /// Whether this hero has run out of ammo
        /// </summary>
        public bool IsOutOfAmmo => !UnlimitedAmmo && _isOutOfAmmo;
        
        /// <summary>
        /// Whether this hero is functional (alive and has ammo)
        /// </summary>
        public bool IsFunctional => IsAlive && !IsOutOfAmmo;
        
        /// <summary>
        /// Current ammo count
        /// </summary>
        public int CurrentAmmo => UnlimitedAmmo ? -1 : _currentAmmo;
        
        /// <summary>
        /// Maximum ammo capacity
        /// </summary>
        public int MaxAmmo => UnlimitedAmmo ? -1 : StartingAmmo;
        
        /// <summary>
        /// Ammo as percentage (0-1), returns 1.0 if unlimited
        /// </summary>
        public float AmmoPercentage => UnlimitedAmmo ? 1f : (float)_currentAmmo / StartingAmmo;
        
        /// <summary>
        /// Is ammo low (below threshold)?
        /// </summary>
        public bool IsAmmoLow => !UnlimitedAmmo && _currentAmmo <= LowAmmoThreshold && _currentAmmo > 0;
        
        #region Initialization
        
        void Awake()
        {
            // Find TDE components
            Character = GetComponent<Character>();
            Health = GetComponent<Health>();
            HandleWeapon = GetComponent<CharacterHandleWeapon>();
            
            // Cache renderer for visual feedback
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
        
        /// <summary>
        /// Load all stats from HeroDataSO
        /// </summary>
        protected virtual void InitializeFromData()
        {
            if (HeroData == null)
            {
                Debug.LogWarning($"[Hero] InitializeFromData called but HeroData is null on {gameObject.name}");
                return;
            }
            
            // Apply all hero data (this will override inspector values)
            HeroData.ApplyToHero(this);
            
            Debug.Log($"[Hero] Loaded stats from {HeroData.name}. DPS: {HeroData.DPS:F1}, Ammo Lifetime: {HeroData.AmmoLifetime:F1}s");
        }
        
        /// <summary>
        /// Initialize hero components and setup
        /// </summary>
        protected virtual void InitializeHero()
        {
            // Validate HeroDataSO
            if (HeroData == null)
            {
                Debug.LogError($"[Hero] {gameObject.name} has no HeroDataSO assigned! Hero will not function properly.");
                return;
            }
            
            // Load stats from HeroDataSO
            InitializeFromData();
            
            Debug.Log($"[Hero] {HeroName} initialized from {HeroData.name}");
            
            // Validate TDE components
            if (Character == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} missing Character component. Some TDE features won't work.");
            }
            
            if (Health == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} missing Health component. Hero cannot take damage.");
            }
            else
            {
                // Subscribe to health events
                Health.OnDeath += OnHeroDeath;
                Health.OnRevive += OnHeroRevive;
            }
            
            // Setup weapon system
            InitializeWeaponSystem();
            
            // Initialize ammo
            InitializeAmmo();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from health events
            if (Health != null)
            {
                Health.OnDeath -= OnHeroDeath;
                Health.OnRevive -= OnHeroRevive;
            }
        }
        
        #endregion
        
        #region Weapon System
        
        /// <summary>
        /// Initialize weapon system and equip weapon if provided
        /// </summary>
        protected virtual void InitializeWeaponSystem()
        {
            // Validate HandleWeapon ability
            if (HandleWeapon == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} missing CharacterHandleWeapon ability. Cannot use weapons.");
                return;
            }
            
            // Find or create weapon attachment point
            if (WeaponAttachment == null)
            {
                // Look for child named "WeaponAttachment"
                WeaponAttachment = transform.Find("WeaponAttachment");
                
                if (WeaponAttachment == null)
                {
                    // Create weapon attachment point
                    GameObject attachment = new GameObject("WeaponAttachment");
                    attachment.transform.SetParent(transform);
                    attachment.transform.localPosition = Vector3.zero;
                    attachment.transform.localRotation = Quaternion.identity;
                    WeaponAttachment = attachment.transform;
                    Debug.Log($"[Hero] {HeroName} created WeaponAttachment transform.");
                }
            }
            
            // Set weapon attachment on HandleWeapon ability
            HandleWeapon.WeaponAttachment = WeaponAttachment;
            
            // Equip weapon if prefab provided
            if (WeaponPrefab != null)
            {
                EquipWeapon(WeaponPrefab);
            }
        }
        
        /// <summary>
        /// Equip a weapon (instantiate and assign to character)
        /// </summary>
        public virtual void EquipWeapon(Weapon weaponPrefab)
        {
            if (HandleWeapon == null)
            {
                Debug.LogError($"[Hero] {HeroName} cannot equip weapon - no HandleWeapon ability.");
                return;
            }
            
            if (weaponPrefab == null)
            {
                Debug.LogError($"[Hero] {HeroName} cannot equip null weapon.");
                return;
            }
            
            // Instantiate weapon
            GameObject weaponObject = Instantiate(weaponPrefab.gameObject);
            _currentWeapon = weaponObject.GetComponent<Weapon>();
            
            if (_currentWeapon == null)
            {
                Debug.LogError($"[Hero] {HeroName} weapon prefab has no Weapon component.");
                Destroy(weaponObject);
                return;
            }
            
            // Setup weapon
            _currentWeapon.transform.SetParent(WeaponAttachment);
            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;
            
            // Set weapon owner
            _currentWeapon.SetOwner(Character, HandleWeapon);
            
            // Configure auto-aim if weapon supports it
            ConfigureWeaponAutoAim(_currentWeapon);
            
            // Assign to HandleWeapon ability
            HandleWeapon.ChangeWeapon(_currentWeapon, null);
            
            Debug.Log($"[Hero] {HeroName} equipped weapon: {_currentWeapon.name}");
        }
        
        /// <summary>
        /// Configure auto-aim settings on weapon
        /// </summary>
        protected virtual void ConfigureWeaponAutoAim(Weapon weapon)
        {
            // Check if weapon has WeaponAim component
            WeaponAim weaponAim = weapon.GetComponent<WeaponAim>();
            if (weaponAim != null)
            {
                // Configure for script-controlled aiming (we'll handle targeting)
                weaponAim.AimControl = WeaponAim.AimControls.Script;
                
                // Disable reticle to prevent MMUIFollowMouse errors
                weaponAim.ReticleType = WeaponAim.ReticleTypes.None;
                weaponAim.DisplayReticle = false;
                
                Debug.Log($"[Hero] {HeroName} configured weapon aim to Script mode (reticle disabled).");
            }
            
            // Disable any MMUIFollowMouse components that might cause errors
            MMUIFollowMouse[] followMouseComponents = weapon.GetComponentsInChildren<MMUIFollowMouse>();
            foreach (var component in followMouseComponents)
            {
                component.enabled = false;
                Debug.Log($"[Hero] {HeroName} disabled MMUIFollowMouse component on weapon.");
            }
            
            // For auto-targeting, we'll need to implement target finding separately
            // TDE doesn't have built-in auto-targeting - we'll add that logic next
        }
        
        /// <summary>
        /// Start firing weapon (called when entering Firing zone)
        /// </summary>
        public virtual void StartFiring()
        {
            if (_isFiring) return;
            
            if (_currentWeapon == null)
            {
                Debug.LogWarning($"[Hero] {HeroName} cannot start firing - no weapon equipped.");
                return;
            }
            
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
            
            _isFiring = true;
            _lastTargetSearchTime = Time.time;
            _lastFireTime = Time.time;
            
            string ammoInfo = UnlimitedAmmo ? "unlimited ammo" : $"{_currentAmmo} ammo";
            Debug.Log($"[Hero] {HeroName} started firing with {ammoInfo}. Searching for targets in range {DetectionRange}m.");
        }
        
        /// <summary>
        /// Stop firing weapon (called when leaving Firing zone or dying)
        /// </summary>
        public virtual void StopFiring()
        {
            if (!_isFiring) return;
            
            _isFiring = false;
            _currentTarget = null;
            
            // Stop weapon if currently shooting
            if (HandleWeapon != null)
            {
                HandleWeapon.ShootStop();
            }
            
            Debug.Log($"[Hero] {HeroName} stopped firing.");
        }
        
        /// <summary>
        /// Whether this hero is currently firing
        /// </summary>
        public bool IsFiring => _isFiring;
        
        /// <summary>
        /// Get current weapon reference
        /// </summary>
        public Weapon CurrentWeapon => _currentWeapon;
        
        #endregion
        
        #region Ammo System
        
        /// <summary>
        /// Initialize ammo system
        /// </summary>
        protected virtual void InitializeAmmo()
        {
            if (UnlimitedAmmo)
            {
                _currentAmmo = -1; // Unlimited
                _isOutOfAmmo = false;
                Debug.Log($"[Hero] {HeroName} has unlimited ammo.");
            }
            else
            {
                _currentAmmo = StartingAmmo;
                _isOutOfAmmo = false;
                Debug.Log($"[Hero] {HeroName} initialized with {_currentAmmo} ammo.");
            }
        }
        
        /// <summary>
        /// Get ammo consumption rate from weapon (or fallback to 1)
        /// </summary>
        protected virtual int GetAmmoConsumptionRate()
        {
            // Try to get from weapon's WeaponDataHolder
            if (_currentWeapon != null)
            {
                var weaponDataHolder = _currentWeapon.GetComponent<WeaponDataHolder>();
                if (weaponDataHolder != null && weaponDataHolder.WeaponData != null)
                {
                    return weaponDataHolder.GetAmmoPerShot();
                }
            }
            
            // Fallback to 1 ammo per shot
            return 1;
        }
        
        /// <summary>
        /// Consume ammo (called when firing)
        /// </summary>
        /// <returns>True if ammo was consumed, false if out of ammo</returns>
        public virtual bool ConsumeAmmo(int amount = -1)
        {
            // Use ammo consumption from weapon if available
            if (amount <= 0)
            {
                amount = GetAmmoConsumptionRate();
            }
            
            // Unlimited ammo always succeeds
            if (UnlimitedAmmo)
            {
                return true;
            }
            
            // Already out of ammo
            if (_isOutOfAmmo)
            {
                return false;
            }
            
            // Check if we have enough ammo
            if (_currentAmmo < amount)
            {
                // Not enough ammo - mark as depleted
                _currentAmmo = 0;
                OnAmmoDepletion();
                return false;
            }
            
            // Consume ammo
            _currentAmmo -= amount;
            
            // Check for depletion
            if (_currentAmmo <= 0)
            {
                _currentAmmo = 0;
                OnAmmoDepletion();
                return false;
            }
            
            // Check for low ammo warning
            if (_currentAmmo == LowAmmoThreshold)
            {
                OnAmmoLow();
            }
            
            return true;
        }
        
        /// <summary>
        /// Add ammo (for pickups or abilities)
        /// </summary>
        public virtual void AddAmmo(int amount)
        {
            if (UnlimitedAmmo) return;
            
            _currentAmmo += amount;
            
            // Clamp to max
            if (_currentAmmo > StartingAmmo)
            {
                _currentAmmo = StartingAmmo;
            }
            
            // If was depleted, restore functionality
            if (_isOutOfAmmo && _currentAmmo > 0)
            {
                _isOutOfAmmo = false;
                Debug.Log($"[Hero] {HeroName} ammo restored! Now has {_currentAmmo} ammo.");
                
                // TODO: Trigger ammo restored feedback
            }
            
            Debug.Log($"[Hero] {HeroName} gained {amount} ammo. Total: {_currentAmmo}");
        }
        
        /// <summary>
        /// Called when ammo is depleted (permanent loss condition)
        /// </summary>
        protected virtual void OnAmmoDepletion()
        {
            _isOutOfAmmo = true;
            
            // Stop firing immediately
            StopFiring();
            
            Debug.LogWarning($"[Hero] {HeroName} is OUT OF AMMO! Hero is now useless.");
            
            // Play out-of-ammo feedback/animation
            // TODO: MMFeedbacks for ammo depletion
            
            // Remove from grid after delay
            StartCoroutine(RemoveFromGridAfterDelay("ammo depletion"));
        }
        
        /// <summary>
        /// Called when ammo reaches low threshold
        /// </summary>
        protected virtual void OnAmmoLow()
        {
            Debug.LogWarning($"[Hero] {HeroName} ammo is LOW! ({_currentAmmo} remaining)");
            
            // TODO: Play low ammo warning feedback
            // TODO: UI warning indicator
        }
        
        #endregion
        
        #region Auto-Targeting & Combat
        
        private Transform _currentTarget;
        private float _lastTargetSearchTime;
        private float _lastFireTime;
        
        /// <summary>
        /// Update loop for combat when in Firing zone
        /// </summary>
        void Update()
        {
            if (_isFiring && IsInFiringZone && IsFunctional)
            {
                UpdateCombat();
            }
            else if (_isFiring && IsOutOfAmmo)
            {
                // Stop firing if we ran out of ammo mid-combat
                StopFiring();
            }
            
            // Update display fields for Inspector visibility
            _displayCurrentAmmo = _currentAmmo;
            _displayIsFiring = _isFiring;
        }
        
        /// <summary>
        /// Main combat update - find targets and shoot
        /// </summary>
        protected virtual void UpdateCombat()
        {
            // Search for target periodically
            if (Time.time - _lastTargetSearchTime >= TargetSearchInterval)
            {
                FindTarget();
                _lastTargetSearchTime = Time.time;
            }
            
            // If we have a target, aim and fire
            if (_currentTarget != null)
            {
                AimAtTarget(_currentTarget);
                TryFireWeapon();
            }
        }
        
        /// <summary>
        /// Find the nearest enemy to target
        /// </summary>
        protected virtual void FindTarget()
        {
            // Clear target if it's dead/destroyed
            if (_currentTarget != null && (_currentTarget.gameObject == null || !_currentTarget.gameObject.activeInHierarchy))
            {
                Debug.Log($"[Hero] {HeroName} clearing dead/inactive target.");
                _currentTarget = null;
            }
            
            // Find all potential targets in range
            Collider[] hits = Physics.OverlapSphere(transform.position, DetectionRange, TargetLayerMask);
            
            if (hits.Length == 0)
            {
                if (_currentTarget != null)
                {
                    Debug.Log($"[Hero] {HeroName} lost target - no enemies in range {DetectionRange}m. Position: {transform.position}");
                }
                _currentTarget = null;
                return;
            }
            
            // Debug: Show what we found
            if (_currentTarget == null)
            {
                Debug.Log($"[Hero] {HeroName} found {hits.Length} potential targets in range. Searching for closest...");
            }
            
            // Find closest target
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider hit in hits)
            {
                // Skip if same as current target (keep current target if still in range)
                if (_currentTarget != null && hit.transform == _currentTarget)
                {
                    closestTarget = _currentTarget;
                    break;
                }
                
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.transform;
                }
            }
            
            // Log when we acquire a new target
            if (closestTarget != null && closestTarget != _currentTarget)
            {
                Debug.Log($"[Hero] {HeroName} acquired target: {closestTarget.name} at distance {closestDistance:F2}m");
            }
            
            _currentTarget = closestTarget;
        }
        
        /// <summary>
        /// Aim weapon at target
        /// </summary>
        protected virtual void AimAtTarget(Transform target)
        {
            if (_currentWeapon == null) return;
            
            // Calculate direction to target
            Vector3 directionToTarget = (target.position - _currentWeapon.transform.position).normalized;
            
            // Rotate weapon to face target
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _currentWeapon.transform.rotation = Quaternion.Slerp(
                    _currentWeapon.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f // Rotation speed
                );
            }
        }
        
        /// <summary>
        /// Fire weapon if cooldown allows
        /// </summary>
        protected virtual void TryFireWeapon()
        {
            if (_currentWeapon == null) return;
            
            // Check if we have ammo
            if (!ConsumeAmmo())
            {
                // Out of ammo - stop firing
                Debug.LogWarning($"[Hero] {HeroName} cannot fire - out of ammo!");
                StopFiring();
                return;
            }
            
            // Check fire rate cooldown
            float timeSinceLastFire = Time.time - _lastFireTime;
            float fireInterval = 1f / AutoFireRate;
            
            if (timeSinceLastFire >= fireInterval)
            {
                // Request weapon to shoot
                if (HandleWeapon != null)
                {
                    HandleWeapon.ShootStart();
                    _lastFireTime = Time.time;
                    
                    // Stop shooting immediately (semi-auto fire)
                    // For continuous fire, remove this or call ShootStop() after delay
                    StartCoroutine(StopShootingAfterFrame());
                }
            }
        }
        
        /// <summary>
        /// Stop shooting after one frame (for semi-auto fire)
        /// </summary>
        protected virtual System.Collections.IEnumerator StopShootingAfterFrame()
        {
            yield return null;
            if (HandleWeapon != null)
            {
                HandleWeapon.ShootStop();
            }
        }
        
        /// <summary>
        /// Get current target (for debugging/UI)
        /// </summary>
        public Transform CurrentTarget => _currentTarget;
        
        #endregion
        
        #region Debug Visualization
        
        void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, DetectionRange);
            
            // Draw line to current target
            if (_currentTarget != null && _isFiring)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _currentTarget.position);
            }
        }
        
        #endregion
        
        /// <summary>
        /// Called when hero is clicked (requires Collider component)
        /// </summary>
        void OnMouseDown()
        {
            // Block input during animations
            if (HeroQueueManager.Instance != null && HeroQueueManager.Instance.IsAnimating)
            {
                return;
            }
            
            // Only heroes in Active zone can be deployed
            if (IsInActiveZone)
            {
                HeroQueueManager.Instance?.OnHeroClicked(this);
            }
        }
        
        /// <summary>
        /// Highlight this hero (visual feedback)
        /// </summary>
        public void Highlight()
        {
            if (_renderer != null && HighlightMaterial != null)
            {
                _renderer.material = HighlightMaterial;
            }
        }
        
        /// <summary>
        /// Remove highlight from this hero
        /// </summary>
        public void Unhighlight()
        {
            if (_renderer != null && _originalMaterial != null)
            {
                _renderer.material = _originalMaterial;
            }
        }
        
        #region Death & Lifecycle
        
        /// <summary>
        /// Called when hero's health reaches 0
        /// </summary>
        protected virtual void OnHeroDeath()
        {
            _isDead = true;
            
            // Stop firing immediately
            StopFiring();
            
            Debug.Log($"[Hero] {HeroName} has died!");
            
            // Play death feedback/animation
            // TODO: MMFeedbacks for death effects
            
            // Remove from grid after delay
            StartCoroutine(RemoveFromGridAfterDelay("death"));
        }
        
        /// <summary>
        /// Called if hero is revived (unlikely in our game)
        /// </summary>
        protected virtual void OnHeroRevive()
        {
            _isDead = false;
            Debug.Log($"[Hero] {HeroName} has been revived!");
        }
        
        /// <summary>
        /// Remove hero from grid slot after delay, freeing it for new deployment
        /// </summary>
        protected virtual System.Collections.IEnumerator RemoveFromGridAfterDelay(string reason)
        {
            if (_isBeingRemoved)
            {
                yield break; // Already being removed
            }
            
            _isBeingRemoved = true;
            
            Debug.Log($"[Hero] {HeroName} will be removed from grid in {RemovalDelay}s (reason: {reason}).");
            
            // Wait for delay (allows death animation/feedback to play)
            yield return new WaitForSeconds(RemovalDelay);
            
            // Free the grid slot
            if (CurrentGridSlot != null)
            {
                // Clear hero from slot using GridManager
                if (ProjectBlast.Grid.GridManager.Instance != null)
                {
                    ProjectBlast.Grid.GridManager.Instance.RemoveHero(this);
                    Debug.Log($"[Hero] {HeroName} removed from slot at {CurrentGridSlot.Zone} ({CurrentGridSlot.Row}, {CurrentGridSlot.Column}).");
                }
                
                CurrentGridSlot = null;
            }
            
            // Notify HeroQueueManager
            if (HeroQueueManager.Instance != null)
            {
                HeroQueueManager.Instance.OnHeroRemoved(this, reason);
            }
            
            // Destroy or disable hero GameObject
            if (DestroyOnRemoval)
            {
                Debug.Log($"[Hero] {HeroName} destroyed.");
                Destroy(gameObject);
            }
            else
            {
                // Move off-screen and disable
                transform.position = new Vector3(1000, -1000, 1000); // Far away
                gameObject.SetActive(false);
                Debug.Log($"[Hero] {HeroName} disabled.");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Hero class types for categorization
    /// </summary>
    public enum HeroClass
    {
        Ranged,     // Long-range DPS
        Tank,       // High HP, low damage
        Support,    // Healing/buffs
        AOE,        // Area damage
        Melee,      // Close-range (unused for now)
        Special     // Unique mechanics
    }
}
