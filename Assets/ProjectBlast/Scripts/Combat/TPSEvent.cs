using UnityEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Event types for TPS system communication
    /// </summary>
    public enum TPSEventType
    {
        StageStarted,       // Stage begins
        StageStopped,       // Stage manually stopped
        StageCompleted,     // Stage finished (all waves complete)
        
        WaveStarted,        // New wave begins
        WaveEnded,          // Wave duration elapsed
        
        BreakStarted,       // Break between waves begins
        BreakEnded,         // Break ends
        
        EnemySpawned,       // Enemy spawned by LaneSpawner
        EnemyKilled,        // Enemy killed by player/hero
        
        TPSChanged,         // Current TPS value changed
        BudgetChanged       // Threat budget changed
    }
    
    /// <summary>
    /// TPS system event - communicates between TPSDirector, spawners, and game systems
    /// 
    /// Usage:
    /// - Listen: Implement MMEventListener<TPSEvent> interface
    /// - Trigger: TPSEvent.Trigger(TPSEventType.EnemySpawned, laneIndex, enemyData)
    /// </summary>
    public struct TPSEvent
    {
        // Event type
        public TPSEventType EventType;
        
        // Context data
        public int StageNumber;
        public int WaveIndex;
        public int LaneIndex;
        public EnemyDataSO EnemyData;
        public float TPSValue;
        public float BudgetValue;
        
        // Timestamp
        public float Timestamp;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TPSEvent(TPSEventType eventType)
        {
            EventType = eventType;
            StageNumber = -1;
            WaveIndex = -1;
            LaneIndex = -1;
            EnemyData = null;
            TPSValue = 0f;
            BudgetValue = 0f;
            Timestamp = Time.time;
        }
        
        #region Static Trigger Methods
        
        /// <summary>
        /// Trigger stage event
        /// </summary>
        public static void TriggerStageEvent(TPSEventType eventType, int stageNumber)
        {
            TPSEvent evt = new TPSEvent(eventType);
            evt.StageNumber = stageNumber;
            MMEventManager.TriggerEvent(evt);
        }
        
        /// <summary>
        /// Trigger wave event
        /// </summary>
        public static void TriggerWaveEvent(TPSEventType eventType, int waveIndex)
        {
            TPSEvent evt = new TPSEvent(eventType);
            evt.WaveIndex = waveIndex;
            MMEventManager.TriggerEvent(evt);
        }
        
        /// <summary>
        /// Trigger enemy event
        /// </summary>
        public static void TriggerEnemyEvent(TPSEventType eventType, int laneIndex, EnemyDataSO enemyData)
        {
            TPSEvent evt = new TPSEvent(eventType);
            evt.LaneIndex = laneIndex;
            evt.EnemyData = enemyData;
            MMEventManager.TriggerEvent(evt);
        }
        
        /// <summary>
        /// Trigger TPS change event
        /// </summary>
        public static void TriggerTPSChange(float newTPS)
        {
            TPSEvent evt = new TPSEvent(TPSEventType.TPSChanged);
            evt.TPSValue = newTPS;
            MMEventManager.TriggerEvent(evt);
        }
        
        /// <summary>
        /// Trigger budget change event
        /// </summary>
        public static void TriggerBudgetChange(float newBudget)
        {
            TPSEvent evt = new TPSEvent(TPSEventType.BudgetChanged);
            evt.BudgetValue = newBudget;
            MMEventManager.TriggerEvent(evt);
        }
        
        #endregion
    }
}
