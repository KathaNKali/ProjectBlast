using UnityEngine;
using System.Collections.Generic;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Wave Configuration ScriptableObject
    /// 
    /// Defines a single wave's spawning behavior including:
    /// - TPS (Threat Per Second) values and progression curve
    /// - Duration and timing
    /// - Lane-based budget allocation
    /// - Enemy types and spawn weights
    /// - Wave completion conditions
    /// 
    /// Multiple WaveConfigs are used in a StageConfigSO to create multi-wave levels.
    /// </summary>
    [CreateAssetMenu(fileName = "Wave_", menuName = "ProjectBlast/Combat/Wave Config")]
    public class WaveConfigSO : ScriptableObject
    {
        #region Wave Identity
        
        [Header("=== WAVE IDENTITY ===")]
        [Tooltip("Wave number in stage sequence")]
        [Min(1)]
        public int WaveNumber = 1;
        
        [Tooltip("Display name of this wave")]
        public string WaveName = "Wave 1";
        
        [TextArea(2, 3)]
        [Tooltip("Description of this wave (e.g., 'Warm-up wave with weak enemies')")]
        public string Description = "Standard wave";
        
        #endregion
        
        #region TPS Configuration
        
        [Header("=== TPS CONFIGURATION ===")]
        [Tooltip("Starting TPS (Threat Per Second) at wave start")]
        [Range(10f, 500f)]
        public float StartingTPS = 30f;
        
        [Tooltip("Peak TPS reached during wave (if curve reaches 1.0)")]
        [Range(10f, 500f)]
        public float PeakTPS = 60f;
        
        [Tooltip("TPS curve over wave duration (X=time 0-1, Y=TPS 0-1)")]
        public AnimationCurve TPSCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("Current TPS value (auto-calculated during wave)")]
        [SerializeField] private float _currentTPS;
        
        /// <summary>
        /// Get current TPS at normalized time (0-1)
        /// </summary>
        public float GetTPSAtTime(float normalizedTime)
        {
            float curveValue = TPSCurve.Evaluate(normalizedTime);
            return Mathf.Lerp(StartingTPS, PeakTPS, curveValue);
        }
        
        #endregion
        
        #region Wave Duration
        
        [Header("=== WAVE DURATION ===")]
        [Tooltip("Wave duration in seconds")]
        [Range(15f, 300f)]
        public float WaveDuration = 45f;
        
        [Tooltip("Break time before next wave starts (seconds)")]
        [Range(0f, 30f)]
        public float BreakAfterWave = 10f;
        
        #endregion
        
        #region Lane Budget Allocation
        
        [Header("=== LANE BUDGET ALLOCATION ===")]
        [Tooltip("Per-lane TPS multipliers (should sum to 1.0 for even distribution)")]
        public float[] LaneTPS_Multipliers = new float[] { 0.33f, 0.34f, 0.33f };
        
        [Tooltip("Auto-calculate even distribution based on lane count")]
        public bool AutoEvenDistribution = true;
        
        /// <summary>
        /// Get TPS allocation for a specific lane
        /// </summary>
        public float GetLaneTPS(int laneIndex, float globalTPS)
        {
            if (laneIndex < 0 || laneIndex >= LaneTPS_Multipliers.Length)
            {
                Debug.LogWarning($"[WaveConfig] Invalid lane index: {laneIndex}");
                return globalTPS / LaneTPS_Multipliers.Length;
            }
            
            return globalTPS * LaneTPS_Multipliers[laneIndex];
        }
        
        /// <summary>
        /// Update lane multipliers for even distribution
        /// </summary>
        public void SetEvenDistribution(int laneCount)
        {
            LaneTPS_Multipliers = new float[laneCount];
            float evenValue = 1f / laneCount;
            
            for (int i = 0; i < laneCount; i++)
            {
                LaneTPS_Multipliers[i] = evenValue;
            }
        }
        
        #endregion
        
        #region Enemy Configuration
        
        [Header("=== ENEMY CONFIGURATION ===")]
        [Tooltip("Enemy types allowed to spawn in this wave")]
        public List<EnemyDataSO> AllowedEnemies = new List<EnemyDataSO>();
        
        [Tooltip("Spawn weights for each enemy - higher = more likely (order matches AllowedEnemies)")]
        public List<float> EnemyWeights = new List<float>();
        
        /// <summary>
        /// Get weighted random enemy from allowed list
        /// </summary>
        public EnemyDataSO GetRandomEnemy()
        {
            if (AllowedEnemies == null || AllowedEnemies.Count == 0)
            {
                Debug.LogError($"[WaveConfig] {name}: No enemies configured!");
                return null;
            }
            
            // If no weights or mismatched count, use even distribution
            if (EnemyWeights == null || EnemyWeights.Count != AllowedEnemies.Count)
            {
                int randomIndex = Random.Range(0, AllowedEnemies.Count);
                return AllowedEnemies[randomIndex];
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (float weight in EnemyWeights)
            {
                totalWeight += weight;
            }
            
            // Pick random value
            float randomValue = Random.Range(0f, totalWeight);
            
            // Find enemy based on weight
            float currentWeight = 0f;
            for (int i = 0; i < AllowedEnemies.Count; i++)
            {
                currentWeight += EnemyWeights[i];
                if (randomValue <= currentWeight)
                {
                    return AllowedEnemies[i];
                }
            }
            
            // Fallback to last enemy
            return AllowedEnemies[AllowedEnemies.Count - 1];
        }
        
        /// <summary>
        /// Get enemy that can be afforded with given budget
        /// </summary>
        public EnemyDataSO GetAffordableEnemy(float availableBudget)
        {
            List<EnemyDataSO> affordableEnemies = new List<EnemyDataSO>();
            List<float> affordableWeights = new List<float>();
            
            // Filter enemies by budget
            for (int i = 0; i < AllowedEnemies.Count; i++)
            {
                if (AllowedEnemies[i].ThreatValue <= availableBudget)
                {
                    affordableEnemies.Add(AllowedEnemies[i]);
                    
                    // Add weight if available
                    if (i < EnemyWeights.Count)
                    {
                        affordableWeights.Add(EnemyWeights[i]);
                    }
                    else
                    {
                        affordableWeights.Add(1f);
                    }
                }
            }
            
            // No affordable enemies
            if (affordableEnemies.Count == 0)
            {
                return null;
            }
            
            // Pick weighted random from affordable list
            float totalWeight = 0f;
            foreach (float weight in affordableWeights)
            {
                totalWeight += weight;
            }
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            for (int i = 0; i < affordableEnemies.Count; i++)
            {
                currentWeight += affordableWeights[i];
                if (randomValue <= currentWeight)
                {
                    return affordableEnemies[i];
                }
            }
            
            return affordableEnemies[affordableEnemies.Count - 1];
        }
        
        #endregion
        
        #region Wave Completion
        
        [Header("=== WAVE COMPLETION ===")]
        [Tooltip("Has a maximum enemy spawn limit")]
        public bool HasEnemyLimit = false;
        
        [Tooltip("Maximum enemies to spawn (if limited)")]
        [Min(1)]
        public int MaxEnemiesSpawned = 20;
        
        [Tooltip("Must kill all enemies before next wave")]
        public bool MustClearWave = false;
        
        [Tooltip("Wave progression type")]
        public WaveProgressionType ProgressionType = WaveProgressionType.TimeBased;
        
        #endregion
        
        #region Validation
        
        private void OnValidate()
        {
            ValidateConfiguration();
            
            // Auto-update even distribution if enabled
            if (AutoEvenDistribution && LaneTPS_Multipliers.Length > 0)
            {
                SetEvenDistribution(LaneTPS_Multipliers.Length);
            }
            
            // Sync weights with enemies
            if (EnemyWeights.Count != AllowedEnemies.Count)
            {
                SyncEnemyWeights();
            }
        }
        
        private void ValidateConfiguration()
        {
            // Check TPS values
            if (StartingTPS > PeakTPS)
            {
                Debug.LogWarning($"[WaveConfig] {name}: Starting TPS ({StartingTPS}) is higher than Peak TPS ({PeakTPS}). This creates a declining wave.");
            }
            
            // Check lane multipliers sum
            float sum = 0f;
            foreach (float mult in LaneTPS_Multipliers)
            {
                sum += mult;
            }
            
            if (Mathf.Abs(sum - 1f) > 0.01f && !AutoEvenDistribution)
            {
                Debug.LogWarning($"[WaveConfig] {name}: Lane multipliers sum to {sum:F2} (should be 1.0 for proper distribution)");
            }
            
            // Check enemy configuration
            if (AllowedEnemies.Count == 0)
            {
                Debug.LogWarning($"[WaveConfig] {name}: No enemies configured!");
            }
            
            // Check for null enemies
            for (int i = 0; i < AllowedEnemies.Count; i++)
            {
                if (AllowedEnemies[i] == null)
                {
                    Debug.LogWarning($"[WaveConfig] {name}: Enemy at index {i} is null!");
                }
            }
        }
        
        private void SyncEnemyWeights()
        {
            // Resize weights list to match enemies
            while (EnemyWeights.Count < AllowedEnemies.Count)
            {
                EnemyWeights.Add(1f);
            }
            
            while (EnemyWeights.Count > AllowedEnemies.Count)
            {
                EnemyWeights.RemoveAt(EnemyWeights.Count - 1);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get formatted summary of wave configuration
        /// </summary>
        public string GetWaveSummary()
        {
            string summary = $"=== {WaveName.ToUpper()} ===\n";
            summary += $"Duration: {WaveDuration}s\n";
            summary += $"TPS: {StartingTPS:F0} → {PeakTPS:F0}\n";
            summary += $"Break After: {BreakAfterWave}s\n\n";
            
            summary += $"Enemies ({AllowedEnemies.Count}):\n";
            for (int i = 0; i < AllowedEnemies.Count; i++)
            {
                if (AllowedEnemies[i] != null)
                {
                    float weight = i < EnemyWeights.Count ? EnemyWeights[i] : 1f;
                    float percentage = (weight / GetTotalWeight()) * 100f;
                    summary += $"  • {AllowedEnemies[i].EnemyName} ({percentage:F0}%)\n";
                }
            }
            
            return summary;
        }
        
        private float GetTotalWeight()
        {
            float total = 0f;
            foreach (float weight in EnemyWeights)
            {
                total += weight;
            }
            return total > 0 ? total : 1f;
        }
        
        /// <summary>
        /// Calculate total threat budget over wave duration
        /// </summary>
        public float CalculateTotalThreat()
        {
            // Approximate by sampling curve at intervals
            float totalThreat = 0f;
            int samples = 100;
            
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float tps = GetTPSAtTime(t);
                float timeSlice = WaveDuration / samples;
                totalThreat += tps * timeSlice;
            }
            
            return totalThreat;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Wave progression type
    /// </summary>
    public enum WaveProgressionType
    {
        TimeBased,      // Progress after duration expires
        ClearBased,     // Progress after all enemies killed
        Hybrid          // Progress after duration OR all enemies killed
    }
}
