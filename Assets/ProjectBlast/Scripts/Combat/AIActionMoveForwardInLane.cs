using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// AI Action: Move Forward In Lane
    /// 
    /// Makes the character move forward (negative Z direction) in their lane.
    /// Simple continuous forward movement without needing a target.
    /// 
    /// Usage:
    /// - Add to AIBrain "Moving" state
    /// - Character will move forward automatically
    /// - Movement speed controlled by CharacterMovement ability
    /// 
    /// Integration:
    /// - Works with LaneSpawner system
    /// - Respects MovementSpeed from EnemyDataSO
    /// - Use with AIDecisionReachedWall to stop at base
    /// </summary>
    [AddComponentMenu("ProjectBlast/Combat/AI/Actions/AI Action Move Forward In Lane")]
    public class AIActionMoveForwardInLane : AIAction
    {
        [Header("Movement Direction")]
        [Tooltip("Movement direction in 3D space (default: forward = negative Z)")]
        public Vector3 MovementDirection = new Vector3(0, 0, -1);
        
        [Tooltip("Normalize the movement direction (recommended)")]
        public bool NormalizeDirection = true;
        
        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool DebugMode = false;
        
        // Cached components
        protected CharacterMovement _characterMovement;
        protected Character _character;
        protected Vector3 _normalizedDirection;
        protected Vector2 _movementVector;
        
        /// <summary>
        /// On init we grab our CharacterMovement ability
        /// </summary>
        public override void Initialization()
        {
            if (!ShouldInitialize) return;
            base.Initialization();
            
            _character = this.gameObject.GetComponentInParent<Character>();
            if (_character != null)
            {
                _characterMovement = _character.FindAbility<CharacterMovement>();
            }
            
            // Pre-calculate normalized direction
            _normalizedDirection = NormalizeDirection ? MovementDirection.normalized : MovementDirection;
            
            if (_characterMovement == null)
            {
                Debug.LogError($"[AIActionMoveForwardInLane] No CharacterMovement ability found on {gameObject.name}!");
            }
            
            if (DebugMode)
            {
                Debug.Log($"[AIActionMoveForwardInLane] Initialized on {gameObject.name} - Direction: {_normalizedDirection}");
            }
        }
        
        /// <summary>
        /// On PerformAction we move forward continuously
        /// </summary>
        public override void PerformAction()
        {
            MoveForward();
        }
        
        /// <summary>
        /// Move character forward in the specified direction
        /// </summary>
        protected virtual void MoveForward()
        {
            if (_characterMovement == null) return;
            
            // Convert 3D direction to 2D movement vector
            // X maps to horizontal, Z maps to vertical in TopDown Engine
            _movementVector.x = _normalizedDirection.x;
            _movementVector.y = _normalizedDirection.z;
            
            // Set the movement
            _characterMovement.SetMovement(_movementVector);
            
            if (DebugMode && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[AIActionMoveForwardInLane] {gameObject.name} moving: {_movementVector}");
            }
        }
        
        /// <summary>
        /// On exit state we stop movement
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();
            
            if (_characterMovement != null)
            {
                _characterMovement.SetHorizontalMovement(0f);
                _characterMovement.SetVerticalMovement(0f);
            }
            
            if (DebugMode)
            {
                Debug.Log($"[AIActionMoveForwardInLane] {gameObject.name} stopped moving");
            }
        }
        
        /// <summary>
        /// Gizmo visualization of movement direction
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Vector3 direction = NormalizeDirection ? MovementDirection.normalized : MovementDirection;
            Vector3 end = start + direction * 2f;
            
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.2f);
        }
    }
}
