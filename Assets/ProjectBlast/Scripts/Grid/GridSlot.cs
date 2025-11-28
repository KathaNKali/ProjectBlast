using UnityEngine;
using ProjectBlast.Heroes;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Represents a single slot in a grid zone.
    /// Tracks position, occupancy, and state information.
    /// </summary>
    [System.Serializable]
    public class GridSlot
    {
        // ===== IDENTITY =====
        /// <summary>Which grid zone this slot belongs to</summary>
        [SerializeField] private GridZone _zone;
        public GridZone Zone { get => _zone; private set => _zone = value; }
        
        /// <summary>Row index (0 = front/closest to enemies, higher = back)</summary>
        [SerializeField] private int _row;
        public int Row { get => _row; private set => _row = value; }
        
        /// <summary>Column index (0 = left, higher = right)</summary>
        [SerializeField] private int _column;
        public int Column { get => _column; private set => _column = value; }
        
        // ===== SPATIAL =====
        /// <summary>World space position of this slot's center</summary>
        public Vector3 WorldPosition { get; private set; }
        
        // ===== OCCUPANCY =====
        /// <summary>Hero currently occupying this slot (null if empty)</summary>
        public Hero OccupyingHero { get; set; }
        
        /// <summary>Whether this slot is currently occupied by a hero</summary>
        public bool IsOccupied => OccupyingHero != null;
        
        /// <summary>Whether this slot is empty and available</summary>
        public bool IsEmpty => OccupyingHero == null;
        
        // ===== CONSTRUCTOR =====
        public GridSlot(GridZone zone, int row, int column, Vector3 worldPosition)
        {
            Zone = zone;
            Row = row;
            Column = column;
            WorldPosition = worldPosition;
            OccupyingHero = null;
        }
        
        // ===== UTILITY =====
        /// <summary>
        /// Clears the slot, removing hero reference.
        /// Does NOT move/destroy the hero object.
        /// </summary>
        public void Clear()
        {
            OccupyingHero = null;
        }
        
        /// <summary>
        /// Returns a formatted string for debugging
        /// </summary>
        public override string ToString()
        {
            string occupancyStatus = IsOccupied ? $"Occupied by {OccupyingHero.name}" : "Empty";
            return $"{Zone} Grid [{Row},{Column}] - {occupancyStatus}";
        }
        
        /// <summary>
        /// Returns coordinate identifier (e.g., "F[0,1]" for Firing grid row 0, col 1)
        /// </summary>
        public string GetCoordinateLabel()
        {
            char zonePrefix = Zone switch
            {
                GridZone.Passive => 'P',
                GridZone.Active => 'A',
                GridZone.Firing => 'F',
                _ => '?'
            };
            return $"{zonePrefix}[{Row},{Column}]";
        }
    }
}
