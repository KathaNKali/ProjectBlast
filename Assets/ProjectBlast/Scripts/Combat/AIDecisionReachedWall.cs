using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// AI Decision: Reached Wall
    /// 
    /// Returns true when the character has reached or passed the base wall position.
    /// Used to transition from moving state to attacking state.
    /// 
    /// Usage:
    /// - Add to AIBrain state transitions
    /// - Requires BattlefieldConfigSO to be set (or use manual wall Z position)
    /// - Transition: Moving → Attacking when wall reached
    /// 
    /// Integration:
    /// - Works with AIActionMoveForwardInLane
    /// - Uses BattlefieldConfigSO.BaseWallZ for wall position
    /// - Includes buffer distance for smooth transitions
    /// </summary>
    [AddComponentMenu("ProjectBlast/Combat/AI/Decisions/AI Decision Reached Wall")]
    public class AIDecisionReachedWall : AIDecision
    {
        [Header("Wall Configuration")]
        [Tooltip("Battlefield configuration (reads BaseWallZ from here)")]
        public BattlefieldConfigSO BattlefieldConfig;
        
        [Tooltip("Manual wall Z position (used if BattlefieldConfig is null)")]
        public float ManualWallZ = -5f;
        
        [Tooltip("Buffer distance before wall (helps smooth transitions)")]
        [Range(0f, 2f)]
        public float BufferDistance = 0.5f;
        
        [Header("Comparison Mode")]
        [Tooltip("How to check if wall is reached")]
        public ComparisonMode Comparison = ComparisonMode.LessThanOrEqual;
        
        public enum ComparisonMode
        {
            LessThan,           // Character Z < Wall Z (passed the wall)
            LessThanOrEqual     // Character Z <= Wall Z (at or passed the wall)
        }
        
        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool DebugMode = false;
        
        private float _effectiveWallZ;
        private bool _initialized = false;
        
        /// <summary>
        /// Initialize and cache wall position
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            
            // Get wall Z from config or use manual value
            if (BattlefieldConfig != null)
            {
                _effectiveWallZ = BattlefieldConfig.BaseWallZ + BufferDistance;
            }
            else
            {
                _effectiveWallZ = ManualWallZ + BufferDistance;
                if (DebugMode)
                {
                    Debug.LogWarning($"[AIDecisionReachedWall] No BattlefieldConfig set on {gameObject.name}, using manual wall Z: {ManualWallZ}");
                }
            }
            
            _initialized = true;
            
            if (DebugMode)
            {
                Debug.Log($"[AIDecisionReachedWall] Initialized on {gameObject.name} - Effective wall Z: {_effectiveWallZ}");
            }
        }
        
        /// <summary>
        /// On Decide we check if character has reached the wall
        /// </summary>
        /// <returns>True if wall reached, false otherwise</returns>
        public override bool Decide()
        {
            return EvaluateWallDistance();
        }
        
        /// <summary>
        /// Check if character position has reached/passed the wall
        /// </summary>
        protected virtual bool EvaluateWallDistance()
        {
            if (!_initialized)
            {
                Initialization();
            }
            
            float characterZ = transform.position.z;
            bool hasReached = false;
            
            switch (Comparison)
            {
                case ComparisonMode.LessThan:
                    hasReached = characterZ < _effectiveWallZ;
                    break;
                    
                case ComparisonMode.LessThanOrEqual:
                    hasReached = characterZ <= _effectiveWallZ;
                    break;
            }
            
            if (DebugMode && hasReached)
            {
                Debug.Log($"[AIDecisionReachedWall] {gameObject.name} reached wall! Character Z: {characterZ:F2}, Wall Z: {_effectiveWallZ:F2}");
            }
            
            return hasReached;
        }
        
        /// <summary>
        /// Gizmo visualization of wall position
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying && !_initialized)
            {
                // Calculate effective wall position for gizmos
                if (BattlefieldConfig != null)
                {
                    _effectiveWallZ = BattlefieldConfig.BaseWallZ + BufferDistance;
                }
                else
                {
                    _effectiveWallZ = ManualWallZ + BufferDistance;
                }
            }
            
            // Draw wall line
            Gizmos.color = Color.red;
            Vector3 characterPos = transform.position;
            Vector3 wallLeft = new Vector3(characterPos.x - 5f, characterPos.y, _effectiveWallZ);
            Vector3 wallRight = new Vector3(characterPos.x + 5f, characterPos.y, _effectiveWallZ);
            Gizmos.DrawLine(wallLeft, wallRight);
            
            // Draw distance indicator
            Gizmos.color = Color.yellow;
            Vector3 characterWallPoint = new Vector3(characterPos.x, characterPos.y, _effectiveWallZ);
            Gizmos.DrawLine(characterPos, characterWallPoint);
            
            // Draw buffer zone
            if (BufferDistance > 0)
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
                float actualWallZ = BattlefieldConfig != null ? BattlefieldConfig.BaseWallZ : ManualWallZ;
                Vector3 bufferLeft = new Vector3(characterPos.x - 5f, characterPos.y, actualWallZ);
                Vector3 bufferRight = new Vector3(characterPos.x + 5f, characterPos.y, actualWallZ);
                Gizmos.DrawLine(bufferLeft, bufferRight);
            }
        }
    }
}
