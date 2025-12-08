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
    /// NEW SYSTEM (Cooperative Allocation):
    /// - Heroes calculate bullets needed and REQUEST ALLOCATION upfront
    /// - Multiple heroes can allocate bullets to same enemy (cooperative kills)
    /// - Heroes are LOCKED to their allocation (committed until bullets fired or enemy dies)
    /// - After allocation complete, heroes can switch to boss or find new target
    /// 
    /// USAGE:
    /// 1. Hero detects target → calls RequestBulletAllocation(enemy, damage, bulletCount)
    /// 2. Before each shot → calls CanHeroFireNextBullet(enemy)
    /// 3. After firing → calls OnHeroFiredBullet(enemy)
    /// 4. When bullet hits → Health calls OnBulletHit(enemy, damage)
    /// 5. When switching targets → calls ReleaseHeroAllocation(enemy)
    /// </summary>
    public class CombatCoordinator : MMSingleton<CombatCoordinator>
    {
        /// <summary>
        /// Tracks a hero's bullet allocation for a specific enemy
        /// </summary>
        public class BulletAllocation
        {
            public GameObject Hero;
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
            public Dictionary<GameObject, BulletAllocation> Allocations = new Dictionary<GameObject, BulletAllocation>();
            
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
        
        [Header("Debug Info")]
        [SerializeField] private int _trackedEnemyCount;
        [SerializeField] private int _totalHeroesEngaged;
        [SerializeField] private float _totalAllocatedDamage;
        [SerializeField] private int _totalBulletsAllocated;
        [SerializeField] private int _totalBulletsFired;
        
        // Core data structure: Enemy GameObject -> Target Data
        private Dictionary<GameObject, EnemyTargetData> _targets = new Dictionary<GameObject, EnemyTargetData>();
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        void Update()
        {
            UpdateDebugInfo();
            CleanupDeadEnemies();
        }
        
        /// <summary>
        /// Checks if there are any enemies with no allocations (unclaimed).
        /// Used to determine if cooperative finish is allowed.
        /// PHASE 1: If unclaimed enemies exist, enforce strict 1-to-1
        /// PHASE 2: If no unclaimed enemies, allow cooperative finish
        /// </summary>
        public bool HasUnclaimedEnemies()
        {
            // Check all tracked enemies
            foreach (var kvp in _targets)
            {
                var targetData = kvp.Value;
                
                // Skip dead enemies
                if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
                    continue;
                
                // If enemy has no allocations, it's unclaimed
                if (targetData.Allocations.Count == 0)
                    return true;
            }
            
            // Also check for enemies not yet tracked (newly spawned)
            var allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (var health in allHealthComponents)
            {
                // Skip dead enemies
                if (health == null || health.CurrentHealth <= 0)
                    continue;
                
                // Skip if already tracked
                if (_targets.ContainsKey(health.gameObject))
                    continue;
                
                // Skip player characters
                var character = health.GetComponent<Character>();
                if (character != null && character.CharacterType == Character.CharacterTypes.Player)
                    continue;
                
                // Found unclaimed enemy
                return true;
            }
            
            return false; // All enemies are claimed
        }
        
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
            
            // PHASE 2: If no unclaimed enemies exist, all enemies are available for cooperative finish
            if (!HasUnclaimedEnemies())
                return true;
            
            // PHASE 1: Only unclaimed enemies are available
            // If enemy not tracked yet, it's unclaimed
            if (!_targets.ContainsKey(enemy))
                return true;
            
            var targetData = _targets[enemy];
            
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
            
            // Get or create enemy target data
            if (!_targets.ContainsKey(enemy))
            {
                var health = enemy.GetComponent<Health>();
                if (health == null || health.CurrentHealth <= 0)
                {
                    return AllocationResult.EnemyDead;
                }
                
                _targets[enemy] = new EnemyTargetData
                {
                    Health = health
                };
            }
            
            var targetData = _targets[enemy];
            
            // Check if enemy is still alive
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                return AllocationResult.EnemyDead;
            }
            
            // PHASE 1: STRICT 1-TO-1 ASSIGNMENT
            // If enemy already has a hero allocated AND unclaimed enemies exist,
            // deny allocation (hero must find different enemy)
            if (!targetData.Allocations.ContainsKey(hero) && targetData.Allocations.Count > 0)
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
            if (!targetData.Allocations.ContainsKey(hero))
            {
                targetData.Allocations[hero] = new BulletAllocation
                {
                    Hero = hero,
                    BulletsAllocated = bulletsToAllocate,
                    BulletsFired = 0,
                    BulletsHit = 0,
                    DamagePerBullet = damagePerBullet
                };
            }
            else
            {
                // Hero re-allocating (e.g., after target switch) - update allocation
                var allocation = targetData.Allocations[hero];
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
            if (hero == null || enemy == null || !_targets.ContainsKey(enemy))
            {
                return false;
            }
            
            var targetData = _targets[enemy];
            
            if (!targetData.Allocations.ContainsKey(hero))
            {
                return false; // No allocation
            }
            
            var allocation = targetData.Allocations[hero];
            
            // Check if hero has bullets remaining in allocation
            return !allocation.IsComplete;
        }
        
        /// <summary>
        /// Notify that hero fired a bullet. Call this AFTER firing.
        /// Increments the fired bullet counter.
        /// </summary>
        public void OnHeroFiredBullet(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null || !_targets.ContainsKey(enemy))
            {
                return;
            }
            
            var targetData = _targets[enemy];
            
            if (!targetData.Allocations.ContainsKey(hero))
            {
                return;
            }
            
            var allocation = targetData.Allocations[hero];
            allocation.BulletsFired++;
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] {hero.name} fired bullet {allocation.BulletsFired}/{allocation.BulletsAllocated} at {enemy.name}");
            }
        }
        
        /// <summary>
        /// Called when a bullet hits an enemy and deals damage.
        /// Call from Health.Damage() method.
        /// </summary>
        public void OnBulletHit(GameObject enemy, float damageDealt)
        {
            if (enemy == null || !_targets.ContainsKey(enemy))
            {
                return;
            }
            
            var targetData = _targets[enemy];
            
            // Find which hero's bullet hit (FIFO - first hero with unfired bullets)
            foreach (var allocation in targetData.Allocations.Values)
            {
                if (allocation.BulletsHit < allocation.BulletsFired)
                {
                    allocation.BulletsHit++;
                    
                    if (EnableDebugLogs)
                    {
                        Debug.Log($"[CombatCoordinator] Bullet hit {enemy.name} for {damageDealt:F1} damage " +
                                 $"(hero: {allocation.Hero?.name}, hits: {allocation.BulletsHit}/{allocation.BulletsFired})");
                    }
                    
                    break; // Only count one hit per call
                }
            }
            
            // Check if enemy died
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                OnEnemyDied(enemy, targetData);
            }
        }
        
        /// <summary>
        /// Release hero's allocation when switching targets or exiting combat.
        /// </summary>
        public void ReleaseHeroAllocation(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null || !_targets.ContainsKey(enemy))
            {
                return;
            }
            
            var targetData = _targets[enemy];
            
            if (targetData.Allocations.ContainsKey(hero))
            {
                if (EnableDebugLogs)
                {
                    var allocation = targetData.Allocations[hero];
                    Debug.Log($"[CombatCoordinator] {hero.name} released allocation on {enemy.name} " +
                             $"(fired {allocation.BulletsFired}/{allocation.BulletsAllocated})");
                }
                
                targetData.Allocations.Remove(hero);
            }
            
            // Cleanup if no more heroes targeting this enemy
            if (targetData.Allocations.Count == 0)
            {
                _targets.Remove(enemy);
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
            
            if (!_targets.ContainsKey(enemy))
            {
                var health = enemy.GetComponent<Health>();
                return health != null ? health.CurrentHealth : 0f;
            }
            
            return _targets[enemy].GetEffectiveHP();
        }
        
        /// <summary>
        /// Gets total allocated damage for an enemy.
        /// </summary>
        public float GetEnemyAllocatedDamage(GameObject enemy)
        {
            if (enemy == null || !_targets.ContainsKey(enemy))
            {
                return 0f;
            }
            
            return _targets[enemy].GetTotalAllocatedDamage();
        }
        
        /// <summary>
        /// Gets hero's allocation info for an enemy (for debugging/UI).
        /// </summary>
        public BulletAllocation GetHeroAllocation(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null || !_targets.ContainsKey(enemy))
            {
                return null;
            }
            
            var targetData = _targets[enemy];
            
            if (!targetData.Allocations.ContainsKey(hero))
            {
                return null;
            }
            
            return targetData.Allocations[hero];
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
        private void OnEnemyDied(GameObject enemy, EnemyTargetData targetData)
        {
            // Copy heroes to list to avoid collection modification during enumeration
            var heroesToNotify = new List<GameObject>(targetData.Allocations.Keys);
            
            if (EnableDebugLogs)
            {
                Debug.Log($"[CombatCoordinator] {enemy.name} died. Notifying {heroesToNotify.Count} heroes");
            }
            
            // Notify all heroes that had allocations on this enemy
            foreach (var hero in heroesToNotify)
            {
                if (hero != null)
                {
                    var aiAction = hero.GetComponentInParent<AIActionShoot3D>();
                    if (aiAction != null)
                    {
                        aiAction.OnCurrentTargetDied();
                    }
                }
            }
            
            // Remove enemy from tracking
            _targets.Remove(enemy);
        }
        
        /// <summary>
        /// Cleanup enemies that are no longer valid (destroyed, null health, etc.)
        /// </summary>
        private void CleanupDeadEnemies()
        {
            // Use ToList() to avoid collection modification during enumeration
            var deadEnemies = new List<GameObject>();
            
            foreach (var kvp in _targets.ToList())
            {
                // Check if enemy GameObject was destroyed
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
            foreach (var enemy in deadEnemies)
            {
                _targets.Remove(enemy);
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
                Debug.Log("[CombatCoordinator] Clearing all allocations");
            }
            
            _targets.Clear();
        }
        
        void OnDestroy()
        {
            ClearAll();
        }
        
        #endregion
    }
}
