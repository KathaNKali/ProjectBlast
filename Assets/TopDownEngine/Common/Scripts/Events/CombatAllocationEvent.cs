using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Combat allocation event following TDE's MMEventManager pattern.
    /// Used for decoupled communication between AI actions, heroes, and CombatCoordinator.
    /// 
    /// USAGE PATTERN:
    /// 1. AIActionShoot3D triggers Request event with bullet requirements
    /// 2. CombatCoordinator listens, processes allocation, triggers Grant/Deny response
    /// 3. AIActionShoot3D listens for Grant/Deny and proceeds accordingly
    /// 4. Hero triggers Release event when switching targets or dying
    /// 
    /// This eliminates direct method calls and follows TDE's event-driven architecture.
    /// </summary>
    public struct CombatAllocationEvent
    {
        /// <summary>
        /// Event types for allocation lifecycle
        /// </summary>
        public enum EventType
        {
            Request,        // Hero requests bullet allocation for enemy
            Grant,          // CombatCoordinator grants allocation request
            Deny,           // CombatCoordinator denies allocation request
            Release,        // Hero releases allocation (target switch/death)
            BulletFired,    // Hero fired allocated bullet
            BulletHit,      // Bullet successfully hit enemy
            EnemyDied       // Enemy died, release all allocations
        }
        
        // Event type
        public EventType Type;
        
        // Core references (TDE component-based pattern)
        public Character Hero;              // Hero character component
        public Health EnemyHealth;          // Enemy health component
        public GameObject HeroObject;       // Hero GameObject (for backwards compatibility)
        public GameObject EnemyObject;      // Enemy GameObject (for backwards compatibility)
        
        // Allocation data
        public int BulletsRequested;        // How many bullets hero wants to allocate
        public int BulletsGranted;          // How many bullets actually granted
        public float DamagePerBullet;       // Damage value per bullet
        public AllocationResult Result;     // Result code (Success, Denied, etc.)
        
        // Additional context
        public int RemainingAmmo;           // Hero's remaining ammo after operation
        public float EnemyRemainingHP;      // Enemy's remaining HP after operation
        
        /// <summary>
        /// Trigger allocation request event (from AI/Hero)
        /// </summary>
        public static void TriggerRequest(Character hero, Health enemyHealth, GameObject heroObject, GameObject enemyObject, int bullets, float damagePerBullet)
        {
            e.Type = EventType.Request;
            e.Hero = hero;
            e.EnemyHealth = enemyHealth;
            e.HeroObject = heroObject;
            e.EnemyObject = enemyObject;
            e.BulletsRequested = bullets;
            e.DamagePerBullet = damagePerBullet;
            e.Result = AllocationResult.InvalidParameters;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger allocation grant response (from CombatCoordinator)
        /// </summary>
        public static void TriggerGrant(Character hero, Health enemyHealth, GameObject heroObject, GameObject enemyObject, int bulletsGranted, AllocationResult result)
        {
            e.Type = EventType.Grant;
            e.Hero = hero;
            e.EnemyHealth = enemyHealth;
            e.HeroObject = heroObject;
            e.EnemyObject = enemyObject;
            e.BulletsGranted = bulletsGranted;
            e.Result = result;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger allocation deny response (from CombatCoordinator)
        /// </summary>
        public static void TriggerDeny(Character hero, Health enemyHealth, GameObject heroObject, GameObject enemyObject, AllocationResult result)
        {
            e.Type = EventType.Deny;
            e.Hero = hero;
            e.EnemyHealth = enemyHealth;
            e.HeroObject = heroObject;
            e.EnemyObject = enemyObject;
            e.Result = result;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger allocation release event (from Hero/AI)
        /// </summary>
        public static void TriggerRelease(Character hero, Health enemyHealth, GameObject heroObject, GameObject enemyObject)
        {
            e.Type = EventType.Release;
            e.Hero = hero;
            e.EnemyHealth = enemyHealth;
            e.HeroObject = heroObject;
            e.EnemyObject = enemyObject;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger bullet fired event (from Hero/AI)
        /// </summary>
        public static void TriggerBulletFired(Character hero, Health enemyHealth, GameObject heroObject, GameObject enemyObject, int remainingAmmo)
        {
            e.Type = EventType.BulletFired;
            e.Hero = hero;
            e.EnemyHealth = enemyHealth;
            e.HeroObject = heroObject;
            e.EnemyObject = enemyObject;
            e.RemainingAmmo = remainingAmmo;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger bullet hit event (from Projectile/DamageOnTouch)
        /// </summary>
        public static void TriggerBulletHit(Health enemyHealth, GameObject enemyObject, float damage, float remainingHP)
        {
            e.Type = EventType.BulletHit;
            e.EnemyHealth = enemyHealth;
            e.EnemyObject = enemyObject;
            e.DamagePerBullet = damage;
            e.EnemyRemainingHP = remainingHP;
            
            MMEventManager.TriggerEvent(e);
        }
        
        /// <summary>
        /// Trigger enemy died event (from Health component)
        /// </summary>
        public static void TriggerEnemyDied(Health enemyHealth, GameObject enemyObject)
        {
            e.Type = EventType.EnemyDied;
            e.EnemyHealth = enemyHealth;
            e.EnemyObject = enemyObject;
            
            MMEventManager.TriggerEvent(e);
        }
        
        // Static instance for event pooling (TDE pattern - zero GC allocation)
        static CombatAllocationEvent e;
    }
}
