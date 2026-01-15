using UnityEngine;
using System.Collections.Generic;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Stage Configuration ScriptableObject
    /// 
    /// Defines a complete stage/level including:
    /// - Multiple waves in sequence
    /// - Battlefield layout configuration
    /// - Stage-specific settings (base health, lane count)
    /// - Win/loss conditions
    /// - Rewards and progression
    /// 
    /// Stages are the top-level configuration that ties together all combat systems.
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_", menuName = "ProjectBlast/Combat/Stage Config")]
    public class StageConfigSO : ScriptableObject
    {
        #region Stage Identity
        
        [Header("=== STAGE IDENTITY ===")]
        [Tooltip("Stage number in campaign progression")]
        [Min(1)]
        public int StageNumber = 1;
        
        [Tooltip("Display name of this stage")]
        public string StageName = "Stage 1";
        
        [TextArea(3, 5)]
        [Tooltip("Stage description/story")]
        public string Description = "Your first battle!";
        
        [Tooltip("Stage difficulty rating")]
        public StageDifficulty Difficulty = StageDifficulty.Normal;
        
        #endregion
        
        #region Battlefield Configuration
        
        [Header("=== BATTLEFIELD CONFIGURATION ===")]
        [Tooltip("Battlefield spatial configuration for this stage")]
        public BattlefieldConfigSO BattlefieldConfig;
        
        [Header("Stage-Specific Overrides (0 = use BattlefieldConfig)")]
        [Tooltip("Override enemy spawn Z position")]
        public float EnemySpawnZ_Override = 0f;
        
        [Tooltip("Override wall Z position")]
        public float BaseWallZ_Override = 0f;
        
        [Tooltip("Override lane count (0 = use BattlefieldConfig)")]
        [Range(0, 5)]
        public int LaneCount_Override = 0;
        
        /// <summary>
        /// Get enemy spawn Z with override logic
        /// </summary>
        public float GetEnemySpawnZ()
        {
            if (BattlefieldConfig == null)
            {
                Debug.LogError($"[StageConfig] {name}: No BattlefieldConfig assigned!");
                return 20f;
            }
            return EnemySpawnZ_Override != 0 ? EnemySpawnZ_Override : BattlefieldConfig.EnemySpawnZ;
        }
        
        /// <summary>
        /// Get wall Z position with override logic
        /// </summary>
        public float GetBaseWallZ()
        {
            if (BattlefieldConfig == null)
            {
                Debug.LogError($"[StageConfig] {name}: No BattlefieldConfig assigned!");
                return -5f;
            }
            return BaseWallZ_Override != 0 ? BaseWallZ_Override : BattlefieldConfig.BaseWallZ;
        }
        
        /// <summary>
        /// Get lane count with override logic
        /// </summary>
        public int GetLaneCount()
        {
            if (BattlefieldConfig == null)
            {
                Debug.LogError($"[StageConfig] {name}: No BattlefieldConfig assigned!");
                return 3;
            }
            return LaneCount_Override != 0 ? LaneCount_Override : BattlefieldConfig.LaneCount;
        }
        
        #endregion
        
        #region Wave Configuration
        
        [Header("=== WAVE CONFIGURATION ===")]
        [Tooltip("Waves in sequence for this stage")]
        public List<WaveConfigSO> Waves = new List<WaveConfigSO>();
        
        [Tooltip("Total number of waves (auto-calculated)")]
        [SerializeField] private int _totalWaves;
        
        [Tooltip("Total stage duration (auto-calculated)")]
        [SerializeField] private float _totalDuration;
        
        [Tooltip("Loop waves infinitely (endless mode)")]
        public bool EndlessMode = false;
        
        /// <summary>
        /// Get wave by index
        /// </summary>
        public WaveConfigSO GetWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= Waves.Count)
            {
                if (EndlessMode && Waves.Count > 0)
                {
                    // Loop back to start in endless mode
                    return Waves[waveIndex % Waves.Count];
                }
                
                Debug.LogWarning($"[StageConfig] {name}: Invalid wave index {waveIndex}");
                return null;
            }
            
            return Waves[waveIndex];
        }
        
        /// <summary>
        /// Get total wave count
        /// </summary>
        public int GetTotalWaves()
        {
            return EndlessMode ? int.MaxValue : Waves.Count;
        }
        
        #endregion
        
        #region Stage Stats
        
        [Header("=== STAGE STATS ===")]
        [Tooltip("Player base health for this stage")]
        [Range(100, 10000)]
        public int BaseHealth = 500;
        
        [Tooltip("Starting hero count (for testing)")]
        [Range(0, 20)]
        public int StartingHeroCount = 5;
        
        [Tooltip("Required star rating to unlock next stage")]
        [Range(1, 3)]
        public int UnlockStars = 1;
        
        #endregion
        
        #region Win/Loss Conditions
        
        [Header("=== WIN/LOSS CONDITIONS ===")]
        [Tooltip("Win condition type")]
        public WinConditionType WinCondition = WinConditionType.SurviveAllWaves;
        
        [Tooltip("Required survival time for time-based win (seconds)")]
        [Min(30)]
        public float RequiredSurvivalTime = 180f;
        
        [Tooltip("Required enemy kills for kill-based win")]
        [Min(10)]
        public int RequiredKills = 50;
        
        [Tooltip("Allow retrying after loss")]
        public bool AllowRetry = true;
        
        #endregion
        
        #region Rewards & Progression
        
        [Header("=== REWARDS & PROGRESSION ===")]
        [Tooltip("Currency earned for completing stage")]
        [Min(0)]
        public int CurrencyReward = 100;
        
        [Tooltip("Experience points earned")]
        [Min(0)]
        public int ExperienceReward = 50;
        
        [Tooltip("Hero unlock rewards")]
        public List<GameObject> HeroUnlocks = new List<GameObject>();
        
        [Tooltip("Next stage to unlock on completion")]
        public StageConfigSO NextStage;
        
        #endregion
        
        #region Validation
        
        private void OnValidate()
        {
            CalculateStageTotals();
            ValidateConfiguration();
        }
        
        private void CalculateStageTotals()
        {
            _totalWaves = Waves.Count;
            _totalDuration = 0f;
            
            foreach (var wave in Waves)
            {
                if (wave != null)
                {
                    _totalDuration += wave.WaveDuration + wave.BreakAfterWave;
                }
            }
        }
        
        private void ValidateConfiguration()
        {
            // Check battlefield config
            if (BattlefieldConfig == null)
            {
                Debug.LogWarning($"[StageConfig] {name}: No BattlefieldConfig assigned!");
            }
            
            // Check waves
            if (Waves.Count == 0)
            {
                Debug.LogWarning($"[StageConfig] {name}: No waves configured!");
            }
            
            // Check for null waves
            for (int i = 0; i < Waves.Count; i++)
            {
                if (Waves[i] == null)
                {
                    Debug.LogWarning($"[StageConfig] {name}: Wave at index {i} is null!");
                }
            }
            
            // Check lane count override matches waves
            int laneCount = GetLaneCount();
            foreach (var wave in Waves)
            {
                if (wave != null && wave.LaneTPS_Multipliers.Length != laneCount)
                {
                    Debug.LogWarning($"[StageConfig] {name}: Wave '{wave.name}' lane count ({wave.LaneTPS_Multipliers.Length}) doesn't match stage lane count ({laneCount})");
                }
            }
            
            // Check base health
            if (BaseHealth < 100)
            {
                Debug.LogWarning($"[StageConfig] {name}: Very low base health ({BaseHealth}). Stage may be too difficult.");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get formatted stage summary
        /// </summary>
        public string GetStageSummary()
        {
            string summary = $"=== {StageName.ToUpper()} ===\n";
            summary += $"Stage {StageNumber} - {Difficulty}\n";
            summary += $"{Description}\n\n";
            
            summary += $"Configuration:\n";
            summary += $"  • Base Health: {BaseHealth} HP\n";
            summary += $"  • Lanes: {GetLaneCount()}\n";
            summary += $"  • Waves: {_totalWaves}\n";
            summary += $"  • Duration: {_totalDuration:F0}s ({_totalDuration / 60f:F1} min)\n\n";
            
            summary += $"Rewards:\n";
            summary += $"  • Currency: {CurrencyReward}\n";
            summary += $"  • Experience: {ExperienceReward}\n";
            
            if (HeroUnlocks.Count > 0)
            {
                summary += $"  • Hero Unlocks: {HeroUnlocks.Count}\n";
            }
            
            return summary;
        }
        
        /// <summary>
        /// Calculate total threat budget across all waves
        /// </summary>
        public float CalculateTotalThreat()
        {
            float totalThreat = 0f;
            
            foreach (var wave in Waves)
            {
                if (wave != null)
                {
                    totalThreat += wave.CalculateTotalThreat();
                }
            }
            
            return totalThreat;
        }
        
        /// <summary>
        /// Get wave progression percentage
        /// </summary>
        /// <param name="currentWave">Current wave index</param>
        /// <returns>Progress 0-1</returns>
        public float GetWaveProgress(int currentWave)
        {
            if (EndlessMode) return 0f; // No progress in endless mode
            if (Waves.Count == 0) return 1f;
            
            return Mathf.Clamp01((currentWave + 1) / (float)Waves.Count);
        }
        
        /// <summary>
        /// Check if stage configuration is valid
        /// </summary>
        public bool IsValid()
        {
            return BattlefieldConfig != null && Waves.Count > 0 && BaseHealth > 0;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Stage difficulty levels
    /// </summary>
    public enum StageDifficulty
    {
        Tutorial,
        Easy,
        Normal,
        Hard,
        Expert,
        Nightmare
    }
    
    /// <summary>
    /// Win condition types
    /// </summary>
    public enum WinConditionType
    {
        SurviveAllWaves,    // Complete all waves without losing base
        SurviveTime,        // Survive for X seconds
        KillEnemies,        // Kill X enemies
        Hybrid              // Combination of above
    }
}
