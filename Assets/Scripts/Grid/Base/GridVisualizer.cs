using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Debug visualizer for grid systems. Draws grid lines and cell information
    /// in the Unity editor. Essential for development and debugging.
    /// </summary>
    [RequireComponent(typeof(GridSystem))]
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private bool showInPlayMode = true;
        [SerializeField] private bool showCellCoordinates = false;
        [SerializeField] private bool showCellTypes = true;
        [SerializeField] private bool showOccupiedCells = true;
        
        [Header("Visual Style")]
        [SerializeField] private Color gridLineColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color blockedCellColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color buildableCellColor = new Color(0f, 1f, 0f, 0.2f);
        [SerializeField] private Color occupiedCellColor = new Color(1f, 1f, 0f, 0.4f);
        [SerializeField] private Color pathCellColor = new Color(0f, 0.5f, 1f, 0.3f);
        [SerializeField] private float cellFillAlpha = 0.2f;
        
        private GridSystem gridSystem;
        private GUIStyle labelStyle;
        
        private void Awake()
        {
            gridSystem = GetComponent<GridSystem>();
        }
        
        private void OnDrawGizmos()
        {
            if (!showGrid || (!showInPlayMode && Application.isPlaying))
                return;
                
            if (gridSystem == null)
                gridSystem = GetComponent<GridSystem>();
                
            if (gridSystem == null || !gridSystem.IsInitialized)
            {
                // Draw preview grid even if not initialized
                DrawPreviewGrid();
                return;
            }
            
            DrawGridLines();
            
            if (showCellTypes)
                DrawCellTypes();
                
            if (showOccupiedCells)
                DrawOccupiedCells();
        }
        
        private void DrawPreviewGrid()
        {
            // Draw a preview of what the grid will look like
            Gizmos.color = gridLineColor;
            
            Vector2Int dims = gridSystem.Dimensions;
            float cellSize = gridSystem.CellSize;
            float spacing = gridSystem.CellSpacing;
            float cellStep = cellSize + spacing;
            
            // Draw vertical lines
            for (int x = 0; x <= dims.x; x++)
            {
                Vector3 start = gridSystem.GridToWorld(new Vector2Int(x, 0));
                Vector3 end = gridSystem.GridToWorld(new Vector2Int(x, dims.y));
                Gizmos.DrawLine(start, end);
            }
            
            // Draw horizontal lines
            for (int y = 0; y <= dims.y; y++)
            {
                Vector3 start = gridSystem.GridToWorld(new Vector2Int(0, y));
                Vector3 end = gridSystem.GridToWorld(new Vector2Int(dims.x, y));
                Gizmos.DrawLine(start, end);
            }
        }
        
        private void DrawGridLines()
        {
            Gizmos.color = gridLineColor;
            GridCell[] cells = gridSystem.GetAllCells();
            
            foreach (var cell in cells)
            {
                DrawCellOutline(cell);
            }
        }
        
        private void DrawCellOutline(GridCell cell)
        {
            Vector3 center = cell.WorldPosition;
            float halfSize = gridSystem.CellSize * 0.5f;
            
            // Draw cell outline based on grid type
            if (gridSystem is Grid3D)
            {
                // 3D grid on X-Z plane
                Vector3 bl = center + new Vector3(-halfSize, 0, -halfSize);
                Vector3 br = center + new Vector3(halfSize, 0, -halfSize);
                Vector3 tr = center + new Vector3(halfSize, 0, halfSize);
                Vector3 tl = center + new Vector3(-halfSize, 0, halfSize);
                
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
                Gizmos.DrawLine(tl, bl);
            }
            else
            {
                // 2D grid or isometric on X-Y plane
                Vector3 bl = center + new Vector3(-halfSize, -halfSize, 0);
                Vector3 br = center + new Vector3(halfSize, -halfSize, 0);
                Vector3 tr = center + new Vector3(halfSize, halfSize, 0);
                Vector3 tl = center + new Vector3(-halfSize, halfSize, 0);
                
                Gizmos.DrawLine(bl, br);
                Gizmos.DrawLine(br, tr);
                Gizmos.DrawLine(tr, tl);
                Gizmos.DrawLine(tl, bl);
            }
        }
        
        private void DrawCellTypes()
        {
            GridCell[] cells = gridSystem.GetAllCells();
            
            foreach (var cell in cells)
            {
                Color cellColor = GetCellTypeColor(cell.Type);
                
                if (cellColor.a > 0)
                {
                    Gizmos.color = cellColor;
                    DrawFilledCell(cell);
                }
            }
        }
        
        private void DrawOccupiedCells()
        {
            GridCell[] cells = gridSystem.GetAllCells();
            
            foreach (var cell in cells)
            {
                if (cell.IsOccupied)
                {
                    Gizmos.color = occupiedCellColor;
                    DrawFilledCell(cell);
                }
            }
        }
        
        private void DrawFilledCell(GridCell cell)
        {
            Vector3 center = cell.WorldPosition;
            float size = gridSystem.CellSize * 0.9f; // Slightly smaller than cell
            
            if (gridSystem is Grid3D)
            {
                Gizmos.DrawCube(center, new Vector3(size, 0.1f, size));
            }
            else
            {
                Gizmos.DrawCube(center, new Vector3(size, size, 0.1f));
            }
        }
        
        private Color GetCellTypeColor(CellType type)
        {
            switch (type)
            {
                case CellType.Blocked:
                    return blockedCellColor;
                case CellType.Buildable:
                    return buildableCellColor;
                case CellType.Path:
                    return pathCellColor;
                default:
                    return new Color(0, 0, 0, 0); // Transparent
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showCellCoordinates || gridSystem == null || !gridSystem.IsInitialized)
                return;
                
            // Draw cell coordinates
            GridCell[] cells = gridSystem.GetAllCells();
            
            foreach (var cell in cells)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    cell.WorldPosition,
                    $"({cell.GridPosition.x}, {cell.GridPosition.y})",
                    GetLabelStyle()
                );
#endif
            }
        }
        
        private GUIStyle GetLabelStyle()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontSize = 10;
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }
            return labelStyle;
        }
    }
}
