using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Enemy
{
    /// <summary>
    /// Simple enemy spawner for testing hero shooting and detection systems.
    /// Spawns enemies with randomized health in a specified area.
    /// 
    /// AI INTEGRATION:
    /// - Automatically sets enemy AIBrain.Target to PlayerBase
    /// - Finds PlayerBase by tag or uses assigned TargetTransform
    /// - Enemy prefab must have AIBrain component for movement AI
    /// </summary>
    [AddComponentMenu("ProjectBlast/Enemy/Simple Enemy Spawner")]
    public class SimpleEnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [Tooltip("The enemy prefab to spawn (must have Health component and AIBrain)")]
        public GameObject EnemyPrefab;
        
        [Tooltip("Number of enemies to spawn")]
        public int SpawnCount = 5;
        
        [Tooltip("Spawn all enemies immediately on start")]
        public bool SpawnOnStart = true;
        
        [Header("AI Target Configuration")]
        [Tooltip("Target for enemy AI (usually PlayerBase). If null, will find by tag.")]
        public Transform TargetTransform;
        
        [Tooltip("Tag to search for if TargetTransform not assigned")]
        public string TargetTag = "PlayerBase";
        
        [Header("Spawn Area")]
        [Tooltip("The center point of the spawn area (if null, uses this transform)")]
        public Transform SpawnCenter;
        
        [Tooltip("The size of the rectangular spawn area")]
        public Vector3 SpawnAreaSize = new Vector3(10f, 0f, 10f);
        
        [Tooltip("Minimum distance between spawned enemies")]
        public float MinDistanceBetweenEnemies = 2f;
        
        [Header("Enemy Health Randomization")]
        [Tooltip("Minimum health for spawned enemies")]
        public int MinHealth = 50;
        
        [Tooltip("Maximum health for spawned enemies")]
        public int MaxHealth = 150;
        
        [Header("Spawn Timing (if not spawning on start)")]
        [Tooltip("Delay before starting to spawn (seconds)")]
        public float InitialDelay = 0f;
        
        [Tooltip("Interval between spawning each enemy (seconds)")]
        public float SpawnInterval = 1f;
        
        [Header("Debug")]
        [Tooltip("Show spawn area gizmo in editor")]
        public bool ShowSpawnArea = true;
        
        [Tooltip("Log spawn information to console")]
        public bool DebugMode = false;
        
        [MMInspectorButton("TestSpawn")]
        public bool TestSpawnButton;
        
        private int _spawnedCount = 0;
        private float _nextSpawnTime = 0f;
        private bool _isSpawning = false;
        private Vector3 _spawnCenterPosition;

        /// <summary>
        /// Initialization
        /// </summary>
        protected virtual void Start()
        {
            // Set spawn center
            _spawnCenterPosition = SpawnCenter != null ? SpawnCenter.position : transform.position;
            
            // Auto-find target if not assigned
            if (TargetTransform == null && !string.IsNullOrEmpty(TargetTag))
            {
                GameObject targetObj = GameObject.FindGameObjectWithTag(TargetTag);
                if (targetObj != null)
                {
                    TargetTransform = targetObj.transform;
                    if (DebugMode)
                    {
                        Debug.Log($"[SimpleEnemySpawner] Auto-found target: {TargetTransform.name} at {TargetTransform.position}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SimpleEnemySpawner] Could not find GameObject with tag '{TargetTag}'. Enemies may not move!");
                }
            }
            
            if (SpawnOnStart)
            {
                if (InitialDelay > 0)
                {
                    Invoke(nameof(StartSpawning), InitialDelay);
                }
                else
                {
                    StartSpawning();
                }
            }
        }

        /// <summary>
        /// Update loop for timed spawning
        /// </summary>
        protected virtual void Update()
        {
            if (!_isSpawning || _spawnedCount >= SpawnCount)
            {
                return;
            }
            
            if (Time.time >= _nextSpawnTime)
            {
                SpawnEnemy();
                _nextSpawnTime = Time.time + SpawnInterval;
            }
        }

        /// <summary>
        /// Starts the spawning process
        /// </summary>
        public virtual void StartSpawning()
        {
            if (EnemyPrefab == null)
            {
                Debug.LogError($"[SimpleEnemySpawner] No enemy prefab assigned on {gameObject.name}!");
                return;
            }
            
            _isSpawning = true;
            _nextSpawnTime = Time.time;
            
            if (DebugMode)
            {
                Debug.Log($"[SimpleEnemySpawner] Started spawning {SpawnCount} enemies");
            }
        }

        /// <summary>
        /// Spawns a single enemy at a random position within the spawn area
        /// </summary>
        public virtual void SpawnEnemy()
        {
            if (EnemyPrefab == null)
            {
                Debug.LogError($"[SimpleEnemySpawner] No enemy prefab assigned!");
                return;
            }
            
            // Get random spawn position
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // Try to find a position that's not too close to other enemies
            int maxAttempts = 10;
            int attempts = 0;
            while (attempts < maxAttempts && IsPositionTooClose(spawnPosition))
            {
                spawnPosition = GetRandomSpawnPosition();
                attempts++;
            }
            
            // Instantiate enemy
            GameObject enemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
            enemy.name = $"Enemy_{_spawnedCount + 1}";
            
            // Randomize health
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                int randomHealth = Random.Range(MinHealth, MaxHealth + 1);
                enemyHealth.CurrentHealth = randomHealth;
                enemyHealth.MaximumHealth = randomHealth;
                enemyHealth.InitialHealth = randomHealth;
                
                if (DebugMode)
                {
                    Debug.Log($"[SimpleEnemySpawner] Spawned {enemy.name} at {spawnPosition} with {randomHealth} HP");
                }
            }
            else
            {
                Debug.LogWarning($"[SimpleEnemySpawner] Enemy prefab {EnemyPrefab.name} has no Health component!");
            }
            
            // Configure AI Brain target
            AIBrain enemyBrain = enemy.GetComponentInChildren<AIBrain>();
            if (enemyBrain != null && TargetTransform != null)
            {
                enemyBrain.Target = TargetTransform;
                
                if (DebugMode)
                {
                    Debug.Log($"[SimpleEnemySpawner] Set {enemy.name} AIBrain target to: {TargetTransform.name} at {TargetTransform.position}");
                }
            }
            else if (enemyBrain == null)
            {
                if (DebugMode)
                {
                    Debug.LogWarning($"[SimpleEnemySpawner] Enemy {enemy.name} has no AIBrain component! It will not move.");
                }
            }
            else if (TargetTransform == null)
            {
                Debug.LogWarning($"[SimpleEnemySpawner] No target set for enemy AI! Enemy {enemy.name} may not move.");
            }
            
            _spawnedCount++;
            
            // Stop spawning if we've reached the count
            if (_spawnedCount >= SpawnCount)
            {
                _isSpawning = false;
                
                if (DebugMode)
                {
                    Debug.Log($"[SimpleEnemySpawner] Finished spawning {_spawnedCount} enemies");
                }
            }
        }

        /// <summary>
        /// Returns a random position within the spawn area
        /// </summary>
        protected virtual Vector3 GetRandomSpawnPosition()
        {
            float randomX = Random.Range(-SpawnAreaSize.x / 2f, SpawnAreaSize.x / 2f);
            float randomY = Random.Range(-SpawnAreaSize.y / 2f, SpawnAreaSize.y / 2f);
            float randomZ = Random.Range(-SpawnAreaSize.z / 2f, SpawnAreaSize.z / 2f);
            
            return _spawnCenterPosition + new Vector3(randomX, randomY, randomZ);
        }

        /// <summary>
        /// Checks if a position is too close to existing enemies
        /// </summary>
        protected virtual bool IsPositionTooClose(Vector3 position)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            foreach (GameObject enemy in enemies)
            {
                if (Vector3.Distance(position, enemy.transform.position) < MinDistanceBetweenEnemies)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Test spawn button method
        /// </summary>
        protected virtual void TestSpawn()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SimpleEnemySpawner] Test spawn only works in Play mode!");
                return;
            }
            
            SpawnEnemy();
        }

        /// <summary>
        /// Resets the spawner to spawn again
        /// </summary>
        public virtual void ResetSpawner()
        {
            _spawnedCount = 0;
            _isSpawning = false;
            
            if (DebugMode)
            {
                Debug.Log("[SimpleEnemySpawner] Spawner reset");
            }
        }

        /// <summary>
        /// Spawns all enemies immediately
        /// </summary>
        public virtual void SpawnAllImmediately()
        {
            if (EnemyPrefab == null)
            {
                Debug.LogError($"[SimpleEnemySpawner] No enemy prefab assigned!");
                return;
            }
            
            for (int i = _spawnedCount; i < SpawnCount; i++)
            {
                SpawnEnemy();
            }
        }

        /// <summary>
        /// Clears all spawned enemies
        /// </summary>
        public virtual void ClearAllEnemies()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
            
            ResetSpawner();
            
            if (DebugMode)
            {
                Debug.Log($"[SimpleEnemySpawner] Cleared {enemies.Length} enemies");
            }
        }

        /// <summary>
        /// Draw spawn area gizmo in editor
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (!ShowSpawnArea)
            {
                return;
            }
            
            Vector3 center = SpawnCenter != null ? SpawnCenter.position : transform.position;
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(center, SpawnAreaSize);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, SpawnAreaSize);
        }
    }
}
