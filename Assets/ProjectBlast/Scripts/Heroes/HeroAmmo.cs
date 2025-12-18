using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// CharacterAbility for managing hero ammo state.
    /// Follows TDE pattern: abilities are discovered via Character.FindAbility<T>()
    /// 
    /// Implements IAmmoDepletable for type-safe callbacks (no reflection needed).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Hero Ammo")]
    public class HeroAmmo : CharacterAbility, IAmmoDepletable
    {
        [Header("Ammo Configuration")]
        [Tooltip("Low ammo threshold for warnings")]
        public int LowAmmoThreshold = 20;
        
        [Header("Runtime State")]
        [Tooltip("Current ammo count")]
        public int CurrentAmmo;
        
        [Tooltip("Maximum ammo capacity")]
        public int MaxAmmo;
        
        [Tooltip("Whether this hero has unlimited ammo")]
        public bool UnlimitedAmmo;
        
        private Hero _hero;
        private bool _hasTriggeredLowAmmoWarning;
        
        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            _hero = GetComponent<Hero>();
        }
        
        /// <summary>
        /// Initialize ammo values (called by Hero on start)
        /// </summary>
        /// <param name="maxAmmo">Maximum ammo</param>
        /// <param name="unlimited">Whether ammo is unlimited</param>
        public virtual void InitializeAmmo(int maxAmmo, bool unlimited)
        {
            MaxAmmo = maxAmmo;
            CurrentAmmo = maxAmmo;
            UnlimitedAmmo = unlimited;
            _hasTriggeredLowAmmoWarning = false;
        }
        
        /// <summary>
        /// Called when ammo changes (from CombatCoordinator via MMAmmoEvent or direct call)
        /// </summary>
        /// <param name="currentAmmo">New ammo count</param>
        /// <param name="maxAmmo">Max ammo (for updates)</param>
        public virtual void OnAmmoChanged(int currentAmmo, int maxAmmo)
        {
            CurrentAmmo = currentAmmo;
            MaxAmmo = maxAmmo;
            
            // Check for low ammo warning (trigger once per depletion cycle)
            if (CurrentAmmo <= LowAmmoThreshold && CurrentAmmo > 0 && !_hasTriggeredLowAmmoWarning)
            {
                OnAmmoLow();
                _hasTriggeredLowAmmoWarning = true;
            }
        }
        
        /// <summary>
        /// Called when ammo is low (TDE callback pattern)
        /// </summary>
        public virtual void OnAmmoLow()
        {
            if (_hero != null)
            {
                Debug.Log($"[HeroAmmo] {_hero.HeroName} low ammo warning! {CurrentAmmo}/{MaxAmmo} remaining");
            }
            
            // Trigger visual/audio feedback here
            PlayAbilityStartFeedbacks();
        }
        
        /// <summary>
        /// Called when ammo is depleted (TDE callback pattern)
        /// </summary>
        public virtual void OnAmmoDepletion()
        {
            CurrentAmmo = 0;
            
            if (_hero != null)
            {
                Debug.Log($"[HeroAmmo] {_hero.HeroName} OUT OF AMMO! Initiating removal...");
                _hero.OnAmmoDepletion();
            }
            
            // Trigger depletion feedback
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
        }
        
        /// <summary>
        /// Checks if hero has ammo remaining
        /// </summary>
        public virtual bool HasAmmo()
        {
            return UnlimitedAmmo || CurrentAmmo > 0;
        }
        
        /// <summary>
        /// Gets current ammo count
        /// </summary>
        public virtual int GetCurrentAmmo()
        {
            return UnlimitedAmmo ? int.MaxValue : CurrentAmmo;
        }
        
        /// <summary>
        /// Gets the low ammo threshold (IAmmoDepletable interface)
        /// </summary>
        public virtual int GetLowAmmoThreshold()
        {
            return LowAmmoThreshold;
        }
    }
}
