# TopDown Engine Integration Guide
## How to Use TDE Systems for Your Grid Defense Game

This guide shows **exactly which TopDown Engine systems to use** and how to adapt them for your game.

---

## ✅ TDE Systems You CAN Use Directly

### **1. Weapon & Projectile System** (100% Compatible)

**Use As-Is:**
- `ProjectileWeapon` - Perfect for heroes firing at enemies
- `Projectile` - Bullets/arrows/magic missiles
- `WeaponAmmo` - If you want limited ammo mechanics
- `WeaponAim` - Auto-targeting system

**How to Use:**
```csharp
// On your Hero prefab:
// 1. Add Weapon component (or ProjectileWeapon)
// 2. Configure firing rate, projectile prefab, etc.
// 3. Call weapon.WeaponInputStart() to fire

public class Hero : MonoBehaviour
{
    public Weapon HeroWeapon;
    
    void FireAtEnemy(Transform target)
    {
        // Aim at target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.forward = direction;
        
        // Fire weapon (uses TDE's weapon system)
        HeroWeapon.WeaponInputStart();
        HeroWeapon.WeaponInputStop();
    }
}
```

**TDE Files to Reference:**
- `Assets/TopDownEngine/Common/Scripts/Characters/Weapons/Weapon.cs`
- `Assets/TopDownEngine/Common/Scripts/Characters/Weapons/ProjectileWeapon.cs`
- `Assets/TopDownEngine/Common/Scripts/Characters/Weapons/Projectile.cs`

---

### **2. Health & Damage System** (100% Compatible)

**Use As-Is:**
- `Health` component - For heroes, enemies, base
- `DamageOnTouch` - For projectiles hitting targets
- Health events for UI updates

**How to Use:**
```csharp
// On Hero prefab: Add Health component
// On Enemy prefab: Add Health component
// On Base prefab: Add Health component

// In your code:
public class Hero : MonoBehaviour
{
    private Health _health;
    
    void Start()
    {
        _health = GetComponent<Health>();
        _health.OnDeath += OnHeroDeath;
        _health.OnHit += OnHeroHit;
    }
    
    void OnHeroDeath()
    {
        // Remove hero from grid
        // Play death effects
    }
    
    void OnHeroHit()
    {
        // Play hit feedback
    }
}

// Projectiles automatically damage targets with Health components
// Just ensure your projectile has DamageOnTouch and proper layers
```

**TDE Files to Reference:**
- `Assets/TopDownEngine/Common/Scripts/Characters/Health/Health.cs`
- `Assets/TopDownEngine/Common/Scripts/Characters/Damage/DamageOnTouch.cs`

---

### **3. MMFeedbacks System** (100% Compatible)

**Use As-Is:**
- `MMFeedbacks` for all visual/audio/haptic feedback
- Use for: hero deployment, weapon firing, enemy death, wave start, etc.

**How to Use:**
```csharp
public class Hero : MonoBehaviour
{
    public MMFeedbacks DeploymentFeedback;
    public MMFeedbacks DeathFeedback;
    
    void OnDeployedToFiringGrid()
    {
        DeploymentFeedback?.PlayFeedbacks();
    }
    
    void Die()
    {
        DeathFeedback?.PlayFeedbacks();
        // ... death logic
    }
}
```

**Create Feedbacks in Inspector:**
- Add `MMFeedbacks` component
- Click "Add new feedback" 
- Choose effects: Camera Shake, Particles, Sound, Haptics, etc.

---

### **4. GameManager** (Use with Adaptation)

**What TDE Provides:**
- Game pause/unpause
- Points/score tracking
- Game state management
- Persistent data

**How to Adapt:**
```csharp
// Your custom GameManager can extend TDE's
using MoreMountains.TopDownEngine;

public class GridDefenseGameManager : GameManager
{
    public int CurrentWave = 1;
    public int HeroesDeployed = 0;
    public int BaseHealth = 100;
    
    // Still use TDE's points system for score
    public void AddWaveCompletionScore(int wave)
    {
        AddPoints(wave * 100);
    }
    
    // Use TDE's pause system
    public void PauseGame()
    {
        Pause(PauseMethods.PauseMenu);
    }
}
```

**Keep Using:**
- `GameManager.Instance.Paused` - Pause state
- `GameManager.Instance.AddPoints()` - Score tracking
- `GameManager.Instance.SetPointsPerSecond()` - Time-based scoring

---

### **5. LevelManager** (Use with Adaptation)

**What TDE Provides:**
- Character spawning
- Checkpoint system (can repurpose for hero spawning)
- Level bounds
- Scene management

