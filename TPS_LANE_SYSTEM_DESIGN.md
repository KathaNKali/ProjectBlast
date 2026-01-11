# TPS + Lane System Design - Complete Architecture

**ProjectBlast - Threat Per Second Lane-Based Enemy Spawning**

---

## 🎯 System Overview

This document defines the complete **TPS (Threat Per Second) + Lane System** architecture for ProjectBlast. The system combines:

1. **TPS Budget System** - Dynamic spawning based on threat allocation rather than fixed timers
2. **Lane-Based Enemy Movement** - Enemies spawn and move in defined vertical lanes
3. **Forward Movement** - Enemies advance toward the player base (Z+ → Z-)
4. **Forward Shooting** - Enemies shoot forward in their lane while advancing

---

## 📐 Lane Architecture

### **Lane Configuration**

```
                  TOP (Enemy Spawn Zone Z = +20)
┌──────────────────────────────────────────────────────┐
│                  ENEMY SPAWN AREA                    │
│   Lane 0        Lane 1        Lane 2      (N lanes) │
│     ↓             ↓             ↓                    │
└──────────────────────────────────────────────────────┘
      ↓             ↓             ↓
      ↓             ↓             ↓  (Enemies move forward)
      ↓             ↓             ↓
┌──────────────────────────────────────────────────────┐
│              HERO FIRING ZONE (Z = 0)                │
│   [H][H][H]   [H][H][H]   [H][H][H]                 │
│   Lane 0      Lane 1      Lane 2                     │
└──────────────────────────────────────────────────────┘
      ↓             ↓             ↓
┌──────────────────────────────────────────────────────┐
│               PLAYER BASE (Z = -8)                   │
│   [Base HP]   [Base HP]   [Base HP]                  │
└──────────────────────────────────────────────────────┘
                  BOTTOM (Base Position)
```

### **Lane Specifications**

**Number of Lanes:**
- **Initial:** 3 lanes (matches hero grid columns)
- **Scalable:** 2-5 lanes dynamically based on stage/difficulty
- **Alignment:** Enemy lanes align with hero firing grid columns

**Lane Width:**
- Matches hero grid `CellSize + CellSpacing` from `GridManager`
- Default: 1.5 + 0.3 = 1.8 units per lane
- Center positions: Lane 0 = X:-1.8, Lane 1 = X:0, Lane 2 = X:+1.8

**Lane Rules:**
1. **Strict Lane Containment** - Enemies spawn and stay in assigned lane (no lane switching)
2. **Independent Movement** - Each lane has independent spawn timing and enemy queue
3. **Lane Allocation** - TPS director distributes threat budget across lanes
4. **Empty Lanes Allowed** - Lanes can be temporarily empty (strategic variance)

---

## 💰 TPS (Threat Per Second) System

### **Core Concept**

Instead of spawning enemies on fixed timers (e.g., "1 enemy every 3 seconds"), the **TPS system** uses a **threat budget** that accumulates over time and "purchases" enemies dynamically.

**Key Principle:**
```
TPS = Threat Points Per Second
Enemy Threat Value = Enemy's effective combat power (HP-based calculation)

Every frame: Accumulate threat budget
When budget sufficient: Spawn appropriate enemy type
Spent budget: Deduct enemy's threat cost
```

### **Threat Calculation Formula**

```csharp
// Enemy Threat Value
float CalculateEnemyThreat(EnemyData enemy)
{
    float baseThreat = enemy.MaxHealth; // HP as base threat
    float rangeFactor = enemy.AttackRange / 10f; // Range bonus
    float damageFactor = enemy.DamagePerShot / 10f; // Damage bonus
    float speedFactor = enemy.MovementSpeed / 2f; // Speed bonus
    
    return baseThreat * (1 + rangeFactor + damageFactor + speedFactor);
}

// Examples:
// Weak Enemy:    100 HP × 1.0 = 100 threat
// Normal Enemy:  200 HP × 1.3 = 260 threat  
// Tank Enemy:    500 HP × 1.2 = 600 threat
// Fast Enemy:    150 HP × 1.8 = 270 threat
```

