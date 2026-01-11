using UnityEngine;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Battlefield Configuration ScriptableObject
    /// 
    /// Defines all spatial positioning for the battlefield including:
    /// - Enemy spawn zones
    /// - Player base wall position
    /// - Hero firing zone layout
    /// - Lane configuration
    /// 
    /// This is the master configuration that all combat systems reference
    /// for consistent spatial positioning across the game.
    /// </summary>
    [CreateAssetMenu(fileName = "BattlefieldConfig", menuName = "ProjectBlast/Combat/Battlefield Config")]
    public class BattlefieldConfigSO : ScriptableObject
    {
        #region Battlefield Dimensions
        
        [Header("=== BATTLEFIELD DIMENSIONS ===")]
        [Tooltip("Total battlefield length (enemy spawn to hero back row)")]
        [Min(10f)]
        public float BattlefieldLength = 23f; // Default: +20 to -3
        
        #endregion
        
        #region Enemy Spawn Zone
        
        [Header("=== ENEMY SPAWN ZONE ===")]
        [Tooltip("Z position where enemies spawn (TOP of battlefield)")]
        public float EnemySpawnZ = 20f;
        
        [Tooltip("Width of spawn area per lane (for random positioning)")]
        [Range(0.5f, 5f)]
        public float SpawnAreaWidth = 2f;
        
        [Tooltip("Height variance for spawn effects")]
        [Range(0f, 3f)]
        public float SpawnAreaHeight = 1f;
        
        #endregion
        
        #region Player Base Wall
        
        [Header("=== PLAYER BASE WALL ===")]
        [Tooltip("Z position of the protective wall (enemies stop here)")]
        public float BaseWallZ = -5f;
        
        [Tooltip("Wall width (should span all lanes)")]
        [Range(2f, 20f)]
        public float BaseWallWidth = 8f;
        
        [Tooltip("Wall height (visual)")]
        [Range(1f, 10f)]
        public float BaseWallHeight = 5f;
        
        [Tooltip("Wall thickness (depth along Z-axis)")]
        [Range(0.2f, 2f)]
        public float BaseWallThickness = 1f;
        
        #endregion
        
        #region Hero Firing Zone
        
        [Header("=== HERO FIRING ZONE ===")]
        [Tooltip("Z position of hero firing zone CENTER")]
        public float HeroZoneCenter = -1.5f; // Between 0 and -3 by default
        
        [Tooltip("Number of rows in hero firing grid")]
        [Range(1, 5)]
        public int HeroRows = 3;
        
        [Tooltip("Spacing between hero rows (distance between each row)")]
        [Range(0.5f, 3f)]
        public float HeroRowSpacing = 1.5f;
        
        #endregion
        
        #region Lane Configuration
        
        [Header("=== LANE CONFIGURATION ===")]
        [Tooltip("Number of lanes (vertical columns for enemies/heroes)")]
        [Range(2, 5)]
        public int LaneCount = 3;
        
        [Tooltip("Width of each lane")]
        [Range(1f, 3f)]
        public float LaneWidth = 1.8f;
        
        [Tooltip("Center X position of the middle lane")]
        public float CenterLaneX = 0f;
        
        #endregion
        
        #region Calculated Positions (Read-Only)
        
        [Header("=== CALCULATED POSITIONS (Auto-Calculated) ===")]
        [Tooltip("Z position of hero front row (closest to enemies)")]
        [SerializeField] private float _heroFrontRowZ;
        
        [Tooltip("Z position of hero back row (furthest from enemies)")]
        [SerializeField] private float _heroBackRowZ;
        
        [Tooltip("Distance from wall to hero front row")]
        [SerializeField] private float _distanceWallToHeroes;
        
        [Tooltip("Distance from enemy spawn to wall")]
        [SerializeField] private float _distanceSpawnToWall;
        
        [Tooltip("Total battlefield coverage")]
        [SerializeField] private float _calculatedBattlefieldLength;
        
        // Public read-only accessors
        public float HeroFrontRowZ => _heroFrontRowZ;
        public float HeroBackRowZ => _heroBackRowZ;
        public float DistanceWallToHeroes => _distanceWallToHeroes;
        public float DistanceSpawnToWall => _distanceSpawnToWall;
        public float CalculatedBattlefieldLength => _calculatedBattlefieldLength;
        
        #endregion
        
        #region Validation & Calculations
        
        /// <summary>
        /// Called automatically when values change in the inspector.
        /// Calculates derived values and validates configuration.
        /// </summary>
        private void OnValidate()
        {
            CalculateDerivedPositions();
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Calculate all derived position values based on configured settings
        /// </summary>
        private void CalculateDerivedPositions()
        {
            // Calculate hero row positions based on center and spacing
            if (HeroRows == 1)
            {
                _heroFrontRowZ = HeroZoneCenter;
                _heroBackRowZ = HeroZoneCenter;
            }
            else
            {
                float totalSpacing = (HeroRows - 1) * HeroRowSpacing;
                _heroFrontRowZ = HeroZoneCenter + (totalSpacing / 2f);
                _heroBackRowZ = HeroZoneCenter - (totalSpacing / 2f);
            }
            
            // Calculate distances
            _distanceWallToHeroes = _heroFrontRowZ - BaseWallZ;
            _distanceSpawnToWall = EnemySpawnZ - BaseWallZ;
            _calculatedBattlefieldLength = EnemySpawnZ - _heroBackRowZ;
        }
        
        /// <summary>
        /// Validate configuration and log warnings for invalid setups
        /// </summary>
        private void ValidateConfiguration()
        {
            // Check if wall is in front of heroes (wall should have higher Z)
            if (BaseWallZ >= _heroFrontRowZ)
            {
                Debug.LogWarning($"[BattlefieldConfig] INVALID SETUP: Wall (Z={BaseWallZ}) must be IN FRONT of heroes (Z={_heroFrontRowZ})!\n" +
                                 $"Wall should have HIGHER Z value than heroes.");
            }
            
            // Check if enemy spawn is in front of wall
            if (EnemySpawnZ <= BaseWallZ)
            {
                Debug.LogWarning($"[BattlefieldConfig] INVALID SETUP: Enemy spawn (Z={EnemySpawnZ}) must be IN FRONT of wall (Z={BaseWallZ})!\n" +
                                 $"Enemy spawn should have HIGHER Z value than wall.");
            }
            
            // Check if heroes are too close to wall
            if (_distanceWallToHeroes < 2f)
            {
                Debug.LogWarning($"[BattlefieldConfig] Heroes are very close to wall ({_distanceWallToHeroes:F1} units). " +
                                 $"Consider increasing distance to at least 3 units for better gameplay.");
            }
            
            // Check battlefield size
            if (_calculatedBattlefieldLength < 15f)
            {
                Debug.LogWarning($"[BattlefieldConfig] Battlefield is short ({_calculatedBattlefieldLength:F1} units). " +
                                 $"Consider at least 20 units for better combat timing.");
            }
            
            // Check wall width vs lane coverage
            float totalLaneWidth = LaneCount * LaneWidth;
            if (BaseWallWidth < totalLaneWidth)
            {
                Debug.LogWarning($"[BattlefieldConfig] Wall width ({BaseWallWidth:F1}) is narrower than total lane width ({totalLaneWidth:F1}). " +
                                 $"Wall should span all lanes!");
            }
        }
        
        #endregion
        
        #region Helper Methods - Position Calculations
        
        /// <summary>
        /// Get the spawn position for a specific lane
        /// </summary>
        /// <param name="laneIndex">Lane index (0 to LaneCount-1)</param>
        /// <returns>World position for enemy spawn in this lane</returns>
        public Vector3 GetLaneSpawnPosition(int laneIndex)
        {
            float laneX = GetLaneXPosition(laneIndex);
            return new Vector3(laneX, 0, EnemySpawnZ);
        }
        
        /// <summary>
        /// Get the X position for a specific lane
        /// </summary>
        /// <param name="laneIndex">Lane index (0 to LaneCount-1)</param>
        /// <returns>X coordinate of the lane center</returns>
        public float GetLaneXPosition(int laneIndex)
        {
            if (laneIndex < 0 || laneIndex >= LaneCount)
            {
                Debug.LogError($"[BattlefieldConfig] Invalid lane index: {laneIndex}. Valid range: 0 to {LaneCount - 1}");
                return CenterLaneX;
            }
            
            // Calculate X position based on lane count and width
            if (LaneCount == 1)
            {
                return CenterLaneX;
            }
            
            float totalWidth = (LaneCount - 1) * LaneWidth;
            float startX = CenterLaneX - (totalWidth / 2f);
            return startX + (laneIndex * LaneWidth);
        }
        
        /// <summary>
        /// Get the position for a hero slot in the firing grid
        /// </summary>
        /// <param name="laneIndex">Lane (column) index</param>
        /// <param name="rowIndex">Row index (0 = front, increasing = back)</param>
        /// <returns>World position for hero placement</returns>
        public Vector3 GetHeroSlotPosition(int laneIndex, int rowIndex)
        {
            float x = GetLaneXPosition(laneIndex);
            float z = _heroFrontRowZ - (rowIndex * HeroRowSpacing);
            return new Vector3(x, 0, z);
        }
        
        /// <summary>
        /// Get the center position of the wall
        /// </summary>
        /// <returns>World position for wall center</returns>
        public Vector3 GetWallPosition()
        {
            return new Vector3(CenterLaneX, BaseWallHeight / 2f, BaseWallZ);
        }
        
        /// <summary>
        /// Get the scale for the wall GameObject
        /// </summary>
        /// <returns>Scale vector for wall</returns>
        public Vector3 GetWallScale()
        {
            return new Vector3(BaseWallWidth, BaseWallHeight, BaseWallThickness);
        }
        
        /// <summary>
        /// Check if a Z position is past the wall (in the protected zone)
        /// </summary>
        /// <param name="zPosition">Z position to check</param>
        /// <returns>True if position is behind wall (protected zone)</returns>
        public bool IsPositionBehindWall(float zPosition)
        {
            return zPosition < BaseWallZ;
        }
        
        /// <summary>
        /// Check if an enemy has reached the wall
        /// </summary>
        /// <param name="enemyZ">Enemy Z position</param>
        /// <param name="threshold">Detection threshold (default 0.5 units)</param>
        /// <returns>True if enemy is at or past wall position</returns>
        public bool HasReachedWall(float enemyZ, float threshold = 0.5f)
        {
            return enemyZ <= (BaseWallZ + threshold);
        }
        
        #endregion
        
        #region Debug & Visualization Helpers
        
        /// <summary>
        /// Get a formatted summary of the battlefield configuration
        /// </summary>
        /// <returns>Multi-line string describing the configuration</returns>
        public string GetConfigurationSummary()
        {
            return $"=== BATTLEFIELD CONFIGURATION ===\n" +
                   $"Enemy Spawn: Z = {EnemySpawnZ:F1}\n" +
                   $"     ↓ ({_distanceSpawnToWall:F1} units)\n" +
                   $"Wall: Z = {BaseWallZ:F1}\n" +
                   $"     ↓ ({_distanceWallToHeroes:F1} units)\n" +
                   $"Heroes Front: Z = {_heroFrontRowZ:F1}\n" +
                   $"Heroes Back: Z = {_heroBackRowZ:F1}\n\n" +
                   $"Lanes: {LaneCount} × {LaneWidth:F1} units wide\n" +
                   $"Hero Rows: {HeroRows} × {HeroRowSpacing:F1} units apart\n" +
                   $"Total Length: {_calculatedBattlefieldLength:F1} units";
        }
        
        /// <summary>
        /// Check if configuration is valid (no critical errors)
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsConfigurationValid()
        {
            return BaseWallZ < _heroFrontRowZ && EnemySpawnZ > BaseWallZ;
        }
        
        #endregion
    }
}
