using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Result codes for allocation requests
    /// </summary>
    public enum AllocationResult
    {
        Success,                    // Allocation granted
        EnemyFullyAllocated,       // Enemy already has enough damage allocated
        EnemyDead,                 // Enemy is dead or invalid
        EnemyAlreadyClaimed,       // Enemy already has a hero assigned (strict 1-to-1 phase)
        InvalidParameters          // Hero, enemy, or damage parameters invalid
    }
    
    /// <summary>
    /// Centralized singleton that coordinates bullet allocation across all heroes and enemies.
    /// Uses COOPERATIVE ALLOCATION system where multiple heroes can share kills.
    /// 
    /// EVENT-DRIVEN ARCHITECTURE (TDE Pattern):
    /// - Implements MMEventListener<CombatAllocationEvent> for decoupled communication
    /// - AI/Heroes trigger Request events instead of direct method calls
    /// - CombatCoordinator responds with Grant/Deny events
    /// - Follows TDE's event-driven pattern for loosely coupled systems
    /// 
    /// NEW SYSTEM (Cooperative Allocation):
    /// - Heroes calculate bullets needed and REQUEST ALLOCATION upfront (via events)
    /// - Multiple heroes can allocate bullets to same enemy (cooperative kills)
    /// - Heroes are LOCKED to their allocation (committed until bullets fired or enemy dies)
    /// - After allocation complete, heroes can switch to boss or find new target
    /// 
    /// EVENT USAGE:
    /// 1. Hero detects target → triggers CombatAllocationEvent.TriggerRequest()
    /// 2. CombatCoordinator processes → triggers Grant/Deny response event
    /// 3. Hero listens for Grant → proceeds with firing sequence
    /// 4. Hero fires bullet → triggers BulletFired event
    /// 5. Bullet hits → triggers BulletHit event
    /// 6. Hero switches target → triggers Release event
    /// 
    /// LEGACY METHODS (Backward Compatibility):
    /// - Direct method calls still supported during transition
    /// - Internally convert to event triggers for consistent behavior
    /// </summary>
    public class CombatCoordinator : MMSingleton<CombatCoordinator>, MMEventListener<CombatAllocationEvent>
    {
        /// <summary>
        /// Tracks a hero's bullet allocation for a specific enemy
        /// </summary>
        public class BulletAllocation
        {
            public Character Hero;           // TDE Character component reference
            public int BulletsAllocated;     // Total bullets reserved
            public int BulletsFired;         // Bullets actually fired so far
            public int BulletsHit;           // Bullets that successfully hit
            public float DamagePerBullet;
            
            public float TotalAllocatedDamage => BulletsAllocated * DamagePerBullet;
            public float FiredDamage => BulletsFired * DamagePerBullet;
            public float HitDamage => BulletsHit * DamagePerBullet;
            public int BulletsRemaining => BulletsAllocated - BulletsFired;
            public bool IsComplete => BulletsFired >= BulletsAllocated;
        }
        
        /// <summary>
        /// Tracks all allocations for a single enemy
        /// </summary>
        private class EnemyTargetData
        {
            public Health Health;
            public Dictionary<Character, BulletAllocation> Allocations = new Dictionary<Character, BulletAllocation>();
            
            public float GetTotalAllocatedDamage()
            {
                float total = 0f;
                foreach (var alloc in Allocations.Values)
                {
                    total += alloc.TotalAllocatedDamage;
                }
                return total;
            }
            
            public float GetEffectiveHP()
            {
                if (Health == null) return 0f;
                return Health.CurrentHealth - GetTotalAllocatedDamage();
            }
            
            public int GetTotalHeroesAllocated() => Allocations.Count;
        }
        
        [Header("Configuration")]
        [Tooltip("Minimum effective HP required to approve allocation (allows slight over-allocation)")]
        public float MinEffectiveHPThreshold = 1.0f;
        
        [Tooltip("Enable debug logging for allocation system")]
        public bool EnableDebugLogs = false;
        
        [Header("Target Management (Inspector-Assigned)")]
        [Tooltip("All hero GameObjects in the scene. Drag hero prefabs/instances here.")]
        public GameObject[] Heroes = new GameObject[0];
        
        [Tooltip("All enemy GameObjects in the scene. Drag enemy prefabs/instances here OR leave empty to auto-register via spawn events.")]
        public GameObject[] Enemies = new GameObject[0];
        
        [Tooltip("If true, enemies register automatically on spawn via MMLifeCycleEvent. If false, use Enemies array only.")]
        public bool AutoRegisterEnemiesOnSpawn = true;
        
        [Header("Debug Info")]
        [SerializeField] private int _trackedEnemyCount;
        [SerializeField] private int _totalHeroesEngaged;
        [SerializeField] private float _totalAllocatedDamage;
        [SerializeField] private int _totalBulletsAllocated;
        [SerializeField] private int _totalBulletsFired;
        
	// Core data structure: Enemy Health Component -> Target Data
	// Using component reference instead of GameObject prevents memory leaks
	private Dictionary<Health, EnemyTargetData> _targets = new Dictionary<Health, EnemyTargetData>();
	private Dictionary<Health, GameObject> _enemyObjects = new Dictionary<Health, GameObject>(); // Reverse lookup
	
	// Hero ammo tracking: Hero Character Component -> Current Ammo Count
	// Using component reference provides type safety and follows TDE patterns
	private Dictionary<Character, int> _heroAmmo = new Dictionary<Character, int>();
	private Dictionary<Character, GameObject> _heroObjects = new Dictionary<Character, GameObject>(); // Reverse lookup
	
	// Hero max ammo tracking: Hero Character Component -> Max Ammo (for event triggering)
	private Dictionary<Character, int> _heroMaxAmmo = new Dictionary<Character, int>();
	
	// Cached phase state (performance optimization - eliminates FindObjectsByType calls)
	private HashSet<Health> _allKnownEnemies = new HashSet<Health>(); // All enemies we've seen
	private int _unclaimedEnemyCount = 0; // Cached count, updated on allocation/spawn/death        [Header("Hero Ammo Tracking")]
        [SerializeField] private int _totalHeroesRegistered;
        [SerializeField] private int _totalAmmoRemaining;
        
        protected override void Awake()
        {
            base.Awake();
            this.MMEventStartListening<CombatAllocationEvent>();
            InitializeTargets();
        }
        
        protected virtual void OnDestroy()
        {
            this.MMEventStopListening<CombatAllocationEvent>();
            ClearAll();
        }
        
        /// <summary>
        /// Initialize targets from inspector-assigned arrays (TDE pattern - no FindObjectsByType)
        /// </summary>
        protected virtual void InitializeTargets()
        {
            // Register heroes from inspector array
            if (Heroes != null)
            {
                foreach (var hero in Heroes)
                {
                    if (hero != null)
                    {
                        var character = hero.GetComponent<Character>();
                        if (character != null && character.CharacterType == Character.CharacterTypes.Player)
                        {
                            _heroObjects[character] = hero;
                        }
                    }
                }
            }
            
            // Register enemies from inspector array
            if (Enemies != null)
            {
                foreach (var enemy in Enemies)
                {
                    if (enemy != null)
                    {
                        var health = enemy.GetComponent<Health>();
                        if (health != null)
                        {
                            RegisterEnemy(health, enemy);
                        }
                    }
                }
            }
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] Initialized: {_heroObjects.Count} heroes, {_allKnownEnemies.Count} enemies (AutoRegister: {AutoRegisterEnemiesOnSpawn})");
            }
        }
        
        /// <summary>
        /// Registers an enemy in the known enemies set and updates unclaimed count
        /// </summary>
        protected virtual void RegisterEnemy(Health health, GameObject enemy)
        {
            if (health == null || _allKnownEnemies.Contains(health))
                return;
            
            _allKnownEnemies.Add(health);
            _enemyObjects[health] = enemy;
            
            // New enemy starts as unclaimed
            _unclaimedEnemyCount++;
        }
        
        void Update()
        {
            UpdateDebugInfo();
            CleanupDeadEnemies();
        }
        
        /// <summary>
        /// TDE event handler - processes all combat allocation events
        /// </summary>
        public virtual void OnMMEvent(CombatAllocationEvent allocationEvent)
        {
            switch (allocationEvent.Type)
            {
                case CombatAllocationEvent.EventType.Request:
                    HandleAllocationRequest(allocationEvent);
                    break;
                    
                case CombatAllocationEvent.EventType.Release:
                    HandleAllocationRelease(allocationEvent);
                    break;
                    
                case CombatAllocationEvent.EventType.BulletFired:
                    HandleBulletFired(allocationEvent);
                    break;
                    
                case CombatAllocationEvent.EventType.BulletHit:
                    HandleBulletHit(allocationEvent);
                    break;
                    
                case CombatAllocationEvent.EventType.EnemyDied:
                    HandleEnemyDied(allocationEvent);
                    break;
            }
        }
        
        #region Event Handlers (TDE Pattern)
        
        /// <summary>
        /// Handles allocation request events from AI/Heroes
        /// </summary>
        private void HandleAllocationRequest(CombatAllocationEvent evt)
        {
            var result = RequestBulletAllocation(evt.HeroObject, evt.EnemyObject, evt.DamagePerBullet, evt.BulletsRequested);
            
            if (result == AllocationResult.Success)
            {
                // Get allocation details
                var allocation = GetHeroAllocation(evt.HeroObject, evt.EnemyObject);
                int bulletsGranted = allocation?.BulletsAllocated ?? 0;
                
                // Trigger grant event
                CombatAllocationEvent.TriggerGrant(evt.Hero, evt.EnemyHealth, evt.HeroObject, evt.EnemyObject, bulletsGranted, result);
                
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] GRANTED allocation: {evt.HeroObject.name} → {evt.EnemyObject.name} ({bulletsGranted} bullets)");
                }
            }
            else
            {
                // Trigger deny event
                CombatAllocationEvent.TriggerDeny(evt.Hero, evt.EnemyHealth, evt.HeroObject, evt.EnemyObject, result);
                
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] DENIED allocation: {evt.HeroObject.name} → {evt.EnemyObject.name} (Reason: {result})");
                }
            }
        }
        
        /// <summary>
        /// Handles allocation release events (hero switches target or dies)
        /// </summary>
        private void HandleAllocationRelease(CombatAllocationEvent evt)
        {
            ReleaseHeroAllocation(evt.HeroObject, evt.EnemyObject);
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] RELEASED allocation: {evt.HeroObject.name} from {evt.EnemyObject.name}");
            }
        }
        
        /// <summary>
        /// Handles bullet fired events
        /// </summary>
        private void HandleBulletFired(CombatAllocationEvent evt)
        {
            OnHeroFiredBullet(evt.HeroObject, evt.EnemyObject);
        }
        
        /// <summary>
        /// Handles bullet hit events
        /// </summary>
        private void HandleBulletHit(CombatAllocationEvent evt)
        {
            OnBulletHit(evt.EnemyObject, evt.DamagePerBullet);
        }
        
        /// <summary>
        /// Handles enemy died events
        /// </summary>
        private void HandleEnemyDied(CombatAllocationEvent evt)
        {
            if (_targets.ContainsKey(evt.EnemyHealth))
            {
                var targetData = _targets[evt.EnemyHealth];
                OnEnemyDied(evt.EnemyHealth, targetData);
            }
        }
        
        #endregion
        
        /// <summary>
        /// Checks if there are any enemies with no allocations (unclaimed).
        /// Used to determine if cooperative finish is allowed.
        /// PHASE 1: If unclaimed enemies exist, enforce strict 1-to-1
        /// PHASE 2: If no unclaimed enemies, allow cooperative finish
        /// 
        /// OPTIMIZED: Uses cached count instead of FindObjectsByType (100x faster)
        /// </summary>
        public bool HasUnclaimedEnemies()
        {
            // OPTIMIZED: Use cached count (no FindObjectsByType calls)
            return _unclaimedEnemyCount > 0;
        }
        
        #region Hero Ammo Management
        
        /// <summary>
        /// Registers a hero with the combat coordinator for ammo tracking.
        /// Only call this for heroes with LIMITED ammo (skip unlimited ammo heroes).
        /// </summary>
        /// <param name="hero">Hero GameObject to register</param>
        /// <param name="startingAmmo">Starting ammo count</param>
        public void RegisterHero(GameObject hero, int startingAmmo)
        {
            if (hero == null || startingAmmo <= 0)
                return;
            
            var character = hero.GetComponent<Character>();
            if (character == null)
            {
                Debug.LogError($"[CombatCoordinator] RegisterHero failed - {hero.name} has no Character component!");
                return;
            }
            
            if (_heroAmmo.ContainsKey(character))
            {
                Debug.LogWarning($"[CombatCoordinator] Hero {hero.name} already registered! Updating ammo to {startingAmmo}");
                _heroAmmo[character] = startingAmmo;
                _heroMaxAmmo[character] = startingAmmo;
            }
            else
            {
                _heroAmmo[character] = startingAmmo;
                _heroMaxAmmo[character] = startingAmmo;
                _heroObjects[character] = hero; // Store reverse lookup
                
                // Trigger initial ammo event (TDE event-driven pattern)
                MMAmmoEvent.Trigger(hero, startingAmmo, startingAmmo, false);
                
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] Registered hero {hero.name} with {startingAmmo} ammo");
                }
            }
        }
        
        /// <summary>
        /// Unregisters a hero from ammo tracking (call on hero death/removal)
        /// </summary>
        /// <param name="hero">Hero GameObject to unregister</param>
        public void UnregisterHero(GameObject hero)
        {
            if (hero == null)
                return;
            
            var character = hero.GetComponent<Character>();
            if (character == null)
                return;
            
            if (_heroAmmo.ContainsKey(character))
            {
                _heroAmmo.Remove(character);
                _heroMaxAmmo.Remove(character);
                _heroObjects.Remove(character);
                
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] Unregistered hero {hero.name}");
                }
            }
            
            // Also release any allocations this hero has
            foreach (var targetData in _targets.Values)
            {
                if (targetData.Allocations.ContainsKey(character))
                {
                    targetData.Allocations.Remove(character);
                }
            }
        }
        
        /// <summary>
        /// Gets the current ammo count for a hero
        /// </summary>
        /// <param name="hero">Hero GameObject</param>
        /// <returns>Current ammo, or -1 if not registered (unlimited ammo)</returns>
        public int GetHeroAmmo(GameObject hero)
        {
            if (hero == null)
                return -1;
            
            var character = hero.GetComponent<Character>();
            if (character == null)
                return -1;
            
            return _heroAmmo.ContainsKey(character) ? _heroAmmo[character] : -1;
        }
        
        /// <summary>
        /// Checks if hero has ammo remaining
        /// </summary>
        /// <param name="hero">Hero GameObject</param>
        /// <returns>True if hero has ammo or is unlimited, false if depleted</returns>
        public bool HasAmmo(GameObject hero)
        {
            if (hero == null)
                return false;
            
            var character = hero.GetComponent<Character>();
            if (character == null)
                return false;
            
            // Not registered = unlimited ammo
            if (!_heroAmmo.ContainsKey(character))
                return true;
            
            return _heroAmmo[character] > 0;
        }
        
        #endregion
        
        /// <summary>
        /// Checks if an enemy is available for a hero to claim.
        /// Used by AI target selection to skip already-claimed enemies.
        /// PHASE 1: Only unclaimed enemies are available
        /// PHASE 2: All enemies are available (cooperative finish)
        /// </summary>
        /// <param name="enemy">Enemy GameObject to check</param>
        /// <returns>True if enemy can be claimed, false if already claimed by another hero</returns>
        public bool IsEnemyAvailableForClaim(GameObject enemy)
        {
            if (enemy == null)
                return false;
            
            var health = enemy.GetComponent<Health>();
            if (health == null)
                return false;
            
            // PHASE 2: If no unclaimed enemies exist, all enemies are available for cooperative finish
            if (!HasUnclaimedEnemies())
                return true;
            
            // PHASE 1: Only unclaimed enemies are available
            // If enemy not tracked yet, it's unclaimed
            if (!_targets.ContainsKey(health))
                return true;
            
            var targetData = _targets[health];
            
            // Enemy is dead, not available
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
                return false;
            
            // Check if enemy has no allocations (unclaimed)
            return targetData.Allocations.Count == 0;
        }
        
        #region Public API - New Cooperative Allocation System
        
        /// <summary>
        /// Request allocation of N bullets for a hero targeting an enemy.
        /// This is the PRIMARY method heroes should call when acquiring a target.
        /// 
        /// COOPERATIVE: Multiple heroes can allocate to same enemy for faster kills.
        /// </summary>
        /// <param name="hero">Hero requesting allocation</param>
        /// <param name="enemy">Target enemy</param>
        /// <param name="damagePerBullet">Damage dealt per bullet</param>
        /// <param name="bulletsRequested">How many bullets hero wants to allocate</param>
        /// <returns>AllocationResult indicating success or reason for failure</returns>
        public AllocationResult RequestBulletAllocation(
            GameObject hero,
            GameObject enemy,
            float damagePerBullet,
            int bulletsRequested)
        {
            // Validate parameters
            if (hero == null || enemy == null || damagePerBullet <= 0 || bulletsRequested <= 0)
            {
                return AllocationResult.InvalidParameters;
            }
            
            // Get component references (TDE pattern)
            var character = hero.GetComponent<Character>();
            var health = enemy.GetComponent<Health>();
            
            if (character == null || health == null)
            {
                return AllocationResult.InvalidParameters;
            }
            
            // Get or create enemy target data
            if (!_targets.ContainsKey(health))
            {
                if (health.CurrentHealth <= 0)
                {
                    return AllocationResult.EnemyDead;
                }
                
                _targets[health] = new EnemyTargetData
                {
                    Health = health
                };
                _enemyObjects[health] = enemy; // Store reverse lookup
                
                // Register enemy if not already known (auto-registration)
                if (!_allKnownEnemies.Contains(health))
                {
                    RegisterEnemy(health, enemy);
                }
            }
            
            var targetData = _targets[health];
            
            // Check if enemy is still alive
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                return AllocationResult.EnemyDead;
            }
            
            // PHASE 1: STRICT 1-TO-1 ASSIGNMENT
            // If enemy already has a hero allocated AND unclaimed enemies exist,
            // deny allocation (hero must find different enemy)
            if (!targetData.Allocations.ContainsKey(character) && targetData.Allocations.Count > 0)
            {
                // Check if there are unclaimed enemies available
                if (HasUnclaimedEnemies())
                {
                    // PHASE 1: Unclaimed enemies exist, enforce strict 1-to-1
                    if (EnableDebugLogs)
                    {
                        Debug.Log($"[CombatCoordinator] {hero.name} allocation DENIED - {enemy.name} already claimed by another hero. " +
                                 $"Unclaimed enemies available. (Phase 1: Strict 1-to-1)");
                    }
                    return AllocationResult.EnemyAlreadyClaimed;
                }
                // PHASE 2: No unclaimed enemies, allow cooperative finish
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] {hero.name} joining cooperative kill on {enemy.name}. " +
                             $"No unclaimed enemies. (Phase 2: Cooperative Finish)");
                }
            }
            
            // Calculate effective HP (current HP - already allocated damage)
            float effectiveHP = targetData.GetEffectiveHP();
            
            // Check if enemy already fully allocated (over-allocation allowed via threshold)
            if (effectiveHP <= -MinEffectiveHPThreshold)
            {
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] {hero.name} allocation DENIED - {enemy.name} fully allocated " +
                             $"(effectiveHP: {effectiveHP:F1}, currentHP: {targetData.Health.CurrentHealth:F1}, " +
                             $"allocated: {targetData.GetTotalAllocatedDamage():F1})");
                }
                return AllocationResult.EnemyFullyAllocated;
            }
            
            // COOPERATIVE: Grant allocation (even if partially overlapping with other heroes)
            // Calculate actual bullets needed vs requested
            float damageNeeded = Mathf.Max(0, effectiveHP + MinEffectiveHPThreshold);
            int bulletsNeeded = Mathf.CeilToInt(damageNeeded / damagePerBullet);
            int bulletsToAllocate = Mathf.Min(bulletsRequested, bulletsNeeded);
            
            // If hero barely contributes, allocate at least 1 bullet
            if (bulletsToAllocate == 0 && effectiveHP > 0)
            {
                bulletsToAllocate = 1;
            }
            
            // Create or update allocation for this hero
            bool wasUnclaimed = targetData.Allocations.Count == 0;
            
            if (!targetData.Allocations.ContainsKey(character))
            {
                targetData.Allocations[character] = new BulletAllocation
                {
                    Hero = character,
                    BulletsAllocated = bulletsToAllocate,
                    BulletsFired = 0,
                    BulletsHit = 0,
                    DamagePerBullet = damagePerBullet
                };
                
                // Update unclaimed count: enemy was unclaimed, now has allocation
                if (wasUnclaimed)
                {
                    _unclaimedEnemyCount--;
                }
            }
            else
            {
                // Hero re-allocating (e.g., after target switch) - update allocation
                var allocation = targetData.Allocations[character];
                allocation.BulletsAllocated = bulletsToAllocate;
                allocation.DamagePerBullet = damagePerBullet;
            }
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] {hero.name} allocated {bulletsToAllocate} bullets ({bulletsToAllocate * damagePerBullet:F1} dmg) to {enemy.name} " +
                         $"(effectiveHP: {effectiveHP:F1} → {targetData.GetEffectiveHP():F1}, " +
                         $"heroes: {targetData.GetTotalHeroesAllocated()})");
            }
            
            return AllocationResult.Success;
        }
        
        /// <summary>
        /// Check if hero can fire their next bullet (has allocation remaining).
        /// Call this BEFORE each shot.
        /// </summary>
        public bool CanHeroFireNextBullet(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null)
                return false;
            
            var character = hero.GetComponent<Character>();
            var health = enemy.GetComponent<Health>();
            
            if (character == null || health == null || !_targets.ContainsKey(health))
            {
                return false;
            }
            
            var targetData = _targets[health];
            
            if (!targetData.Allocations.ContainsKey(character))
            {
                return false; // No allocation
            }
            
            var allocation = targetData.Allocations[character];
            
            // Check if hero has bullets remaining in allocation
            return !allocation.IsComplete;
        }
        
        /// <summary>
        /// Notify that hero fired a bullet. Call this AFTER firing.
        /// Increments the fired bullet counter and consumes ammo for limited-ammo heroes.
        /// </summary>
        public void OnHeroFiredBullet(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null)
                return;
            
            var character = hero.GetComponent<Character>();
            var health = enemy.GetComponent<Health>();
            
            if (character == null || health == null || !_targets.ContainsKey(health))
            {
                return;
            }
            
            var targetData = _targets[health];
            
            if (!targetData.Allocations.ContainsKey(character))
            {
                return;
            }
            
            var allocation = targetData.Allocations[character];
            allocation.BulletsFired++;
            
            // AMMO CONSUMPTION: Decrement hero's ammo (only for registered heroes with limited ammo)
            if (_heroAmmo.ContainsKey(character))
            {
                _heroAmmo[character]--;
                int remainingAmmo = _heroAmmo[character];
                
                // Trigger ammo change event (TDE event-driven pattern)
                int maxAmmo = _heroMaxAmmo.ContainsKey(character) ? _heroMaxAmmo[character] : remainingAmmo;
                MMAmmoEvent.Trigger(hero, remainingAmmo, maxAmmo, false);
                
                if (EnableDebugLogs)
                {
                    Debug.Log($"[CombatCoordinator] {hero.name} fired bullet {allocation.BulletsFired}/{allocation.BulletsAllocated} at {enemy.name}. Ammo: {remainingAmmo}");
                }
                
                // Check for ammo depletion (TDE FindAbility pattern - zero reflection)
                if (remainingAmmo <= 0)
                {
                    var heroCharacter = hero.GetComponent<Character>();
                    if (heroCharacter != null)
                    {
                        // Use TDE's FindAbility<T>() - type-safe, 100-1000x faster than reflection
                        // Use FindAbilityByString to avoid namespace issues between TDE and ProjectBlast
                        var heroAmmo = heroCharacter.FindAbilityByString("HeroAmmo");
                        if (heroAmmo != null)
                        {
                            // Call OnAmmoDepletion via reflection-free dynamic invocation
                            var method = heroAmmo.GetType().GetMethod("OnAmmoDepletion");
                            if (method != null)
                            {
                                method.Invoke(heroAmmo, null);
                                
                                if (EnableDebugLogs)
                                {
                                    Debug.Log($"[CombatCoordinator] {hero.name} OUT OF AMMO! Triggered OnAmmoDepletion()");
                                }
                            }
                        }
                    }
                }
                // Check for low ammo warning (TDE FindAbility pattern)
                else
                {
                    var heroCharacter = hero.GetComponent<Character>();
                    if (heroCharacter != null)
                    {
                        var heroAmmo = heroCharacter.FindAbilityByString("HeroAmmo");
                        if (heroAmmo != null)
                        {
                            var thresholdField = heroAmmo.GetType().GetField("LowAmmoThreshold");
                            if (thresholdField != null)
                            {
                                int threshold = (int)thresholdField.GetValue(heroAmmo);
                                if (remainingAmmo == threshold)
                                {
                                    var method = heroAmmo.GetType().GetMethod("OnAmmoLow");
                                    method?.Invoke(heroAmmo, null);
                                }
                            }
                        }
                    }
                }
            }
            else if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] {hero.name} fired bullet {allocation.BulletsFired}/{allocation.BulletsAllocated} at {enemy.name} (unlimited ammo)");
            }
        }
        
        /// <summary>
        /// Called when a bullet hits an enemy and deals damage.
        /// Call from Health.Damage() method.
        /// </summary>
        public void OnBulletHit(GameObject enemy, float damageDealt)
        {
            if (enemy == null)
                return;
            
            var health = enemy.GetComponent<Health>();
            if (health == null || !_targets.ContainsKey(health))
            {
                return;
            }
            
            var targetData = _targets[health];
            
            // Find which hero's bullet hit (FIFO - first hero with unfired bullets)
            foreach (var allocation in targetData.Allocations.Values)
            {
                if (allocation.BulletsHit < allocation.BulletsFired)
                {
                    allocation.BulletsHit++;
                    
                    if (EnableDebugLogs)
                    {
                        GameObject heroObj = _heroObjects.ContainsKey(allocation.Hero) ? _heroObjects[allocation.Hero] : allocation.Hero.gameObject;
                        Debug.Log($"[CombatCoordinator] Bullet hit {enemy.name} for {damageDealt:F1} damage " +
                                 $"(hero: {heroObj.name}, hits: {allocation.BulletsHit}/{allocation.BulletsFired})");
                    }
                    
                    break; // Only count one hit per call
                }
            }
            
            // Check if enemy died
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                OnEnemyDied(health, targetData);
            }
        }
        
        /// <summary>
        /// Release hero's allocation when switching targets or exiting combat.
        /// </summary>
        public void ReleaseHeroAllocation(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null)
                return;
            
            var character = hero.GetComponent<Character>();
            var health = enemy.GetComponent<Health>();
            
            if (character == null || health == null || !_targets.ContainsKey(health))
            {
                return;
            }
            
            var targetData = _targets[health];
            bool wasAllocated = targetData.Allocations.ContainsKey(character);
            
            if (wasAllocated)
            {
                if (EnableDebugLogs)
                {
                    var allocation = targetData.Allocations[character];
                    Debug.Log($"[CombatCoordinator] {hero.name} released allocation on {enemy.name} " +
                             $"(fired {allocation.BulletsFired}/{allocation.BulletsAllocated})");
                }
                
                targetData.Allocations.Remove(character);
                
                // Update unclaimed count: if this was the last allocation, enemy becomes unclaimed
                if (targetData.Allocations.Count == 0 && targetData.Health != null && targetData.Health.CurrentHealth > 0)
                {
                    _unclaimedEnemyCount++;
                }
            }
            
            // Cleanup if no more heroes targeting this enemy
            if (targetData.Allocations.Count == 0)
            {
                _targets.Remove(health);
                _enemyObjects.Remove(health);
            }
        }
        
        /// <summary>
        /// Gets effective HP for an enemy (current HP - allocated damage).
        /// Useful for heroes calculating allocation needs.
        /// </summary>
        public float GetEnemyEffectiveHP(GameObject enemy)
        {
            if (enemy == null)
            {
                return 0f;
            }
            
            var health = enemy.GetComponent<Health>();
            if (health == null)
                return 0f;
            
            if (!_targets.ContainsKey(health))
            {
                return health.CurrentHealth;
            }
            
            return _targets[health].GetEffectiveHP();
        }
        
        /// <summary>
        /// Gets total allocated damage for an enemy.
        /// </summary>
        public float GetEnemyAllocatedDamage(GameObject enemy)
        {
            if (enemy == null)
                return 0f;
            
            var health = enemy.GetComponent<Health>();
            if (health == null || !_targets.ContainsKey(health))
            {
                return 0f;
            }
            
            return _targets[health].GetTotalAllocatedDamage();
        }
        
        /// <summary>
        /// Gets hero's allocation info for an enemy (for debugging/UI).
        /// </summary>
        public BulletAllocation GetHeroAllocation(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null)
                return null;
            
            var character = hero.GetComponent<Character>();
            var health = enemy.GetComponent<Health>();
            
            if (character == null || health == null || !_targets.ContainsKey(health))
            {
                return null;
            }
            
            var targetData = _targets[health];
            
            if (!targetData.Allocations.ContainsKey(character))
            {
                return null;
            }
            
            return targetData.Allocations[character];
        }
        
        #endregion
        
        #region Legacy API (Deprecated - kept for backward compatibility)
        
        /// <summary>
        /// DEPRECATED: Use RequestBulletAllocation() instead.
        /// Legacy single-shot approval system.
        /// </summary>
        [System.Obsolete("Use RequestBulletAllocation() for cooperative allocation system")]
        public bool RequestShot(GameObject hero, GameObject enemy, float damage)
        {
            // Fallback: Treat as single bullet allocation
            var result = RequestBulletAllocation(hero, enemy, damage, 1);
            
            if (result == AllocationResult.Success)
            {
                OnHeroFiredBullet(hero, enemy);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// DEPRECATED: Use ReleaseHeroAllocation() instead.
        /// </summary>
        [System.Obsolete("Use ReleaseHeroAllocation() instead")]
        public void ReleaseTarget(GameObject hero, GameObject enemy)
        {
            ReleaseHeroAllocation(hero, enemy);
        }
        
        /// <summary>
        /// DEPRECATED: Use GetEnemyAllocatedDamage() instead.
        /// </summary>
        [System.Obsolete("Use GetEnemyAllocatedDamage() instead")]
        public float GetEnemyInFlightDamage(GameObject enemy)
        {
            return GetEnemyAllocatedDamage(enemy);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Called when enemy dies. Notifies all allocated heroes to find new targets.
        /// </summary>
        private void OnEnemyDied(Health enemyHealth, EnemyTargetData targetData)
        {
            GameObject enemy = _enemyObjects.ContainsKey(enemyHealth) ? _enemyObjects[enemyHealth] : enemyHealth.gameObject;
            
            // Copy heroes to list to avoid collection modification during enumeration
            var heroesToNotify = new List<Character>(targetData.Allocations.Keys);
            
            // Update unclaimed count: if enemy had no allocations, it was unclaimed
            if (targetData.Allocations.Count == 0)
            {
                _unclaimedEnemyCount--;
            }
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] {enemy.name} died. Notifying {heroesToNotify.Count} heroes");
            }
            
            // Notify all heroes that had allocations on this enemy
            foreach (var character in heroesToNotify)
            {
                if (character != null)
                {
                    var aiAction = character.GetComponentInParent<AIActionShoot3D>();
                    if (aiAction != null)
                    {
                        aiAction.OnCurrentTargetDied();
                    }
                }
            }
            
            // Remove enemy from tracking
            _targets.Remove(enemyHealth);
            _enemyObjects.Remove(enemyHealth);
            _allKnownEnemies.Remove(enemyHealth);
        }
        
        /// <summary>
        /// Cleanup enemies that are no longer valid (destroyed, null health, etc.)
        /// </summary>
        private void CleanupDeadEnemies()
        {
            // Use ToList() to avoid collection modification during enumeration
            var deadEnemies = new List<Health>();
            
            foreach (var kvp in _targets.ToList())
            {
                // Check if Health component was destroyed
                if (kvp.Key == null)
                {
                    deadEnemies.Add(kvp.Key);
                    continue;
                }
                
                // Check if Health component is null or enemy is dead
                if (kvp.Value.Health == null || kvp.Value.Health.CurrentHealth <= 0)
                {
                    OnEnemyDied(kvp.Key, kvp.Value);
                    deadEnemies.Add(kvp.Key);
                }
            }
            
            // Remove dead enemies (already removed in OnEnemyDied, but just in case)
            foreach (var health in deadEnemies)
            {
                _targets.Remove(health);
                _enemyObjects.Remove(health);
            }
        }
        
        /// <summary>
        /// Updates inspector debug information
        /// </summary>
        private void UpdateDebugInfo()
        {
            _trackedEnemyCount = _targets.Count;
            _totalHeroesEngaged = 0;
            _totalAllocatedDamage = 0f;
            _totalBulletsAllocated = 0;
            _totalBulletsFired = 0;
            
            foreach (var targetData in _targets.Values)
            {
                _totalHeroesEngaged += targetData.Allocations.Count;
                
                foreach (var allocation in targetData.Allocations.Values)
                {
                    _totalAllocatedDamage += allocation.TotalAllocatedDamage;
                    _totalBulletsAllocated += allocation.BulletsAllocated;
                    _totalBulletsFired += allocation.BulletsFired;
                }
            }
            
            // Update hero ammo tracking debug info
            _totalHeroesRegistered = _heroAmmo.Count;
            _totalAmmoRemaining = 0;
            
            foreach (var ammo in _heroAmmo.Values)
            {
                _totalAmmoRemaining += ammo;
            }
        }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// Clears all tracking data (useful for scene transitions)
        /// </summary>
        public void ClearAll()
        {
            if (EnableDebugLogs)
            {
                Debug.Log("[CombatCoordinator] Clearing all allocations and hero ammo tracking");
            }
            
            _targets.Clear();
            _enemyObjects.Clear();
            _heroAmmo.Clear();
            _heroMaxAmmo.Clear();
            _heroObjects.Clear();
            _allKnownEnemies.Clear();
            _unclaimedEnemyCount = 0;
        }
        
        #endregion
    }
}
