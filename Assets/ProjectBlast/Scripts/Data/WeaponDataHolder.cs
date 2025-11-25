using UnityEngine;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Data
{
    /// <summary>
    /// Component to attach WeaponDataSO to a Weapon prefab.
    /// This allows the weapon prefab to carry its data configuration.
    /// 
    /// USAGE:
    /// 1. Add this component to your Weapon prefab
    /// 2. Assign the WeaponDataSO
    /// 3. WeaponDataSO will auto-apply on Awake
    /// </summary>
    [RequireComponent(typeof(Weapon))]
    public class WeaponDataHolder : MonoBehaviour
    {
        [Header("Weapon Configuration")]
        [Tooltip("Weapon data to apply to this weapon")]
        public WeaponDataSO WeaponData;
        
        [Tooltip("Auto-apply weapon data on Awake")]
        public bool ApplyOnAwake = true;
        
        private Weapon _weapon;
        
        void Awake()
        {
            _weapon = GetComponent<Weapon>();
            
            if (ApplyOnAwake && WeaponData != null)
            {
                ApplyWeaponData();
            }
        }
        
        /// <summary>
        /// Apply weapon data to the weapon component
        /// </summary>
        public void ApplyWeaponData()
        {
            if (WeaponData == null)
            {
                Debug.LogWarning($"[WeaponDataHolder] No WeaponData assigned on {gameObject.name}!");
                return;
            }
            
            if (_weapon == null)
            {
                _weapon = GetComponent<Weapon>();
            }
            
            WeaponData.ApplyToWeapon(_weapon);
        }
        
        /// <summary>
        /// Get ammo consumption rate for this weapon
        /// </summary>
        public int GetAmmoPerShot()
        {
            return WeaponData != null ? WeaponData.AmmoPerShot : 1;
        }
        
        /// <summary>
        /// Get damage per shot for this weapon
        /// </summary>
        public float GetDamagePerShot()
        {
            return WeaponData != null ? WeaponData.DamagePerShot : 10f;
        }
    }
}
