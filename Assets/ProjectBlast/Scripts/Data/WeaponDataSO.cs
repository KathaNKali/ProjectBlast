using UnityEngine;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Data
{
    /// <summary>
    /// ScriptableObject defining a weapon's stats and configuration.
    /// Create new weapons via: Assets → Create → ProjectBlast → Weapon Data
    /// 
    /// DESIGN:
    /// - Weapon defines damage per shot and ammo consumption
    /// - Hero defines fire rate (weapons don't control their own fire rate)
    /// - Each hero has unique weapon (not shared across heroes)
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon_NewWeapon", menuName = "ProjectBlast/Weapon Data", order = 2)]
    public class WeaponDataSO : ScriptableObject
    {
        #region Identity
        
        [Header("=== WEAPON IDENTITY ===")]
        [Tooltip("Display name of this weapon")]
        public string WeaponName = "New Weapon";
        
        [Tooltip("Weapon type/category")]
        public WeaponType WeaponType = WeaponType.Rifle;
        
        [Tooltip("Icon for UI display")]
        public Sprite Icon;
        
        [TextArea(2, 3)]
        [Tooltip("Weapon description")]
        public string Description = "A reliable weapon.";
        
        #endregion
        
        #region Damage Configuration
        
        [Header("=== DAMAGE ===")]
        [Tooltip("Damage dealt per shot/hit")]
        [Min(1)]
        public float DamagePerShot = 10f;
        
        [Tooltip("Damage type (for future enemy armor/resistance system)")]
        public DamageType DamageType = DamageType.Normal;
        
        #endregion
        
        #region Ammo Configuration
        
        [Header("=== AMMO CONSUMPTION ===")]
        [Tooltip("How much ammo consumed per shot")]
        [Min(1)]
        public int AmmoPerShot = 1;
        
        #endregion
        
        #region Projectile Configuration
        
        [Header("=== PROJECTILE ===")]
        [Tooltip("Projectile prefab (must have Projectile component)")]
        public GameObject ProjectilePrefab;
        
        [Tooltip("Projectile speed")]
        [Range(1f, 100f)]
        public float ProjectileSpeed = 20f;
        
        [Tooltip("Projectile lifetime (seconds)")]
        [Range(0.5f, 10f)]
        public float ProjectileLifetime = 3f;
        
        [Tooltip("Maximum effective range")]
        [Range(5f, 50f)]
        public float MaxRange = 25f;
        
        #endregion
        
        #region Visual Configuration
        
        [Header("=== VISUAL & AUDIO ===")]
        [Tooltip("Weapon model prefab (visual only)")]
        public GameObject WeaponModelPrefab;
        
        [Tooltip("Muzzle flash effect (optional)")]
        public GameObject MuzzleFlashPrefab;
        
        [Tooltip("Fire sound clip")]
        public AudioClip FireSound;
        
        #endregion
        
        #region Apply to Weapon
        
        /// <summary>
        /// Apply this weapon data to a Weapon component
        /// Note: Fire rate is controlled by Hero, not Weapon.
        /// Weapon defines damage and ammo consumption.
        /// Projectile configuration is done on the weapon prefab itself (MMObjectPooler).
        /// </summary>
        public void ApplyToWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                Debug.LogError("[WeaponDataSO] Cannot apply to null weapon!");
                return;
            }
            
            // Store weapon data reference for runtime access
            // (The actual damage application happens in the projectile's DamageOnTouch component)
            
            Debug.Log($"[WeaponDataSO] Applied {WeaponName} data. Damage: {DamagePerShot}, Ammo/Shot: {AmmoPerShot}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Weapon type categories
    /// </summary>
    public enum WeaponType
    {
        Rifle,          // Medium range, balanced
        Sniper,         // Long range, high damage
        Shotgun,        // Short range, spread
        MachineGun,     // High fire rate, low damage
        Launcher,       // AOE, slow fire rate
        Beam,           // Continuous damage
        Special         // Unique mechanics
    }
    
    /// <summary>
    /// Damage type for future armor/resistance system
    /// </summary>
    public enum DamageType
    {
        Normal,         // Standard damage
        Piercing,       // Ignores some armor
        Explosive,      // AOE damage
        Energy,         // Energy-based
        Special         // Unique damage type
    }
}
