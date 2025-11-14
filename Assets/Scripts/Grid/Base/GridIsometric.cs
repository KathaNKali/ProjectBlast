using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Isometric grid implementation. Perfect for isometric tower defense,
    /// strategy games, or any isometric perspective game.
    /// </summary>
    public class GridIsometric : GridSystem
    {
        [Header("Isometric Settings")]
        [SerializeField] private float isoAngle = 30f; // Standard isometric angle
        [SerializeField] private bool useClassicIso = true; // 2:1 ratio
        
        /// <summary>
        /// Convert grid coordinates to isometric world position
        /// </summary>
        public override Vector3 GridToWorld(Vector2Int gridPosition)
        {
            // Cell spacing adds gap between cells
            float cellStep = cellSize + cellSpacing;
            Vector3 originOffset = CalculateOriginOffset();
            
            Vector3 localPosition;
            
            if (useClassicIso)
            {
                // Classic 2:1 isometric projection
                localPosition = new Vector3(
                    (gridPosition.x - gridPosition.y) * cellStep * 0.5f,
                    (gridPosition.x + gridPosition.y) * cellStep * 0.25f,
                    0f
                );
            }
            else
            {
                // Angle-based isometric projection
                float angleRad = isoAngle * Mathf.Deg2Rad;
                float isoRatio = Mathf.Tan(angleRad);
                
                localPosition = new Vector3(
                    (gridPosition.x - gridPosition.y) * cellStep * 0.5f,
                    (gridPosition.x + gridPosition.y) * cellStep * isoRatio,
                    0f
                );
            }
            
            // Apply origin offset
            localPosition += originOffset;
            
            // Convert to world space
            return transform.TransformPoint(localPosition);
        }
        
        /// <summary>
        /// Convert world position to isometric grid coordinates
        /// </summary>
        public override Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // Convert to local space
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            
            // Remove origin offset
            Vector3 originOffset = CalculateOriginOffset();
            localPosition -= originOffset;
            
            float cellStep = cellSize + cellSpacing;
            int x, y;
            
            if (useClassicIso)
            {
                // Inverse classic 2:1 isometric projection
                float isoX = localPosition.x / (cellStep * 0.5f);
                float isoY = localPosition.y / (cellStep * 0.25f);
                
                x = Mathf.RoundToInt((isoX + isoY) * 0.5f);
                y = Mathf.RoundToInt((isoY - isoX) * 0.5f);
            }
            else
            {
                // Inverse angle-based projection
                float angleRad = isoAngle * Mathf.Deg2Rad;
                float isoRatio = Mathf.Tan(angleRad);
                
                float isoX = localPosition.x / (cellStep * 0.5f);
                float isoY = localPosition.y / (cellStep * isoRatio);
                
                x = Mathf.RoundToInt((isoX + isoY) * 0.5f);
                y = Mathf.RoundToInt((isoY - isoX) * 0.5f);
            }
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Get cell at screen position for isometric view
        /// </summary>
        public GridCell GetCellAtScreenPosition(Vector2 screenPosition, Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;
                
            Vector3 worldPos = camera.ScreenToWorldPoint(screenPosition);
            worldPos.z = transform.position.z;
            
            return GetCellAtWorldPosition(worldPos);
        }
    }
}
