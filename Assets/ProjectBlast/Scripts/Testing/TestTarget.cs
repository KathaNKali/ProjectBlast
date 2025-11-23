using UnityEngine;
using MoreMountains.TopDownEngine;

namespace ProjectBlast.Testing
{
    /// <summary>
    /// Simple test target for heroes to shoot at.
    /// Use this to test hero weapon/targeting/firing systems before implementing full Enemy class.
    /// 
    /// SETUP:
    /// 1. Create a Cube GameObject
    /// 2. Add this script
    /// 3. Add Health component
    /// 4. Set layer to "Enemy" (or whatever heroes target)
    /// 5. Position in front of heroes in Firing zone
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TestTarget : MonoBehaviour
    {
        [Header("Target Configuration")]
        [Tooltip("Display name for this target")]
        public string TargetName = "Test Target";
        
        [Tooltip("Starting health")]
        public float StartingHealth = 100f;
        
        [Tooltip("Show damage taken in console")]
        public bool ShowDamageLogs = true;
        
        [Header("Visual Feedback")]
        [Tooltip("Material to flash when taking damage")]
        public Material DamageMaterial;
        
        [Tooltip("Duration of damage flash (seconds)")]
        public float DamageFlashDuration = 0.1f;
        
        private Health _health;
        private Renderer _renderer;
        private Material _originalMaterial;
        private float _damageFlashTimer = 0f;
        
        void Awake()
        {
            // Get or add Health component
            _health = GetComponent<Health>();
            if (_health == null)
            {
                _health = gameObject.AddComponent<Health>();
                _health.InitialHealth = StartingHealth;
                _health.MaximumHealth = StartingHealth;
                Debug.Log($"[TestTarget] {TargetName} added Health component.");
            }
            
            // Cache renderer for visual feedback
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
        }
        
        void Start()
        {
            // Subscribe to health events
            if (_health != null)
            {
                _health.OnDeath += OnTargetDestroyed;
                _health.OnHit += OnTargetHit;
            }
            
            Debug.Log($"[TestTarget] {TargetName} initialized with {_health.CurrentHealth} HP on layer {LayerMask.LayerToName(gameObject.layer)}");
        }
        
        void Update()
        {
            // Handle damage flash timer
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                
                if (_damageFlashTimer <= 0f && _renderer != null && _originalMaterial != null)
                {
                    _renderer.material = _originalMaterial;
                }
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from health events
            if (_health != null)
            {
                _health.OnDeath -= OnTargetDestroyed;
                _health.OnHit -= OnTargetHit;
            }
        }
        
        /// <summary>
        /// Called when target takes damage
        /// </summary>
        void OnTargetHit()
        {
            if (ShowDamageLogs)
            {
                Debug.Log($"[TestTarget] {TargetName} HIT! HP: {_health.CurrentHealth}/{_health.MaximumHealth}");
            }
            
            // Flash damage material
            if (_renderer != null && DamageMaterial != null)
            {
                _renderer.material = DamageMaterial;
                _damageFlashTimer = DamageFlashDuration;
            }
        }
        
        /// <summary>
        /// Called when target health reaches 0
        /// </summary>
        void OnTargetDestroyed()
        {
            Debug.Log($"[TestTarget] {TargetName} DESTROYED!");
            
            // Destroy after short delay (allows death effects to play)
            Destroy(gameObject, 0.5f);
        }
        
        /// <summary>
        /// Manually deal damage to this target (for testing)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (_health != null)
            {
                _health.Damage(damage, gameObject, 0f, 0f, Vector3.zero);
            }
        }
        
        void OnDrawGizmos()
        {
            // Draw health bar above target
            if (_health != null && _health.CurrentHealth > 0)
            {
                Vector3 position = transform.position + Vector3.up * 2f;
                float healthPercent = _health.CurrentHealth / _health.MaximumHealth;
                
                // Background (red)
                Gizmos.color = Color.red;
                Gizmos.DrawCube(position, new Vector3(1f, 0.1f, 0.01f));
                
                // Foreground (green)
                Gizmos.color = Color.green;
                Vector3 healthBarSize = new Vector3(healthPercent * 1f, 0.1f, 0.01f);
                Vector3 healthBarPosition = position - new Vector3((1f - healthPercent) * 0.5f, 0f, 0f);
                Gizmos.DrawCube(healthBarPosition, healthBarSize);
            }
        }
    }
}
