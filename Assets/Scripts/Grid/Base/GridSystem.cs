using UnityEngine;
using System;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Abstract base class for all grid systems. Provides core functionality
    /// while allowing derived classes to implement specific coordinate transformations.
    /// Highly portable and extensible.
    /// </summary>
    public abstract class GridSystem : MonoBehaviour, IGridSystem
    {
        [Header("Grid Dimensions")]
        [SerializeField] protected int width = 10;
        [SerializeField] protected int height = 10;
        
        [Header("Cell Settings")]
        [SerializeField] protected float cellSize = 1f;
        [SerializeField] protected float cellSpacing = 0f;
        
        [Header("Grid Origin")]
        [SerializeField] protected GridOrigin originMode = GridOrigin.Center;
        [SerializeField] [Tooltip("If true, changing cell spacing keeps the first cell's position fixed")]
        protected bool lockOriginWhenSpacing = false;
        
        // Grid data
        protected GridCell[,] cells;
        protected bool isInitialized;
        
        // Properties
        public Vector2Int Dimensions => new Vector2Int(width, height);
        public float CellSize => cellSize;
        public float CellSpacing => cellSpacing;
        public bool IsInitialized => isInitialized;
        
        // Events
        public event Action<GridSystem> OnGridInitialized;
        public event Action<GridCell> OnCellOccupied;
        public event Action<GridCell> OnCellCleared;
        
        protected virtual void Awake()
        {
            InitializeGrid();
        }
        
        /// <summary>
        /// Initialize the grid structure
        /// </summary>
        public virtual void InitializeGrid()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Grid already initialized!");
                return;
            }
            
            cells = new GridCell[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorld(gridPos);
                    
                    cells[x, y] = CreateCell(gridPos, worldPos);
                    RegisterCellEvents(cells[x, y]);
                }
            }
            
            isInitialized = true;
            OnGridInitialized?.Invoke(this);
        }
        
        /// <summary>
        /// Create a cell at the given position. Override for custom cell types.
        /// </summary>
        protected virtual GridCell CreateCell(Vector2Int gridPosition, Vector3 worldPosition)
        {
            return new GridCell(gridPosition, worldPosition);
        }
        
        /// <summary>
        /// Register cell-level events to grid-level events
        /// </summary>
        protected virtual void RegisterCellEvents(GridCell cell)
        {
            cell.OnOccupied += (c, entity) => OnCellOccupied?.Invoke(c);
            cell.OnCleared += (c, entity) => OnCellCleared?.Invoke(c);
        }
        
        /// <summary>
        /// Abstract method: Convert grid coordinates to world position.
        /// Implement this for 2D, 3D, Isometric, Hex, etc.
        /// </summary>
        public abstract Vector3 GridToWorld(Vector2Int gridPosition);
        
        /// <summary>
        /// Abstract method: Convert world position to grid coordinates.
        /// Implement this for 2D, 3D, Isometric, Hex, etc.
        /// </summary>
        public abstract Vector2Int WorldToGrid(Vector3 worldPosition);
        
        /// <summary>
        /// Check if grid position is within bounds
        /// </summary>
        public virtual bool IsValidPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width &&
                   gridPosition.y >= 0 && gridPosition.y < height;
        }
        
        /// <summary>
        /// Get cell at grid position
        /// </summary>
        public virtual GridCell GetCell(Vector2Int gridPosition)
        {
            if (!IsValidPosition(gridPosition))
                return null;
                
            return cells[gridPosition.x, gridPosition.y];
        }
        
        /// <summary>
        /// Get cell at world position
        /// </summary>
        public virtual GridCell GetCellAtWorldPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);
            return GetCell(gridPos);
        }
        
        /// <summary>
        /// Get all cells in the grid
        /// </summary>
        public virtual GridCell[] GetAllCells()
        {
            if (!isInitialized)
                return new GridCell[0];
                
            GridCell[] allCells = new GridCell[width * height];
            int index = 0;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    allCells[index++] = cells[x, y];
                }
            }
            
            return allCells;
        }
        
        /// <summary>
        /// Get cells in a rectangular area
        /// </summary>
        public virtual GridCell[] GetCellsInArea(Vector2Int bottomLeft, Vector2Int topRight)
        {
            var cellList = new System.Collections.Generic.List<GridCell>();
            
            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (IsValidPosition(pos))
                    {
                        cellList.Add(cells[x, y]);
                    }
                }
            }
            
            return cellList.ToArray();
        }
        
        /// <summary>
        /// Get cells within radius (Manhattan distance)
        /// </summary>
        public virtual GridCell[] GetCellsInRadius(Vector2Int center, int radius)
        {
            var cellList = new System.Collections.Generic.List<GridCell>();
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= radius)
                    {
                        Vector2Int pos = center + new Vector2Int(x, y);
                        if (IsValidPosition(pos))
                        {
                            cellList.Add(cells[pos.x, pos.y]);
                        }
                    }
                }
            }
            
            return cellList.ToArray();
        }
        
        /// <summary>
        /// Set cell type for single cell
        /// </summary>
        public virtual void SetCellType(Vector2Int gridPosition, CellType type)
        {
            GridCell cell = GetCell(gridPosition);
            cell?.SetType(type);
        }
        
        /// <summary>
        /// Set cell type for area
        /// </summary>
        public virtual void SetCellTypeInArea(Vector2Int bottomLeft, Vector2Int topRight, CellType type)
        {
            GridCell[] cells = GetCellsInArea(bottomLeft, topRight);
            foreach (var cell in cells)
            {
                cell.SetType(type);
            }
        }
        
        /// <summary>
        /// Clear the grid (remove all entities)
        /// </summary>
        public virtual void ClearGrid()
        {
            if (!isInitialized)
                return;
                
            foreach (var cell in cells)
            {
                cell.Clear();
            }
        }
        
        /// <summary>
        /// Reset the grid (destroy and reinitialize)
        /// </summary>
        public virtual void ResetGrid()
        {
            isInitialized = false;
            cells = null;
            InitializeGrid();
        }
        
        /// <summary>
        /// Calculate origin offset based on origin mode
        /// </summary>
        protected Vector3 CalculateOriginOffset()
        {
            float cellStep = cellSize + cellSpacing;
            
            if (lockOriginWhenSpacing)
            {
                // Keep the origin locked - first cell always at same position
                // regardless of spacing (grid expands outward)
                switch (originMode)
                {
                    case GridOrigin.BottomLeft:
                        return new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                        
                    case GridOrigin.Center:
                        return new Vector3(
                            -(width - 1) * cellStep * 0.5f,
                            -(height - 1) * cellStep * 0.5f,
                            0f
                        );
                        
                    case GridOrigin.TopLeft:
                        return new Vector3(cellSize * 0.5f, -(height - 1) * cellStep - cellSize * 0.5f, 0f);
                        
                    default:
                        return Vector3.zero;
                }
            }
            else
            {
                // Standard behavior - grid bounds expand equally in all directions
                // when spacing increases (keeps grid visually centered)
                float totalWidth = (width - 1) * cellStep + cellSize;
                float totalHeight = (height - 1) * cellStep + cellSize;
                
                switch (originMode)
                {
                    case GridOrigin.BottomLeft:
                        return new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0f);
                        
                    case GridOrigin.Center:
                        return new Vector3(-totalWidth * 0.5f + cellSize * 0.5f,
                                           -totalHeight * 0.5f + cellSize * 0.5f,
                                           0f);
                        
                    case GridOrigin.TopLeft:
                        return new Vector3(cellSize * 0.5f, -totalHeight + cellSize * 0.5f, 0f);
                        
                    default:
                        return Vector3.zero;
                }
            }
        }
    }
    
    /// <summary>
    /// Grid origin modes for different coordinate systems
    /// </summary>
    public enum GridOrigin
    {
        BottomLeft,     // (0,0) at bottom-left
        Center,         // (0,0) at center
        TopLeft         // (0,0) at top-left
    }
}
