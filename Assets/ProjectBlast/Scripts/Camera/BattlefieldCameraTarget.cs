using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Camera
{
    /// <summary>
    /// BattlefieldCameraTarget - Static camera target for top-down battlefield view.
    /// 
    /// This provides a fixed transform for the Cinemachine camera to follow,
    /// creating a stable view of the entire battlefield without character following.
    /// 
    /// USAGE:
    /// - Place at the center of your battlefield
    /// - CinemachineCamera's Follow target points to this
    /// - Position determines what the camera focuses on
    /// </summary>
    [AddComponentMenu("ProjectBlast/Camera/Battlefield Camera Target")]
    public class BattlefieldCameraTarget : MonoBehaviour
    {
        [Header("Target Configuration")]
        [Tooltip("Position of the camera target in world space")]
        public Vector3 TargetPosition = new Vector3(0, 0, -3);
        
        [Header("Debug")]
        [Tooltip("Show gizmo in Scene view")]
        public bool ShowGizmo = true;
        
        [Tooltip("Size of the gizmo")]
        public float GizmoSize = 1f;
        
        [Tooltip("Color of the gizmo")]
        public Color GizmoColor = Color.cyan;
        
        void Start()
        {
            // Set initial position
            transform.position = TargetPosition;
        }
        
        void OnValidate()
        {
            // Update position when changed in Inspector
            if (!Application.isPlaying)
            {
                transform.position = TargetPosition;
            }
        }
        
        /// <summary>
        /// Updates the target position (for future dynamic positioning if needed)
        /// </summary>
        public void SetTargetPosition(Vector3 newPosition)
        {
            TargetPosition = newPosition;
            transform.position = newPosition;
        }
        
        void OnDrawGizmos()
        {
            if (!ShowGizmo) return;
            
            Gizmos.color = GizmoColor;
            
            // Draw sphere at target position
            Gizmos.DrawWireSphere(transform.position, GizmoSize);
            
            // Draw cross hairs
            Gizmos.DrawLine(
                transform.position + Vector3.left * GizmoSize,
                transform.position + Vector3.right * GizmoSize
            );
            Gizmos.DrawLine(
                transform.position + Vector3.forward * GizmoSize,
                transform.position + Vector3.back * GizmoSize
            );
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (GizmoSize + 0.5f),
                "Camera Target",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = GizmoColor },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            );
            #endif
        }
    }
}