### **TPS Budget Accumulation**

```csharp
// Global TPS accumulator
float threatBudget = 0f;
float currentTPS = 50f; // Base 50 threat/second

void Update()
{
    // Accumulate budget every frame
    threatBudget += currentTPS * Time.deltaTime;
    
    // Try to spend budget on spawning
    TrySpawnEnemies();
}

void TrySpawnEnemies()
{
    // Get cheapest affordable enemy
    EnemyData enemy = GetAffordableEnemy(threatBudget);
    
    if (enemy != null)
    {
        SpawnEnemy(enemy);
        threatBudget -= enemy.ThreatCost;
    }
}
```

### **TPS Phases (Implementation Roadmap)**

**Phase 1: Basic Global TPS** *(Start Here)*
- Single global TPS value (e.g., 50 TPS)
- Accumulate threat budget globally
- Spawn enemies when budget available
- Spawn in random lane

**Phase 2: Lane-Based TPS Allocation**
- Distribute global TPS across lanes
- Each lane has independent budget
- Allocation strategies:
  - Even distribution: 50 TPS ÷ 3 = 16.67 TPS per lane
  - Weighted distribution: Lane 0=40%, Lane 1=30%, Lane 2=30%
  - Dynamic: Favor lanes with fewer enemies

**Phase 3: Dynamic TPS Adjustment**
- Increase TPS when heroes are strong (high DPS detected)
- Decrease TPS when player struggling (many leaks, low health)
- Factor in: Hero count, hero DPS, enemy leak rate

**Phase 4: Wave-Based TPS Curves**
- Define TPS curves per wave
- Example Wave 1: Ramp from 20 TPS → 50 TPS over 60s
- Example Wave 2: Start 40 TPS → 80 TPS over 90s
- Boss waves: Accumulate high budget, spawn single boss

**Phase 5: Enemy Type Diversity**
- Weighted random selection based on budget
- Budget 100-200: Spawn weak enemies
- Budget 200-400: Mix weak/normal
- Budget 400+: Can afford tank/fast enemies

**Phase 6: Debug Telemetry**
- Real-time TPS monitoring UI
- Show: Current TPS, budget, spawns/minute, lane distribution
- Tuning tools: TPS multiplier slider, manual spawn buttons

---

## 🚶 Enemy Movement System

### **Movement Pattern**

**Vertical Lane Movement:**
- Enemies spawn at Z = +20 (top of battlefield)
- Move forward (negative Z direction) toward base at Z = -8
- Stay within assigned lane X position (no horizontal movement)
- Constant speed: 2-5 units/second depending on enemy type

**Movement States:**
```
1. SPAWNING    - Instantiate at spawn point, initialize AI
2. ADVANCING   - Moving forward in lane (default state)
3. ENGAGING    - In range of heroes, moving + shooting
4. BREACHING   - Reached base, dealing damage to base
5. DYING       - Health depleted, death animation
```

### **TDE AI Integration**

**AIBrain Configuration:**
```
Enemy GameObject
├── AIBrain (component)
│   ├── States:
│   │   ├── "Advance" State (default)
│   │   │   ├── Decisions: None (always active)
│   │   │   └── Actions: AIActionMoveForward3D (custom)
│   │   └── "Engage" State
│   │       ├── Decisions: AIDecisionDetectTargetRadius3D (detect heroes)
│   │       └── Actions: 
│   │           ├── AIActionMoveForward3D (continue moving)
│   │           └── AIActionShootForward3D (shoot while moving)
│   ├── Target: Assigned to PlayerBase transform
│   └── Brain Active: true on spawn
```

**Custom AI Actions Required:**

**1. AIActionMoveForward3D**
```csharp
// Move enemy forward in lane (toward negative Z)
public class AIActionMoveForward3D : AIAction
{
    public float MoveSpeed = 3f;
    public bool ConstrainToLane = true; // Lock X position
    
    private float _laneXPosition;
    
    public override void OnEnterState()
    {
        _laneXPosition = transform.position.x; // Lock X on spawn
    }
    
    public override void PerformAction()
    {
        Vector3 forward = Vector3.back; // Negative Z
        Vector3 newPos = transform.position + forward * MoveSpeed * Time.deltaTime;
        
        if (ConstrainToLane)
        {
            newPos.x = _laneXPosition; // Enforce lane constraint
        }
        
        transform.position = newPos;
        
        // Check if reached base
        if (transform.position.z <= -8f)
        {
            OnReachedBase();
        }
    }
}
```

