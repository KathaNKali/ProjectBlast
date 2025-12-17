using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Event triggered when a hero's ammo count changes.
	/// Follows TDE MMEventManager pattern for decoupled communication.
	/// 
	/// USAGE:
	/// - CombatCoordinator triggers this event when hero fires a bullet
	/// - Hero listens to this event and updates cached ammo count
	/// - Eliminates polling CombatCoordinator.GetHeroAmmo() every frame
	/// 
	/// PATTERN:
	/// - Event-driven updates instead of polling
	/// - Reduces CPU overhead from repeated dictionary lookups
	/// - Follows TDE architecture for cross-component communication
	/// </summary>
	public struct MMAmmoEvent
	{
		/// <summary>
		/// The hero whose ammo changed
		/// </summary>
		public GameObject Hero;
		
		/// <summary>
		/// Current ammo count after the change
		/// </summary>
		public int CurrentAmmo;
		
		/// <summary>
		/// Maximum ammo capacity for this hero
		/// </summary>
		public int MaxAmmo;
		
		/// <summary>
		/// Whether this hero has unlimited ammo
		/// </summary>
		public bool UnlimitedAmmo;
		
		/// <summary>
		/// Constructor for creating ammo change events
		/// </summary>
		public MMAmmoEvent(GameObject hero, int currentAmmo, int maxAmmo, bool unlimitedAmmo)
		{
			Hero = hero;
			CurrentAmmo = currentAmmo;
			MaxAmmo = maxAmmo;
			UnlimitedAmmo = unlimitedAmmo;
		}
		
		static MMAmmoEvent e;
		
		/// <summary>
		/// Triggers an ammo change event
		/// </summary>
		/// <param name="hero">The hero whose ammo changed</param>
		/// <param name="currentAmmo">Current ammo count</param>
		/// <param name="maxAmmo">Maximum ammo capacity</param>
		/// <param name="unlimitedAmmo">Whether hero has unlimited ammo</param>
		public static void Trigger(GameObject hero, int currentAmmo, int maxAmmo, bool unlimitedAmmo)
		{
			e.Hero = hero;
			e.CurrentAmmo = currentAmmo;
			e.MaxAmmo = maxAmmo;
			e.UnlimitedAmmo = unlimitedAmmo;
			MMEventManager.TriggerEvent(e);
		}
	}
}
