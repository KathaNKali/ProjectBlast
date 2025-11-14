using UnityEngine;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// 3D grid implementation for top-down 3D games.
    /// Y-axis is up, grid lies on X-Z plane.
    /// </summary>
    public class Grid3D : GridSystem
    {
        [Header("3D Specific")]
        [SerializeField] private float groundHeight = 0f;
        
        /// <summary>
        /// Convert grid coordinates to world position (X-Z plane)
        /// </summary>
        public override Vector3 GridToWorld(Vector2Int gridPosition)
        {
            // Cell spacing adds gap between cells
            float cellStep = cellSize + cellSpacing;
            Vector3 originOffset = CalculateOriginOffset();
            
            Vector3 localPosition = new Vector3(
                gridPosition.x * cellStep,
                groundHeight,
                gridPosition.y * cellStep  // Y becomes Z in 3D
            );
            
            // Apply origin offset (adjust for 3D)
            localPosition.x += originOffset.x;
            localPosition.z += originOffset.y;
            
            // Convert to world space
            return transform.TransformPoint(localPosition);
        }
        
        /// <summary>
        /// Convert world position to grid coordinates (X-Z plane)
        /// </summary>
        public override Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // Convert to local space
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            
            // Remove origin offset
            Vector3 originOffset = CalculateOriginOffset();
            localPosition.x -= originOffset.x;
            localPosition.z -= originOffset.y;
            
            // Calculate grid coordinates
            float cellStep = cellSize + cellSpacing;
            int x = Mathf.RoundToInt(localPosition.x / cellStep);
            int y = Mathf.RoundToInt(localPosition.z / cellStep);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Raycast from screen to get cell (for mouse/touch input in 3D)
        /// </summary>
        public GridCell GetCellAtScreenPosition(Vector2 screenPosition, Camera camera = null, LayerMask? layerMask = null)
        {
            if (camera == null)
                camera = Camera.main;
                
            Ray ray = camera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            float maxDistance = 1000f;
            LayerMask mask = layerMask ?? Physics.DefaultRaycastLayers;
            
            if (Physics.Raycast(ray, out hit, maxDistance, mask))
            {
                return GetCellAtWorldPosition(hit.point);
            }
            
            return null;
        }
    }
}
