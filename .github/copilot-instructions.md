# ProjectBlast - AI Coding Agent Instructions

## Project Overview
Unity 6 (6000.2.10f1) **tower defense/shoot 'em up roguelike** with character upgrades targeting iOS/Android/PC. Features grid-based character deployment, auto-combat, merge mechanics, and wave-based enemy progression.

## Game Core Mechanics

### Game Flow & Layout
```
┌─────────────────────────────────────┐
│  ENEMY SPAWN ZONE (Top 50%)         │
│  → Enemies spawn and move down      │
│  → Target: Base/Tower at midpoint   │
├═════════════════════════════════════┤ ← BASE/TOWER (Screen center)
│  DEFENDING ROW (Dynamic X width)    │
│  → Auto-shoot enemies until ammo=0  │
│  → Player taps Ready Row to deploy  │
│  → 3 same type merge → Level up     │
├─────────────────────────────────────┤
│  READY ROW (Dynamic X width)        │
│  → TAP to deploy to Defending       │
│  → Auto-fills from Prep Rows        │
├─────────────────────────────────────┤
│  PREP ROWS (Dynamic count & width)  │
│  → Characters spawn with animation  │
│  → Queue system: auto-advance up    │
│  → Bottom 50% of screen             │
└─────────────────────────────────────┘
```

### Core Mechanics
1. **Character Deployment**: Player taps Ready Row characters → instantly/smoothly moves to Defending Row empty slot
2. **Auto-Combat**: Defending Row characters auto-aim and auto-shoot until ammo depleted or dead
3. **Merge System**: 3 identical characters in same row → merge into Level 2 (enhanced stats)
4. **Queue Advancement**: Slot empties → next character auto-advances from lower row
5. **Win/Loss**: Survive enemy waves (win) or Base destroyed (loss)

## Architecture & Key Components

### Unity Configuration
- **Render Pipeline**: URP 17.2.0 with Mobile/PC renderer assets optimized for 60 FPS
- **Target Platforms**: iOS, Android (primary), PC (secondary)
- **Input System**: New Input System (1.14.2) for touch + keyboard/mouse hybrid
- **Performance Budget**: Max 200 enemies, 50 projectiles, extensive object pooling

### Project Structure
```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs        # State machine: Menu→Playing→Upgrading→GameOver
│   ├── PoolManager.cs        # ObjectPool<T> for projectiles/enemies/VFX
│   ├── ServiceLocator.cs     # Dependency injection pattern
│   └── SaveSystem.cs         # Progress/upgrades persistence
├── Entities/
│   ├── Base/
│   │   ├── Entity.cs         # Abstract base class
│   │   ├── IEntity.cs        # Core entity interface
│   │   └── EntityDataSO.cs   # Data-driven entity definitions
│   ├── Characters/           # Player deployable units
│   ├── Enemies/              # Enemy types (Swarmer, Tank, Flyer, Boss)
│   └── Projectiles/          # Bullet, Missile, Laser types
├── Components/               # Modular entity components
│   ├── HealthComponent.cs
│   ├── MovementComponent.cs
│   ├── AttackComponent.cs
│   └── UpgradeableComponent.cs
├── Grid/
│   ├── Base/
│   │   ├── GridSystem.cs     # Abstract grid base
│   │   ├── Grid2D.cs         # 2D implementation
│   │   ├── GridCell.cs       # Individual cell data
│   │   └── GridVisualizer.cs # Debug drawing
│   ├── GameGridManager.cs    # Multi-row management
│   ├── GridRow.cs            # Single row with dynamic width
│   └── GridSlot.cs           # Individual slot with tap detection
├── Systems/
│   ├── WaveSystem.cs         # Enemy wave spawning
│   ├── UpgradeSystem.cs      # Character progression trees
│   ├── MergeSystem.cs        # 3-character merge logic
│   └── DeploymentSystem.cs   # Character movement between rows
├── Data/                     # ScriptableObject instances
│   ├── Characters/
│   ├── Enemies/
│   ├── Weapons/
│   ├── Upgrades/
│   └── GridLayouts/
├── UI/                       # HUD, menus, upgrade screens
└── Utilities/                # Extensions, helpers, constants
```

