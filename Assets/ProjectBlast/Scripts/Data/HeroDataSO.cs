using UnityEngine;
using MoreMountains.TopDownEngine;
using ProjectBlast.Heroes;

namespace ProjectBlast.Data
{
    /// <summary>
    /// ScriptableObject defining a hero's stats and configuration.
    /// Create new heroes via: Assets → Create → ProjectBlast → Hero Data
    /// 
    /// DESIGN:
    /// - Each hero is a unique prefab with its own model
    /// - Hero defines fire rate, ammo pool, detection
    /// - Weapon defines damage, consumption rate
    /// - DPS is calculated automatically for balancing reference
    /// </summary>
    [CreateAssetMenu(fileName = "Hero_NewHero", menuName = "ProjectBlast/Hero Data", order = 1)]
    public class HeroDataSO : ScriptableObject
    {
        #region Identity
        
        [Header("=== HERO IDENTITY ===")]
        [Tooltip("Display name of this hero")]
        public string HeroName = "New Hero";
        
        [Tooltip("Hero class/type")]
        public HeroClass HeroClass = HeroClass.Ranged;
        
        [Tooltip("Icon for UI display")]
        public Sprite Icon;
        
        [TextArea(2, 4)]
        [Tooltip("Hero description for UI")]
        public string Description = "A brave hero ready for battle.";
        
        #endregion
        
        #region Health Configuration
        
        [Header("=== HEALTH ===")]
        [Tooltip("Maximum health points")]
        [Min(1)]
        public float MaxHealth = 100f;
        
        [Tooltip("Starting health (usually same as max)")]
        [Min(1)]
        public float StartingHealth = 100f;
        
        #endregion
        
        #region Ammo Configuration
        
        [Header("=== AMMO SYSTEM ===")]
        [Tooltip("Does this hero have unlimited ammo?")]
        public bool UnlimitedAmmo = false;
        
        [Tooltip("Starting ammo count (ignored if unlimited)")]
        [Min(1)]
        public int StartingAmmo = 100;
        
        [Tooltip("Warn when ammo reaches this threshold")]
        [Min(1)]
        public int LowAmmoThreshold = 20;
        
        #endregion
        
        #region Combat Configuration
        
        [Header("=== COMBAT STATS ===")]
        [Tooltip("How far this hero can detect enemies (meters)")]
        [Range(5f, 50f)]
        public float DetectionRange = 20f;
        
        [Tooltip("How often to search for new targets (seconds)")]
        [Range(0.1f, 2f)]
        public float TargetSearchInterval = 0.5f;
        
        [Tooltip("Fire rate - shots per second")]
        [Range(0.1f, 10f)]
        public float FireRate = 2f;
        
        [Tooltip("What layers this hero can target")]
        public LayerMask TargetLayerMask;
        
        [Tooltip("Optional tags to filter targets (e.g., 'Boss', 'Flying')")]
        public string[] TargetTags;
        
        #endregion
        
        #region Weapon Configuration
        
        [Header("=== WEAPON ===")]
        [Tooltip("Default weapon prefab for this hero\n\nREQUIREMENTS:\n• Must have Weapon or ProjectileWeapon component (TopDown Engine)\n• Must have WeaponDataHolder component (ProjectBlast)\n• WeaponDataHolder must reference a WeaponDataSO")]
        public Weapon DefaultWeaponPrefab;
        
        #endregion
        
        #region Calculated Stats (Read-Only)
        
        [Header("=== CALCULATED STATS (Read-Only) ===")]
        [Tooltip("Damage per second (calculated from Fire Rate × Weapon Damage)")]
        [SerializeField] private float _cachedDPS = 0f;
        
        [Tooltip("Time to empty ammo (calculated from Ammo ÷ (Fire Rate × Ammo Per Shot))")]
        [SerializeField] private float _cachedAmmoLifetime = 0f;
        
        /// <summary>
        /// Calculated DPS (Damage Per Second)
        /// </summary>
        public float DPS
        {
            get
            {
                if (DefaultWeaponPrefab != null)
                {
                    var weaponData = DefaultWeaponPrefab.GetComponent<WeaponDataHolder>();
                    if (weaponData != null && weaponData.WeaponData != null)
                    {
                        return FireRate * weaponData.WeaponData.DamagePerShot;
                    }
                }
                return 0f;
            }
        }
        
        /// <summary>
        /// Calculated ammo lifetime in seconds (how long before running out)
        /// </summary>
        public float AmmoLifetime
        {
            get
            {
                if (UnlimitedAmmo) return float.PositiveInfinity;
                
                if (DefaultWeaponPrefab != null)
                {
                    var weaponData = DefaultWeaponPrefab.GetComponent<WeaponDataHolder>();
                    if (weaponData != null && weaponData.WeaponData != null)
                    {
                        float shotsPerSecond = FireRate;
                        float ammoPerSecond = shotsPerSecond * weaponData.WeaponData.AmmoPerShot;
                        return StartingAmmo / ammoPerSecond;
                    }
                }
                return 0f;
            }
        }
        
        #endregion
        
        #region Editor Utilities
        
#if UNITY_EDITOR
        /// <summary>
        /// Update cached calculated stats (called in Inspector)
        /// </summary>
        private void OnValidate()
        {
            // Ensure starting health doesn't exceed max
            if (StartingHealth > MaxHealth)
            {
                StartingHealth = MaxHealth;
            }
            
            // Update cached stats for display in Inspector
            _cachedDPS = DPS;
            _cachedAmmoLifetime = AmmoLifetime;
        }
#endif
        
        #endregion
        
        #region Apply to Hero
        
        /// <summary>
        /// Apply this hero data to a Hero component.
        /// Note: Hero.cs now reads stats via properties, so this just validates and sets Health.
        /// </summary>
        public void ApplyToHero(Heroes.Hero hero)
        {
            if (hero == null)
            {
                Debug.LogError("[HeroDataSO] Cannot apply to null hero!");
                return;
            }
            
            // Validate weapon setup
            if (DefaultWeaponPrefab == null)
            {
                Debug.LogWarning($"[HeroDataSO] {HeroName} has no weapon assigned!");
            }
            else
            {
                var weaponHolder = DefaultWeaponPrefab.GetComponent<WeaponDataHolder>();
                if (weaponHolder == null)
                {
                    Debug.LogError($"[HeroDataSO] {HeroName}'s weapon '{DefaultWeaponPrefab.name}' is missing WeaponDataHolder component!");
                }
                else if (weaponHolder.WeaponData == null)
                {
                    Debug.LogError($"[HeroDataSO] {HeroName}'s weapon '{DefaultWeaponPrefab.name}' WeaponDataHolder has no WeaponDataSO assigned!");
                }
            }
            
            // Health (if Health component exists)
            if (hero.Health != null)
            {
                hero.Health.MaximumHealth = MaxHealth;
                hero.Health.InitialHealth = StartingHealth;
                hero.Health.ResetHealthToMaxHealth();
            }
            
            Debug.Log($"[HeroDataSO] Applied {HeroName} data to hero. DPS: {DPS:F1}, Ammo Lifetime: {AmmoLifetime:F1}s");
        }
        
        #endregion
    }
}
