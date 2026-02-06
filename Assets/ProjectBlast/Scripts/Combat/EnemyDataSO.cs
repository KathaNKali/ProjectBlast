using UnityEngine;
using MoreMountains.Tools;

namespace ProjectBlast.Combat
{
    /// <summary>
    /// Enemy Data ScriptableObject
    /// 
    /// Defines all stats and configuration for an enemy type including:
    /// - Health and movement
    /// - Combat stats (range, damage, fire rate)
    /// - Visual configuration (prefab, projectiles)
    /// - Calculated threat value for TPS system
    /// 
    /// Each enemy type should have its own EnemyDataSO asset.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy_", menuName = "ProjectBlast/Combat/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        #region Identity
        
        [Header("=== IDENTITY ===")]
        [Tooltip("Display name of this enemy type")]
        public string EnemyName = "Grunt";
        
        [Tooltip("Enemy prefab with Character, AIBrain, and Weapon components")]
        public GameObject Prefab;
        
        [TextArea(2, 4)]
        [Tooltip("Description of this enemy type")]
        public string Description = "Basic enemy unit";
        
        #endregion
        
        #region Health & Movement
        
        [Header("=== HEALTH & MOVEMENT ===")]
        [Tooltip("Maximum health points")]
        [Min(10)]
        public int MaxHealth = 100;
        
        [Tooltip("Movement speed (units per second)")]
        [Range(0.5f, 10f)]
        public float MovementSpeed = 3f;
        
        #endregion
        
        #region Combat Stats
        
        [Header("=== COMBAT STATS ===")]
        [Tooltip("Attack range - distance to start shooting (units)")]
        [Range(1f, 20f)]
        public float AttackRange = 8f;
        
        [Tooltip("Damage dealt per projectile hit")]
        [Min(1)]
        public int DamagePerShot = 10;
        
        [Tooltip("Fire rate - shots per second")]
        [Range(0.1f, 5f)]
        public float FireRate = 1f;
        
        [Tooltip("Projectile speed (units per second)")]
        [Range(5f, 30f)]
        public float ProjectileSpeed = 15f;
        
        [Header("Homing Behavior")]
        [Tooltip("If true, projectiles will curve toward targets (requires HomingProjectile component)")]
        public bool UseHomingProjectiles = false;
        
        [Tooltip("How quickly projectiles turn toward target (1-20). Higher = sharper curves.")]
        [Range(1f, 20f)]
        [MMCondition("UseHomingProjectiles", true)]
        public float HomingTurnSpeed = 5f;
        
        [Tooltip("Duration projectiles will track target (seconds)")]
        [Range(0.5f, 10f)]
        [MMCondition("UseHomingProjectiles", true)]
        public float HomingDuration = 3f;
        
        [Tooltip("Time between shots (auto-calculated)")]
        [SerializeField] private float _timeBetweenShots;
        
        [Tooltip("DPS - Damage Per Second (auto-calculated)")]
        [SerializeField] private float _dps;
        
        #endregion
        
        #region Visual Configuration
        
        [Header("=== VISUAL CONFIGURATION ===")]
        [Tooltip("Projectile prefab for this enemy's weapon")]
        public GameObject ProjectilePrefab;
        
        [Tooltip("Projectile visual color/tint")]
        public Color ProjectileColor = Color.red;
        
        [Tooltip("Enemy scale multiplier (1.0 = normal size)")]
        [Range(0.5f, 3f)]
        public float ScaleMultiplier = 1f;
        
        #endregion
        
        #region Base Damage
        
        [Header("=== BASE DAMAGE ===")]
        [Tooltip("Damage dealt to base when reaching wall")]
        [Min(1)]
        public int BaseDamagePerShot = 10;
        
        [Tooltip("How long enemy attacks base before despawning (0 = infinite)")]
        [Min(0)]
        public float BaseAttackDuration = 0f; // 0 = attacks until killed
        
        #endregion
        
        #region Threat Calculation
        
        [Header("=== TPS THREAT VALUE (Auto-Calculated) ===")]
        [Tooltip("Calculated threat cost for TPS spawning system")]
        [SerializeField] private float _calculatedThreat;
        
        [Tooltip("Threat multiplier for manual balancing (1.0 = normal)")]
        [Range(0.5f, 3f)]
        public float ThreatMultiplier = 1f;
        
        /// <summary>
        /// Public accessor for calculated threat value
        /// </summary>
        public float ThreatValue => _calculatedThreat * ThreatMultiplier;
        
        /// <summary>
        /// Public accessor for DPS
        /// </summary>
        public float DPS => _dps;
        
        /// <summary>
        /// Public accessor for time between shots
        /// </summary>
        public float TimeBetweenShots => _timeBetweenShots;
        
