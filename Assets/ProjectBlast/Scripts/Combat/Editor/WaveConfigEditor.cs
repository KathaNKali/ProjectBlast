#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ProjectBlast.Combat.Editor
{
    /// <summary>
    /// Custom Inspector for WaveConfigSO
    /// 
    /// Provides:
    /// - Visual wave summary
    /// - TPS curve preview
    /// - Enemy distribution display
    /// - Quick preset buttons
    /// </summary>
    [CustomEditor(typeof(WaveConfigSO))]
    public class WaveConfigEditor : UnityEditor.Editor
    {
        private WaveConfigSO _waveConfig;
        
        private void OnEnable()
        {
            _waveConfig = (WaveConfigSO)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(20);
            
            // Draw wave summary
            DrawWaveSummary();
            
            EditorGUILayout.Space(10);
            
            // Draw TPS analysis
            DrawTPSAnalysis();
            
            EditorGUILayout.Space(10);
            
            // Draw enemy distribution
            DrawEnemyDistribution();
            
            EditorGUILayout.Space(10);
            
            // Draw preset buttons
            DrawPresetButtons();
            
            EditorGUILayout.Space(10);
            
            // Draw tools
            DrawTools();
        }
        
        private void DrawWaveSummary()
        {
            EditorGUILayout.LabelField("=== WAVE SUMMARY ===", EditorStyles.boldLabel);
            
            string summary = _waveConfig.GetWaveSummary();
            
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fontSize = 11;
            style.padding = new RectOffset(10, 10, 10, 10);
            
            EditorGUILayout.TextArea(summary, style, GUILayout.Height(120));
        }
        
        private void DrawTPSAnalysis()
        {
            EditorGUILayout.LabelField("=== TPS ANALYSIS ===", EditorStyles.boldLabel);
            
            float totalThreat = _waveConfig.CalculateTotalThreat();
            float avgTPS = totalThreat / _waveConfig.WaveDuration;
            
            string analysis = $"Total Threat Budget: {totalThreat:F0}\n" +
                             $"Average TPS: {avgTPS:F1}\n" +
                             $"TPS Range: {_waveConfig.StartingTPS:F0} → {_waveConfig.PeakTPS:F0}\n" +
                             $"Duration: {_waveConfig.WaveDuration}s\n";
            
            // Sample TPS at key points
            analysis += $"\nTPS Curve Samples:\n";
            analysis += $"  • Start (0%): {_waveConfig.GetTPSAtTime(0f):F1}\n";
            analysis += $"  • 25%: {_waveConfig.GetTPSAtTime(0.25f):F1}\n";
            analysis += $"  • 50%: {_waveConfig.GetTPSAtTime(0.5f):F1}\n";
            analysis += $"  • 75%: {_waveConfig.GetTPSAtTime(0.75f):F1}\n";
            analysis += $"  • End (100%): {_waveConfig.GetTPSAtTime(1f):F1}";
            
            EditorGUILayout.HelpBox(analysis, MessageType.Info);
        }
        
        private void DrawEnemyDistribution()
        {
            EditorGUILayout.LabelField("=== ENEMY DISTRIBUTION ===", EditorStyles.boldLabel);
            
            if (_waveConfig.AllowedEnemies.Count == 0)
            {
                EditorGUILayout.HelpBox("No enemies configured!", MessageType.Warning);
                return;
            }
            
            // Calculate percentages
            float totalWeight = 0f;
            for (int i = 0; i < _waveConfig.EnemyWeights.Count; i++)
            {
                totalWeight += _waveConfig.EnemyWeights[i];
            }
            
            if (totalWeight <= 0) totalWeight = 1f;
            
            // Draw each enemy
            for (int i = 0; i < _waveConfig.AllowedEnemies.Count; i++)
            {
                if (_waveConfig.AllowedEnemies[i] == null) continue;
                
                float weight = i < _waveConfig.EnemyWeights.Count ? _waveConfig.EnemyWeights[i] : 1f;
                float percentage = (weight / totalWeight) * 100f;
                
                EnemyDataSO enemy = _waveConfig.AllowedEnemies[i];
                
                EditorGUILayout.BeginHorizontal();
                
                // Enemy name
                EditorGUILayout.LabelField($"{enemy.EnemyName}", GUILayout.Width(100));
                
                // Progress bar showing percentage
                Rect rect = GUILayoutUtility.GetRect(200, 18);
                EditorGUI.ProgressBar(rect, percentage / 100f, $"{percentage:F1}%");
                
                // Threat value
                EditorGUILayout.LabelField($"({enemy.ThreatValue:F0} threat)", GUILayout.Width(100));
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawPresetButtons()
        {
            EditorGUILayout.LabelField("=== QUICK PRESETS ===", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Easy Wave\n(Warm-up)", GUILayout.Height(40)))
            {
                SetEasyPreset();
            }
            
            if (GUILayout.Button("Normal Wave\n(Balanced)", GUILayout.Height(40)))
            {
                SetNormalPreset();
            }
            
            if (GUILayout.Button("Hard Wave\n(Intense)", GUILayout.Height(40)))
            {
                SetHardPreset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Boss Wave\n(Single Lane)", GUILayout.Height(40)))
            {
                SetBossPreset();
            }
            
            if (GUILayout.Button("Rush Wave\n(Fast Enemies)", GUILayout.Height(40)))
            {
                SetRushPreset();
            }
            
            if (GUILayout.Button("Survival Wave\n(Long Duration)", GUILayout.Height(40)))
            {
                SetSurvivalPreset();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTools()
        {
            EditorGUILayout.LabelField("=== TOOLS ===", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Auto-Balance Weights"))
            {
                AutoBalanceWeights();
            }
            
            if (GUILayout.Button("Reset TPS Curve"))
            {
                ResetTPSCurve();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #region Preset Methods
        
        private void SetEasyPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Easy Wave Preset");
            
            _waveConfig.WaveName = "Easy Wave";
            _waveConfig.Description = "Warm-up wave with weak enemies";
            _waveConfig.StartingTPS = 20f;
            _waveConfig.PeakTPS = 40f;
            _waveConfig.WaveDuration = 45f;
            _waveConfig.BreakAfterWave = 10f;
            _waveConfig.TPSCurve = AnimationCurve.Linear(0, 0, 1, 1);
            _waveConfig.AutoEvenDistribution = true;
            _waveConfig.SetEvenDistribution(3);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Easy preset to {_waveConfig.name}");
        }
        
        private void SetNormalPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Normal Wave Preset");
            
            _waveConfig.WaveName = "Normal Wave";
            _waveConfig.Description = "Balanced wave with mixed enemies";
            _waveConfig.StartingTPS = 40f;
            _waveConfig.PeakTPS = 80f;
            _waveConfig.WaveDuration = 60f;
            _waveConfig.BreakAfterWave = 10f;
            _waveConfig.TPSCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            _waveConfig.AutoEvenDistribution = true;
            _waveConfig.SetEvenDistribution(3);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Normal preset to {_waveConfig.name}");
        }
        
        private void SetHardPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Hard Wave Preset");
            
            _waveConfig.WaveName = "Hard Wave";
            _waveConfig.Description = "Intense wave with strong enemies";
            _waveConfig.StartingTPS = 60f;
            _waveConfig.PeakTPS = 120f;
            _waveConfig.WaveDuration = 90f;
            _waveConfig.BreakAfterWave = 15f;
            
            // Exponential ramp
            _waveConfig.TPSCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(1f, 1f, 2f, 0f)
            );
            
            _waveConfig.AutoEvenDistribution = true;
            _waveConfig.SetEvenDistribution(3);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Hard preset to {_waveConfig.name}");
        }
        
        private void SetBossPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Boss Wave Preset");
            
            _waveConfig.WaveName = "Boss Wave";
            _waveConfig.Description = "Single powerful enemy";
            _waveConfig.StartingTPS = 10f;
            _waveConfig.PeakTPS = 10f;
            _waveConfig.WaveDuration = 120f;
            _waveConfig.BreakAfterWave = 20f;
            _waveConfig.TPSCurve = AnimationCurve.Constant(0, 1, 1);
            
            // Single lane focus
            _waveConfig.AutoEvenDistribution = false;
            _waveConfig.LaneTPS_Multipliers = new float[] { 0f, 1f, 0f };
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Boss preset to {_waveConfig.name}");
        }
        
        private void SetRushPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Rush Wave Preset");
            
            _waveConfig.WaveName = "Rush Wave";
            _waveConfig.Description = "Fast-paced wave with quick enemies";
            _waveConfig.StartingTPS = 50f;
            _waveConfig.PeakTPS = 100f;
            _waveConfig.WaveDuration = 30f;
            _waveConfig.BreakAfterWave = 5f;
            _waveConfig.TPSCurve = AnimationCurve.Linear(0, 0.5f, 1, 1);
            _waveConfig.AutoEvenDistribution = true;
            _waveConfig.SetEvenDistribution(3);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Rush preset to {_waveConfig.name}");
        }
        
        private void SetSurvivalPreset()
        {
            Undo.RecordObject(_waveConfig, "Set Survival Wave Preset");
            
            _waveConfig.WaveName = "Survival Wave";
            _waveConfig.Description = "Long endurance test";
            _waveConfig.StartingTPS = 30f;
            _waveConfig.PeakTPS = 90f;
            _waveConfig.WaveDuration = 180f;
            _waveConfig.BreakAfterWave = 30f;
            
            // Gradual increase with plateau
            _waveConfig.TPSCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.3f, 0.5f),
                new Keyframe(0.7f, 0.8f),
                new Keyframe(1f, 1f)
            );
            
            _waveConfig.AutoEvenDistribution = true;
            _waveConfig.SetEvenDistribution(3);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Applied Survival preset to {_waveConfig.name}");
        }
        
        #endregion
        
        #region Tool Methods
        
        private void AutoBalanceWeights()
        {
            Undo.RecordObject(_waveConfig, "Auto-Balance Enemy Weights");
            
            // Set all weights to 1.0 for even distribution
            for (int i = 0; i < _waveConfig.EnemyWeights.Count; i++)
            {
                _waveConfig.EnemyWeights[i] = 1f;
            }
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Auto-balanced enemy weights for {_waveConfig.name}");
        }
        
        private void ResetTPSCurve()
        {
            Undo.RecordObject(_waveConfig, "Reset TPS Curve");
            
            _waveConfig.TPSCurve = AnimationCurve.Linear(0, 0, 1, 1);
            
            EditorUtility.SetDirty(_waveConfig);
            Debug.Log($"[WaveConfig] Reset TPS curve for {_waveConfig.name}");
        }
        
        #endregion
    }
}
#endif