**How to Adapt:**
```csharp
using MoreMountains.TopDownEngine;

public class HeroSpawner : MonoBehaviour
{
    public GameObject HeroPrefab;
    public Transform SpawnPoint;
    
    void SpawnHero()
    {
        // Use TDE's spawning pattern
        GameObject heroObject = Instantiate(
            HeroPrefab, 
            SpawnPoint.position, 
            Quaternion.identity
        );
        
        // Register with your grid system
        Hero hero = heroObject.GetComponent<Hero>();
        GridManager.Instance.PlaceHeroInPassiveZone(hero);
    }
}
```

---

### **6. AI System** (Use for Enemy Pathing)

**What TDE Provides:**
- `AIBrain` - Controls AI decision making
- `AIActionMoveTowardsTarget3D` - Move towards player base
- `AIActionShoot3D` - Enemy attacks (if they shoot)
- `AIDecision` classes - When to act

**How to Use:**
```csharp
// On Enemy prefab:
// 1. Add Character component (CharacterTypes.AI)
// 2. Add TopDownController3D
// 3. Add CharacterMovement ability
// 4. Add AIBrain component
// 5. Add AIActionMoveTowardsTarget3D
// 6. Set Target as your player base object

// Enemy will automatically:
// - Move towards base
// - Navigate around obstacles
// - Attack when in range (if you add AIActionShoot3D)
```

**TDE Files to Reference:**
- `Assets/TopDownEngine/Common/Scripts/Characters/AI/AIBrain.cs`
- `Assets/TopDownEngine/Common/Scripts/Characters/AI/Advanced/AIActionMoveTowardsTarget3D.cs`

---

## 🔨 TDE Patterns to Extend (Not Use Directly)

### **7. CharacterAbility Pattern** (Extend for Custom Abilities)

**What TDE Provides:**
- Base class for all character abilities
- Input handling framework
- State machine integration

**Create Your Custom Abilities:**

```csharp
using MoreMountains.TopDownEngine;

// Custom ability for auto-combat
[AddComponentMenu("TopDown Engine/Character/Abilities/Character Auto Combat")]
public class CharacterAutoCombat : CharacterAbility
{
    [Header("Auto Combat Settings")]
    public float AttackRange = 5f;
    public float FireRate = 1f;
    public LayerMask TargetLayer;
    
    protected Weapon _weapon;
    protected Transform _currentTarget;
    protected float _nextFireTime;
    
    protected override void Initialization()
    {
        base.Initialization();
        _weapon = GetComponent<Weapon>();
    }
    
    public override void ProcessAbility()
    {
        base.ProcessAbility();
        
        // Only fire if in firing zone
        Hero hero = GetComponent<Hero>();
        if (!hero.IsInFiringZone) return;
        
        // Find target
        if (_currentTarget == null)
        {
            _currentTarget = FindNearestTarget();
        }
        
        // Fire at target
        if (_currentTarget != null && Time.time >= _nextFireTime)
        {
            FireAtTarget();
            _nextFireTime = Time.time + (1f / FireRate);
        }
    }
    
    Transform FindNearestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, AttackRange, TargetLayer);
        
        Transform nearest = null;
        float minDist = float.MaxValue;
        
        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        
        return nearest;
    }
    
    void FireAtTarget()
    {
        if (_weapon == null) return;
        
        // Aim at target
        Vector3 direction = (_currentTarget.position - transform.position).normalized;
        transform.forward = direction;
        
        // Fire weapon (uses TDE weapon system!)
        _weapon.WeaponInputStart();
        _weapon.WeaponInputStop();
        
        PlayAbilityStartFeedbacks(); // Uses TDE feedback system!
    }
}
```

**Now you can:**
- Add `CharacterAutoCombat` ability to hero prefabs
- It integrates with TDE's Character system
- Uses TDE's weapon system automatically
- Uses TDE's feedback system

---

### **8. Grid Positioning** (Adapt from CharacterGridMovement)

**TDE Has:**
- `CharacterGridMovement.cs` - Grid-based movement

**You Can Use Its Math:**

```csharp
// Look at CharacterGridMovement.cs to see how TDE:
// 1. Converts world position to grid coordinates
// 2. Snaps characters to grid cells
// 3. Validates grid boundaries

// Example from TDE that you can adapt:
public class GridManager : MonoBehaviour
{
    public float GridUnitSize = 1.5f;
    
    // Similar to TDE's grid positioning
    public Vector3 GridPositionToWorldPosition(int row, int col, Vector3 gridOrigin)
    {
        return gridOrigin + new Vector3(
            col * GridUnitSize,
            0f,
            row * GridUnitSize
        );
    }
    
    public Vector2Int WorldPositionToGridPosition(Vector3 worldPos, Vector3 gridOrigin)
    {
        Vector3 offset = worldPos - gridOrigin;
        return new Vector2Int(
            Mathf.RoundToInt(offset.x / GridUnitSize),
            Mathf.RoundToInt(offset.z / GridUnitSize)
        );
    }
}
```

