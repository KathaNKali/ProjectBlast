using UnityEngine;

namespace ProjectBlast.Interfaces
{
    /// <summary>
    /// Interface for components that can have ammo depleted.
    /// Eliminates reflection-based method invocation in CombatCoordinator.
    /// 
    /// TDE PATTERN:
    /// - Type-safe interface instead of reflection
    /// - FindAbility returns typed interface
    /// - Direct method calls (0 overhead vs. ~1000x reflection overhead)
    /// 
    /// USAGE:
    /// 1. HeroAmmo implements IAmmoDepletable
    /// 2. CombatCoordinator uses: FindAbility<IAmmoDepletable>()?.OnAmmoDepletion()
    /// 3. No reflection, no strings, no runtime errors
    /// </summary>
    public interface IAmmoDepletable
    {
        /// <summary>
        /// Called when ammo reaches zero
        /// </summary>
        void OnAmmoDepletion();
        
        /// <summary>
        /// Called when ammo reaches low threshold
        /// </summary>
        void OnAmmoLow();
        
        /// <summary>
        /// Gets the low ammo threshold value
        /// </summary>
        int GetLowAmmoThreshold();
    }
}
