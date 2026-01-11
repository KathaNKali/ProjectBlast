using UnityEngine;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Battlefield Visualizer Component
    /// 
    /// Attach to a GameObject in the scene to visualize the battlefield configuration
    /// using Unity Gizmos. Shows:
    /// - Enemy spawn zones per lane
    /// - Player base wall position
    /// - Hero slot positions
    /// - Movement paths
    /// 
    /// Useful for level design and debugging spatial positioning.
    /// </summary>
    [ExecuteInEditMode]
    public class BattlefieldVisualizer : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Battlefield configuration to visualize")]
        public BattlefieldConfigSO Config;
        
        [Header("Visualization Settings")]
        [Tooltip("Show gizmos in Scene view")]
        public bool ShowGizmos = true;
        
        [Tooltip("Show spawn zones")]
        public bool ShowSpawnZones = true;
        
        [Tooltip("Show wall")]
        public bool ShowWall = true;
        
        [Tooltip("Show hero slots")]
        public bool ShowHeroSlots = true;
        
        [Tooltip("Show movement paths")]
        public bool ShowMovementPaths = true;
        
        [Tooltip("Show labels")]
        public bool ShowLabels = true;
        
        [Header("Colors")]
        public Color SpawnZoneColor = new Color(1f, 0.2f, 0.2f, 0.5f); // Red
        public Color WallColor = new Color(0.2f, 0.5f, 1f, 0.8f); // Blue
        public Color HeroSlotColor = new Color(0.2f, 1f, 0.2f, 0.6f); // Green
        public Color PathColor = new Color(1f, 1f, 0f, 0.7f); // Yellow
        public Color LabelColor = Color.white;
        
        private void OnDrawGizmos()
        {
            if (!ShowGizmos || Config == null) return;
            
            if (ShowSpawnZones)
                DrawSpawnZones();
            
            if (ShowWall)
                DrawWall();
            
            if (ShowHeroSlots)
                DrawHeroSlots();
            
            if (ShowMovementPaths)
                DrawMovementPaths();
        }
        
        /// <summary>
        /// Draw enemy spawn zones for each lane
        /// </summary>
        private void DrawSpawnZones()
        {
            Gizmos.color = SpawnZoneColor;
            
            for (int i = 0; i < Config.LaneCount; i++)
            {
                Vector3 spawnPos = Config.GetLaneSpawnPosition(i);
                Vector3 size = new Vector3(Config.SpawnAreaWidth, Config.SpawnAreaHeight, 1f);
                
                // Draw spawn area box
                Gizmos.DrawCube(spawnPos, size);
                Gizmos.DrawWireCube(spawnPos, size);
                
                // Draw label
                if (ShowLabels)
                {
                    DrawLabel(spawnPos + Vector3.up * (Config.SpawnAreaHeight + 1f), 
                             $"Lane {i}\nSpawn", 
                             SpawnZoneColor);
                }
            }
            
            // Draw overall spawn zone indicator
            Vector3 spawnCenterTop = new Vector3(Config.CenterLaneX, 0, Config.EnemySpawnZ);
            DrawLabel(spawnCenterTop + Vector3.up * 3f, 
                     $"ENEMY SPAWN ZONE\nZ = {Config.EnemySpawnZ:F1}", 
                     SpawnZoneColor);
        }
        
        /// <summary>
        /// Draw the player base wall
        /// </summary>
        private void DrawWall()
        {
            Gizmos.color = WallColor;
            
            Vector3 wallPos = Config.GetWallPosition();
            Vector3 wallScale = Config.GetWallScale();
            
            // Draw solid wall
            Gizmos.DrawCube(wallPos, wallScale);
            
            // Draw wire outline
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(wallPos, wallScale);
            
            // Draw label
            if (ShowLabels)
            {
                DrawLabel(wallPos + Vector3.up * (Config.BaseWallHeight + 1f), 
                         $"PLAYER BASE WALL\nZ = {Config.BaseWallZ:F1}\nHP: {GetBaseHealth()}", 
                         WallColor);
            }
            
            // Draw wall line across all lanes
            Gizmos.color = WallColor;
            float leftX = Config.GetLaneXPosition(0) - Config.LaneWidth;
            float rightX = Config.GetLaneXPosition(Config.LaneCount - 1) + Config.LaneWidth;
            Vector3 leftPoint = new Vector3(leftX, 0, Config.BaseWallZ);
            Vector3 rightPoint = new Vector3(rightX, 0, Config.BaseWallZ);
            Gizmos.DrawLine(leftPoint, rightPoint);
        }
        
        /// <summary>
        /// Draw hero slot positions
        /// </summary>
        private void DrawHeroSlots()
        {
            Gizmos.color = HeroSlotColor;
            
            for (int lane = 0; lane < Config.LaneCount; lane++)
            {
                for (int row = 0; row < Config.HeroRows; row++)
                {
                    Vector3 heroPos = Config.GetHeroSlotPosition(lane, row);
                    
                    // Draw sphere for hero slot
                    Gizmos.DrawSphere(heroPos, 0.3f);
                    Gizmos.DrawWireSphere(heroPos, 0.5f);
                    
                    // Draw forward direction indicator
                    Gizmos.color = Color.yellow;
                    Vector3 forwardDir = Vector3.forward * 1f;
                    Gizmos.DrawLine(heroPos, heroPos + forwardDir);
                    Gizmos.DrawSphere(heroPos + forwardDir, 0.1f);
                    
                    Gizmos.color = HeroSlotColor;
                }
            }
            
            // Draw hero zone labels
            if (ShowLabels)
            {
                Vector3 heroCenterFront = new Vector3(Config.CenterLaneX, 0, Config.HeroFrontRowZ);
                DrawLabel(heroCenterFront + Vector3.down * 2f, 
                         $"HERO ZONE\nFront: Z = {Config.HeroFrontRowZ:F1}\nBack: Z = {Config.HeroBackRowZ:F1}", 
                         HeroSlotColor);
            }
        }
        
        /// <summary>
        /// Draw movement paths from spawn to wall
        /// </summary>
        private void DrawMovementPaths()
        {
            Gizmos.color = PathColor;
            
            for (int i = 0; i < Config.LaneCount; i++)
            {
                Vector3 spawnPos = Config.GetLaneSpawnPosition(i);
                Vector3 wallPos = new Vector3(Config.GetLaneXPosition(i), 0, Config.BaseWallZ);
                
                // Draw path line
                Gizmos.DrawLine(spawnPos, wallPos);
                
                // Draw directional arrows along path
                DrawArrowAlongPath(spawnPos, wallPos);
            }
            
            // Draw distance markers
            if (ShowLabels)
            {
                Vector3 midPoint = new Vector3(Config.CenterLaneX, 0, 
                                              (Config.EnemySpawnZ + Config.BaseWallZ) / 2f);
                DrawLabel(midPoint + Vector3.right * 5f, 
                         $"Distance: {Config.DistanceSpawnToWall:F1}m", 
                         PathColor);
            }
        }
        
        /// <summary>
        /// Draw directional arrows along a path
        /// </summary>
        private void DrawArrowAlongPath(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            int arrowCount = Mathf.Max(2, (int)(distance / 5f)); // Arrow every 5 units
            
            for (int i = 1; i <= arrowCount; i++)
            {
                float t = i / (float)(arrowCount + 1);
                Vector3 arrowPos = Vector3.Lerp(start, end, t);
                
                // Draw small arrow
                Vector3 arrowTip = arrowPos;
                Vector3 arrowLeft = arrowPos - direction * 0.5f + Vector3.left * 0.3f;
                Vector3 arrowRight = arrowPos - direction * 0.5f + Vector3.right * 0.3f;
                
                Gizmos.DrawLine(arrowTip, arrowLeft);
                Gizmos.DrawLine(arrowTip, arrowRight);
            }
        }
        
        /// <summary>
        /// Draw a text label at a position
        /// </summary>
        private void DrawLabel(Vector3 position, string text, Color color)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            
            UnityEditor.Handles.Label(position, text, style);
#endif
        }
        
        /// <summary>
        /// Get base health for display (checks if PlayerBase exists in scene)
        /// </summary>
        private string GetBaseHealth()
        {
            // PlayerBase will be created in Phase 7
            // For now, return placeholder
            return "TBD";
        }
        
        /// <summary>
        /// Validate configuration on enable
        /// </summary>
        private void OnEnable()
        {
            if (Config != null && !Config.IsConfigurationValid())
            {
                Debug.LogWarning($"[BattlefieldVisualizer] Configuration on {gameObject.name} has validation errors! Check the inspector.");
            }
        }
    }
}
