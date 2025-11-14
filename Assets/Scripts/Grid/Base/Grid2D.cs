using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Standard 2D grid implementation. Perfect for top-down games, tower defense, etc.
    /// Fully portable to any Unity project.
    /// </summary>
    public class Grid2D : GridSystem
    {
        [Header("2D Specific")]
        [SerializeField] private Vector3 gridRotation = Vector3.zero;
        
        /// <summary>
        /// Convert grid coordinates to world position (2D plane)
        /// </summary>
        public override Vector3 GridToWorld(Vector2Int gridPosition)
        {
            // Cell spacing adds gap between cells, doesn't scale the cell
            float cellStep = cellSize + cellSpacing;
            Vector3 originOffset = CalculateOriginOffset();
            
            Vector3 localPosition = new Vector3(
                gridPosition.x * cellStep,
                gridPosition.y * cellStep,
                0f
            );
            
            // Apply origin offset
            localPosition += originOffset;
            
            // Apply rotation if needed
            if (gridRotation != Vector3.zero)
            {
                localPosition = Quaternion.Euler(gridRotation) * localPosition;
            }
            
            // Convert to world space
            return transform.TransformPoint(localPosition);
        }
        
        /// <summary>
        /// Convert world position to grid coordinates (2D plane)
        /// </summary>
        public override Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // Convert to local space
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            
            // Apply inverse rotation if needed
            if (gridRotation != Vector3.zero)
            {
                localPosition = Quaternion.Euler(-gridRotation) * localPosition;
            }
            
            // Remove origin offset
            Vector3 originOffset = CalculateOriginOffset();
            localPosition -= originOffset;
            
            // Calculate grid coordinates (spacing affects distance between cell centers)
            float cellStep = cellSize + cellSpacing;
            int x = Mathf.RoundToInt(localPosition.x / cellStep);
            int y = Mathf.RoundToInt(localPosition.y / cellStep);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Get cell at screen position (for mobile tap input)
        /// </summary>
        public GridCell GetCellAtScreenPosition(Vector2 screenPosition, Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;
                
            Vector3 worldPos = camera.ScreenToWorldPoint(screenPosition);
            worldPos.z = transform.position.z; // Ensure Z matches grid plane
            
            return GetCellAtWorldPosition(worldPos);
        }
    }
}