**2. AIActionShootForward3D**
```csharp
// Shoot forward in lane while moving
public class AIActionShootForward3D : AIAction
{
    public float ShootRange = 10f;
    public LayerMask TargetLayer; // Heroes layer
    
    private CharacterHandleWeapon _weaponHandler;
    
    public override void PerformAction()
    {
        // Raycast forward to detect heroes in lane
        Vector3 forward = Vector3.back;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, forward, out hit, ShootRange, TargetLayer))
        {
            // Hero detected in range, shoot
            _weaponHandler.ShootStart();
        }
        else
        {
            _weaponHandler.ShootStop();
        }
    }
}
```

### **Movement Speed Tiers**

```csharp
// Enemy speed categories
public enum EnemySpeedType
{
    Slow = 0,   // 2 units/s  - Tank enemies
    Normal = 1, // 3 units/s  - Standard enemies
    Fast = 2,   // 4 units/s  - Light enemies
    Rushing = 3 // 6 units/s  - Suicide rushers
}

// Movement time examples (Z: +20 → -8 = 28 units)
// Slow:    28 / 2 = 14 seconds
// Normal:  28 / 3 = 9.3 seconds
// Fast:    28 / 4 = 7 seconds
// Rushing: 28 / 6 = 4.7 seconds
```

---

## 🔫 Enemy Shooting System

### **Forward Shooting Mechanics**

**Shooting Rules:**
1. **Direction:** Enemies shoot FORWARD (negative Z) in their lane
2. **Range Detection:** Raycast forward to detect heroes in lane
3. **Continuous Fire:** Shoot while moving if hero in range
4. **No Tracking:** Enemies don't rotate/aim, just shoot straight forward
5. **Projectile:** Use TDE `ProjectileWeapon` system

### **Weapon Configuration**

**Enemy Weapon Setup:**
```
Enemy_Prefab
├── EnemyModel (visual)
├── Character (TDE component)
├── CharacterHandleWeapon (TDE ability)
├── AIBrain
└── WeaponAttachment (transform)
    └── EnemyWeapon_Prefab
        ├── ProjectileWeapon (TDE component)
        │   ├── ProjectileSpawnOffset: (0, 0.5, -0.5) - Forward offset
        │   ├── WeaponAngle: (0, 0, 0) - Straight forward
        │   ├── Recoil: None
        │   └── Spread: 0-5 degrees (slight inaccuracy)
        ├── WeaponAmmo
        │   └── MaxAmmo: Unlimited (-1)
        └── Projectile Prefab
            ├── Speed: 15 units/s
            ├── Lifetime: 2 seconds
            ├── DamageOnTouch: 10-50 damage
            └── Layer: EnemyProjectile (collides with Heroes)
```

### **Shooting States**

**State 1: Out of Range**
- No heroes detected in forward raycast
- Weapon idle, no firing
- Continue advancing

**State 2: Hero in Range**
- Hero detected within `ShootRange` (e.g., 10 units forward)
- Trigger `CharacterHandleWeapon.ShootStart()`
- Fire continuously while moving
- Projectiles spawn and fly forward

**State 3: Hero Killed/Moved**
- Hero no longer in range
- Trigger `CharacterHandleWeapon.ShootStop()`
- Return to advancing only

### **Range Detection System**