        #endregion
        
        #region Validation & Calculations
        
        /// <summary>
        /// Called automatically when values change in inspector
        /// </summary>
        private void OnValidate()
        {
            CalculateDerivedStats();
            CalculateThreatValue();
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Calculate derived combat stats
        /// </summary>
        private void CalculateDerivedStats()
        {
            // Calculate time between shots
            _timeBetweenShots = FireRate > 0 ? 1f / FireRate : 1f;
            
            // Calculate DPS
            _dps = DamagePerShot * FireRate;
        }
        
        /// <summary>
        /// Calculate threat value for TPS system
        /// 
        /// Formula:
        /// Base Threat = MaxHealth (HP is primary factor)
        /// Range Factor = AttackRange / 10 (longer range = more threat)
        /// DPS Factor = DPS / 10 (higher damage = more threat)
        /// Speed Factor = MovementSpeed / 3 (faster = more threat)
        /// 
        /// Final Threat = BaseThreat × (1 + RangeFactor + DPSFactor + SpeedFactor)
        /// </summary>
        private void CalculateThreatValue()
        {
            // Base threat from health
            float baseThreat = MaxHealth;
            
            // Range factor (longer range = more dangerous)
            float rangeFactor = AttackRange / 10f;
            
            // DPS factor (higher damage output = more dangerous)
            float dpsFactor = _dps / 10f;
            
            // Speed factor (faster enemies = more threatening)
            float speedFactor = MovementSpeed / 3f;
            
            // Combined threat calculation
            _calculatedThreat = baseThreat * (1f + rangeFactor + dpsFactor + speedFactor);
            
            // Round to 1 decimal place for cleaner values
            _calculatedThreat = Mathf.Round(_calculatedThreat * 10f) / 10f;
        }
        
        /// <summary>
        /// Validate configuration and log warnings
        /// </summary>
        private void ValidateConfiguration()
        {
            // Check if prefab is assigned
            if (Prefab == null)
            {
                Debug.LogWarning($"[EnemyData] {name}: No prefab assigned!");
            }
            
            // Check if projectile prefab is assigned
            if (ProjectilePrefab == null)
            {
                Debug.LogWarning($"[EnemyData] {name}: No projectile prefab assigned!");
            }
            
            // Warn about extreme values
            if (MaxHealth < 50)
            {
                Debug.LogWarning($"[EnemyData] {name}: Very low health ({MaxHealth}). Enemy may die too quickly.");
            }
            
            if (AttackRange > 15f)
            {
                Debug.LogWarning($"[EnemyData] {name}: Very long range ({AttackRange}). May hit heroes before heroes can respond.");
            }
            
            if (_dps > 50f)
            {
                Debug.LogWarning($"[EnemyData] {name}: Very high DPS ({_dps:F1}). May destroy base too quickly.");
            }
            
            if (MovementSpeed > 6f)
            {
                Debug.LogWarning($"[EnemyData] {name}: Very fast ({MovementSpeed}). May reach base before heroes can kill.");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Get a formatted summary of this enemy's stats
        /// </summary>
        public string GetStatsSummary()
        {
            return $"=== {EnemyName.ToUpper()} ===\n" +
                   $"Health: {MaxHealth} HP\n" +
                   $"Speed: {MovementSpeed:F1} units/s\n" +
                   $"Range: {AttackRange:F1} units\n" +
                   $"Damage: {DamagePerShot} × {FireRate:F1}/s = {_dps:F1} DPS\n" +
                   $"Projectile Speed: {ProjectileSpeed:F1} units/s\n" +
                   $"\nTHREAT VALUE: {ThreatValue:F1}\n" +
                   $"(Base: {_calculatedThreat:F1} × Multiplier: {ThreatMultiplier:F1})";
        }
        
        /// <summary>
        /// Get time to reach wall from spawn
        /// </summary>
        /// <param name="distance">Distance from spawn to wall</param>
        /// <returns>Time in seconds</returns>
        public float GetTimeToReachWall(float distance)
        {
            return MovementSpeed > 0 ? distance / MovementSpeed : float.PositiveInfinity;
        }
        
        /// <summary>
        /// Get time to kill this enemy at a given DPS
        /// </summary>
        /// <param name="heroDPS">Hero's damage per second</param>
        /// <returns>Time in seconds to kill</returns>
        public float GetTimeToKill(float heroDPS)
        {
            return heroDPS > 0 ? MaxHealth / heroDPS : float.PositiveInfinity;
        }
        
        /// <summary>
        /// Compare this enemy to another (for sorting by threat)
        /// </summary>
        public int CompareTo(EnemyDataSO other)
        {
            return ThreatValue.CompareTo(other.ThreatValue);
        }
        
        #endregion
    }
}
