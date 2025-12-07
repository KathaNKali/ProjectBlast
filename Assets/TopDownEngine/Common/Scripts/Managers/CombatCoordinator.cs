using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Centralized singleton that coordinates bullet allocation across all heroes and enemies.
    /// Prevents bullet waste by ensuring atomic shot approval + tracking.
    /// Heroes request permission before firing - coordinator checks if enemy has enough HP.
    /// 
    /// KEY FEATURE: RequestShot() is ATOMIC - check + track happen together, preventing race conditions.
    /// 
    /// USAGE:
    /// - Heroes call RequestShot() before firing each bullet
    /// - If approved (true), hero fires and bullet is already tracked
    /// - If denied (false), hero switches to new target
    /// - Health component calls OnBulletHit() when damage dealt
    /// </summary>
    public class CombatCoordinator : MMSingleton<CombatCoordinator>
    {
        /// <summary>
        /// Tracks targeting data for a single enemy
        /// </summary>
        private class EnemyTargetData
        {
            public Health Health;                           // Enemy's Health component
            public float InFlightDamage;                    // Total damage from bullets currently in-flight
            public HashSet<GameObject> AssignedHeroes;      // Heroes currently targeting this enemy
            
            public float GetEffectiveHP()
            {
                if (Health == null) return 0f;
                return Health.CurrentHealth - InFlightDamage;
            }
        }
        
        [Header("Configuration")]
        [Tooltip("Minimum effective HP required to approve a shot (prevents firing at doomed enemies)")]
        public float MinEffectiveHPThreshold = 1.0f;
        
        [Header("Debug Info")]
        [SerializeField] private int _trackedEnemyCount;
        [SerializeField] private int _totalHeroesEngaged;
        [SerializeField] private float _totalInFlightDamage;
        
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
        /// ATOMIC OPERATION: Request permission to shoot AND track in-flight damage.
        /// This is the core coordination method - ensures no race conditions.
        /// 
        /// Returns true if shot approved (enemy has enough HP), false if enemy is doomed.
        /// If approved, bullet damage is IMMEDIATELY added to in-flight tracking.
        /// </summary>
        public bool RequestShot(GameObject hero, GameObject enemy, float damage)
        {
            if (hero == null || enemy == null || damage <= 0)
            {
                return false;
            }
            
            // Get or create target data for this enemy
            if (!_targets.ContainsKey(enemy))
            {
                var health = enemy.GetComponent<Health>();
                if (health == null || health.CurrentHealth <= 0)
                {
                    return false; // Enemy has no Health component or already dead
                }
                
                _targets[enemy] = new EnemyTargetData
                {
                    Health = health,
                    InFlightDamage = 0f,
                    AssignedHeroes = new HashSet<GameObject>()
                };
            }
            
            var targetData = _targets[enemy];
            
            // Check if enemy is still alive
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                return false;
            }
            
            // Check if shot is worthwhile (enemy has enough effective HP)
            float effectiveHP = targetData.GetEffectiveHP();
            if (effectiveHP <= MinEffectiveHPThreshold)
            {
                // Enemy is "doomed" - enough bullets in-flight to kill it
                return false;
            }
            
            // ATOMIC: Approve shot + track immediately (prevents race condition)
            targetData.InFlightDamage += damage;
            targetData.AssignedHeroes.Add(hero);
            
            return true; // Shot approved - hero can fire
        }
        
        /// <summary>
        /// Called when a bullet hits an enemy and deals damage.
        /// Reduces in-flight damage tracking by the amount dealt.
        /// Call from Health.Damage() method.
        /// </summary>
        public void OnBulletHit(GameObject enemy, float damageDealt)
        {
            if (enemy == null || !_targets.ContainsKey(enemy))
            {
                return;
            }
            
            var targetData = _targets[enemy];
            
            // Reduce in-flight damage (bullet has landed)
            targetData.InFlightDamage -= damageDealt;
            
            // Clamp to prevent negative values (floating point precision)
            if (targetData.InFlightDamage < 0)
            {
                targetData.InFlightDamage = 0;
            }
            
            // Check if enemy died
            if (targetData.Health == null || targetData.Health.CurrentHealth <= 0)
            {
                OnEnemyDied(enemy, targetData);
            }
        }
        
        /// <summary>
        /// Called when enemy dies. Notifies all assigned heroes to find new targets.
        /// </summary>
        private void OnEnemyDied(GameObject enemy, EnemyTargetData targetData)
        {
            // Copy heroes to list to avoid collection modification during enumeration
            var heroesToNotify = new List<GameObject>(targetData.AssignedHeroes);
            
            // Notify all heroes targeting this enemy
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
        /// Hero releases target when switching to a different enemy or exiting combat.
        /// Call from AIActionShoot3D when changing targets.
        /// </summary>
        public void ReleaseTarget(GameObject hero, GameObject enemy)
        {
            if (hero == null || enemy == null || !_targets.ContainsKey(enemy))
            {
                return;
            }
            
            _targets[enemy].AssignedHeroes.Remove(hero);
            
            // Cleanup if no more heroes targeting this enemy
            if (_targets[enemy].AssignedHeroes.Count == 0)
            {
                _targets.Remove(enemy);
            }
        }
        
        /// <summary>
        /// Gets effective HP for an enemy (for debugging/UI purposes)
        /// </summary>
        public float GetEnemyEffectiveHP(GameObject enemy)
        {
            if (enemy == null || !_targets.ContainsKey(enemy))
            {
                var health = enemy?.GetComponent<Health>();
                return health != null ? health.CurrentHealth : 0f;
            }
            
            return _targets[enemy].GetEffectiveHP();
        }
        
        /// <summary>
        /// Gets in-flight damage for an enemy (for debugging/UI purposes)
        /// </summary>
        public float GetEnemyInFlightDamage(GameObject enemy)
        {
            if (enemy == null || !_targets.ContainsKey(enemy))
            {
                return 0f;
            }
            
            return _targets[enemy].InFlightDamage;
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
            _totalInFlightDamage = 0f;
            
            foreach (var targetData in _targets.Values)
            {
                _totalHeroesEngaged += targetData.AssignedHeroes.Count;
                _totalInFlightDamage += targetData.InFlightDamage;
            }
        }
        
        /// <summary>
        /// Clears all tracking data (useful for scene transitions)
        /// </summary>
        public void ClearAll()
        {
            _targets.Clear();
        }
        
        void OnDestroy()
        {
            ClearAll();
        }
    }
}
