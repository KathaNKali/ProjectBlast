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
        /// If true, sets Brain's Target to null when no targets found
        /// </summary>
        [Tooltip("If true, this decision will set the AI Brain's Target to null if no target is found")]
        public bool SetTargetToNullIfNoneIsFound = true;
        
        [Header("Debug")]
        [SerializeField] private int _visibleTargetsCount;
        [SerializeField] private string _currentTargetName;
        
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
            
            // No targets found
            if (TargetConeOfVision.VisibleTargets.Count == 0)
            {
                if (SetTargetToNullIfNoneIsFound)
                {
                    _brain.Target = null;
                    _currentTargetName = "None";
                }
                return false;
            }
            
            // Single target - no sorting needed
            if (TargetConeOfVision.VisibleTargets.Count == 1)
            {
                _brain.Target = TargetConeOfVision.VisibleTargets[0];
                _currentTargetName = _brain.Target != null ? _brain.Target.name : "None";
                return true;
            }
            
            // Multiple targets - sort based on priority
            Transform selectedTarget = SelectTargetByPriority(TargetConeOfVision.VisibleTargets);
            
            if (selectedTarget != null)
            {
                _brain.Target = selectedTarget;
                _currentTargetName = selectedTarget.name;
                return true;
            }
            else
            {
                if (SetTargetToNullIfNoneIsFound)
                {
                    _brain.Target = null;
                    _currentTargetName = "None";
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
                
                Health health = target.GetComponent<Health>();
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
                
                Health health = target.GetComponent<Health>();
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
