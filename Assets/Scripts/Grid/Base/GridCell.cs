using UnityEngine;
using System;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Represents a single cell in the grid. Highly flexible and data-driven.
    /// Can be extended for game-specific needs.
    /// </summary>
    [Serializable]
    public class GridCell
    {
        // Core positioning
        public Vector2Int GridPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        
        // Cell state
        public CellType Type { get; set; }
        public bool IsOccupied => OccupyingEntity != null;
        public bool IsWalkable => Type != CellType.Blocked && !IsOccupied;
        
        // Entity reference (generic object for max flexibility)
        public object OccupyingEntity { get; private set; }
        
        // Pathfinding support
        public float MovementCost { get; set; }
        public int PathfindingWeight { get; set; }
        
        // Metadata (extensible for custom data)
        public object CustomData { get; set; }
        
        // Events
        public event Action<GridCell, object> OnOccupied;
        public event Action<GridCell, object> OnCleared;
        public event Action<GridCell, CellType, CellType> OnTypeChanged;
        
        public GridCell(Vector2Int gridPosition, Vector3 worldPosition)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
            Type = CellType.Empty;
            MovementCost = 1f;
            PathfindingWeight = 1;
        }
        
        /// <summary>
        /// Place an entity in this cell
        /// </summary>
        public bool Occupy(object entity)
        {
            if (IsOccupied || entity == null)
                return false;
                
            OccupyingEntity = entity;
            OnOccupied?.Invoke(this, entity);
            return true;
        }
        
        /// <summary>
        /// Remove entity from this cell
        /// </summary>
        public object Clear()
        {
            if (!IsOccupied)
                return null;
                
            object entity = OccupyingEntity;
            OccupyingEntity = null;
            OnCleared?.Invoke(this, entity);
            return entity;
        }
        
        /// <summary>
        /// Change cell type with event notification
        /// </summary>
        public void SetType(CellType newType)
        {
            if (Type == newType)
                return;
                
            CellType oldType = Type;
            Type = newType;
            OnTypeChanged?.Invoke(this, oldType, newType);
        }
        
        /// <summary>
        /// Update world position (useful for dynamic grids)
        /// </summary>
        public void UpdateWorldPosition(Vector3 newWorldPosition)
        {
            WorldPosition = newWorldPosition;
        }
    }
    
    /// <summary>
    /// Cell type enumeration. Extend this for your game-specific needs.
    /// </summary>
    public enum CellType
    {
        Empty,          // Default empty cell
        Blocked,        // Cannot place or walk through
        Buildable,      // Can place towers/structures
        Path,           // Enemy movement path
        SpawnZone,      // Entity spawn area
        GoalZone,       // Objective area
        Highlight,      // UI/visual state
        Custom1,        // Game-specific types
        Custom2,
        Custom3
    }
}
