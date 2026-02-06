using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// AI Decision: Detect Hero Or Wall
    /// 
    /// Target selection logic for enemy shooting:
    /// 1. Check for heroes in detection range (priority)
    /// 2. If heroes found: Target closest hero
    /// 3. If no heroes: Target wall GameObject
    /// 
    /// Sets AIBrain.Target for use by AIActionShoot3D and AIActionAimWeaponAtTarget3D.
    /// This follows TDE's standard pattern of using AIBrain.Target for shooting.
    /// 
    /// Usage:
    /// - Add to AIBrain "Attacking" state as a decision (NOT a transition)
    /// - Runs continuously while in Attacking state
    /// - Works with standard TDE shooting actions
    /// 
    /// Integration:
    /// - LaneSpawner automatically configures WallTarget reference
    /// - AttackRange from EnemyDataSO determines detection radius
    /// </summary>
    [AddComponentMenu("ProjectBlast/Combat/AI/Decisions/AI Decision Detect Hero Or Wall")]
    public class AIDecisionDetectHeroOrWall : AIDecision
    {
        [Header("Target Configuration")]
        [Tooltip("Reference to wall GameObject to shoot at when no heroes present")]
        public GameObject WallTarget;
        
        [Tooltip("Manual wall position if WallTarget not set")]
        public Vector3 ManualWallPosition = new Vector3(0, 2.5f, -5f);
        
        [Header("Detection Settings")]
        [Tooltip("Detection radius for finding heroes (0 = use from EnemyDataSO)")]
        public float HeroDetectionRange = 8f;
        
        [Tooltip("Layer mask for detecting heroes")]
        public LayerMask HeroLayerMask = 1 << 10; // Default: Layer 10 (Player)
        
        [Tooltip("Offset from character position for detection origin")]
        public Vector3 DetectionOriginOffset = Vector3.zero;
        
        [Header("Target Switching")]
        [Tooltip("Minimum time before switching targets (prevents flickering)")]
        [Range(0f, 2f)]
        public float MinimumTargetLockDuration = 0.3f;
        
        [Tooltip("Prefer current target if within this score percentage (0-100)")]
        [Range(0f, 100f)]
        public float TargetStickyness = 20f;
        
        [Header("Debug")]
        [Tooltip("Show debug logs and gizmos")]
        public bool DebugMode = false;
        
        // Cached components
        protected Character _character;
        protected Collider[] _detectionResults;
        protected const int MAX_DETECTION_RESULTS = 20;
        protected GameObject _currentTarget;
        protected float _lastTargetSwitchTime;
        protected Vector3 _detectionPosition;
        
        /// <summary>
        /// Initialize
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            
            _character = GetComponentInParent<Character>();
            _detectionResults = new Collider[MAX_DETECTION_RESULTS];
            _lastTargetSwitchTime = -999f;
            
            if (DebugMode)
            {
                Debug.Log($"[AIDecisionDetectHeroOrWall] Initialized on {_character?.name}. Detection Range: {HeroDetectionRange}m");
            }
        }
        
        /// <summary>
        /// Main decision logic - runs every frame to update target
        /// </summary>
        public override bool Decide()
        {
            if (_brain == null) return false;
            
            // Calculate detection origin
            _detectionPosition = transform.position + DetectionOriginOffset;
            
            // Check if enough time has passed to allow target switching
            bool canSwitchTarget = (Time.time - _lastTargetSwitchTime) >= MinimumTargetLockDuration;
            
            // 1. Try to find heroes in range
            GameObject bestHero = FindBestHero();
            
            if (bestHero != null)
            {
                // Hero found
                if (_brain.Target != bestHero && canSwitchTarget)
                {
                    SetTarget(bestHero, "Hero");
                    return true;
                }
                else if (_brain.Target == bestHero)
                {
                    // Already targeting this hero, keep it
                    return true;
                }
            }
            
            // 2. No heroes or can't switch yet - target wall
            if (WallTarget != null)
            {
                if (_brain.Target != WallTarget && canSwitchTarget)
                {
                    SetTarget(WallTarget, "Wall GameObject");
                    return true;
                }
                else if (_brain.Target == WallTarget)
                {
                    // Already targeting wall
                    return true;
                }
            }
            else
            {
                // No wall GameObject, create virtual target at wall position
                if (_brain.Target == null || _brain.Target.transform.position != ManualWallPosition)
                {
                    if (canSwitchTarget)
                    {
                        // Store position in brain's last known target position
                        _brain._lastKnownTargetPosition = ManualWallPosition;
                        
                        if (DebugMode && Time.frameCount % 60 == 0)
                        {
                            Debug.Log($"[AIDecisionDetectHeroOrWall] {_character.name} targeting manual wall position: {ManualWallPosition}");
                        }
                    }
                }
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Find best hero to target (closest)
        /// </summary>
        protected virtual GameObject FindBestHero()
        {
            // Perform overlap sphere to detect heroes
            int hitCount = Physics.OverlapSphereNonAlloc(
                _detectionPosition,
                HeroDetectionRange,
                _detectionResults,
                HeroLayerMask
            );
            
            if (hitCount == 0)
            {
                return null;
            }
            
            GameObject closestHero = null;
            float closestDistance = float.MaxValue;
            
            for (int i = 0; i < hitCount; i++)
            {
                if (_detectionResults[i] == null) continue;
                
                GameObject potentialTarget = _detectionResults[i].gameObject;
                
                // Check if it's a valid hero (has Character component of Player type)
                Character heroChar = potentialTarget.MMGetComponentNoAlloc<Character>();
                if (heroChar == null || heroChar.CharacterType != Character.CharacterTypes.Player)
                {
                    continue;
                }
                
                // Check if hero is alive
                Health heroHealth = heroChar.CharacterHealth;
                if (heroHealth != null && heroHealth.CurrentHealth <= 0)
                {
                    continue;
                }
                
                // Calculate distance
                float distance = Vector3.Distance(_detectionPosition, potentialTarget.transform.position);
                
                // Apply stickyness - prefer current target if it's close enough
                if (_brain.Target == potentialTarget && TargetStickyness > 0)
                {
                    distance *= (1f - (TargetStickyness / 100f));
                }
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHero = potentialTarget;
                }
            }
            
            return closestHero;
        }
        
        /// <summary>
        /// Set the target in AIBrain
        /// </summary>
        protected virtual void SetTarget(GameObject target, string targetType)
        {
            _brain.Target = target.transform;
            _currentTarget = target;
            _lastTargetSwitchTime = Time.time;
            
            if (DebugMode)
            {
                float distance = Vector3.Distance(_detectionPosition, target.transform.position);
                Debug.Log($"[AIDecisionDetectHeroOrWall] {_character.name} targeting {targetType}: {target.name} at distance {distance:F1}m");
            }
        }
        
        /// <summary>
        /// Draw debug gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (!DebugMode) return;
            if (_character == null) return;
            
            Vector3 origin = transform.position + DetectionOriginOffset;
            
            // Draw detection radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, HeroDetectionRange);
            
            // Draw line to current target
            if (_brain != null && _brain.Target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, _brain.Target.transform.position);
                
                // Draw target indicator
                Gizmos.DrawWireSphere(_brain.Target.transform.position, 0.5f);
            }
            else if (WallTarget == null)
            {
                // Draw manual wall position
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(origin, ManualWallPosition);
                Gizmos.DrawWireSphere(ManualWallPosition, 0.5f);
            }
            
#if UNITY_EDITOR
            // Draw label
            if (_brain != null && _brain.Target != null)
            {
                string targetType = _brain.Target == WallTarget ? "WALL" : "HERO";
                float distance = Vector3.Distance(origin, _brain.Target.transform.position);
                UnityEditor.Handles.Label(
                    _brain.Target.transform.position + Vector3.up * 2,
                    $"TARGET: {targetType}\n{_brain.Target.name}\nDist: {distance:F1}m"
                );
            }
#endif
        }
    }
}
