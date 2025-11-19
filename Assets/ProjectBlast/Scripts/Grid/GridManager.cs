using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using ProjectBlast.Heroes;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// GridManager - Core grid system for tower defense gameplay.
    /// 
    /// RESPONSIBILITIES:
    /// - Manages three grid zones: Passive, Active, Firing
    /// - Tracks hero placement and occupancy
    /// - Provides spatial positioning and coordinate conversion
    /// - Handles placement/removal validation
    /// 
    /// TOPDOWN ENGINE INTEGRATION:
    /// - Uses MMSingleton pattern for global access
    /// - Compatible with TDE Weapon, Health, Feedback systems
    /// - Ready for MMEventManager integration
    /// 
    /// COORDINATE SYSTEM:
    /// - Row 0 = Front (closest to enemies at top)
    /// - Column 0 = Left side
    /// - Z-axis: Top (enemies) to Bottom (base)
    /// </summary>
    public class GridManager : MMSingleton<GridManager>
    {
        #region Configuration
        
        [Header("=== PASSIVE GRID CONFIGURATION ===")]
        [Tooltip("Number of rows in Passive grid (front to back)")]
        [Range(1, 5)]
        public int PassiveRows = 3;
        
        [Tooltip("Number of columns in Passive grid (left to right)")]
        [Range(1, 5)]
        public int PassiveColumns = 3;
        
        [Tooltip("Center position of Passive grid in world space")]
        public Vector3 PassiveGridCenter = new Vector3(0, 0, -6);
        
        [Header("=== ACTIVE GRID CONFIGURATION ===")]
        [Tooltip("Number of rows in Active grid (front to back)")]
        [Range(1, 5)]
        public int ActiveRows = 3;
        
        [Tooltip("Number of columns in Active grid (left to right)")]
        [Range(1, 5)]
        public int ActiveColumns = 3;
        
        [Tooltip("Center position of Active grid in world space")]
        public Vector3 ActiveGridCenter = new Vector3(0, 0, -3);
        
        [Header("=== FIRING GRID CONFIGURATION ===")]
        [Tooltip("Number of rows in Firing grid (front to back)")]
        [Range(1, 5)]
        public int FiringRows = 3;
        
        [Tooltip("Number of columns in Firing grid (left to right)")]
        [Range(1, 5)]
        public int FiringColumns = 3;
        
        [Tooltip("Center position of Firing grid in world space")]
        public Vector3 FiringGridCenter = new Vector3(0, 0, 0);
        
        [Header("=== CELL SPACING ===")]
        [Tooltip("Size of each grid cell (width/depth)")]
        [Range(0.5f, 5f)]
        public float CellSize = 1.5f;
        
        [Tooltip("Gap between cells")]
        [Range(0f, 1f)]
        public float CellSpacing = 0.3f;
        
        [Header("=== VISUAL DEBUG ===")]
        [Tooltip("Show grid gizmos in Scene view")]
        public bool ShowGridGizmos = true;
        
        [Tooltip("Show slot coordinate labels in Scene view")]
        public bool ShowSlotLabels = true;
        
        [Tooltip("Color for Passive grid gizmos")]
        public Color PassiveColor = Color.green;
        
        [Tooltip("Color for Active grid gizmos")]
        public Color ActiveColor = Color.yellow;
        
        [Tooltip("Color for Firing grid gizmos")]
        public Color FiringColor = Color.red;
        
        [Tooltip("Color for occupied slots")]
        public Color OccupiedColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange
        
        #endregion
        
        #region Private Fields
        
        private GridSlot[,] _passiveGrid;
        private GridSlot[,] _activeGrid;
        private GridSlot[,] _firingGrid;
        
        private bool _isInitialized = false;
        
        #endregion
        
        #region Initialization
        
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                InitializeGrids();
            }
        }
        
        /// <summary>
        /// Creates all grid zones with their slots
        /// </summary>
        public void InitializeGrids()
        {
            _passiveGrid = CreateGrid(GridZone.Passive, PassiveRows, PassiveColumns, PassiveGridCenter);
            
            _activeGrid = CreateGrid(GridZone.Active, ActiveRows, ActiveColumns, ActiveGridCenter);
            
            _firingGrid = CreateGrid(GridZone.Firing, FiringRows, FiringColumns, FiringGridCenter);
            
            _isInitialized = true;
            
            Debug.Log($"[GridManager] Initialized - " +
                      $"Passive: {PassiveRows}x{PassiveColumns}, " +
                      $"Active: {ActiveRows}x{ActiveColumns}, " +
                      $"Firing: {FiringRows}x{FiringColumns}");
        }
        
        /// <summary>
        /// Reinitializes grids when values change in editor
        /// </summary>
        void OnValidate()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Reinitialize grids when inspector values change
                _isInitialized = false;
                InitializeGrids();
            }
            #endif
        }
        
        /// <summary>
        /// Creates a 2D array of GridSlots for a zone
        /// </summary>
        private GridSlot[,] CreateGrid(GridZone zone, int rows, int cols, Vector3 center)
        {
            var grid = new GridSlot[rows, cols];
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 worldPos = CalculateSlotWorldPosition(row, col, rows, cols, center);
                    grid[row, col] = new GridSlot(zone, row, col, worldPos);
                }
            }
            
            return grid;
        }
        
        /// <summary>
        /// Calculates world position for a specific slot
        /// Row 0 = Front (positive Z offset from center)
        /// </summary>
        private Vector3 CalculateSlotWorldPosition(int row, int col, int totalRows, int totalCols, Vector3 center)
        {
            float totalCellSize = CellSize + CellSpacing;
            
            // Calculate X position (columns: left to right)
            float xOffset = (col - (totalCols - 1) / 2f) * totalCellSize;
            
            // Calculate Z position (rows: front to back)
            // Row 0 is front (higher Z), higher rows are back (lower Z)
            float zOffset = ((totalRows - 1) / 2f - row) * totalCellSize;
            
            // Use center as the base position
            return new Vector3(center.x + xOffset, center.y, center.z + zOffset);
        }
        
        #endregion
        
        #region Placement & Removal
        
        /// <summary>
        /// Places a hero in a specific slot
        /// </summary>
        public bool PlaceHero(Hero hero, GridSlot slot)
        {
            if (hero == null)
            {
                Debug.LogWarning("[GridManager] Cannot place null hero.");
                return false;
            }
            
            if (slot == null)
            {
                Debug.LogWarning("[GridManager] Cannot place hero in null slot.");
                return false;
            }
            
            if (slot.IsOccupied)
            {
                Debug.LogWarning($"[GridManager] Slot {slot.GetCoordinateLabel()} is already occupied by {slot.OccupyingHero.name}");
                return false;
            }
            
            // Remove hero from previous slot if it had one
            if (hero.CurrentGridSlot != null)
            {
                hero.CurrentGridSlot.Clear();
            }
            
            // Place hero in new slot (bidirectional reference)
            slot.OccupyingHero = hero;
            hero.CurrentGridSlot = slot;
            hero.transform.position = slot.WorldPosition;
            
            Debug.Log($"[GridManager] Placed {hero.name} at {slot.GetCoordinateLabel()}");
            return true;
        }
        
        /// <summary>
        /// Places a hero in a specific zone/row/column
        /// </summary>
        public bool PlaceHero(Hero hero, GridZone zone, int row, int col)
        {
            var slot = GetSlot(zone, row, col);
            if (slot == null)
            {
                Debug.LogWarning($"[GridManager] Invalid slot: {zone} [{row},{col}]");
                return false;
            }
            
            return PlaceHero(hero, slot);
        }
        
        /// <summary>
        /// Places a hero in the first available empty slot in a zone
        /// </summary>
        public bool TryPlaceHeroInZone(Hero hero, GridZone zone)
        {
            var emptySlots = GetEmptySlots(zone);
            
            if (emptySlots.Count == 0)
            {
                Debug.LogWarning($"[GridManager] No empty slots in {zone} zone");
                return false;
            }
            
            return PlaceHero(hero, emptySlots[0]);
        }
        
        /// <summary>
        /// Removes a hero from its current slot
        /// </summary>
        public bool RemoveHero(Hero hero)
        {
            if (hero == null)
            {
                Debug.LogWarning("[GridManager] Cannot remove null hero.");
                return false;
            }
            
            if (hero.CurrentGridSlot == null)
            {
                Debug.LogWarning($"[GridManager] Hero {hero.name} is not in any slot.");
                return false;
            }
            
            var slot = hero.CurrentGridSlot;
            slot.Clear();
            hero.CurrentGridSlot = null;
            
            Debug.Log($"[GridManager] Removed {hero.name} from {slot.GetCoordinateLabel()}");
            return true;
        }
        
        /// <summary>
        /// Removes hero from a specific zone/row/column
        /// </summary>
        public bool RemoveHero(GridZone zone, int row, int col)
        {
            var slot = GetSlot(zone, row, col);
            if (slot == null || slot.IsEmpty)
            {
                Debug.LogWarning($"[GridManager] No hero to remove at {zone} [{row},{col}]");
                return false;
            }
            
            return RemoveHero(slot.OccupyingHero);
        }
        
        #endregion
        
        #region Queries
        
        /// <summary>
        /// Gets a specific slot by zone/row/column
        /// </summary>
        public GridSlot GetSlot(GridZone zone, int row, int col)
        {
            var grid = GetGridArray(zone);
            
            if (grid == null)
            {
                Debug.LogWarning($"[GridManager] Grid not initialized for zone: {zone}");
                return null;
            }
            
            if (!IsValidPosition(zone, row, col))
            {
                Debug.LogWarning($"[GridManager] Invalid position: {zone} [{row},{col}]");
                return null;
            }
            
            return grid[row, col];
        }
        
        /// <summary>
        /// Gets the nearest empty slot to a world position
        /// </summary>
        public GridSlot GetNearestEmptySlot(GridZone zone, Vector3 worldPosition)
        {
            var emptySlots = GetEmptySlots(zone);
            
            if (emptySlots.Count == 0)
            {
                return null;
            }
            
            GridSlot nearest = emptySlots[0];
            float minDistance = Vector3.Distance(worldPosition, nearest.WorldPosition);
            
            for (int i = 1; i < emptySlots.Count; i++)
            {
                float distance = Vector3.Distance(worldPosition, emptySlots[i].WorldPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = emptySlots[i];
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Gets all slots in a zone
        /// </summary>
        public List<GridSlot> GetAllSlots(GridZone zone)
        {
            var grid = GetGridArray(zone);
            if (grid == null) return new List<GridSlot>();
            
            var slots = new List<GridSlot>();
            
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    slots.Add(grid[row, col]);
                }
            }
            
            return slots;
        }
        
        /// <summary>
        /// Gets all occupied slots in a zone
        /// </summary>
        public List<GridSlot> GetOccupiedSlots(GridZone zone)
        {
            return GetAllSlots(zone).FindAll(slot => slot.IsOccupied);
        }
        
        /// <summary>
        /// Gets all empty slots in a zone
        /// </summary>
        public List<GridSlot> GetEmptySlots(GridZone zone)
        {
            return GetAllSlots(zone).FindAll(slot => slot.IsEmpty);
        }
        
        /// <summary>
        /// Gets all heroes currently in a zone
        /// </summary>
        public List<Hero> GetAllHeroesInZone(GridZone zone)
        {
            var heroes = new List<Hero>();
            var occupiedSlots = GetOccupiedSlots(zone);
            
            foreach (var slot in occupiedSlots)
            {
                if (slot.OccupyingHero != null)
                {
                    heroes.Add(slot.OccupyingHero);
                }
            }
            
            return heroes;
        }
        
        /// <summary>
        /// Counts occupied slots in a zone
        /// </summary>
        public int GetOccupiedCount(GridZone zone)
        {
            return GetOccupiedSlots(zone).Count;
        }
        
        /// <summary>
        /// Counts empty slots in a zone
        /// </summary>
        public int GetEmptyCount(GridZone zone)
        {
            return GetEmptySlots(zone).Count;
        }
        
        #endregion
        
        #region Spatial Conversion
        
        /// <summary>
        /// Converts grid coordinates to world position
        /// </summary>
        public Vector3 GridToWorldPosition(GridZone zone, int row, int col)
        {
            var slot = GetSlot(zone, row, col);
            return slot?.WorldPosition ?? Vector3.zero;
        }
        
        /// <summary>
        /// Converts world position to nearest grid coordinates
        /// </summary>
        public (int row, int col) WorldToGridPosition(GridZone zone, Vector3 worldPosition)
        {
            var grid = GetGridArray(zone);
            if (grid == null) return (-1, -1);
            
            int nearestRow = 0;
            int nearestCol = 0;
            float minDistance = float.MaxValue;
            
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    float distance = Vector3.Distance(worldPosition, grid[row, col].WorldPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestRow = row;
                        nearestCol = col;
                    }
                }
            }
            
            return (nearestRow, nearestCol);
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Checks if a position is valid for a zone
        /// </summary>
        public bool IsValidPosition(GridZone zone, int row, int col)
        {
            var grid = GetGridArray(zone);
            if (grid == null) return false;
            
            return row >= 0 && row < grid.GetLength(0) && 
                   col >= 0 && col < grid.GetLength(1);
        }
        
        /// <summary>
        /// Checks if a slot is empty
        /// </summary>
        public bool IsSlotEmpty(GridZone zone, int row, int col)
        {
            var slot = GetSlot(zone, row, col);
            return slot != null && slot.IsEmpty;
        }
        
        /// <summary>
        /// Checks if a slot is occupied
        /// </summary>
        public bool IsSlotOccupied(GridZone zone, int row, int col)
        {
            var slot = GetSlot(zone, row, col);
            return slot != null && slot.IsOccupied;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Gets the grid array for a specific zone
        /// </summary>
        private GridSlot[,] GetGridArray(GridZone zone)
        {
            return zone switch
            {
                GridZone.Passive => _passiveGrid,
                GridZone.Active => _activeGrid,
                GridZone.Firing => _firingGrid,
                _ => null
            };
        }
        
        /// <summary>
        /// Gets row count for a zone
        /// </summary>
        public int GetRowCount(GridZone zone)
        {
            return zone switch
            {
                GridZone.Passive => PassiveRows,
                GridZone.Active => ActiveRows,
                GridZone.Firing => FiringRows,
                _ => 0
            };
        }
        
        /// <summary>
        /// Gets column count for a zone
        /// </summary>
        public int GetColumnCount(GridZone zone)
        {
            return zone switch
            {
                GridZone.Passive => PassiveColumns,
                GridZone.Active => ActiveColumns,
                GridZone.Firing => FiringColumns,
                _ => 0
            };
        }
        
        #endregion
        
        #region Visual Debug (Gizmos)
        
        void OnDrawGizmos()
        {
            if (!ShowGridGizmos) return;
            
            // Always reinitialize in editor when not playing to reflect inspector changes
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!_isInitialized)
                {
                    InitializeGrids();
                }
            }
            #endif
            
            if (_isInitialized)
            {
                DrawGridZone(GridZone.Passive, PassiveColor);
                DrawGridZone(GridZone.Active, ActiveColor);
                DrawGridZone(GridZone.Firing, FiringColor);
            }
        }
        
        void DrawGridZone(GridZone zone, Color zoneColor)
        {
            var slots = GetAllSlots(zone);
            
            foreach (var slot in slots)
            {
                // Draw wireframe cube for empty slots
                if (slot.IsEmpty)
                {
                    Gizmos.color = zoneColor;
                    Gizmos.DrawWireCube(slot.WorldPosition, Vector3.one * CellSize * 0.9f);
                }
                // Draw filled cube for occupied slots
                else
                {
                    Gizmos.color = OccupiedColor;
                    Gizmos.DrawCube(slot.WorldPosition, Vector3.one * CellSize * 0.3f);
                    
                    // Draw wireframe around occupied
                    Gizmos.color = zoneColor;
                    Gizmos.DrawWireCube(slot.WorldPosition, Vector3.one * CellSize * 0.9f);
                }
                
                // Draw slot labels
                #if UNITY_EDITOR
                if (ShowSlotLabels)
                {
                    UnityEditor.Handles.Label(
                        slot.WorldPosition + Vector3.up * 0.5f,
                        slot.GetCoordinateLabel(),
                        new GUIStyle()
                        {
                            normal = new GUIStyleState() { textColor = zoneColor },
                            fontSize = 10,
                            fontStyle = FontStyle.Bold
                        }
                    );
                }
                #endif
            }
        }
        
        #endregion
    }
}
