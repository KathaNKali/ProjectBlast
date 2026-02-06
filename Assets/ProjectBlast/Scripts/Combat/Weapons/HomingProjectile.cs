using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// A homing projectile that smoothly curves toward its target using lerp-based steering.
    /// Extends TDE's base Projectile class with intelligent target tracking.
    /// </summary>
    [AddComponentMenu("ProjectBlast/Combat/Weapons/Homing Projectile")]
    public class HomingProjectile : Projectile
    {
        [Header("Homing Behavior")]
        
        [Tooltip("The target transform to home in on (usually set by weapon system from AIBrain.Target)")]
        public Transform Target;
        
        [Tooltip("How quickly the projectile turns toward the target (degrees per second equivalent). Higher = sharper curves.")]
        [Range(1f, 20f)]
        public float TurnSpeed = 5f;
        
        [Tooltip("Maximum duration in seconds that the projectile will track the target. After this, flies straight.")]
        public float HomingDuration = 3f;
        
        [Tooltip("Minimum distance to target before homing stops (prevents spiraling at close range)")]
        public float MinTrackingDistance = 0.5f;
        
        [Tooltip("If true, shows debug lines indicating target direction and current trajectory")]
        public bool ShowDebugGizmos = false;
        
        [Header("Advanced Settings")]
        
        [Tooltip("If true, enables maximum turn rate limiting (more realistic but less aggressive tracking)")]
        public bool UseTurnRateLimit = false;
        
        [Tooltip("Maximum turn rate in degrees per second (only used if UseTurnRateLimit is true)")]
        [MMCondition("UseTurnRateLimit", true)]
        public float MaxTurnRate = 180f;
        
        protected float _timeAlive = 0f;
        protected bool _isHoming = true;
        
        /// <summary>
        /// Sets the target for this homing projectile
        /// </summary>
        /// <param name="target">The target transform to track</param>
        public virtual void SetTarget(Transform target)
        {
            Target = target;
            
            if (ShowDebugGizmos && target != null)
            {
                Debug.Log($"[HomingProjectile] {gameObject.name} acquired target: {target.name} at distance {Vector3.Distance(transform.position, target.position):F1}m");
            }
        }
        
        /// <summary>
        /// Overrides base Movement to add homing behavior
        /// </summary>
        public override void Movement()
        {
            _timeAlive += Time.deltaTime;
            
            // Check if we should still be homing
            if (_isHoming && _timeAlive >= HomingDuration)
            {
                _isHoming = false;
                if (ShowDebugGizmos)
                {
                    Debug.Log($"[HomingProjectile] {gameObject.name} homing duration expired, flying straight");
                }
            }
            
            // Homing phase
            if (_isHoming && Target != null)
            {
                // Check distance to target
                float distanceToTarget = Vector3.Distance(transform.position, Target.position);
                
                // Stop homing if too close (prevents spiraling)
                if (distanceToTarget < MinTrackingDistance)
                {
                    if (ShowDebugGizmos)
                    {
                        Debug.Log($"[HomingProjectile] {gameObject.name} within minimum tracking distance, flying straight");
                    }
                    base.Movement();
                    return;
                }
                
                // Calculate direction to target
                Vector3 targetDirection = (Target.position - transform.position).normalized;
                
                // Apply turning based on method
                if (UseTurnRateLimit)
                {
                    // More realistic: limit maximum turn rate
                    float maxRadians = MaxTurnRate * Mathf.Deg2Rad * Time.deltaTime;
                    Direction = Vector3.RotateTowards(Direction, targetDirection, maxRadians, 0f);
                }
                else
                {
                    // Simpler: smooth lerp toward target
                    Direction = Vector3.Lerp(Direction, targetDirection, TurnSpeed * Time.deltaTime).normalized;
                }
                
                // Rotate visual to face movement direction
                UpdateVisualRotation();
            }
            
            // Call base movement (handles actual position update)
            base.Movement();
        }
        
        /// <summary>
        /// Updates the projectile's visual rotation to face its movement direction
        /// </summary>
        protected virtual void UpdateVisualRotation()
        {
            // Determine which axis to align based on MovementVector setting
            switch (MovementVector)
            {
                case MovementVectors.Forward:
                    transform.forward = _spawnerIsFacingRight ? Direction : -Direction;
                    break;
                case MovementVectors.Right:
                    transform.right = _spawnerIsFacingRight ? Direction : -Direction;
                    break;
                case MovementVectors.Up:
                    transform.up = _spawnerIsFacingRight ? Direction : -Direction;
                    break;
            }
        }
        
        /// <summary>
        /// Reset homing state when projectile is enabled
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _timeAlive = 0f;
            _isHoming = true;
        }
        
        /// <summary>
        /// Initialization override to reset homing state
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _timeAlive = 0f;
            _isHoming = true;
        }
        
        /// <summary>
        /// Draw debug gizmos to visualize homing behavior
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (!ShowDebugGizmos || !Application.isPlaying) return;
            
            // Draw current direction (blue line)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Direction * 2f);
            
            // Draw line to target (green if homing, red if not)
            if (Target != null)
            {
                Gizmos.color = _isHoming ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, Target.position);
                
                // Draw minimum tracking distance sphere
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawWireSphere(Target.position, MinTrackingDistance);
            }
            
            // Draw homing range indicator
            if (_timeAlive < HomingDuration)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, 0.3f);
            }
        }
        
        /// <summary>
        /// Validates target is still valid (not destroyed)
        /// </summary>
        protected virtual bool IsTargetValid()
        {
            if (Target == null) return false;
            
            // Check if target has Health component and is alive
            Health targetHealth = Target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.CurrentHealth <= 0)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Update to check target validity each frame
        /// </summary>
        protected override void FixedUpdate()
        {
            // Clear target if it becomes invalid
            if (Target != null && !IsTargetValid())
            {
                if (ShowDebugGizmos)
                {
                    Debug.Log($"[HomingProjectile] {gameObject.name} target became invalid, flying straight");
                }
                Target = null;
                _isHoming = false;
            }
            
            base.FixedUpdate();
        }
    }
}
