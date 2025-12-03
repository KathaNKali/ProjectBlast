using UnityEngine;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Data
{
    /// <summary>
    /// Configures projectile damage from WeaponDataSO.
    /// Add this component to projectile prefabs to enable data-driven damage.
    /// 
    /// USAGE:
    /// 1. Add this component to your projectile prefab
    /// 2. Add DamageOnTouch component to the same prefab
    /// 3. When weapon spawns projectile, call SetDamage() with WeaponDataSO damage value
    /// 4. DamageOnTouch will automatically deal that damage to enemies
    /// 
    /// FLOW:
    /// Weapon spawns projectile → ProjectileDamageConfigurator.SetDamage(damage) →
    /// Updates DamageOnTouch.DamageCaused → Projectile hits enemy → 
    /// DamageOnTouch finds Health on enemy → Health.Damage(amount) called
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    [RequireComponent(typeof(DamageOnTouch))]
    public class ProjectileDamageConfigurator : MonoBehaviour
    {
        [Header("Damage Configuration")]
        [Tooltip("Base damage value (can be overridden dynamically)")]
        [Min(1)]
        public float BaseDamage = 10f;
        
        [Tooltip("Apply base damage on Awake (if not set dynamically)")]
        public bool UseBaseDamageOnAwake = true;
        
        [Header("References")]
        [Tooltip("DamageOnTouch component (auto-found if null)")]
        public DamageOnTouch DamageOnTouch;
        
        [Tooltip("Projectile component (auto-found if null)")]
        public Projectile ProjectileComponent;
        
        [Header("Debug")]
        [SerializeField] private float _currentDamage;
        [SerializeField] private bool _damageConfigured = false;
        
        void Awake()
        {
            // Find components if not assigned
            if (DamageOnTouch == null)
            {
                DamageOnTouch = GetComponent<DamageOnTouch>();
            }
            
            if (ProjectileComponent == null)
            {
                ProjectileComponent = GetComponent<Projectile>();
            }
            
            // Validate components
            if (DamageOnTouch == null)
            {
                Debug.LogError($"[ProjectileDamageConfigurator] DamageOnTouch component not found on {gameObject.name}! Projectile will not deal damage.");
                return;
            }
            
            // Apply base damage if configured to do so
            if (UseBaseDamageOnAwake)
            {
                SetDamage(BaseDamage);
            }
        }
        
        /// <summary>
        /// Set the damage value for this projectile.
        /// Call this when spawning projectiles from WeaponDataSO.
        /// </summary>
        /// <param name="damage">Damage amount to deal on hit</param>
        public void SetDamage(float damage)
        {
            if (DamageOnTouch == null)
            {
                Debug.LogError($"[ProjectileDamageConfigurator] Cannot set damage - DamageOnTouch is null on {gameObject.name}!");
                return;
            }
            
            _currentDamage = damage;
            
            // TDE uses MinDamageCaused and MaxDamageCaused for damage range
            // Set both to same value for consistent damage
            DamageOnTouch.MinDamageCaused = damage;
            DamageOnTouch.MaxDamageCaused = damage;
            
            _damageConfigured = true;
            
            Debug.Log($"[ProjectileDamageConfigurator] Projectile damage set to {damage}");
        }
        
        /// <summary>
        /// Set damage from a WeaponDataSO.
        /// Convenience method for direct integration.
        /// </summary>
        /// <param name="weaponData">Weapon data containing damage value</param>
        public void SetDamageFromWeaponData(WeaponDataSO weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogWarning($"[ProjectileDamageConfigurator] WeaponDataSO is null!");
                return;
            }
            
            SetDamage(weaponData.DamagePerShot);
        }
        
        /// <summary>
        /// Get current configured damage value.
        /// </summary>
        public float GetDamage()
        {
            return _currentDamage;
        }
        
        /// <summary>
        /// Check if damage has been configured.
        /// </summary>
        public bool IsDamageConfigured()
        {
            return _damageConfigured;
        }
        
        void OnValidate()
        {
            // Clamp base damage to minimum of 1
            if (BaseDamage < 1f)
            {
                BaseDamage = 1f;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Editor utility: Apply base damage in editor
        /// </summary>
        [ContextMenu("Apply Base Damage to DamageOnTouch")]
        void ApplyBaseDamageInEditor()
        {
            if (DamageOnTouch == null)
            {
                DamageOnTouch = GetComponent<DamageOnTouch>();
            }
            
            if (DamageOnTouch != null)
            {
                DamageOnTouch.MinDamageCaused = BaseDamage;
                DamageOnTouch.MaxDamageCaused = BaseDamage;
                Debug.Log($"[ProjectileDamageConfigurator] Applied base damage {BaseDamage} to DamageOnTouch in editor");
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogError("[ProjectileDamageConfigurator] DamageOnTouch component not found!");
            }
        }
#endif
    }
}
