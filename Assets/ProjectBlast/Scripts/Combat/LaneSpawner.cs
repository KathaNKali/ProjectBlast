using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Lane Spawner - Individual lane spawning logic
    /// 
    /// Responsibilities:
    /// - Receive threat budget from TPSDirector
    /// - Select appropriate enemies based on budget and wave config
    /// - Spawn enemies at lane spawn position
    /// - Track active enemies in lane
    /// 
    /// Phase 5 Implementation: Full enemy spawning with weighted selection
    /// </summary>
    public class LaneSpawner : MonoBehaviour
    {
        #region Configuration
        
        [Header("=== LANE CONFIGURATION ===")]
        [Tooltip("Lane index (0-based)")]
        public int LaneIndex = 0;
        
        [Tooltip("Battlefield configuration")]
        public BattlefieldConfigSO BattlefieldConfig;
        
        [Header("=== SPAWN SETTINGS ===")]
        [Tooltip("Minimum time between spawns in this lane (seconds)")]
        [Range(0.1f, 5f)]
        public float SpawnCooldown = 0.3f;
        
        [Tooltip("Maximum active enemies in this lane (0 = unlimited)")]
        public int MaxActiveEnemies = 20;
        
        [Tooltip("Spawn position offset randomization (X/Y)")]
        public Vector2 SpawnPositionRandomness = new Vector2(0.3f, 0.3f);
        
        [Header("=== DEBUG ===")]
        [Tooltip("Show debug logs")]
        public bool DebugMode = false;
        
        #endregion
        
        #region Runtime State
        
        private TPSDirector _director;
        private WaveConfigSO _currentWave;
        private float _currentBudget = 0f;
        private Vector3 _spawnPosition;
        private bool _isActive = false;
        
        // Spawn timing
        private float _lastSpawnTime = -999f;
        private float _nextSpawnTime = 0f;
        
        // Tracking
        private List<GameObject> _activeEnemies = new List<GameObject>();
        private int _totalSpawned = 0;
        private float _totalThreatSpawned = 0f;
        
        #endregion
        
        #region Properties
        
        public int ActiveEnemyCount => _activeEnemies.Count;
        public int TotalSpawned => _totalSpawned;
        public float TotalThreatSpawned => _totalThreatSpawned;
        public float CurrentBudget => _currentBudget;
        public bool IsActive => _isActive;
        public bool CanSpawn => _isActive && Time.time >= _nextSpawnTime && 
                                (MaxActiveEnemies == 0 || _activeEnemies.Count < MaxActiveEnemies);
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the lane spawner
        /// </summary>
        public void Initialize(int laneIndex, BattlefieldConfigSO config, TPSDirector director)
        {
            LaneIndex = laneIndex;
            BattlefieldConfig = config;
            _director = director;
            
            if (BattlefieldConfig != null)
            {
                _spawnPosition = BattlefieldConfig.GetLaneSpawnPosition(laneIndex);
                transform.position = _spawnPosition;
            }
            
            if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] Lane {laneIndex} initialized at {_spawnPosition}");
            }
        }
        
        #endregion
        
        #region Wave Management
        
        /// <summary>
        /// Called when a new wave starts
        /// </summary>
        public void OnWaveStarted(WaveConfigSO wave)
        {
            _currentWave = wave;
            _currentBudget = 0f;
            _isActive = true;
            _lastSpawnTime = Time.time;
            _nextSpawnTime = Time.time + SpawnCooldown;
            _totalSpawned = 0;
            _totalThreatSpawned = 0f;
            
            // Clear any dead enemy references
            CleanupDestroyedEnemies();
            
            if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] Lane {LaneIndex} wave started: {wave.WaveName}");
            }
        }
        
        /// <summary>
        /// Stop spawning
        /// </summary>
        public void StopSpawning()
        {
            _isActive = false;
            _currentBudget = 0f;
            
            if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] Lane {LaneIndex} stopped spawning");
            }
        }
        
        #endregion
        
        #region Budget & Spawning
        
        /// <summary>
        /// Receive threat budget from TPSDirector
        /// </summary>
        public void ReceiveThreatBudget(float budget)
        {
            _currentBudget = budget;
            
            // Try to spawn multiple times if we have budget (allows burst spawning)
            int maxSpawnAttempts = 5; // Prevent infinite loops
            int spawnAttempts = 0;
            
            while (CanSpawn && _currentWave != null && spawnAttempts < maxSpawnAttempts)
            {
                // Check if we have enough budget before attempting spawn
                EnemyDataSO cheapestEnemy = _currentWave.GetAffordableEnemy(_currentBudget);
                if (cheapestEnemy == null)
                {
                    // Not enough budget, stop trying
                    break;
                }
                
                TrySpawnEnemy();
                spawnAttempts++;
                
                // If spawn succeeded, cooldown will prevent further spawns this frame
                if (!CanSpawn)
                {
                    break;
                }
            }
        }
        
        /// <summary>
        /// Attempt to spawn an enemy based on available budget
        /// </summary>
        private void TrySpawnEnemy()
        {
            if (_currentWave == null)
            {
                Debug.LogWarning($"[LaneSpawner] Lane {LaneIndex} - No active wave!");
                return;
            }
            
            // Get affordable enemy based on current budget
            EnemyDataSO selectedEnemy = _currentWave.GetAffordableEnemy(_currentBudget);
            
            if (selectedEnemy == null)
            {
                // Not enough budget for any enemy
                if (DebugMode && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[LaneSpawner] Lane {LaneIndex} - Insufficient budget: {_currentBudget:F0}. Cheapest enemy costs more.");
                }
                return;
            }
            
            // Check if we have a prefab to spawn
            if (selectedEnemy.Prefab == null)
            {
                Debug.LogWarning($"[LaneSpawner] Lane {LaneIndex} - {selectedEnemy.EnemyName} has no prefab assigned!");
                return;
            }
            
            // Spawn the enemy
            SpawnEnemy(selectedEnemy);
        }
        
        /// <summary>
        /// Spawn a specific enemy
        /// </summary>
        private void SpawnEnemy(EnemyDataSO enemyData)
        {
            // Calculate spawn position with randomness
            Vector3 spawnPos = _spawnPosition;
            spawnPos.x += Random.Range(-SpawnPositionRandomness.x, SpawnPositionRandomness.x);
            spawnPos.y += Random.Range(-SpawnPositionRandomness.y, SpawnPositionRandomness.y);
            
            // Instantiate enemy
            GameObject enemyInstance = Instantiate(enemyData.Prefab, spawnPos, Quaternion.identity);
            enemyInstance.name = $"{enemyData.EnemyName}_Lane{LaneIndex}_{_totalSpawned}";
            
            // Apply enemy data to the spawned instance
            ApplyEnemyData(enemyInstance, enemyData);
            
            // Configure AI for movement
            ConfigureEnemyAI(enemyInstance, enemyData);
            
            // Track enemy
            _activeEnemies.Add(enemyInstance);
            _totalSpawned++;
            _totalThreatSpawned += enemyData.ThreatValue;
            
            // Update spawn timing
            _lastSpawnTime = Time.time;
            _nextSpawnTime = Time.time + SpawnCooldown;
            
            // Notify director
            if (_director != null)
            {
                _director.OnEnemySpawned(LaneIndex, enemyData);
            }
            
            // Setup death callback
            Health health = enemyInstance.MMGetComponentNoAlloc<Health>();
            if (health != null)
            {
                health.OnDeath += () => OnEnemyDeath(enemyInstance, enemyData);
            }
            
            if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] Lane {LaneIndex} spawned {enemyData.EnemyName} " +
                         $"(Threat: {enemyData.ThreatValue:F0}, Active: {_activeEnemies.Count})");
            }
        }
        
        /// <summary>
        /// Apply EnemyDataSO stats to spawned enemy instance
        /// </summary>
        private void ApplyEnemyData(GameObject enemy, EnemyDataSO data)
        {
            // Apply health
            Health health = enemy.MMGetComponentNoAlloc<Health>();
            if (health != null)
            {
                health.MaximumHealth = data.MaxHealth;
                health.CurrentHealth = data.MaxHealth;
                health.InitialHealth = data.MaxHealth;
            }
            
            // Apply movement speed (FIXED: Use WalkSpeed instead of MovementSpeed property)
            Character character = enemy.MMGetComponentNoAlloc<Character>();
            if (character != null)
            {
                CharacterMovement movement = character.FindAbility<CharacterMovement>();
                if (movement != null)
                {
                    movement.WalkSpeed = data.MovementSpeed;
                    
                    if (DebugMode)
                    {
                        Debug.Log($"[LaneSpawner] Applied movement speed {data.MovementSpeed} to {enemy.name}");
                    }
                }
                else if (DebugMode)
                {
                    Debug.LogWarning($"[LaneSpawner] No CharacterMovement ability found on {enemy.name}!");
                }
            }
            
            // Apply scale multiplier
            if (data.ScaleMultiplier != 1f)
            {
                enemy.transform.localScale = Vector3.one * data.ScaleMultiplier;
                
                if (DebugMode)
                {
                    Debug.Log($"[LaneSpawner] Applied scale {data.ScaleMultiplier} to {enemy.name}");
                }
            }
            
            // Apply weapon damage and fire rate (if enemy has weapon)
            CharacterHandleWeapon weaponHandler = enemy.MMGetComponentNoAlloc<CharacterHandleWeapon>();
            if (weaponHandler != null && weaponHandler.CurrentWeapon != null)
            {
                Weapon weapon = weaponHandler.CurrentWeapon;
                weapon.TimeBetweenUses = 1f / data.FireRate;
                
                // Apply damage to projectile weapon
                ProjectileWeapon projectileWeapon = weapon as ProjectileWeapon;
                if (projectileWeapon != null)
                {
                    // Store damage data on weapon for later use
                    EnemyWeaponData weaponData = weapon.gameObject.GetComponent<EnemyWeaponData>();
                    if (weaponData == null)
                    {
                        weaponData = weapon.gameObject.AddComponent<EnemyWeaponData>();
                    }
                    weaponData.Damage = data.DamagePerShot;
                    weaponData.AttackRange = data.AttackRange;
                    weaponData.ProjectileSpeed = data.ProjectileSpeed;
                    
                    // Apply homing settings if enabled
                    if (data.UseHomingProjectiles && projectileWeapon.ObjectPooler != null)
                    {
                        // Get projectile prefab from object pooler
                        MMSimpleObjectPooler simplePooler = projectileWeapon.ObjectPooler as MMSimpleObjectPooler;
                        if (simplePooler != null && simplePooler.GameObjectToPool != null)
                        {
                            HomingProjectile homingProjectile = simplePooler.GameObjectToPool.GetComponent<HomingProjectile>();
                            if (homingProjectile != null)
                            {
                                homingProjectile.TurnSpeed = data.HomingTurnSpeed;
                                homingProjectile.HomingDuration = data.HomingDuration;
                                
                                if (DebugMode)
                                {
                                    Debug.Log($"[LaneSpawner] Applied homing settings to {enemy.name}: TurnSpeed={data.HomingTurnSpeed}, Duration={data.HomingDuration}");
                                }
                            }
                            else if (DebugMode)
                            {
                                Debug.LogWarning($"[LaneSpawner] Enemy {enemy.name} has UseHomingProjectiles=true but projectile prefab doesn't have HomingProjectile component!");
                            }
                        }
                    }
                    
                    if (DebugMode)
                    {
                        Debug.Log($"[LaneSpawner] Applied weapon stats to {enemy.name}: Damage={data.DamagePerShot}, FireRate={data.FireRate}, Range={data.AttackRange}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Configure AI components for enemy movement and shooting
        /// </summary>
        private void ConfigureEnemyAI(GameObject enemy, EnemyDataSO data)
        {
            // Configure AIDecisionReachedWall with BattlefieldConfig
            var wallDecisionType = System.Type.GetType("ProjectBlast.Combat.AIDecisionReachedWall, Assembly-CSharp");
            if (wallDecisionType != null)
            {
                var wallDecisions = enemy.GetComponentsInChildren(wallDecisionType, true);
                foreach (var decision in wallDecisions)
                {
                    var configField = wallDecisionType.GetField("BattlefieldConfig");
                    if (configField != null)
                    {
                        configField.SetValue(decision, BattlefieldConfig);
                        
                        if (DebugMode)
                        {
                            Debug.Log($"[LaneSpawner] Configured AIDecisionReachedWall on {((Component)decision).gameObject.name} with wall Z: {BattlefieldConfig.BaseWallZ}");
                        }
                    }
                }
                
                if (wallDecisions.Length == 0 && DebugMode)
                {
                    Debug.LogWarning($"[LaneSpawner] No AIDecisionReachedWall found on {enemy.name}! Enemy won't stop at wall.");
                }
            }
            else if (DebugMode)
            {
                Debug.LogWarning($"[LaneSpawner] AIDecisionReachedWall type not found yet - Unity may need to recompile.");
            }
            
            // Configure AIDecisionDetectHeroOrWall with detection range from EnemyDataSO
            var detectDecisionType = System.Type.GetType("ProjectBlast.Combat.AIDecisionDetectHeroOrWall, Assembly-CSharp");
            if (detectDecisionType != null)
            {
                var detectDecisions = enemy.GetComponentsInChildren(detectDecisionType, true);
                foreach (var decision in detectDecisions)
                {
                    // Set detection range from EnemyDataSO
                    var rangeField = detectDecisionType.GetField("HeroDetectionRange");
                    if (rangeField != null)
                    {
                        rangeField.SetValue(decision, data.AttackRange);
                    }
                    
                    // Set manual wall position from BattlefieldConfig
                    var wallPosField = detectDecisionType.GetField("ManualWallPosition");
                    if (wallPosField != null)
                    {
                        Vector3 wallPos = new Vector3(
                            BattlefieldConfig.CenterLaneX,
                            BattlefieldConfig.BaseWallHeight / 2f,
                            BattlefieldConfig.BaseWallZ
                        );
                        wallPosField.SetValue(decision, wallPos);
                    }
                    
                    if (DebugMode)
                    {
                        Debug.Log($"[LaneSpawner] Configured AIDecisionDetectHeroOrWall on {((Component)decision).gameObject.name} - Range: {data.AttackRange}m, Wall Z: {BattlefieldConfig.BaseWallZ}");
                    }
                }
                
                if (detectDecisions.Length == 0 && DebugMode)
                {
                    Debug.LogWarning($"[LaneSpawner] No AIDecisionDetectHeroOrWall found on {enemy.name}! Enemy won't detect targets.");
                }
            }
            else if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] AIDecisionDetectHeroOrWall type not found (OK if not yet added to prefab)");
            }
        }
        
        /// <summary>
        /// Called when an enemy dies
        /// </summary>
        private void OnEnemyDeath(GameObject enemy, EnemyDataSO enemyData)
        {
            if (_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Remove(enemy);
            }
            
            // Notify director
            if (_director != null)
            {
                _director.OnEnemyKilled(LaneIndex, enemyData);
            }
            
            if (DebugMode)
            {
                Debug.Log($"[LaneSpawner] Lane {LaneIndex} enemy killed: {enemyData.EnemyName} " +
                         $"(Active: {_activeEnemies.Count})");
            }
        }
        
        /// <summary>
        /// Remove null references from active enemies list
        /// </summary>
        private void CleanupDestroyedEnemies()
        {
            _activeEnemies.RemoveAll(enemy => enemy == null);
        }
        
        #endregion
        
        #region Periodic Cleanup
        
        private void Update()
        {
            // Periodic cleanup of destroyed enemies (every 2 seconds)
            if (Time.frameCount % 120 == 0)
            {
                CleanupDestroyedEnemies();
            }
        }
        
        #endregion
        
        #region Debug Visualization
        
        private void OnDrawGizmos()
        {
            if (BattlefieldConfig == null) return;
            
            // Draw spawn position
            Vector3 pos = transform.position;
            
            // Color based on state
            if (_isActive)
            {
                Gizmos.color = CanSpawn ? Color.green : Color.yellow;
            }
            else
            {
                Gizmos.color = Color.gray;
            }
            
            Gizmos.DrawWireSphere(pos, 0.5f);
            
            // Draw spawn randomness area
            if (_isActive && SpawnPositionRandomness.magnitude > 0)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawWireCube(pos, new Vector3(SpawnPositionRandomness.x * 2, SpawnPositionRandomness.y * 2, 0.1f));
            }
            
            // Draw lane indicator
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 start = pos;
            Vector3 end = new Vector3(pos.x, pos.y, BattlefieldConfig.BaseWallZ);
            Gizmos.DrawLine(start, end);
            
#if UNITY_EDITOR
            // Draw label with detailed info
            string label = $"Lane {LaneIndex}\n" +
                          $"Budget: {_currentBudget:F0}\n" +
                          $"Active: {_activeEnemies.Count}/{(MaxActiveEnemies > 0 ? MaxActiveEnemies.ToString() : "∞")}\n" +
                          $"Spawned: {_totalSpawned}";
            
            if (_isActive && !CanSpawn)
            {
                if (Time.time < _nextSpawnTime)
                {
                    float cooldownRemaining = _nextSpawnTime - Time.time;
                    label += $"\nCooldown: {cooldownRemaining:F1}s";
                }
                else if (MaxActiveEnemies > 0 && _activeEnemies.Count >= MaxActiveEnemies)
                {
                    label += "\nMAX REACHED";
                }
            }
            
            UnityEditor.Handles.Label(pos + Vector3.up * 2, label);
#endif
        }
        
        #endregion
    }
    
    /// <summary>
    /// Helper component to store weapon data applied from EnemyDataSO
    /// </summary>
    public class EnemyWeaponData : MonoBehaviour
    {
        public int Damage = 10;
        public float AttackRange = 8f;
        public float ProjectileSpeed = 15f;
        
        /// <summary>
        /// Apply damage and speed to projectiles spawned by this weapon
        /// </summary>
        public void ApplyToProjectile(Projectile projectile)
        {
            if (projectile != null)
            {
                // Apply damage
                DamageOnTouch damageComponent = projectile.GetComponent<DamageOnTouch>();
                if (damageComponent != null)
                {
                    damageComponent.MinDamageCaused = Damage;
                    damageComponent.MaxDamageCaused = Damage;
                }
                
                // Apply projectile speed
                projectile.Speed = ProjectileSpeed;
            }
        }
    }
}