```csharp
// Forward lane detection
public bool DetectHeroInLane(out Transform target)
{
    Vector3 forward = Vector3.back; // -Z direction
    RaycastHit[] hits = Physics.RaycastAll(
        transform.position, 
        forward, 
        ShootRange, 
        HeroLayerMask
    );
    
    // Find closest hero in lane (same X position within tolerance)
    Transform closestHero = null;
    float closestDistance = float.MaxValue;
    float laneXPosition = transform.position.x;
    float laneTolerance = 0.5f; // Allow small X variance
    
    foreach (RaycastHit hit in hits)
    {
        float xDifference = Mathf.Abs(hit.transform.position.x - laneXPosition);
        
        if (xDifference <= laneTolerance && hit.distance < closestDistance)
        {
            closestHero = hit.transform;
            closestDistance = hit.distance;
        }
    }
    
    target = closestHero;
    return closestHero != null;
}
```

**Shooting Decision Flow:**
```
Every frame:
1. Check if hero in forward raycast (ShootRange distance)
2. Verify hero is in same lane (X position tolerance)
3. If YES:
   - Start shooting
   - Continue moving forward
   - Projectiles fly forward
4. If NO:
   - Stop shooting
   - Continue moving forward
```

---

## 🏗️ Implementation Architecture

### **Component Structure**

```
Scene Hierarchy:
├── GameManager (TDE)
├── LevelManager (TDE)
├── GridManager (Custom - existing)
├── TPSDirector (NEW)
│   ├── Global TPS configuration
│   ├── Lane management
│   ├── Budget accumulation
│   └── Spawn coordination
├── LaneSpawner (NEW - per lane)
│   ├── Spawn point position
│   ├── Lane index
│   ├── Lane threat budget
│   └── Enemy queue
└── PlayerBase (3 instances, one per lane)
    ├── Health component
    └── Base breach detector
```

### **Class Diagram**

```
TPSDirector (MonoBehaviour, MMSingleton)
├── Fields:
│   ├── float GlobalTPS (50)
│   ├── float ThreatBudget (accumulated)
│   ├── List<LaneSpawner> Lanes
│   ├── List<EnemyDataSO> EnemyTypes
│   └── WaveConfigSO CurrentWave
├── Methods:
│   ├── Update() - Accumulate budget, distribute to lanes
│   ├── AllocateThreatToLanes() - Distribute budget
│   ├── GetAffordableEnemy(budget) - Find spawnable enemy
│   └── OnWaveStart() - Initialize wave TPS curve

LaneSpawner (MonoBehaviour)
├── Fields:
│   ├── int LaneIndex (0, 1, 2)
│   ├── Vector3 SpawnPosition
│   ├── float LaneThreatBudget
│   ├── float LaneWidth
│   └── List<GameObject> ActiveEnemies
├── Methods:
│   ├── ReceiveThreatBudget(amount) - Get budget from director
│   ├── TrySpawnEnemy() - Spawn if budget allows
│   ├── SpawnEnemy(enemyData) - Instantiate & configure
│   └── OnEnemyDestroyed(enemy) - Track lane state

EnemyDataSO (ScriptableObject)
├── Fields:
│   ├── string EnemyName
│   ├── GameObject Prefab
│   ├── int MaxHealth
│   ├── float MovementSpeed
│   ├── float AttackRange
│   ├── int DamagePerShot
│   ├── float FireRate
│   └── float CalculatedThreatValue (derived)
├── Methods:
│   └── CalculateThreat() - Compute threat from stats

AIActionMoveForward3D (AIAction - Custom TDE extension)
├── Fields:
│   ├── float MoveSpeed
│   ├── bool ConstrainToLane
│   └── float _laneXPosition
├── Methods:
│   ├── OnEnterState() - Lock lane position
│   ├── PerformAction() - Move forward, enforce lane
│   └── OnReachedBase() - Trigger base damage

AIActionShootForward3D (AIAction - Custom TDE extension)
├── Fields:
│   ├── float ShootRange
│   ├── LayerMask HeroLayer
│   └── CharacterHandleWeapon _weaponHandler
├── Methods:
│   ├── Initialization() - Cache weapon handler
│   ├── PerformAction() - Detect heroes, shoot
│   └── DetectHeroInLane() - Forward raycast

WaveConfigSO (ScriptableObject)
├── Fields:
│   ├── int WaveNumber
│   ├── float StartTPS
│   ├── float EndTPS
│   ├── float WaveDuration
│   ├── AnimationCurve TPSCurve
│   └── List<EnemyDataSO> AllowedEnemies
├── Methods:
│   └── GetTPSAtTime(time) - Evaluate TPS curve
```

