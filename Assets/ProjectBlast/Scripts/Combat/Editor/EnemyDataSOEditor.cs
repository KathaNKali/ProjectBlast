#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ProjectBlast.Combat.Editor
{
    /// <summary>
    /// Custom Inspector for EnemyDataSO
    /// 
    /// Provides:
    /// - Visual stats summary
    /// - Threat value breakdown
    /// - Quick preset buttons for common enemy types
    /// - Balance comparison tools
    /// </summary>
    [CustomEditor(typeof(EnemyDataSO))]
    public class EnemyDataSOEditor : UnityEditor.Editor
    {
        private EnemyDataSO _enemyData;
        
        private void OnEnable()
        {
            _enemyData = (EnemyDataSO)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            // Add spacing
            EditorGUILayout.Space(20);
            
            // Draw stats summary
            DrawStatsSummary();
            
            EditorGUILayout.Space(10);
            
            // Draw threat breakdown
            DrawThreatBreakdown();
            
            EditorGUILayout.Space(10);
            
            // Draw balance analysis
            DrawBalanceAnalysis();
            
            EditorGUILayout.Space(10);
            
            // Draw preset buttons
            DrawPresetButtons();
        }
        
        /// <summary>
        /// Draw enemy stats summary
        /// </summary>
        private void DrawStatsSummary()
        {
            EditorGUILayout.LabelField("=== STATS SUMMARY ===", EditorStyles.boldLabel);
            
            string summary = _enemyData.GetStatsSummary();
            
            GUIStyle summaryStyle = new GUIStyle(EditorStyles.helpBox);
            summaryStyle.fontSize = 11;
            summaryStyle.padding = new RectOffset(10, 10, 10, 10);
            
            EditorGUILayout.TextArea(summary, summaryStyle, GUILayout.Height(140));
        }
        
        /// <summary>
        /// Draw threat value calculation breakdown
        /// </summary>
        private void DrawThreatBreakdown()
        {
            EditorGUILayout.LabelField("=== THREAT BREAKDOWN ===", EditorStyles.boldLabel);
            
            // Calculate individual factors
            float baseThreat = _enemyData.MaxHealth;
            float rangeFactor = _enemyData.AttackRange / 10f;
            float dpsFactor = _enemyData.DPS / 10f;
            float speedFactor = _enemyData.MovementSpeed / 3f;
            float totalMultiplier = 1f + rangeFactor + dpsFactor + speedFactor;
            
            string breakdown = $"Base Threat (HP): {baseThreat:F1}\n" +
                              $"Range Factor: +{rangeFactor:F2} ({_enemyData.AttackRange:F1} ÷ 10)\n" +
                              $"DPS Factor: +{dpsFactor:F2} ({_enemyData.DPS:F1} ÷ 10)\n" +
                              $"Speed Factor: +{speedFactor:F2} ({_enemyData.MovementSpeed:F1} ÷ 3)\n" +
                              $"Total Multiplier: ×{totalMultiplier:F2}\n" +
                              $"\nCalculated: {baseThreat:F1} × {totalMultiplier:F2} = {_enemyData.ThreatValue / _enemyData.ThreatMultiplier:F1}\n" +
                              $"Final (with multiplier): {_enemyData.ThreatValue:F1}";
            
            MessageType messageType = MessageType.Info;
            if (_enemyData.ThreatValue < 100) messageType = MessageType.None;
            else if (_enemyData.ThreatValue < 300) messageType = MessageType.Info;
            else if (_enemyData.ThreatValue < 600) messageType = MessageType.Warning;
            else messageType = MessageType.Error;
            
            EditorGUILayout.HelpBox(breakdown, messageType);
        }
        
        /// <summary>
        /// Draw balance analysis
        /// </summary>
        private void DrawBalanceAnalysis()
        {
            EditorGUILayout.LabelField("=== BALANCE ANALYSIS ===", EditorStyles.boldLabel);
            
            // Example: Time to reach wall from spawn (assuming 25 unit distance)
            float timeToReach = _enemyData.GetTimeToReachWall(25f);
            
            // Example: Time to kill at 30 DPS (average hero)
            float timeToKill = _enemyData.GetTimeToKill(30f);
            
            // Damage to base if not killed
            float baseDamage = _enemyData.BaseDamagePerShot * _enemyData.FireRate * 10f; // 10 seconds of attacking
            
            string analysis = $"Time to reach wall (25m): {timeToReach:F1}s\n" +
                             $"Time to kill (30 DPS hero): {timeToKill:F1}s\n" +
                             $"Base damage (10s attack): {baseDamage:F0} HP\n\n";
            
            // Analysis
            if (timeToKill > timeToReach)
            {
                analysis += "⚠️ Warning: Enemy may reach wall before being killed!\n";
                analysis += "Consider: Lower HP, slower speed, or increase hero DPS.";
                EditorGUILayout.HelpBox(analysis, MessageType.Warning);
            }
            else
            {
                analysis += "✓ Good: Heroes have time to kill before wall reached.";
                EditorGUILayout.HelpBox(analysis, MessageType.Info);
            }
        }
        
        /// <summary>
        /// Draw quick preset buttons
        /// </summary>
        private void DrawPresetButtons()
        {
            EditorGUILayout.LabelField("=== QUICK PRESETS ===", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Grunt\n(Weak)", GUILayout.Height(40)))
            {
                SetGruntPreset();
            }
            
            if (GUILayout.Button("Soldier\n(Normal)", GUILayout.Height(40)))
            {
                SetSoldierPreset();
            }
            
            if (GUILayout.Button("Gunner\n(Ranged)", GUILayout.Height(40)))
            {
                SetGunnerPreset();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Tank\n(High HP)", GUILayout.Height(40)))
            {
                SetTankPreset();
            }
            
            if (GUILayout.Button("Rusher\n(Fast)", GUILayout.Height(40)))
            {
                SetRusherPreset();
            }
            
            if (GUILayout.Button("Elite\n(Strong)", GUILayout.Height(40)))
            {
                SetElitePreset();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #region Preset Methods
        
        private void SetGruntPreset()
        {
            Undo.RecordObject(_enemyData, "Set Grunt Preset");
            
            _enemyData.EnemyName = "Grunt";
            _enemyData.Description = "Basic weak enemy. Low health, short range.";
            _enemyData.MaxHealth = 100;
            _enemyData.MovementSpeed = 3f;
            _enemyData.AttackRange = 3f;
            _enemyData.DamagePerShot = 5;
            _enemyData.FireRate = 1f;
            _enemyData.ProjectileSpeed = 12f;
            _enemyData.BaseDamagePerShot = 5;
            _enemyData.ScaleMultiplier = 1f;
            _enemyData.ThreatMultiplier = 1f;
            _enemyData.ProjectileColor = new Color(1f, 0.3f, 0.3f); // Light red
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Grunt preset to {_enemyData.name}");
        }
        
        private void SetSoldierPreset()
        {
            Undo.RecordObject(_enemyData, "Set Soldier Preset");
            
            _enemyData.EnemyName = "Soldier";
            _enemyData.Description = "Standard enemy unit. Balanced stats.";
            _enemyData.MaxHealth = 200;
            _enemyData.MovementSpeed = 3f;
            _enemyData.AttackRange = 8f;
            _enemyData.DamagePerShot = 10;
            _enemyData.FireRate = 1f;
            _enemyData.ProjectileSpeed = 15f;
            _enemyData.BaseDamagePerShot = 10;
            _enemyData.ScaleMultiplier = 1.1f;
            _enemyData.ThreatMultiplier = 1f;
            _enemyData.ProjectileColor = new Color(1f, 0.5f, 0f); // Orange
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Soldier preset to {_enemyData.name}");
        }
        
        private void SetGunnerPreset()
        {
            Undo.RecordObject(_enemyData, "Set Gunner Preset");
            
            _enemyData.EnemyName = "Gunner";
            _enemyData.Description = "Long-range sniper. High damage, low health.";
            _enemyData.MaxHealth = 120;
            _enemyData.MovementSpeed = 2.5f;
            _enemyData.AttackRange = 15f;
            _enemyData.DamagePerShot = 20;
            _enemyData.FireRate = 0.8f;
            _enemyData.ProjectileSpeed = 20f;
            _enemyData.BaseDamagePerShot = 15;
            _enemyData.ScaleMultiplier = 0.9f;
            _enemyData.ThreatMultiplier = 1.2f; // Long range makes it more dangerous
            _enemyData.ProjectileColor = new Color(1f, 1f, 0f); // Yellow
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Gunner preset to {_enemyData.name}");
        }
        
        private void SetTankPreset()
        {
            Undo.RecordObject(_enemyData, "Set Tank Preset");
            
            _enemyData.EnemyName = "Tank";
            _enemyData.Description = "Heavy armored unit. Very high health, slow.";
            _enemyData.MaxHealth = 500;
            _enemyData.MovementSpeed = 1.5f;
            _enemyData.AttackRange = 5f;
            _enemyData.DamagePerShot = 15;
            _enemyData.FireRate = 0.7f;
            _enemyData.ProjectileSpeed = 10f;
            _enemyData.BaseDamagePerShot = 20;
            _enemyData.ScaleMultiplier = 1.5f;
            _enemyData.ThreatMultiplier = 1.3f; // High HP makes it very threatening
            _enemyData.ProjectileColor = new Color(0.5f, 0.5f, 0.5f); // Gray
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Tank preset to {_enemyData.name}");
        }
        
        private void SetRusherPreset()
        {
            Undo.RecordObject(_enemyData, "Set Rusher Preset");
            
            _enemyData.EnemyName = "Rusher";
            _enemyData.Description = "Fast suicide unit. Low health, high speed.";
            _enemyData.MaxHealth = 80;
            _enemyData.MovementSpeed = 6f;
            _enemyData.AttackRange = 2f;
            _enemyData.DamagePerShot = 15;
            _enemyData.FireRate = 1.5f;
            _enemyData.ProjectileSpeed = 15f;
            _enemyData.BaseDamagePerShot = 25; // High base damage if reaches wall
            _enemyData.ScaleMultiplier = 0.8f;
            _enemyData.ThreatMultiplier = 1.4f; // Speed makes it very dangerous
            _enemyData.ProjectileColor = new Color(1f, 0f, 0f); // Bright red
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Rusher preset to {_enemyData.name}");
        }
        
        private void SetElitePreset()
        {
            Undo.RecordObject(_enemyData, "Set Elite Preset");
            
            _enemyData.EnemyName = "Elite";
            _enemyData.Description = "Elite enemy. High stats across the board.";
            _enemyData.MaxHealth = 300;
            _enemyData.MovementSpeed = 3.5f;
            _enemyData.AttackRange = 10f;
            _enemyData.DamagePerShot = 25;
            _enemyData.FireRate = 1.2f;
            _enemyData.ProjectileSpeed = 18f;
            _enemyData.BaseDamagePerShot = 20;
            _enemyData.ScaleMultiplier = 1.3f;
            _enemyData.ThreatMultiplier = 1.5f; // Overall strong = high threat
            _enemyData.ProjectileColor = new Color(0.8f, 0f, 1f); // Purple
            
            EditorUtility.SetDirty(_enemyData);
            Debug.Log($"[EnemyData] Applied Elite preset to {_enemyData.name}");
        }
        
        #endregion
    }
}
#endif
