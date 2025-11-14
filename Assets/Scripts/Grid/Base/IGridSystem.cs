using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Core interface for all grid systems. Implement this for maximum portability.
    /// </summary>
    public interface IGridSystem
    {
        /// <summary>
        /// Grid dimensions (width, height)
        /// </summary>
        Vector2Int Dimensions { get; }
        
        /// <summary>
        /// Size of each cell in world units
        /// </summary>
        float CellSize { get; }
        
        /// <summary>
        /// Spacing between cells
        /// </summary>
        float CellSpacing { get; }
        
        /// <summary>
        /// Convert grid coordinates to world position
        /// </summary>
        Vector3 GridToWorld(Vector2Int gridPosition);
        
        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        Vector2Int WorldToGrid(Vector3 worldPosition);
        
        /// <summary>
        /// Check if grid position is within valid bounds
        /// </summary>
        bool IsValidPosition(Vector2Int gridPosition);
        
        /// <summary>
        /// Get cell at grid position (returns null if invalid)
        /// </summary>
        GridCell GetCell(Vector2Int gridPosition);
        
        /// <summary>
        /// Get all cells in the grid
        /// </summary>
        GridCell[] GetAllCells();
    }
}