---

## 📊 TPS Balancing Reference

### **TPS Values by Difficulty**

```
Easy Mode:
- Starting TPS: 30
- Peak TPS: 60
- Enemy Types: Mostly weak (100-200 threat)

Normal Mode:
- Starting TPS: 50
- Peak TPS: 100
- Enemy Types: Mixed (100-400 threat)

Hard Mode:
- Starting TPS: 80
- Peak TPS: 150
- Enemy Types: More tanks/fast (200-600 threat)

Nightmare Mode:
- Starting TPS: 120
- Peak TPS: 250
- Enemy Types: All types, weighted toward strong
```

### **Enemy Threat Value Examples**

```csharp
// Weak Enemies (100-200 threat)
Grunt:      100 HP, 3 spd, 5 dmg  = 100 threat
Scout:      120 HP, 4 spd, 3 dmg  = 156 threat (speed bonus)

// Normal Enemies (200-400 threat)
Soldier:    200 HP, 3 spd, 10 dmg = 260 threat
Gunner:     180 HP, 2 spd, 15 dmg = 252 threat (damage bonus)

// Strong Enemies (400-800 threat)
Tank:       500 HP, 2 spd, 8 dmg  = 600 threat
Rusher:     150 HP, 6 spd, 20 dmg = 420 threat (speed + damage)

// Boss Enemies (1000+ threat)
Mini-Boss:  1000 HP, 2 spd, 30 dmg = 1400 threat
Boss:       2500 HP, 1.5 spd, 50 dmg = 3500 threat
```

### **Spawn Rate Calculations**

```
Example: 50 TPS with mixed enemies

Budget accumulation:
t=0s:   0 threat
t=1s:   50 threat  → Spawn Grunt (100) - WAIT
t=2s:   100 threat → Spawn Grunt (100) - cost 100, remaining 0
t=3s:   50 threat  → WAIT
t=4s:   100 threat → Spawn Grunt (100)
t=5s:   50 threat  → WAIT
t=6s:   100 threat → Spawn Grunt (100)

Result: 3 enemies in 6 seconds = 0.5 enemies/second

Example: 50 TPS trying to spawn Tank (600 threat)
t=0s:    0 threat
t=5s:    250 threat → WAIT
t=10s:   500 threat → WAIT
t=12s:   600 threat → Spawn Tank (600) - cost 600, remaining 0
t=13s:   50 threat  → Accumulating for next...

Result: 1 Tank every 12 seconds
```

---

## 🎮 Gameplay Flow Example

### **Scenario: 3-Lane System, 50 TPS, Wave 1**

**Initial State:**
```
Time: 0s
TPS: 50 threat/second
Budget: 0
Lanes: All empty

Heroes deployed:
Lane 0: [Knight] (HP:200, DPS:25, Range:5)
Lane 1: [Archer] (HP:100, DPS:40, Range:10)
Lane 2: [Mage]   (HP:80,  DPS:60, Range:8)
```

**Timeline:**

**t=2s:**
```
Budget: 100 threat accumulated
Action: Spawn Grunt (100 threat) in Lane 1
Result: Budget = 0

Enemy State:
Lane 0: Empty
Lane 1: [Grunt] moving forward (Z=+20)
Lane 2: Empty
```

**t=4s:**
```
Budget: 100 threat accumulated
Action: Spawn Grunt (100 threat) in Lane 0
Result: Budget = 0

Enemy State:
Lane 0: [Grunt] moving forward (Z=+20)
Lane 1: [Grunt] advancing (Z=+14) - 6 units traveled
Lane 2: Empty
```

**t=6s:**
```
Budget: 100 threat accumulated
Lane 1 Grunt: Z=+8 (in range of Archer at Z=0)
Action: 
  - Grunt starts shooting at Archer
  - Archer shoots back
  - Spawn Soldier (260 threat) - NOT ENOUGH BUDGET, WAIT

Enemy State:
Lane 0: [Grunt] advancing (Z=+14)
Lane 1: [Grunt] ENGAGING (Z=+8, shooting)
Lane 2: Empty
Budget: 100 (waiting for 260)
```

