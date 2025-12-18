using UnityEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Events
{
    /// <summary>
    /// Event triggered when a hero's ammo is depleted.
    /// Follows TDE MMEventManager pattern for complete decoupling.
    /// 
    /// ALTERNATIVE TO INTERFACE-BASED APPROACH:
    /// - More TDE-idiomatic (events are TDE's primary communication pattern)
    /// - Zero coupling between CombatCoordinator and HeroAmmo
    /// - Allows multiple systems to react to ammo depletion (UI, sound, etc.)
    /// 
    /// USAGE (If you want to switch to event-based):
    /// 1. CombatCoordinator triggers: HeroAmmoDepletionEvent.Trigger(hero, 0);
    /// 2. HeroAmmo listens: implements MMEventListener<HeroAmmoDepletionEvent>
    /// 3. UI/Sound can also listen to same event
    /// 
    /// See REFLECTION_ELIMINATION_GUIDE.md for full implementation details.
    /// </summary>
    public struct HeroAmmoDepletionEvent
    {
        /// <summary>
        /// The hero whose ammo was depleted
        /// </summary>
        public GameObject Hero;
        
        /// <summary>
        /// Remaining ammo (should be 0 for depletion events)
        /// </summary>
        public int RemainingAmmo;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public HeroAmmoDepletionEvent(GameObject hero, int remainingAmmo)
        {
            Hero = hero;
            RemainingAmmo = remainingAmmo;
        }
        
        static HeroAmmoDepletionEvent e;
        
        /// <summary>
        /// Triggers a hero ammo depletion event
        /// </summary>
        /// <param name="hero">The hero whose ammo was depleted</param>
        /// <param name="remainingAmmo">Remaining ammo count (typically 0)</param>
        public static void Trigger(GameObject hero, int remainingAmmo)
        {
            e.Hero = hero;
            e.RemainingAmmo = remainingAmmo;
            MMEventManager.TriggerEvent(e);
        }
    }
    
    /// <summary>
    /// Event triggered when a hero's ammo reaches low threshold.
    /// Useful for UI warnings, sound effects, visual feedback.
    /// </summary>
    public struct HeroAmmoLowEvent
    {
        public GameObject Hero;
        public int RemainingAmmo;
        public int Threshold;
        
        public HeroAmmoLowEvent(GameObject hero, int remainingAmmo, int threshold)
        {
            Hero = hero;
            RemainingAmmo = remainingAmmo;
            Threshold = threshold;
        }
        
        static HeroAmmoLowEvent e;
        
        public static void Trigger(GameObject hero, int remainingAmmo, int threshold)
        {
            e.Hero = hero;
            e.RemainingAmmo = remainingAmmo;
            e.Threshold = threshold;
            MMEventManager.TriggerEvent(e);
        }
    }
}