### Core Systems

#### Grid System (Multi-Row, Dynamic Width)
```csharp
// Dynamic row configuration
public class DynamicRowConfig {
    public RowType Type;           // Defending, Ready, Preparation
    public int SlotCount;          // Configurable X width
    public float CellSize;
    public Vector3 WorldOffset;
    public bool AllowPlayerTap;    // Only Ready Row
    public bool EnableMerging;     // 3 same = level up
}

// Row types
public enum RowType { Defending, Ready, Preparation }
```

#### Entity Class System
```csharp
// Base entity with component architecture
public abstract class Entity : MonoBehaviour, IEntity {
    protected HealthComponent health;
    protected MovementComponent movement;
    protected AttackComponent attack;
}

// Entity classes
public enum EntityClass {
    // Characters (deployable)
    Gunner, Sniper, Engineer, Tank, Mage,
    // Enemies
    Swarmer, HeavyTank, Flyer, Boss, Spawner,
    // Other
    Projectile, Collectible
}
```

#### Character Merge System
- 3 identical characters in same row → auto-merge to Level 2
- Stats increase: Damage ×1.5, AttackSpeed ×0.9 (faster), Ammo ×1.2
- Visual merge effect + level indicator

#### Queue Advancement Logic
1. Defending slot empties → Ready Row character auto-advances
2. Ready slot empties → Prep Row character auto-advances (FIFO)
3. Prep slot empties → New character spawns from queue

## Mobile-Optimized Coding Conventions

### Performance Patterns
```csharp
// Use readonly for fixed references (reduces GC)
private readonly Transform firePoint;

// Cache component references in Awake/Start
private SpriteRenderer spriteRenderer;

// AVOID LINQ on mobile (causes allocations)
// BAD:  enemies.Where(e => e.Health > 0).ToList();
// GOOD: for loop with manual list building

// Use struct for data-only types
public struct DamageInfo {
    public float Amount;
    public DamageType Type;
    public Vector3 HitPoint;
}

// Prefer [SerializeField] over public
[SerializeField] private float attackSpeed = 1.5f;
```

### Namespaces (Organized as project grows)
```csharp
namespace ProjectBlast.Core { }
namespace ProjectBlast.Entities { }
namespace ProjectBlast.Grid { }
namespace ProjectBlast.Systems { }
```

### Object Pooling (Critical for Mobile)
```csharp
// Use Unity's ObjectPool<T> or custom pool
private ObjectPool<Projectile> projectilePool;

// Target: <100 instantiations per second in combat
// Pre-warm pools at scene start
```

### Input System Pattern
```csharp
// Touch + keyboard/mouse hybrid
private InputSystem_Actions inputActions;

private void OnEnable() {
    inputActions.UI.Click.performed += OnTap;
    inputActions.Enable();
}

private void OnTap(InputAction.CallbackContext context) {
    Vector2 screenPos = context.ReadValue<Vector2>();
    // Raycast to detect GridSlot tap
}
```

## Development Workflow

### Building & Running
- Unity 6 (6000.2.10f1+) Play button for iteration
- Target 60 FPS on mid-range mobile devices
- VSCode integration: "Attach to Unity" debugger (`.vscode/launch.json`)

### Testing Strategy
- Test Framework (1.6.0) available
- Create PlayMode tests for merge logic, queue advancement, combat
- Performance profiling: Deep Profile on mobile builds

### URP-Specific
- Use URP/Lit or URP/Unlit shaders only
- Mobile renderer: `Mobile_Renderer.asset` for optimization
- Post-processing via `DefaultVolumeProfile.asset`

## Data-Driven Design

