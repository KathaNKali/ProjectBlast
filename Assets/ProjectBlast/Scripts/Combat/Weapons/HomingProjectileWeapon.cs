using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Extended ProjectileWeapon that automatically assigns targets to HomingProjectile components.
    /// Integrates with TDE's AIBrain system to enable enemy homing missiles.
    /// </summary>
    [AddComponentMenu("ProjectBlast/Combat/Weapons/Homing Projectile Weapon")]
    public class HomingProjectileWeapon : ProjectileWeapon
    {
        [Header("Homing Configuration")]
        
        [Tooltip("If true, automatically assigns AIBrain.Target to spawned homing projectiles")]
        public bool AutoAssignTarget = true;
        
        [Tooltip("If true, shows debug logs when assigning targets to projectiles")]
        public bool DebugMode = false;
        
        /// <summary>
        /// Override SpawnProjectile to assign target to homing projectiles
        /// </summary>
        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            // Call base implementation to spawn the projectile
            GameObject projectileObject = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);
            
            if (projectileObject == null) return null;
            
            // Check if this is a homing projectile
            HomingProjectile homingProjectile = projectileObject.GetComponent<HomingProjectile>();
            
            if (homingProjectile != null && AutoAssignTarget)
            {
                // Try to get target from AIBrain
                Transform target = GetTargetFromAIBrain();
                
                if (target != null)
                {
                    homingProjectile.SetTarget(target);
                    
                    if (DebugMode)
                    {
                        Debug.Log($"[HomingProjectileWeapon] Assigned target {target.name} to projectile {projectileObject.name}");
                    }
                }
                else if (DebugMode)
                {
                    Debug.LogWarning($"[HomingProjectileWeapon] No target found for homing projectile {projectileObject.name}");
                }
            }
            
            return projectileObject;
        }
        
        /// <summary>
        /// Gets the target from the owner's AIBrain component
        /// </summary>
        /// <returns>Target transform from AIBrain, or null if not found</returns>
        protected virtual Transform GetTargetFromAIBrain()
        {
            if (Owner == null) return null;
            
            // Try to get AIBrain from owner character
            AIBrain brain = Owner.GetComponentInParent<AIBrain>();
            
            if (brain != null && brain.Target != null)
            {
                return brain.Target;
            }
            
            // Also check direct parent (in case AIBrain is on same GameObject)
            brain = Owner.GetComponent<AIBrain>();
            
            if (brain != null && brain.Target != null)
            {
                return brain.Target;
            }
            
            return null;
        }
        
        /// <summary>
        /// Alternative method: Manually set target on projectile (for non-AI weapons)
        /// </summary>
        /// <param name="target">The target to assign to next spawned projectile</param>
        public virtual void SetNextProjectileTarget(Transform target)
        {
            _manualTarget = target;
        }
        
        protected Transform _manualTarget;
        
        /// <summary>
        /// Enhanced version that checks manual target first
        /// </summary>
        protected virtual Transform GetTarget()
        {
            // Priority 1: Manual target (set programmatically)
            if (_manualTarget != null)
            {
                Transform target = _manualTarget;
                _manualTarget = null; // Clear after use
                return target;
            }
            
            // Priority 2: AIBrain target (for AI-controlled weapons)
            return GetTargetFromAIBrain();
        }
    }
}
