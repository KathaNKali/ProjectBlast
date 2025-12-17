# ProjectBlast - Scripts Summary

**Last Updated:** December 17, 2025  
**Total Scripts:** 15  
**Total Lines:** 4,341

This document provides a quick reference for all custom scripts in ProjectBlast.

---

## 📂 Script Organization

```
Assets/ProjectBlast/Scripts/
├── Grid/                    (844 lines)
│   ├── GridManager.cs      (731 lines) - MMSingleton grid system
│   ├── GridSlot.cs         (84 lines)  - Slot data structure
│   └── GridZone.cs         (29 lines)  - Zone enum
│
├── Heroes/                  (1,612 lines)
│   ├── Hero.cs             (883 lines) - Main hero orchestration
│   ├── HeroQueueManager.cs (608 lines) - MMSingleton queue system
│   └── HeroAmmo.cs         (121 lines) - CharacterAbility for ammo
│
├── Data/                    (606 lines)
│   ├── HeroDataSO.cs       (221 lines) - Hero ScriptableObject
│   ├── ProjectileDamageConfigurator.cs (172 lines) - Projectile setup
│   ├── WeaponDataSO.cs     (141 lines) - Weapon ScriptableObject
│   └── WeaponDataHolder.cs (72 lines)  - Weapon prefab component
│
├── AI/                      (411 lines)
│   └── AIDecisionDetectTargetPriority3D.cs (411 lines) - Custom AI decision
│
├── Enemy/                   (295 lines)
│   └── SimpleEnemySpawner.cs (295 lines) - Test spawner
│
├── Camera/                  (93 lines)
│   └── BattlefieldCameraTarget.cs (93 lines) - Dynamic camera focus
│
└── Testing/                 (480 lines)
    ├── GridManagerTester.cs (322 lines) - Grid testing utility
    └── TestTarget.cs        (158 lines) - Combat target dummy
```

---

## 🎯 Core Systems Overview

### Grid System (844 lines)
**Purpose:** Manages the 3-zone battlefield grid with lane-based hero placement.

**Key Files:**
- `GridManager.cs` (731 lines)
  - MMSingleton pattern for global access
  - 3 grid zones: Passive, Active, Firing
  - Slot tracking and occupancy management
  - Lane query methods (GetLaneHeroes, IsLaneEmpty, etc.)
  - World-to-grid coordinate conversion
  - Visual gizmos for debugging

- `GridSlot.cs` (84 lines)
  - Represents a single grid slot
  - Tracks position, occupancy, zone
  - Hero reference management

- `GridZone.cs` (29 lines)
  - Enum: Passive, Active, Firing

**Key Methods:**
```csharp
// GridManager
GridSlot GetSlot(GridZone zone, int row, int column)
List<GridSlot> GetEmptySlots(GridZone zone)
List<Hero> GetLaneHeroes(int column)
GridSlot GetLeftmostEmptySlot(GridZone zone)
void PlaceHeroInSlot(Hero hero, GridSlot slot)
void RemoveHeroFromSlot(GridSlot slot)
```

---

### Hero System (1,612 lines)
**Purpose:** Hero spawning, deployment, combat, and queue management.

**Key Files:**
- `Hero.cs` (883 lines) - **The Heart of Combat**
  - TDE Character + Health + Weapon integration
  - AIBrain orchestration (state control)
  - Zone-based combat (only fire in Firing zone)
  - ScriptableObject-driven configuration
  - Ammo tracking and consumption
  - Weapon state monitoring
  - Death and removal handling
  - Event-driven communication (MMAmmoEvent)

- `HeroQueueManager.cs` (608 lines) - **Queue Orchestration**
  - MMSingleton pattern
  - Vertical lane-based shifting
  - BuildLaneShiftPlan (bottom-to-top iteration)
  - Smooth Lerp animations (0.3s)
  - Input blocking during animations
  - Test spawning system
  - Click detection and deployment

- `HeroAmmo.cs` (121 lines) - **TDE Ability Pattern**
  - Extends CharacterAbility
  - Ammo initialization and consumption
  - Low ammo warnings
  - Event firing (MMAmmoEvent)
  - Follows TDE's FindAbility<T>() pattern

**Key Methods:**
```csharp
// Hero.cs
void ConfigureAI()                    // Apply HeroDataSO to AI components
void StartFiring()                    // Activate AIBrain for combat
void StopFiring()                     // Deactivate AIBrain
void ConsumeAmmo(int amount)          // Reduce ammo, check for depletion
void OnZoneChanged(GridZone newZone)  // Handle zone transitions

// HeroQueueManager.cs
void SpawnTestHeroes()                // Fill Passive and Active zones
void DeployToFiring(Hero hero)        // Deploy Active hero to Firing
void BuildLaneShiftPlan(int column)   // Calculate lane compacting
IEnumerator AnimateLaneShift(...)     // Smooth hero movement
```