---

### **9. MMEventManager** (Use for All Game Events)

**TDE Pattern:**
```csharp
// Define your custom events
public enum GridDefenseEventTypes
{
    HeroDeployed,
    HeroMerged,
    WaveStarted,
    WaveCompleted,
    BaseHealthChanged
}

public struct GridDefenseEvent
{
    static GridDefenseEvent e;
    
    public GridDefenseEventTypes EventType;
    public Hero Hero;
    public int WaveNumber;
    public int BaseHealth;
    
    public GridDefenseEvent(GridDefenseEventTypes eventType)
    {
        e.EventType = eventType;
        e.Hero = null;
        e.WaveNumber = 0;
        e.BaseHealth = 0;
    }
    
    public static void Trigger(GridDefenseEventTypes eventType, Hero hero = null)
    {
        e.EventType = eventType;
        e.Hero = hero;
        MMEventManager.TriggerEvent(e);
    }
}

// Listen to events
public class UIManager : MonoBehaviour, MMEventListener<GridDefenseEvent>
{
    void OnEnable()
    {
        this.MMEventStartListening<GridDefenseEvent>();
    }
    
    void OnDisable()
    {
        this.MMEventStopListening<GridDefenseEvent>();
    }
    
    public void OnMMEvent(GridDefenseEvent gameEvent)
    {
        switch (gameEvent.EventType)
        {
            case GridDefenseEventTypes.HeroDeployed:
                UpdateHeroCount();
                break;
            case GridDefenseEventTypes.WaveStarted:
                ShowWaveStartUI(gameEvent.WaveNumber);
                break;
        }
    }
}
```

---

## 🎯 Summary: What to Use vs Build

| System | Use TDE Directly | Extend TDE | Build Custom | Notes |
|--------|------------------|------------|--------------|-------|
| **Weapons** | ✅ Yes | | | ProjectileWeapon perfect for auto-fire |
| **Health/Damage** | ✅ Yes | | | Use as-is for all entities |
| **MMFeedbacks** | ✅ Yes | | | Essential for game feel |
| **AI Pathing** | ✅ Yes | | | For enemy movement to base |
| **GameManager** | | ✅ Extend | | Add wave/base health tracking |
| **Auto-Combat** | | ✅ Extend | | Create CharacterAutoCombat ability |
| **Grid System** | | ✅ Adapt Math | ✅ Build | Use TDE's positioning math |
| **Hero Queue** | | ✅ Use Spawning | ✅ Build | Use TDE spawning, custom queue logic |
| **Deployment Input** | | ✅ Create Ability | | CharacterDeployment ability |
| **Wave System** | | ✅ Use Spawning | ✅ Build | Use TDE enemy spawning patterns |
| **Merge System** | | | ✅ Build | Completely custom logic |

---

## 🚀 Recommended Workflow

### **Phase 1: Use TDE Systems**
1. Set up `GameManager` and `LevelManager` in scene
2. Create hero prefab with:
   - `Health` component
   - `Weapon` component
   - `MMFeedbacks` components
3. Create enemy prefab with:
   - `Character` (AI type)
   - `AIBrain` + `AIActionMoveTowardsTarget3D`
   - `Health` component

### **Phase 2: Extend TDE Patterns**
4. Create `CharacterAutoCombat` ability (extends `CharacterAbility`)
5. Create custom `GridDefenseGameManager` (extends `GameManager`)
6. Use TDE's event system for all game events

### **Phase 3: Build Custom Systems**
7. Build `GridManager` (use TDE positioning math)
8. Build `HeroQueueManager` (use TDE spawning)
9. Build merge detection system

---

## 📚 Key TDE Files to Study

**Must Read:**
1. `CharacterAbility.cs` - Base for all abilities
2. `Weapon.cs` - Weapon system
3. `Health.cs` - Health system
4. `AIBrain.cs` - AI behavior
5. `GameManager.cs` - Game state

**Good to Reference:**
6. `CharacterGridMovement.cs` - Grid positioning math
7. `MMEventManager.cs` - Event system
8. `LevelManager.cs` - Spawning patterns

---

**Bottom Line:** You're not building from scratch - you're **extending TopDown Engine** with custom gameplay systems while leveraging its combat, health, AI, and feedback infrastructure!
