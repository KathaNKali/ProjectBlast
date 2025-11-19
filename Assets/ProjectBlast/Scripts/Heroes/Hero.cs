using UnityEngine;
using ProjectBlast.Grid;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// Base Hero class - represents a deployable combat unit.
    /// Heroes are placed on grids and auto-fire at enemies.
    /// 
    /// This is a minimal stub for GridManager integration.
    /// Full implementation will include TDE Weapon, Health, and combat logic.
    /// </summary>
    public class Hero : MonoBehaviour
    {
        [Header("Grid Integration")]
        [Tooltip("Current slot this hero occupies (managed by GridManager)")]
        public GridSlot CurrentGridSlot;
        
        [Header("Hero Identity")]
        public string HeroName = "Hero";
        
        /// <summary>
        /// Whether this hero is currently in the Firing zone and can attack
        /// </summary>
        public bool IsInFiringZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Firing;
        
        /// <summary>
        /// Whether this hero is in the Active zone and ready for deployment
        /// </summary>
        public bool IsInActiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Active;
        
        /// <summary>
        /// Whether this hero is in the Passive zone and waiting in queue
        /// </summary>
        public bool IsInPassiveZone => CurrentGridSlot != null && CurrentGridSlot.Zone == GridZone.Passive;
        
        void Start()
        {
            if (string.IsNullOrEmpty(HeroName))
            {
                HeroName = gameObject.name;
            }
        }
    }
}
