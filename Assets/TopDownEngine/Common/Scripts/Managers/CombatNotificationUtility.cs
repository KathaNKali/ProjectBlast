using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Utility class for notifying CombatCoordinator about weapon firing events.
	/// Centralizes the logic for getting target from AIBrain and notifying coordinator.
	/// 
	/// USAGE:
	/// - Call from Weapon.ShootRequest() after ammo consumption
	/// - Call from WeaponAmmo.ConsumeAmmo() after ammo consumption
	/// - Automatically handles target lookup from Character's AIBrain
	/// 
	/// PATTERN:
	/// - Follows TDE pattern of static utility classes for cross-component communication
	/// - Single source of truth eliminates code duplication
	/// - Easy to extend (e.g., add MMFeedbacks, events, logging)
	/// </summary>
	public static class CombatNotificationUtility
	{
		/// <summary>
		/// Notifies CombatCoordinator that a weapon fired a bullet.
		/// Automatically retrieves the target from the Character's AIBrain component.
		/// </summary>
		/// <param name="owner">The weapon owner (Character component)</param>
		public static void NotifyBulletFired(Character owner)
		{
			if (owner == null) return;
			if (!CombatCoordinator.HasInstance) return;
			
			GameObject target = GetCurrentTarget(owner);
			if (target != null)
			{
				CombatCoordinator.Instance.OnHeroFiredBullet(owner.gameObject, target);
			}
		}
		
		/// <summary>
		/// Gets the current target from a Character's AIBrain component.
		/// Searches in children to support common TDE hierarchy (Character → AI GameObject → AIBrain).
		/// </summary>
		/// <param name="character">The character to get the target from</param>
		/// <returns>Target GameObject if found and valid, null otherwise</returns>
		private static GameObject GetCurrentTarget(Character character)
		{
			if (character == null) return null;
			
			// Get AIBrain from children (standard TDE AI setup)
			var aiBrain = character.GetComponentInChildren<AIBrain>();
			if (aiBrain != null && aiBrain.Target != null)
			{
				return aiBrain.Target.gameObject;
			}
			
			return null;
		}
	}
}
