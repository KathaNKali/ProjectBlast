using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// TPS Director - Core spawning system for ProjectBlast
    /// 
    /// Manages:
    /// - Global TPS (Threat Per Second) budget accumulation
    /// - Wave progression through StageConfigSO
    /// - Lane budget distribution
    /// - LaneSpawner coordination
    /// - Game state management (wave start/end, stage complete)
    /// 
    /// This is the central runtime manager that orchestrates all enemy spawning.
    /// Use MMSingleton pattern for global access.
    /// </summary>
    public class TPSDirector : MMSingleton<TPSDirector>
    {
        #region Configuration
        
        [Header("=== STAGE CONFIGURATION ===")]
        [Tooltip("Current stage configuration (waves, enemies, settings)")]
        public StageConfigSO CurrentStage;
        
        [Tooltip("Auto-start stage on Awake")]
        public bool AutoStartStage = true;
        
        [Header("=== TPS SETTINGS ===")]
        [Tooltip("Global TPS multiplier for difficulty adjustment (1.0 = normal)")]
        [Range(0.1f, 5f)]
        public float GlobalTPSMultiplier = 1f;
        
        [Tooltip("Enable TPS accumulation (false = pauses spawning)")]
        public bool EnableTPS = true;
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug logs")]
        public bool DebugMode = false;
        
        [Tooltip("Show debug UI overlay")]
        public bool ShowDebugUI = true;
        
        #endregion
        
        #region Runtime State
        
        // Current wave state
        private int _currentWaveIndex = -1;
        private WaveConfigSO _currentWave;
        private float _waveStartTime;
        private float _waveElapsedTime;
        private float _waveNormalizedTime; // 0-1
        private bool _isWaveActive = false;
        private bool _isBreakActive = false;
        
        // TPS budget
        private float _globalThreatBudget = 0f;
        private float _currentTPS = 0f;
        
        // Lane tracking
        private List<LaneSpawner> _laneSpawners = new List<LaneSpawner>();
        private float[] _laneBudgets; // Per-lane threat budgets
        
        // Statistics
        private int _totalEnemiesSpawned = 0;
        private int _totalEnemiesKilled = 0;
        private float _totalThreatSpent = 0f;
        
        // Stage state
        private bool _isStageActive = false;
        private bool _isStageComplete = false;
        private float _stageStartTime;
        
        #endregion
        
        #region Properties (Public Accessors)
        
        public bool IsStageActive => _isStageActive;
        public bool IsWaveActive => _isWaveActive;
        public bool IsBreakActive => _isBreakActive;
        public int CurrentWaveIndex => _currentWaveIndex;
        public int TotalWaves => CurrentStage != null ? CurrentStage.GetTotalWaves() : 0;
        public float WaveProgress => _waveNormalizedTime;
        public float CurrentTPS => _currentTPS;
        public float GlobalThreatBudget => _globalThreatBudget;
        public int TotalEnemiesSpawned => _totalEnemiesSpawned;
        public int TotalEnemiesKilled => _totalEnemiesKilled;
        public float TotalThreatSpent => _totalThreatSpent;
        
        #endregion
        
        #region Initialization
        
        protected override void Awake()
        {
            base.Awake();
            
            if (CurrentStage == null)
            {
                Debug.LogError("[TPSDirector] No CurrentStage assigned! Cannot start.");
                return;
            }
            
            InitializeLanes();
            
            if (AutoStartStage)
            {
                StartStage();
            }
        }
        
        /// <summary>
        /// Initialize lane spawners based on stage configuration
        /// </summary>
        private void InitializeLanes()
        {
            if (CurrentStage == null || CurrentStage.BattlefieldConfig == null)
            {
                Debug.LogError("[TPSDirector] Cannot initialize lanes - missing configuration!");
                return;
            }
            
            int laneCount = CurrentStage.GetLaneCount();
            _laneBudgets = new float[laneCount];
            
            // Create lane spawners
            for (int i = 0; i < laneCount; i++)
            {
                GameObject spawnerObj = new GameObject($"LaneSpawner_{i}");
                spawnerObj.transform.SetParent(transform);
                
                LaneSpawner spawner = spawnerObj.AddComponent<LaneSpawner>();
                spawner.Initialize(i, CurrentStage.BattlefieldConfig, this);
                
                _laneSpawners.Add(spawner);
            }
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Initialized {laneCount} lane spawners");
            }
        }
        
        #endregion
        
        #region Stage Management
        
        /// <summary>
        /// Start the stage - begins first wave
        /// </summary>
        public void StartStage()
        {
            if (CurrentStage == null)
            {
                Debug.LogError("[TPSDirector] Cannot start stage - no configuration!");
                return;
            }
            
            if (_isStageActive)
            {
                Debug.LogWarning("[TPSDirector] Stage already active!");
                return;
            }
            
            _isStageActive = true;
            _isStageComplete = false;
            _stageStartTime = Time.time;
            _currentWaveIndex = -1;
            _totalEnemiesSpawned = 0;
            _totalEnemiesKilled = 0;
            _totalThreatSpent = 0f;
            
            // Trigger stage start event
            TPSEvent.TriggerStageEvent(TPSEventType.StageStarted, CurrentStage.StageNumber);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Stage {CurrentStage.StageNumber} started - {CurrentStage.StageName}");
            }
            
            // Start first wave
            StartNextWave();
        }
        
        /// <summary>
        /// Stop the stage
        /// </summary>
        public void StopStage()
        {
            if (!_isStageActive) return;
            
            _isStageActive = false;
            _isWaveActive = false;
            _isBreakActive = false;
            
            // Stop all spawners
            foreach (var spawner in _laneSpawners)
            {
                spawner.StopSpawning();
            }
            
            TPSEvent.TriggerStageEvent(TPSEventType.StageStopped, CurrentStage.StageNumber);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Stage {CurrentStage.StageNumber} stopped");
            }
        }
        
        /// <summary>
        /// Complete the stage (all waves finished)
        /// </summary>
        private void CompleteStage()
        {
            if (_isStageComplete) return;
            
            _isStageComplete = true;
            _isStageActive = false;
            _isWaveActive = false;
            
            float stageDuration = Time.time - _stageStartTime;
            
            TPSEvent.TriggerStageEvent(TPSEventType.StageCompleted, CurrentStage.StageNumber);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Stage {CurrentStage.StageNumber} completed! " +
                         $"Duration: {stageDuration:F1}s, Enemies: {_totalEnemiesSpawned}, " +
                         $"Threat: {_totalThreatSpent:F0}");
            }
            
            // TODO: Trigger rewards, level progression, etc.
        }
        
        #endregion
        
        #region Wave Management
        
        /// <summary>
        /// Start the next wave in sequence
        /// </summary>
        private void StartNextWave()
        {
            _currentWaveIndex++;
            
            if (_currentWaveIndex >= CurrentStage.Waves.Count)
            {
                // All waves complete
                CompleteStage();
                return;
            }
            
            _currentWave = CurrentStage.GetWave(_currentWaveIndex);
            
            if (_currentWave == null)
            {
                Debug.LogError($"[TPSDirector] Wave {_currentWaveIndex} is null!");
                CompleteStage();
                return;
            }
            
            StartWave(_currentWave);
        }
        
        /// <summary>
        /// Start a specific wave
        /// </summary>
        private void StartWave(WaveConfigSO wave)
        {
            _currentWave = wave;
            _waveStartTime = Time.time;
            _waveElapsedTime = 0f;
            _waveNormalizedTime = 0f;
            _isWaveActive = true;
            _isBreakActive = false;
            _globalThreatBudget = 0f; // Reset budget for new wave
            
            // Reset lane budgets
            for (int i = 0; i < _laneBudgets.Length; i++)
            {
                _laneBudgets[i] = 0f;
            }
            
            // Notify spawners of new wave
            foreach (var spawner in _laneSpawners)
            {
                spawner.OnWaveStarted(wave);
            }
            
            TPSEvent.TriggerWaveEvent(TPSEventType.WaveStarted, _currentWaveIndex);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Wave {_currentWaveIndex + 1}/{CurrentStage.Waves.Count} started - " +
                         $"{wave.WaveName} (TPS: {wave.StartingTPS:F0}→{wave.PeakTPS:F0})");
            }
        }
        
        /// <summary>
        /// End the current wave
        /// </summary>
        private void EndWave()
        {
            if (!_isWaveActive) return;
            
            _isWaveActive = false;
            
            TPSEvent.TriggerWaveEvent(TPSEventType.WaveEnded, _currentWaveIndex);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Wave {_currentWaveIndex + 1} ended - " +
                         $"Enemies spawned: {_totalEnemiesSpawned}, Budget spent: {_totalThreatSpent:F0}");
            }
            
            // Start break before next wave
            if (_currentWave.BreakAfterWave > 0)
            {
                StartBreak(_currentWave.BreakAfterWave);
            }
            else
            {
                StartNextWave();
            }
        }
        
        /// <summary>
        /// Start break period between waves
        /// </summary>
        private void StartBreak(float duration)
        {
            _isBreakActive = true;
            
            TPSEvent.TriggerWaveEvent(TPSEventType.BreakStarted, _currentWaveIndex);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Break started - {duration}s");
            }
            
            Invoke(nameof(EndBreak), duration);
        }
        
        /// <summary>
        /// End break period
        /// </summary>
        private void EndBreak()
        {
            _isBreakActive = false;
            
            TPSEvent.TriggerWaveEvent(TPSEventType.BreakEnded, _currentWaveIndex);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Break ended - starting next wave");
            }
            
            StartNextWave();
        }
        
        #endregion
        
        #region Update Loop
        
        private void Update()
        {
            if (!_isStageActive || !_isWaveActive) return;
            
            UpdateWaveTimer();
            UpdateTPS();
            DistributeBudget();
        }
        
        /// <summary>
        /// Update wave timing
        /// </summary>
        private void UpdateWaveTimer()
        {
            _waveElapsedTime = Time.time - _waveStartTime;
            _waveNormalizedTime = Mathf.Clamp01(_waveElapsedTime / _currentWave.WaveDuration);
            
            // Check if wave duration elapsed
            if (_waveElapsedTime >= _currentWave.WaveDuration)
            {
                EndWave();
            }
        }
        
        /// <summary>
        /// Update TPS and accumulate global threat budget
        /// </summary>
        private void UpdateTPS()
        {
            if (!EnableTPS) return;
            
            // Get current TPS from wave curve
            _currentTPS = _currentWave.GetTPSAtTime(_waveNormalizedTime);
            _currentTPS *= GlobalTPSMultiplier;
            
            // Accumulate threat budget
            float threatThisFrame = _currentTPS * Time.deltaTime;
            _globalThreatBudget += threatThisFrame;
        }
        
        /// <summary>
        /// Distribute global budget to lanes
        /// </summary>
        private void DistributeBudget()
        {
            // Distribute budget based on wave's lane multipliers
            for (int i = 0; i < _laneBudgets.Length && i < _currentWave.LaneTPS_Multipliers.Length; i++)
            {
                float laneShare = _globalThreatBudget * _currentWave.LaneTPS_Multipliers[i];
                _laneBudgets[i] = laneShare;
            }
            
            // Send budgets to spawners
            for (int i = 0; i < _laneSpawners.Count; i++)
            {
                _laneSpawners[i].ReceiveThreatBudget(_laneBudgets[i]);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Called by LaneSpawner when an enemy is spawned
        /// </summary>
        public void OnEnemySpawned(int laneIndex, EnemyDataSO enemyData)
        {
            _totalEnemiesSpawned++;
            _totalThreatSpent += enemyData.ThreatValue;
            
            // Deduct from global budget
            _globalThreatBudget -= enemyData.ThreatValue;
            _globalThreatBudget = Mathf.Max(0, _globalThreatBudget);
            
            TPSEvent.TriggerEnemyEvent(TPSEventType.EnemySpawned, laneIndex, enemyData);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Enemy spawned: {enemyData.EnemyName} in Lane {laneIndex} " +
                         $"(Threat: {enemyData.ThreatValue:F0}, Remaining budget: {_globalThreatBudget:F0})");
            }
        }
        
        /// <summary>
        /// Called when an enemy is killed
        /// </summary>
        public void OnEnemyKilled(int laneIndex, EnemyDataSO enemyData)
        {
            _totalEnemiesKilled++;
            
            TPSEvent.TriggerEnemyEvent(TPSEventType.EnemyKilled, laneIndex, enemyData);
            
            if (DebugMode)
            {
                Debug.Log($"[TPSDirector] Enemy killed: {enemyData.EnemyName} in Lane {laneIndex}");
            }
        }
        
        /// <summary>
        /// Pause TPS accumulation
        /// </summary>
        public void PauseTPS()
        {
            EnableTPS = false;
            if (DebugMode) Debug.Log("[TPSDirector] TPS paused");
        }
        
        /// <summary>
        /// Resume TPS accumulation
        /// </summary>
        public void ResumeTPS()
        {
            EnableTPS = true;
            if (DebugMode) Debug.Log("[TPSDirector] TPS resumed");
        }
        
        /// <summary>
        /// Force advance to next wave (cheat/debug)
        /// </summary>
        public void SkipToNextWave()
        {
            if (!_isStageActive) return;
            
            if (_isBreakActive)
            {
                CancelInvoke(nameof(EndBreak));
                EndBreak();
            }
            else if (_isWaveActive)
            {
                EndWave();
            }
        }
        
        #endregion
        
        #region Debug UI
        
        private void OnGUI()
        {
            if (!ShowDebugUI || !_isStageActive) return;
            
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            
            string debugText = $"=== TPS DIRECTOR DEBUG ===\n" +
                              $"Stage: {CurrentStage.StageName} ({CurrentStage.StageNumber})\n" +
                              $"Wave: {_currentWaveIndex + 1}/{CurrentStage.Waves.Count} - {(_currentWave != null ? _currentWave.WaveName : "None")}\n" +
                              $"Status: {(_isWaveActive ? "ACTIVE" : (_isBreakActive ? "BREAK" : "IDLE"))}\n\n" +
                              $"Time: {_waveElapsedTime:F1}s / {(_currentWave != null ? _currentWave.WaveDuration : 0):F0}s ({_waveNormalizedTime * 100:F0}%)\n" +
                              $"Current TPS: {_currentTPS:F1}\n" +
                              $"Global Budget: {_globalThreatBudget:F0}\n\n" +
                              $"Enemies Spawned: {_totalEnemiesSpawned}\n" +
                              $"Enemies Killed: {_totalEnemiesKilled}\n" +
                              $"Threat Spent: {_totalThreatSpent:F0}\n\n";
            
            // Lane budgets
            debugText += "Lane Budgets:\n";
            for (int i = 0; i < _laneBudgets.Length; i++)
            {
                debugText += $"  Lane {i}: {_laneBudgets[i]:F0}\n";
            }
            
            GUI.Box(new Rect(10, 10, 300, 300), debugText, style);
        }
        
        #endregion
    }
}