---

### Data Architecture (606 lines)
**Purpose:** ScriptableObject-driven configuration for heroes and weapons.

**Key Files:**
- `HeroDataSO.cs` (221 lines) - **Hero Configuration**
  - Identity: Name, class, icon, description
  - Health: Max/starting health
  - Detection: Range, layers, search interval
  - Ammo: Starting ammo, unlimited flag, low threshold
  - Weapon: Default weapon prefab reference
  - Fire Rate: Shots per second
  - DPS calculation helper

- `WeaponDataSO.cs` (141 lines) - **Weapon Configuration**
  - Identity: Name, type, icon, description
  - Damage: Per shot, damage type
  - Ammo: Consumption per shot
  - Fire Rate: Shots per second (optional override)
  - Projectile: Prefab, speed, lifetime
  - ApplyToWeapon() method for runtime config

- `WeaponDataHolder.cs` (72 lines) - **Weapon Prefab Component**
  - Attaches to weapon prefabs
  - Carries WeaponDataSO reference
  - Auto-applies data on Awake
  - Provides getter methods for ammo/damage

- `ProjectileDamageConfigurator.cs` (172 lines) - **Projectile Setup**
  - Configures projectile damage on spawn
  - Applies damage type and values
  - Handles DamageOnTouch component

**ScriptableObject Pattern:**
```
1. Create SO asset in Unity (Assets → Create → ProjectBlast)
2. Configure stats in Inspector
3. Reference SO in prefab
4. SO auto-applies to components on initialization
```

---

### AI System (411 lines)
**Purpose:** Extended TDE AI with priority-based targeting.

**Key Files:**
- `AIDecisionDetectTargetPriority3D.cs` (411 lines) - **Priority Targeting**
  - Extends TDE's AIDecision base class
  - 5 priority modes:
    - Closest: Target nearest enemy
    - Farthest: Target farthest enemy
    - LowestHealth: Focus fire on weak enemies
    - HighestHealth: Eliminate threats first
    - FirstDetected: Original TDE behavior
  - Lock-on mode option
  - Health-based sorting with frame caching
  - MMConeOfVision integration
  - Per-hero configuration in Inspector

**Usage:**
```csharp
// Add to AIBrain GameObject (replaces AIDecisionDetectTargetRadius3D)
// Configure in Inspector:
// - TargetConeOfVision (MMConeOfVision reference)
// - Priority (Closest/Farthest/LowestHealth/etc.)
// - LockOntoTarget (true/false)
```

---

### Enemy System (295 lines)
**Purpose:** Enemy spawning and configuration.

**Key Files:**
- `SimpleEnemySpawner.cs` (295 lines) - **Test Spawner**
  - Configurable spawn count and timing
  - Randomized enemy health (min/max range)
  - Spawn area with size configuration
  - Min distance between enemies
  - Initial delay and spawn intervals
  - SpawnOnStart option
  - Inspector test buttons
  - Gizmo visualization

**Usage:**
```csharp
// Add to scene GameObject
// Configure in Inspector:
// - EnemyPrefab (must have Health component)
// - SpawnCount, SpawnOnStart
// - SpawnAreaSize, MinDistanceBetweenEnemies
// - MinHealth, MaxHealth
```

---

### Camera System (93 lines)
**Purpose:** Dynamic camera targeting for battlefield view.

**Key Files:**
- `BattlefieldCameraTarget.cs` (93 lines) - **Camera Focus**
  - Calculates battlefield center from GridManager
  - Updates camera target as grid state changes
  - Designed for Cinemachine integration
  - Configurable offset and smoothing

---

### Testing Infrastructure (480 lines)
**Purpose:** Development testing and debugging utilities.

**Key Files:**
- `GridManagerTester.cs` (322 lines) - **Grid Testing**
  - Inspector buttons for grid operations
  - Test slot queries (empty, occupied, lane)
  - Test hero placement and removal
  - Visual feedback and debug logging
  - Comprehensive grid API testing

- `TestTarget.cs` (158 lines) - **Combat Target**
  - Simple target dummy for testing hero firing
  - Configurable health
  - Visual feedback on hit
  - Destruction on death
  - Spawn button in Inspector

---

## 🔗 System Integration Map

```
GridManager (Scene)
    ↓
HeroQueueManager (Scene)
    ↓ spawns
Hero (GameObject)
    ├── Uses: HeroDataSO (config)
    ├── Has: Character, Health, CharacterHandleWeapon (TDE)
    ├── Has: HeroAmmo (CharacterAbility)
    ├── Controls: AIBrain (child GameObject)
    │   ├── AIActionShoot3D (TDE)
    │   ├── AIActionAimWeaponAtTarget3D (TDE)
    │   ├── AIDecisionDetectTargetPriority3D (Custom)
    │   └── AIDecisionLineOfSightToTarget3D (TDE)
    └── Equips: Weapon (TDE)
        ├── Uses: WeaponDataSO (config)
        ├── Has: WeaponDataHolder (component)
        └── Spawns: Projectile (TDE)
            └── Has: ProjectileDamageConfigurator
```

