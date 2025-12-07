using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.TopDownEngine;

/// <summary>
/// Tracks in-flight bullets to enemies to prevent bullet waste.
/// Multiple heroes coordinate by tracking total damage in-flight from all heroes.
/// Heroes stop shooting when enemy is "doomed" (in-flight damage >= current HP).
/// 
/// USAGE:
/// - Automatically added to enemies by AIActionShoot3D when first targeted
/// - Heroes check CanHeroFire() BEFORE each shot
/// - Heroes call OnBulletFired() AFTER firing (bullet now in-flight)
/// - System auto-removes bullets when they hit (via Health.Damage hook)
/// </summary>
public class EnemyCombatTracker : MonoBehaviour
{
    [Header("In-Flight Tracking")]
    [SerializeField] private float _totalInFlightDamage;
    [SerializeField] private int _activeHeroCount;
    
    [Header("Debug Info")]
    [SerializeField] private float _currentHP;
    [SerializeField] private float _effectiveHP;
    [SerializeField] private List<string> _heroesTracking = new List<string>();
    
    // Tracks in-flight bullets per hero: Hero -> List of bullet damages
    private Dictionary<GameObject, List<float>> _inFlightBullets = new Dictionary<GameObject, List<float>>();
    private Health _health;
    
    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            Debug.LogError($"[EnemyCombatTracker] No Health component found on {gameObject.name}!");
        }
    }
    
    void Update()
    {
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// Gets the effective HP (current HP minus all in-flight damage).
    /// This represents how much HP is actually "available" for new shots.
    /// </summary>
    public float GetEffectiveHP()
    {
        if (_health == null) return 0f;
        
        float currentHP = _health.CurrentHealth;
        float inFlightDamage = GetTotalInFlightDamage();
        
        return currentHP - inFlightDamage;
    }
    
    /// <summary>
    /// Gets total damage from all in-flight bullets
    /// </summary>
    public float GetTotalInFlightDamage()
    {
        float total = 0f;
        foreach (var bullets in _inFlightBullets.Values)
        {
            total += bullets.Sum();
        }
        return total;
    }
    
    /// <summary>
    /// Checks if a hero should fire at this enemy.
    /// Returns true only if enemy has enough effective HP remaining after accounting for in-flight bullets.
    /// Uses the hero's damage as threshold - enemy must have more HP than what ONE bullet would deal.
    /// </summary>
    public bool CanHeroFire(GameObject hero, float damage)
    {
        if (_health == null || _health.CurrentHealth <= 0)
        {
            return false;
        }
        
        float effectiveHP = GetEffectiveHP();
        
        // Enemy must have enough HP to justify firing this bullet
        // Threshold: At least 1 HP remaining after this bullet would hit
        // This prevents multiple heroes from firing when one bullet is enough
        // Example: Enemy 11 HP, Hero_A fires 15 dmg (effectiveHP = 11-15 = -4)
        //          Hero_B should NOT fire since enemy will die from Hero_A's bullet
        return effectiveHP > 1.0f;
    }
    
    /// <summary>
    /// Records that a hero has fired a bullet (now in-flight to this enemy).
    /// Call this IMMEDIATELY after ShootStart().
    /// </summary>
    public void OnBulletFired(GameObject hero, float damage)
    {
        if (hero == null) return;
        
        // Create list for this hero if needed
        if (!_inFlightBullets.ContainsKey(hero))
        {
            _inFlightBullets[hero] = new List<float>();
        }
        
        // Track this bullet
        _inFlightBullets[hero].Add(damage);
    }
    
    /// <summary>
    /// Called when enemy takes damage (bullet hit).
    /// Removes one in-flight bullet (FIFO - first fired, first removed).
    /// Called automatically from Health.Damage().
    /// </summary>
    public void OnBulletHit(float damageDealt)
    {
        // Remove one bullet (closest match to damage dealt)
        // Use FIFO: first hero with bullets gets one removed
        foreach (var kvp in _inFlightBullets.ToList())
        {
            if (kvp.Value.Count > 0)
            {
                // Remove first bullet from this hero
                kvp.Value.RemoveAt(0);
                
                // Clean up hero entry if no more bullets
                if (kvp.Value.Count == 0)
                {
                    _inFlightBullets.Remove(kvp.Key);
                }
                
                // Only remove one bullet per hit
                break;
            }
        }
        
        // Cleanup: If enemy dead, notify all heroes
        if (_health != null && _health.CurrentHealth <= 0)
        {
            NotifyHeroesTargetDied();
        }
    }
    
    /// <summary>
    /// Clears all in-flight bullets for a specific hero.
    /// Call when hero changes target or exits combat.
    /// </summary>
    public void ReleaseHeroTracking(GameObject hero)
    {
        if (hero != null && _inFlightBullets.ContainsKey(hero))
        {
            _inFlightBullets.Remove(hero);
        }
    }
    
    /// <summary>
    /// Notifies all tracking heroes that this target died.
    /// Heroes should immediately find new targets.
    /// </summary>
    protected void NotifyHeroesTargetDied()
    {
        foreach (var hero in _inFlightBullets.Keys.ToList())
        {
            if (hero != null)
            {
                var aiAction = hero.GetComponentInParent<AIActionShoot3D>();
                if (aiAction != null)
                {
                    aiAction.OnCurrentTargetDied();
                }
            }
        }
        
        _inFlightBullets.Clear();
    }
    
    /// <summary>
    /// Updates inspector debug information
    /// </summary>
    protected void UpdateDebugInfo()
    {
        if (_health != null)
        {
            _currentHP = _health.CurrentHealth;
        }
        
        _effectiveHP = GetEffectiveHP();
        _totalInFlightDamage = GetTotalInFlightDamage();
        _activeHeroCount = _inFlightBullets.Count;
        
        // Update hero list for debugging
        _heroesTracking.Clear();
        foreach (var kvp in _inFlightBullets)
        {
            if (kvp.Key != null && kvp.Value.Count > 0)
            {
                float totalDamage = kvp.Value.Sum();
                _heroesTracking.Add($"{kvp.Key.name}: {kvp.Value.Count} bullets ({totalDamage:F1} dmg)");
            }
        }
    }
    
    void OnDestroy()
    {
        // Notify heroes before destruction
        NotifyHeroesTargetDied();
    }
}