**t=9s:**
```
Budget: 250 threat accumulated
Lane 1 Grunt: KILLED by Archer
Action: Spawn Soldier (260 threat) in Lane 2
Result: Budget = 0 (spent 250, slight overspend)

Enemy State:
Lane 0: [Grunt] advancing (Z=+8, engaging Knight)
Lane 1: Empty (grunt killed)
Lane 2: [Soldier] moving forward (Z=+20)
```

**t=12s:**
```
Budget: 150 threat
Lane 0 Grunt: KILLED by Knight
Action: Spawn Grunt in Lane 1 (100 threat)
Result: Budget = 50

Enemy State:
Lane 0: Empty
Lane 1: [Grunt] moving (Z=+20)
Lane 2: [Soldier] advancing (Z=+11, engaging Mage)
```

**Key Observations:**
- TPS creates **natural pacing** - no rigid "every 3 seconds" timing
- **Stronger enemies spawn less frequently** due to higher cost
- **Lanes fill dynamically** based on budget availability
- **Combat happens when enemies enter hero range**
- **Budget carries over** - partial budget accumulates for next spawn

---

## 🔧 Implementation Phases

### **Phase 1: Basic TPS (Week 1)**
**Goal:** Replace SimpleEnemySpawner with basic TPS system

**Tasks:**
1. Create `TPSDirector` singleton
2. Create `EnemyDataSO` with threat calculation
3. Implement global budget accumulation
4. Spawn enemies when budget allows (random lane)
5. Test with 2-3 enemy types

**Deliverables:**
- TPSDirector.cs (150 lines)
- EnemyDataSO.cs (100 lines)
- 3 enemy SO assets configured
- Replace SimpleEnemySpawner in scene

---

### **Phase 2: Lane-Based Spawning (Week 1-2)**
**Goal:** Spawn enemies in specific lanes with lane constraints

**Tasks:**
1. Create `LaneSpawner` component (one per lane)
2. Implement lane threat allocation in TPSDirector
3. Spawn enemies at lane-specific positions
4. Create custom `AIActionMoveForward3D`
5. Test lane containment (enemies stay in lane)

**Deliverables:**
- LaneSpawner.cs (200 lines)
- AIActionMoveForward3D.cs (120 lines)
- 3 LaneSpawner GameObjects in scene
- Enemy prefabs with new AI action

---

### **Phase 3: Forward Shooting (Week 2)**
**Goal:** Enemies shoot forward while moving

**Tasks:**
1. Create custom `AIActionShootForward3D`
2. Configure enemy weapons (ProjectileWeapon)
3. Implement forward raycast detection
4. Set up enemy projectile layer/collision
5. Test combat: enemies shoot heroes in lane

**Deliverables:**
- AIActionShootForward3D.cs (180 lines)
- EnemyWeapon prefabs configured
- EnemyProjectile prefab with DamageOnTouch
- Enemy AIBrain with "Engage" state

---

### **Phase 4: Wave System Integration (Week 3)**
**Goal:** Control TPS via wave configurations

**Tasks:**
1. Create `WaveConfigSO` scriptable object
2. Implement TPS curves (AnimationCurve)
3. Wave progression in TPSDirector
4. Stage manager integration
5. Test multi-wave scenarios

**Deliverables:**
- WaveConfigSO.cs (100 lines)
- 5+ wave config assets (easy → hard)
- Stage progression logic
- Wave UI display

---

### **Phase 5: Advanced TPS (Week 4)**
**Goal:** Dynamic TPS adjustment and telemetry

**Tasks:**
1. Hero DPS detection
2. Dynamic TPS scaling based on performance
3. Debug UI for TPS monitoring
4. Lane balancing algorithms
5. Enemy type weighting system

**Deliverables:**
- Dynamic TPS adjustment logic
- Debug overlay UI (TPS graph, lane status)
- Balancing tools for designers
- Documentation for TPS tuning

