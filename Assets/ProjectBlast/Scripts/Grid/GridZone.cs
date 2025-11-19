using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Defines the three grid zones in the game.
    /// Heroes progress through these zones: Passive → Active → Firing
    /// </summary>
    public enum GridZone
    {
        /// <summary>
        /// Passive zone - Heroes waiting in queue at the bottom
        /// Auto-shifts heroes upward to Active zone
        /// </summary>
        Passive,
        
        /// <summary>
        /// Active zone - Heroes ready for player deployment
        /// Player taps heroes here to deploy to Firing zone
        /// </summary>
        Active,
        
        /// <summary>
        /// Firing zone - Heroes actively attacking enemies
        /// Heroes in this zone auto-fire at enemies approaching from above
        /// </summary>
        Firing
    }
}
