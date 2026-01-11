#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ProjectBlast.Combat.Editor
{
    /// <summary>
    /// Custom Inspector for BattlefieldConfigSO
    /// 
    /// Provides:
    /// - Visual preview of battlefield layout
    /// - Validation warnings
    /// - Quick preset buttons
    /// - Configuration summary
    /// </summary>
    [CustomEditor(typeof(BattlefieldConfigSO))]
    public class BattlefieldConfigEditor : UnityEditor.Editor
    {
        private BattlefieldConfigSO _config;
        
        private void OnEnable()
        {
            _config = (BattlefieldConfigSO)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector fields
            DrawDefaultInspector();
            
            // Add spacing
            EditorGUILayout.Space(20);
            
            // Draw battlefield preview section
            DrawBattlefieldPreview();
            
            EditorGUILayout.Space(10);
            
            // Draw validation warnings
            DrawValidationWarnings();
            
            EditorGUILayout.Space(10);
            
            // Draw preset buttons
            DrawPresetButtons();
            
            EditorGUILayout.Space(10);
            
            // Draw configuration summary
            DrawConfigurationSummary();
        }
        
        /// <summary>
        /// Draw visual preview of battlefield layout
        /// </summary>
        private void DrawBattlefieldPreview()
        {
            EditorGUILayout.LabelField("=== BATTLEFIELD PREVIEW ===", EditorStyles.boldLabel);
            
            // Create preview text
            string preview = $"Enemy Spawn: Z = {_config.EnemySpawnZ:F1}\n" +
                            $"     ↓ ({_config.DistanceSpawnToWall:F1} units)\n" +
                            $"█████ WALL █████ Z = {_config.BaseWallZ:F1}\n" +
                            $"     ↓ ({_config.DistanceWallToHeroes:F1} units)\n" +
                            $"Heroes Front: Z = {_config.HeroFrontRowZ:F1}\n" +
                            $"Heroes Back: Z = {_config.HeroBackRowZ:F1}\n\n" +
                            $"Battlefield Length: {_config.CalculatedBattlefieldLength:F1} units\n" +
                            $"Lanes: {_config.LaneCount} × {_config.LaneWidth:F1}m wide";
            
            // Display in help box
            EditorGUILayout.HelpBox(preview, MessageType.Info);
            
            // Draw mini diagram
            DrawMiniDiagram();
        }
        
        /// <summary>
        /// Draw a simple ASCII-art diagram
        /// </summary>
        private void DrawMiniDiagram()
        {
            GUIStyle diagramStyle = new GUIStyle(EditorStyles.helpBox);
            diagramStyle.fontSize = 10;
            diagramStyle.alignment = TextAnchor.MiddleLeft;
            diagramStyle.padding = new RectOffset(10, 10, 10, 10);
            
            string diagram = "         TOP (Enemies)\n" +
                           "             ↓\n" +
                           "     [Enemy Spawn Zone]\n" +
                           "             ↓\n" +
                           "    ████████████████████\n" +
                           "    █   WALL (Base)   █\n" +
                           "    ████████████████████\n" +
                           "             ↓\n" +
                           "       [H] [H] [H]  (Heroes)\n" +
                           "       [H] [H] [H]\n" +
                           "       [H] [H] [H]\n" +
                           "             ↓\n" +
                           "        BOTTOM (Safe)";
            
            EditorGUILayout.TextArea(diagram, diagramStyle, GUILayout.Height(200));
        }
        
        /// <summary>
        /// Draw validation warnings if any
        /// </summary>
        private void DrawValidationWarnings()
        {
            EditorGUILayout.LabelField("=== VALIDATION ===", EditorStyles.boldLabel);
            
            bool hasErrors = false;
            
            // Check wall position relative to heroes
            if (_config.BaseWallZ >= _config.HeroFrontRowZ)
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ CRITICAL: Wall (Z={_config.BaseWallZ:F1}) is BEHIND heroes (Z={_config.HeroFrontRowZ:F1})!\n" +
                    "Wall must be IN FRONT of heroes (higher Z value).",
                    MessageType.Error
                );
                hasErrors = true;
            }
            
            // Check enemy spawn position
            if (_config.EnemySpawnZ <= _config.BaseWallZ)
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ CRITICAL: Enemy spawn (Z={_config.EnemySpawnZ:F1}) is BEHIND wall (Z={_config.BaseWallZ:F1})!\n" +
                    "Enemies must spawn IN FRONT of wall (higher Z value).",
                    MessageType.Error
                );
                hasErrors = true;
            }
            
            // Check distance from wall to heroes
            if (_config.DistanceWallToHeroes < 2f && _config.DistanceWallToHeroes > 0)
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ Warning: Heroes are very close to wall ({_config.DistanceWallToHeroes:F1} units).\n" +
                    "Recommended: At least 3 units for better gameplay.",
                    MessageType.Warning
                );
            }
            
            // Check battlefield length
            if (_config.CalculatedBattlefieldLength < 15f)
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ Warning: Battlefield is short ({_config.CalculatedBattlefieldLength:F1} units).\n" +
                    "Recommended: At least 20 units for proper combat timing.",
                    MessageType.Warning
                );
            }
            
            // Check wall width
            float totalLaneWidth = _config.LaneCount * _config.LaneWidth;
            if (_config.BaseWallWidth < totalLaneWidth)
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ Warning: Wall width ({_config.BaseWallWidth:F1}m) is narrower than total lane width ({totalLaneWidth:F1}m).\n" +
                    "Wall should span all lanes!",
                    MessageType.Warning
                );
            }
            
            // Show success message if no errors
            if (!hasErrors)
            {
                EditorGUILayout.HelpBox("✓ Configuration is valid!", MessageType.Info);
            }
        }
        
        /// <summary>
        /// Draw quick preset buttons
        /// </summary>
        private void DrawPresetButtons()
        {
            EditorGUILayout.LabelField("=== QUICK PRESETS ===", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Default Setup\n(25 unit battlefield)", GUILayout.Height(40)))
            {
                SetDefaultPreset();
            }
            
            if (GUILayout.Button("Compact Setup\n(15 unit battlefield)", GUILayout.Height(40)))
            {
                SetCompactPreset();
            }
            
            if (GUILayout.Button("Long Range\n(35 unit battlefield)", GUILayout.Height(40)))
            {
                SetLongRangePreset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Wide Layout\n(5 lanes)", GUILayout.Height(40)))
            {
                SetWidePreset();
            }
            
            if (GUILayout.Button("Narrow Layout\n(2 lanes)", GUILayout.Height(40)))
            {
                SetNarrowPreset();
            }
            
            if (GUILayout.Button("Boss Arena\n(1 lane, long)", GUILayout.Height(40)))
            {
                SetBossPreset();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Draw configuration summary at bottom
        /// </summary>
        private void DrawConfigurationSummary()
        {
            EditorGUILayout.LabelField("=== SUMMARY ===", EditorStyles.boldLabel);
            
            GUIStyle summaryStyle = new GUIStyle(EditorStyles.textArea);
            summaryStyle.wordWrap = true;
            
            string summary = _config.GetConfigurationSummary();
            EditorGUILayout.TextArea(summary, summaryStyle, GUILayout.Height(120));
        }
        
        #region Preset Methods
        
        private void SetDefaultPreset()
        {
            Undo.RecordObject(_config, "Set Default Preset");
            
            _config.EnemySpawnZ = 20f;
            _config.BaseWallZ = -5f;
            _config.HeroZoneCenter = -1.5f;
            _config.HeroRows = 3;
            _config.HeroRowSpacing = 1.5f;
            _config.LaneCount = 3;
            _config.LaneWidth = 1.8f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 8f;
            _config.BaseWallHeight = 5f;
            _config.BaseWallThickness = 1f;
            _config.SpawnAreaWidth = 2f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Default Preset (25 unit battlefield, 3 lanes)");
        }
        
        private void SetCompactPreset()
        {
            Undo.RecordObject(_config, "Set Compact Preset");
            
            _config.EnemySpawnZ = 12f;
            _config.BaseWallZ = -3f;
            _config.HeroZoneCenter = -1f;
            _config.HeroRows = 2;
            _config.HeroRowSpacing = 1.2f;
            _config.LaneCount = 3;
            _config.LaneWidth = 1.5f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 6f;
            _config.BaseWallHeight = 4f;
            _config.BaseWallThickness = 0.8f;
            _config.SpawnAreaWidth = 1.5f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Compact Preset (15 unit battlefield, tight spacing)");
        }
        
        private void SetLongRangePreset()
        {
            Undo.RecordObject(_config, "Set Long Range Preset");
            
            _config.EnemySpawnZ = 30f;
            _config.BaseWallZ = -5f;
            _config.HeroZoneCenter = -2f;
            _config.HeroRows = 4;
            _config.HeroRowSpacing = 1.8f;
            _config.LaneCount = 3;
            _config.LaneWidth = 2f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 10f;
            _config.BaseWallHeight = 6f;
            _config.BaseWallThickness = 1.2f;
            _config.SpawnAreaWidth = 2.5f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Long Range Preset (35 unit battlefield)");
        }
        
        private void SetWidePreset()
        {
            Undo.RecordObject(_config, "Set Wide Preset");
            
            _config.EnemySpawnZ = 20f;
            _config.BaseWallZ = -5f;
            _config.HeroZoneCenter = -1.5f;
            _config.HeroRows = 3;
            _config.HeroRowSpacing = 1.5f;
            _config.LaneCount = 5;
            _config.LaneWidth = 1.8f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 12f;
            _config.BaseWallHeight = 5f;
            _config.BaseWallThickness = 1f;
            _config.SpawnAreaWidth = 2f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Wide Preset (5 lanes)");
        }
        
        private void SetNarrowPreset()
        {
            Undo.RecordObject(_config, "Set Narrow Preset");
            
            _config.EnemySpawnZ = 20f;
            _config.BaseWallZ = -5f;
            _config.HeroZoneCenter = -1.5f;
            _config.HeroRows = 3;
            _config.HeroRowSpacing = 1.5f;
            _config.LaneCount = 2;
            _config.LaneWidth = 1.8f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 5f;
            _config.BaseWallHeight = 5f;
            _config.BaseWallThickness = 1f;
            _config.SpawnAreaWidth = 2f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Narrow Preset (2 lanes)");
        }
        
        private void SetBossPreset()
        {
            Undo.RecordObject(_config, "Set Boss Preset");
            
            _config.EnemySpawnZ = 30f;
            _config.BaseWallZ = -5f;
            _config.HeroZoneCenter = -2f;
            _config.HeroRows = 4;
            _config.HeroRowSpacing = 2f;
            _config.LaneCount = 1;
            _config.LaneWidth = 3f;
            _config.CenterLaneX = 0f;
            _config.BaseWallWidth = 4f;
            _config.BaseWallHeight = 6f;
            _config.BaseWallThickness = 1.5f;
            _config.SpawnAreaWidth = 3f;
            
            EditorUtility.SetDirty(_config);
            Debug.Log("[BattlefieldConfig] Applied Boss Preset (1 lane, 35 unit battlefield)");
        }
        
        #endregion
    }
}
#endif