---

## 📝 Key Design Decisions Summary

### **✅ Confirmed Decisions:**

1. **TPS over Fixed Timers**
   - Why: More flexible, easier to balance, dynamic difficulty
   - Threat budget accumulation allows varied enemy timing

2. **3-Lane System (Scalable to 2-5)**
   - Why: Matches hero grid, clear strategic choices
   - Lanes align with hero columns for direct combat

3. **Strict Lane Containment**
   - Why: Predictable gameplay, clear hero-enemy matchups
   - Enemies never switch lanes (unlike heroes who deploy)

4. **Forward Movement + Forward Shooting**
   - Why: Simple, understandable, mobile-friendly
   - Enemies advance toward base, shoot anything in path

5. **Threat = HP-based Calculation**
   - Why: HP is primary enemy stat, easy to understand
   - Modifiers for range/damage/speed add nuance

6. **TDE AI Integration (AIBrain + Custom Actions)**
   - Why: Leverage existing TDE systems
   - Custom actions: AIActionMoveForward3D, AIActionShootForward3D

7. **Lane-Level Threat Allocation**
   - Why: Distribute TPS across lanes independently
   - Allows lane-specific pressure and variety

### **🔄 Flexible for Iteration:**

1. **TPS Values** - Start conservative (30-50), tune based on testing
2. **Threat Formula** - May need tweaking after combat testing
3. **Lane Count** - Start with 3, expand to 5 if needed
4. **Enemy Speed** - Balance between too fast (unfair) and too slow (boring)
5. **Shoot Range** - Start 10 units, adjust for engagement timing

---

## 🎯 Next Steps

### **Immediate Actions:**

1. **Review This Document**
   - Discuss lane count (3 vs 5 lanes?)
   - Confirm TPS starting values (50 TPS?)
   - Agree on threat calculation formula
   - Verify enemy movement speed (2-5 units/s?)

2. **Create ScriptableObjects**
   - EnemyDataSO structure
   - 3-5 enemy types with stats
   - WaveConfigSO (if doing waves immediately)

3. **Implement Phase 1: Basic TPS**
   - TPSDirector.cs
   - EnemyDataSO.cs
   - Budget accumulation + spawning
   - Test with SimpleEnemySpawner replacement

4. **Create Custom AI Actions**
   - AIActionMoveForward3D (lane-constrained movement)
   - AIActionShootForward3D (forward shooting)

5. **Configure Enemy Prefabs**
   - AIBrain with new actions
   - Weapon setup (ProjectileWeapon)
   - Layer configuration (EnemyProjectile)

### **Questions to Answer:**

1. **Lane Count:** 3 lanes fixed, or 3-5 scalable by stage?
2. **TPS Starting Value:** 30, 50, or 80 TPS for Wave 1?
3. **Enemy Variety:** Start with 3 types or 5+ types?
4. **Wave System:** Implement immediately or after basic TPS working?
5. **Base Health:** Single shared base or 3 independent lane bases?
6. **Enemy Shooting:** All enemies shoot, or only ranged types?
7. **Projectile Speed:** Match hero projectiles or faster/slower?

---

## 📚 Additional Resources

**Related Documentation:**
- `VERTICAL_LANE_QUEUE_SYSTEM.md` - Hero lane system (vertical queue logic)
- `GRID_DEFENSE_ARCHITECTURE.md` - Overall game architecture
- `TDE_INTEGRATION_GUIDE.md` - TopDown Engine integration patterns
- `ENEMY_AI_SETUP.md` - Current enemy AI implementation
- `GAME_LOOP.md` - Wave and stage progression design

**TDE References:**
- AIBrain: https://topdown-engine-docs.moremountains.com/AI/ai-brain.html
- AIAction: https://topdown-engine-docs.moremountains.com/AI/ai-actions.html
- ProjectileWeapon: https://topdown-engine-docs.moremountains.com/weapons.html

---

**Document Status:** Complete - Ready for Review
**Last Updated:** January 11, 2026
**Next:** Discuss key decisions and begin Phase 1 implementation