---

## 📈 Code Statistics

### By Category

| Category | Scripts | Lines | % of Total |
|----------|---------|-------|------------|
| **Heroes** | 3 | 1,612 | 37.1% |
| **Grid** | 3 | 844 | 19.4% |
| **Data** | 4 | 606 | 14.0% |
| **Testing** | 2 | 480 | 11.1% |
| **AI** | 1 | 411 | 9.5% |
| **Enemy** | 1 | 295 | 6.8% |
| **Camera** | 1 | 93 | 2.1% |
| **TOTAL** | **15** | **4,341** | **100%** |

### Largest Files

1. `Hero.cs` - 883 lines (20.3%)
2. `GridManager.cs` - 731 lines (16.8%)
3. `HeroQueueManager.cs` - 608 lines (14.0%)
4. `AIDecisionDetectTargetPriority3D.cs` - 411 lines (9.5%)
5. `GridManagerTester.cs` - 322 lines (7.4%)

### TDE Integration

| Type | Count | Examples |
|------|-------|----------|
| **MMSingleton<T>** | 2 | GridManager, HeroQueueManager |
| **CharacterAbility** | 1 | HeroAmmo |
| **AIDecision** | 1 | AIDecisionDetectTargetPriority3D |
| **ScriptableObject** | 2 | HeroDataSO, WeaponDataSO |
| **MonoBehaviour** | 9 | Hero, TestTarget, etc. |

---

## 🎯 Key Design Patterns

### 1. MMSingleton Pattern
```csharp
public class GridManager : MMSingleton<GridManager>
{
    // Global access: GridManager.Instance
}
```
**Used by:** GridManager, HeroQueueManager

### 2. ScriptableObject Configuration
```csharp
[CreateAssetMenu(menuName = "ProjectBlast/Hero Data")]
public class HeroDataSO : ScriptableObject
{
    // Inspector-configured stats
}
```
**Used by:** HeroDataSO, WeaponDataSO

### 3. CharacterAbility Extension
```csharp
public class HeroAmmo : CharacterAbility
{
    // Discovered via: character.FindAbility<HeroAmmo>()
}
```
**Used by:** HeroAmmo

### 4. AIDecision Extension
```csharp
public class AIDecisionDetectTargetPriority3D : AIDecision
{
    // Drop-in replacement for TDE AI decisions
}
```
**Used by:** AIDecisionDetectTargetPriority3D

### 5. Event-Driven Communication
```csharp
public class Hero : MonoBehaviour, MMEventListener<MMAmmoEvent>
{
    public void OnMMEvent(MMAmmoEvent ammoEvent) { }
}
```
**Used by:** Hero (listens to MMAmmoEvent)

---

## 🚀 Next Scripts to Implement

### Phase 3: Enemy AI & Stages (In Progress)
1. **Enemy.cs** (~500 lines)
   - TDE Character + Health + AIBrain
   - Pathfinding toward base
   - Attack logic
   - Death handling

2. **StageManager.cs** (~400 lines)
   - Multi-stage level flow
   - Stage transition logic
   - Completion detection
   - Stage data ScriptableObject

3. **WaveManager.cs** (~350 lines)
   - Coordinated enemy spawning
   - Wave patterns and timing
   - Difficulty scaling
   - Wave data ScriptableObject

4. **Base.cs** (~200 lines)
   - Base Health management
   - Damage visualization
   - Destruction logic
   - Target for enemies

### Phase 4-6: UI & Meta-Game (Planned)
5. **BattleUI.cs** (~300 lines)
6. **DeckManager.cs** (~400 lines)
7. **HeroCollectionManager.cs** (~500 lines)
8. **ProgressionManager.cs** (~400 lines)

---

## 📚 Related Documentation

For detailed implementation guides, see:
- **[GAME_DEVELOPMENT_PLAN.md](./GAME_DEVELOPMENT_PLAN.md)** - Full development plan
- **[GRID_DEFENSE_ARCHITECTURE.md](./GRID_DEFENSE_ARCHITECTURE.md)** - TDE integration
- **[Documentation/HERO_AIBRAIN_INTEGRATION.md](./Documentation/HERO_AIBRAIN_INTEGRATION.md)** - AI setup
- **[Documentation/AI_TARGET_PRIORITY.md](./Documentation/AI_TARGET_PRIORITY.md)** - Priority system
- **[QUICK_START.md](./QUICK_START.md)** - Quick start guide

---

**Note:** This summary reflects the actual implemented code as of December 17, 2025. All line counts verified via `wc -l` on actual script files.
