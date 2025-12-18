using UnityEngine;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.AI
{
    /// <summary>
    /// Extended version of AIDecisionDetectTargetConeOfVision3D with target priority/sorting.
    /// Detects targets in cone of vision and selects based on priority strategy.
    /// 
    /// PRIORITY MODES:
    /// - Closest: Target nearest enemy (good for consistent damage)
    /// - Farthest: Target farthest enemy (good for area control)
    /// - LowestHealth: Target weakest enemy (focus fire)
    /// - HighestHealth: Target strongest enemy (threat elimination)
    /// - FirstDetected: Original TDE behavior (first in list)
    /// 
    /// USAGE:
    /// 1. Add this component to AIBrain GameObject (replace AIDecisionDetectTargetConeOfVision3D)
    /// 2. Assign MMConeOfVision reference
    /// 3. Choose TargetPriority mode
    /// 4. Use in AI state transitions like normal detection decision
    /// </summary>
    [AddComponentMenu("ProjectBlast/AI/Decisions/AI Decision Detect Target Priority 3D")]
    public class AIDecisionDetectTargetPriority3D : AIDecision
    {
        /// <summary>
        /// Target selection priority strategies
        /// </summary>
        public enum TargetPriority
        {
            Closest,        // Target nearest enemy
            Farthest,       // Target farthest enemy
            LowestHealth,   // Target weakest enemy (focus fire)
            HighestHealth,  // Target strongest enemy (threat)
            FirstDetected   // Original behavior (no sorting)
        }
        
        [Header("Cone of Vision")]
        /// <summary>
        /// The MMConeOfVision component that detects targets
        /// </summary>
        [Tooltip("The MMConeOfVision component that detects targets. Will auto-find if not assigned.")]
        public MMConeOfVision TargetConeOfVision;
        
        [Header("Target Selection")]
        /// <summary>
        /// How to prioritize multiple targets in cone of vision
        /// </summary>
        [Tooltip("Priority strategy when multiple enemies are detected")]
        public TargetPriority Priority = TargetPriority.Closest;
        
        /// <summary>
        /// If true, locks onto first target until destroyed/lost. If false, continuously re-evaluates targets.
        /// </summary>
        [Tooltip("If true, locks onto first target until destroyed/lost. If false, continuously re-evaluates targets.")]
        public bool LockOntoTarget = true;
        
        /// <summary>
        /// If true, sets Brain's Target to null when no targets found
        /// </summary>
        [Tooltip("If true, this decision will set the AI Brain's Target to null if no target is found")]
        public bool SetTargetToNullIfNoneIsFound = true;
        
        [Header("Debug")]
        [SerializeField] private int _visibleTargetsCount;
        [SerializeField] private string _currentTargetName;
        [SerializeField] private bool _isLockedOn;
        
        // Health component cache (TDE performance pattern - frame-based caching)
        // Cleared every frame to prevent stale data
        private Dictionary<GameObject, Health> _healthCache = new Dictionary<GameObject, Health>();
        private int _lastCacheFrame = -1;
        
        /// <summary>
        /// On Init we grab our MMConeOfVision
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            
            if (TargetConeOfVision == null)
            {
                TargetConeOfVision = GetComponentInParent<MMConeOfVision>();
                
                if (TargetConeOfVision == null)
                {
                    TargetConeOfVision = GetComponent<MMConeOfVision>();
                }
            }
            
            if (TargetConeOfVision == null)
            {
                Debug.LogError($"[AIDecisionDetectTargetPriority3D] No MMConeOfVision found on {gameObject.name}! This decision requires a MMConeOfVision component.");
            }
            
            _isLockedOn = false;
        }
        
        /// <summary>
        /// When entering a state, reset lock status
        /// </summary>
        public override void OnEnterState()
        {
            base.OnEnterState();
            _isLockedOn = false;
        }
        
        /// <summary>
        /// When exiting a state, clear target lock
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            _isLockedOn = false;
        }
        
        /// <summary>
        /// Gets cached Health component for target, avoiding repeated GetComponent calls.
        /// Cache is cleared every frame to prevent stale references.
        /// TDE Performance Pattern: Frame-based component caching.
        /// </summary>
        protected virtual Health GetCachedHealth(GameObject target)
        {
            if (target == null) return null;
            
            // Clear cache every frame (prevents stale data from destroyed objects)
            if (_lastCacheFrame != Time.frameCount)
            {
                _healthCache.Clear();
                _lastCacheFrame = Time.frameCount;
            }
            
            // Check cache first
            if (!_healthCache.TryGetValue(target, out Health health))
            {
                // Cache miss - get component and store
                health = target.GetComponent<Health>();
                _healthCache[target] = health;
            }
            
            return health;
        }
        
        /// <summary>
        /// On Decide we look for a target with priority sorting
        /// </summary>
        public override bool Decide()
        {
            return DetectTargetWithPriority();
        }
        
        /// <summary>
        /// Detects targets in cone of vision and selects based on priority
        /// </summary>
        protected virtual bool DetectTargetWithPriority()
        {
            if (TargetConeOfVision == null)
            {
                Debug.LogWarning($"[AIDecisionDetectTargetPriority3D] TargetConeOfVision is null on {gameObject.name}");
                return false;
            }
            
            _visibleTargetsCount = TargetConeOfVision.VisibleTargets.Count;
            
            // If LockOntoTarget is enabled and we already have a target
            if (LockOntoTarget && _brain.Target != null)
            {
                // Check if current target is still valid (exists and still in visible targets list)
                if (_brain.Target != null && TargetConeOfVision.VisibleTargets.Contains(_brain.Target))
                {
                    // Keep locked onto current target
                    _currentTargetName = _brain.Target.name;
                    _isLockedOn = true;
                    return true;
                }
                else
                {
                    // Current target lost (destroyed or out of cone) - unlock and find new target
                    _isLockedOn = false;
                    _brain.Target = null;
                }
            }
            
            // No targets found
            if (TargetConeOfVision.VisibleTargets.Count == 0)
            {
                if (SetTargetToNullIfNoneIsFound)
                {
                    _brain.Target = null;
                    _currentTargetName = "None";
                    _isLockedOn = false;
                }
                return false;
            }
            
            // Find new target (either first time, or previous target lost)
            Transform selectedTarget = null;
            
            // Single target - no sorting needed
            if (TargetConeOfVision.VisibleTargets.Count == 1)
            {
                selectedTarget = TargetConeOfVision.VisibleTargets[0];
            }
            else
            {
                // Multiple targets - sort based on priority
                selectedTarget = SelectTargetByPriority(TargetConeOfVision.VisibleTargets);
            }
            
            if (selectedTarget != null)
            {
                _brain.Target = selectedTarget;
                _currentTargetName = selectedTarget.name;
                _isLockedOn = LockOntoTarget;
                return true;
            }
            else
            {
                if (SetTargetToNullIfNoneIsFound)
                {
                    _brain.Target = null;
                    _currentTargetName = "None";
                    _isLockedOn = false;
                }
                return false;
            }
        }
        
        /// <summary>
        /// Selects the best target from visible targets based on priority strategy
        /// </summary>
        protected virtual Transform SelectTargetByPriority(List<Transform> visibleTargets)
        {
            if (visibleTargets == null || visibleTargets.Count == 0)
            {
                return null;
            }
            
            // Create a copy to avoid modifying the original list
            List<Transform> targets = new List<Transform>(visibleTargets);
            
            // Remove any null targets
            targets.RemoveAll(t => t == null);
            
            // COOPERATIVE ALLOCATION: Filter out dead/fully-allocated enemies
            if (CombatCoordinator.HasInstance)
            {
                targets.RemoveAll(t => !CombatCoordinator.Instance.IsEnemyAvailableForClaim(t.gameObject));
            }
            
            if (targets.Count == 0)
            {
                return null;
            }
            
            // SMART DISTRIBUTION: Prefer unclaimed enemies over already-allocated ones
            // This naturally spreads heroes across multiple enemies
            if (CombatCoordinator.HasInstance && targets.Count > 1)
            {
                List<Transform> unclaimedTargets = targets.FindAll(t => 
                    CombatCoordinator.Instance.GetEnemyAllocatedHeroCount(t.gameObject) == 0);
                
                // If unclaimed enemies exist, prioritize them
                if (unclaimedTargets.Count > 0)
                {
                    targets = unclaimedTargets;
                }
            }
            
            if (targets.Count == 0)
            {
                return null;
            }
            
            switch (Priority)
            {
                case TargetPriority.Closest:
                    return GetClosestTarget(targets);
                    
                case TargetPriority.Farthest:
                    return GetFarthestTarget(targets);
                    
                case TargetPriority.LowestHealth:
                    return GetLowestHealthTarget(targets);
                    
                case TargetPriority.HighestHealth:
                    return GetHighestHealthTarget(targets);
                    
                case TargetPriority.FirstDetected:
                default:
                    return targets[0];
            }
        }
        
        /// <summary>
        /// Returns the closest target by distance
        /// </summary>
        protected virtual Transform GetClosestTarget(List<Transform> targets)
        {
            Transform closest = null;
            float minDistance = float.MaxValue;
            
            foreach (Transform target in targets)
            {
                if (target == null) continue;
                
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = target;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Returns the farthest target by distance
        /// </summary>
        protected virtual Transform GetFarthestTarget(List<Transform> targets)
        {
            Transform farthest = null;
            float maxDistance = 0f;
            
            foreach (Transform target in targets)
            {
                if (target == null) continue;
                
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthest = target;
                }
            }
            
            return farthest;
        }
        
        /// <summary>
        /// Returns the target with lowest current health
        /// </summary>
        protected virtual Transform GetLowestHealthTarget(List<Transform> targets)
        {
            Transform lowestHealthTarget = null;
            float lowestHealth = float.MaxValue;
            
            foreach (Transform target in targets)
            {
                if (target == null) continue;
                
                // Use cached Health lookup (TDE performance pattern)
                Health health = GetCachedHealth(target.gameObject);
                if (health != null)
                {
                    if (health.CurrentHealth < lowestHealth)
                    {
                        lowestHealth = health.CurrentHealth;
                        lowestHealthTarget = target;
                    }
                }
                else
                {
                    // If no Health component, treat as low priority (high health)
                    // This ensures enemies with Health are prioritized
                    if (lowestHealthTarget == null)
                    {
                        lowestHealthTarget = target;
                    }
                }
            }
            
            return lowestHealthTarget != null ? lowestHealthTarget : targets[0];
        }
        
        /// <summary>
        /// Returns the target with highest current health (biggest threat)
        /// </summary>
        protected virtual Transform GetHighestHealthTarget(List<Transform> targets)
        {
            Transform highestHealthTarget = null;
            float highestHealth = 0f;
            
            foreach (Transform target in targets)
            {
                if (target == null) continue;
                
                // Use cached Health lookup (TDE performance pattern)
                Health health = GetCachedHealth(target.gameObject);
                if (health != null)
                {
                    if (health.CurrentHealth > highestHealth)
                    {
                        highestHealth = health.CurrentHealth;
                        highestHealthTarget = target;
                    }
                }
                else
                {
                    // If no Health component, treat as high priority (potential threat)
                    if (highestHealthTarget == null)
                    {
                        highestHealthTarget = target;
                    }
                }
            }
            
            return highestHealthTarget != null ? highestHealthTarget : targets[0];
        }
        
        /// <summary>
        /// Draws gizmos showing the current target
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (_brain == null || _brain.Target == null) return;
            
            // Draw line to current target
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _brain.Target.position);
            
            // Draw sphere at target
            Gizmos.DrawWireSphere(_brain.Target.position, 0.5f);
        }
    }
}