### ScriptableObject Pattern
```csharp
[CreateAssetMenu(fileName = "Character", menuName = "ProjectBlast/Character")]
public class CharacterDataSO : ScriptableObject {
    public string CharacterName;
    public EntityClass Class;
    public float MaxHealth;
    public float AttackDamage;
    public float AttackSpeed;
    public int MaxAmmo;
    public GameObject Prefab;
}

// Designers tweak stats in Unity Inspector without code changes
```

### Grid Layout Configuration
```csharp
[CreateAssetMenu(fileName = "GridLayout", menuName = "ProjectBlast/Grid Layout")]
public class GridLayoutSO : ScriptableObject {
    public int defendingRowSlots = 5;
    public int readyRowSlots = 5;
    public int prepRowCount = 3;
    public int prepRowSlots = 5;
    public float cellSize = 1.5f;
}
```

## Critical Game-Specific Patterns

### Character Lifecycle
```csharp
// 1. Spawn in Prep Row with animation
// 2. Auto-advance to Ready Row when slot available
// 3. Player taps → Move to Defending Row
// 4. EnableCombatMode() → Auto-shoot until ammo=0 or death
// 5. Slot empties → Queue advancement triggers
```

### Merge Detection
```csharp
// After placing character in row:
// 1. Check for 3+ matching type in same row
// 2. Keep first, destroy others
// 3. LevelUp() first character (stats × multipliers)
// 4. Play merge VFX
```

### State Machine (GameManager)
```csharp
public enum GameState { Menu, Playing, Upgrading, GameOver }

// Transitions:
// Menu → Playing (start wave)
// Playing → Upgrading (wave complete)
// Upgrading → Playing (upgrades selected)
// Playing → GameOver (base destroyed)
```

## Common Pitfalls

### Unity-Specific
- **New Input System**: Use `InputSystem_Actions.inputactions`, not legacy `Input.GetKey()`
- **URP Shaders**: Standard shaders incompatible - use URP materials
- **Meta Files**: Auto-generated, version controlled (hidden in VS Code)
- **Scene References**: Use ScriptableObjects for cross-scene data

### Mobile Performance
- **Avoid allocations in Update()**: Cache, use object pools, no LINQ
- **Texture atlasing**: Combine sprites to reduce draw calls
- **Update budget**: Max 200 active enemies, 50 projectiles on screen
- **Pathfinding**: Use NavMesh or A* with chunked updates (not every frame)

### Grid System
- **Dynamic width**: All rows configurable in inspector via `SlotCount`
- **Queue order**: FIFO from bottom Prep Row to top
- **Merge timing**: Check after PlaceCharacter(), not in Update()
- **Tap detection**: Only Ready Row has colliders for input

## Design Flexibility & Extension

### Easy to Change Later
✅ Add new character classes (inherit Entity, implement components)
✅ Modify stats via ScriptableObjects (no code changes)
✅ Add new row types (extend RowType enum, configure in inspector)
✅ Change merge rules (adjust MergeSystem thresholds)
✅ Add weapon variety (ScriptableObject weapon data)
✅ Implement AI behaviors (swap component implementations)
✅ Add multiplayer (interface-based design supports it)

### Migration Strategy
```csharp
// When refactoring major systems:
[Obsolete("Use HealthComponent instead")]
public class OldHealthSystem { }

public class HealthComponent : MonoBehaviour { } // New version
// Keep old code, gradually migrate, then remove
```

## Next Steps for Development
1. ✅ Update architecture documentation (this file)
2. Create folder structure and namespace organization
3. Implement base Grid system (GridSystem.cs, Grid2D.cs, GridCell.cs)
4. Build multi-row grid manager (GameGridManager, GridRow, GridSlot)
5. Create entity base classes and components
6. Set up ScriptableObject data templates
7. Implement Character class with merge and combat
8. Create object pooling system
9. Refactor GameManager with state machine
10. Configure Input System for tap detection
