# Grid Defense Game - Custom Architecture Guide

This guide explains how to build your **queue-based tactical tower defense** game using selective TopDown Engine components plus custom systems.

---

## 🏗️ System Architecture Overview

### **What to USE from TopDown Engine:**
✅ **Weapon System** - ProjectileWeapon for hero auto-firing  
✅ **Health System** - For heroes, enemies, and base  
✅ **AI System** - For enemy pathing and behavior  
✅ **Damage System** - DamageOnTouch for projectiles  
✅ **MMFeedbacks** - For polish (hit effects, deploy effects, merge effects)  
✅ **Event System** - MMEventManager for decoupled communication  
✅ **GameManager** - Score, game state, pause  

### **What to BUILD Custom (but using TDE patterns):**
🔨 **Grid System** - 3-zone grid management (extend from `CharacterGridMovement` concepts)  
🔨 **Hero Queue System** - Use `LevelManager` spawning + custom queue logic  
🔨 **Deployment System** - Create `CharacterDeployment` ability (extends `CharacterAbility`)  
🔨 **Merge System** - Detect 3 matching heroes and merge (custom logic)  
🔨 **Wave Manager** - Use `LevelManager` spawning + custom wave timing  
🔨 **Lane System** - Use `AIBrain` + `AIActionMoveTowardsTarget3D` for pathing  
🔨 **Auto-Combat Controller** - Create `CharacterAutoCombat` ability (extends `CharacterAbility`)

**Key Insight:** Most of these CAN use TopDown Engine's architecture patterns (abilities, managers, events) even though they aren't pre-built features. You're extending TDE, not replacing it.  

---

## 📐 Core System 1: Grid Manager

### **Purpose:** 
Manages the 3-zone grid system where heroes are positioned using a **vertical lane-based queue system**.

### **Queue System Architecture:**
**Vertical Lane System** - Each column is an independent queue:
- Heroes only move within their lane (column)
- Active Grid = 1 row × N columns (e.g., 1×3)
- Passive Grid = 2+ rows × N columns (e.g., 2×3)
- When Active hero deploys → Passive hero in same column moves up
- All heroes in that lane shift up one row
- Empty slots appear at bottom of lanes (spawn points)

**Example Flow:**
```
Column 0   Column 1   Column 2
[1]       [2]       [3]        ← Active (1 row)
[4]       [5]       [6]        ← Passive Row 0
[7]       [8]       [9]        ← Passive Row 1

Player taps [2] → Deploys to Firing
↓
[1]       [5]       [3]        ← [5] moves to Active
[4]       [8]       [6]        ← [8] moves up
[7]       [ ]       [9]        ← Empty slot at bottom
```

### **Key Responsibilities:**
- Track all grid positions (Passive, Active, Firing)
- Store which hero occupies each slot
- Validate placement rules (strict lane enforcement)
- Handle vertical lane shifting when heroes deploy
- Notify systems when heroes move between zones

### **TopDown Engine Connection:**
This extends concepts from `CharacterGridMovement` but adapts them for:
- **Multi-zone system** (TDE assumes single grid)
- **Vertical lane queues** (TDE has no queue concept)
- **Static placement** (TDE assumes grid movement)
- **Slot ownership tracking** (TDE doesn't track this)
- **Strict lane enforcement** (heroes can't cross lanes)

You can reference TDE's grid positioning math in `CharacterGridMovement.cs`:
- Cell size calculations
- World position to grid coordinate conversion
- Grid boundary validation

### **Lane Rules:**
1. **Strict Lanes:** Heroes can only move within their column (no cross-lane movement)
2. **Active Refill:** When Active slot empties, Passive hero from same column moves up
3. **Passive Compacting:** All heroes in lane shift up when hero above them leaves
4. **Empty Lanes:** If entire lane is empty, it stays empty (no cross-lane filling)
5. **Firing Deployment:** Firing grid has NO lanes - heroes fill left-to-right (first available)

### **Implementation Pattern:**

```csharp
using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace YourGame.Grid
{
    public enum GridZone
    {
        Passive,    // Queue waiting area
        Active,     // Ready deployment area
        Firing      // Combat zone
    }

    public class GridPosition
    {
        public GridZone Zone;
        public int Row;
        public int Column;
        public Vector3 WorldPosition;
        public Hero OccupyingHero;
        public bool IsOccupied => OccupyingHero != null;

        public GridPosition(GridZone zone, int row, int col, Vector3 worldPos)
        {
            Zone = zone;
            Row = row;
            Column = col;
            WorldPosition = worldPos;
            OccupyingHero = null;
        }
    }

    public class GridManager : MMSingleton<GridManager>
    {
        [Header("Grid Configuration")]
        public int PassiveRows = 2;
        public int PassiveCols = 3;
        public int ActiveRows = 2;
        public int ActiveCols = 3;
        public int FiringRows = 3;
        public int FiringCols = 3;

        [Header("Grid Spacing")]
        public float CellSize = 1.5f;
        public Vector3 PassiveGridOrigin;
        public Vector3 ActiveGridOrigin;
        public Vector3 FiringGridOrigin;

        private Dictionary<GridZone, List<GridPosition>> _allGridPositions;

        protected override void Awake()
        {
            base.Awake();
            InitializeGrids();
        }

        void InitializeGrids()
        {
            _allGridPositions = new Dictionary<GridZone, List<GridPosition>>();
            
            // Create Passive Grid
            _allGridPositions[GridZone.Passive] = CreateGrid(
                GridZone.Passive, PassiveRows, PassiveCols, PassiveGridOrigin);
            
            // Create Active Grid
            _allGridPositions[GridZone.Active] = CreateGrid(
                GridZone.Active, ActiveRows, ActiveCols, ActiveGridOrigin);
            
            // Create Firing Grid
            _allGridPositions[GridZone.Firing] = CreateGrid(
                GridZone.Firing, FiringRows, FiringCols, FiringGridOrigin);
        }

        List<GridPosition> CreateGrid(GridZone zone, int rows, int cols, Vector3 origin)
        {
            var positions = new List<GridPosition>();
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 worldPos = origin + new Vector3(col * CellSize, 0, row * CellSize);
                    positions.Add(new GridPosition(zone, row, col, worldPos));
                }
            }
            
            return positions;
        }

        public GridPosition GetNearestEmptyPosition(GridZone zone, Vector3 clickWorldPos)
        {
            var zonePositions = _allGridPositions[zone];
            GridPosition nearest = null;
            float minDist = float.MaxValue;

            foreach (var pos in zonePositions)
            {
                if (!pos.IsOccupied)
                {
                    float dist = Vector3.Distance(pos.WorldPosition, clickWorldPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = pos;
                    }
                }
            }

            return nearest;
        }

        public bool PlaceHeroAtPosition(Hero hero, GridPosition position)
        {
            if (position.IsOccupied)
                return false;

            // Remove hero from previous position
            if (hero.CurrentGridPosition != null)
            {
                hero.CurrentGridPosition.OccupyingHero = null;
            }

            // Place at new position
            position.OccupyingHero = hero;
            hero.CurrentGridPosition = position;
            hero.transform.position = position.WorldPosition;

            return true;
        }

        public List<GridPosition> GetAllPositionsInZone(GridZone zone)
        {
            return _allGridPositions[zone];
        }

        public List<GridPosition> GetOccupiedPositionsInZone(GridZone zone)
        {
            return _allGridPositions[zone].FindAll(p => p.IsOccupied);
        }

        void OnDrawGizmos()
        {
            if (_allGridPositions == null) return;

            foreach (var kvp in _allGridPositions)
            {
                Color zoneColor = kvp.Key switch
                {
                    GridZone.Passive => Color.yellow,
                    GridZone.Active => Color.cyan,
                    GridZone.Firing => Color.red,
                    _ => Color.white
                };

                Gizmos.color = zoneColor;
                foreach (var pos in kvp.Value)
                {
                    Gizmos.DrawWireCube(pos.WorldPosition, Vector3.one * CellSize * 0.9f);
                    if (pos.IsOccupied)
                    {
                        Gizmos.DrawSphere(pos.WorldPosition, 0.2f);
                    }
                }
            }
        }
    }
}
```

---

## 👤 Core System 2: Hero System (AIBrain Integration)

### **Purpose:** 
Represents individual heroes with **automatic combat using TDE's AIBrain system**. Heroes engage enemies when in Firing zone using AI-driven targeting, aiming, and shooting.

### **Current Implementation (Nov 28, 2025):**
ProjectBlast heroes use **TDE's AIBrain** instead of manual combat code. This provides:
- ✅ Inspector-configurable AI behavior
- ✅ Automatic target detection with line-of-sight
- ✅ Zone-based combat activation
- ✅ Data-driven configuration via HeroDataSO
- ✅ Cleaner architecture (~100 fewer lines)

### **Key Components:**

**TDE Components:**
- `Character` (CharacterTypes.Player, Type3D)
- `Health` - Hit points and damage
- `CharacterHandleWeapon` - Weapon management
- `CharacterOrientation3D` - Body rotation
- `TopDownController3D` - 3D controller
- **`AIBrain`** - AI state machine
- **`AIActionShoot3D`** - Auto-firing
- **`AIActionAimWeaponAtTarget3D`** - Weapon aiming
- **`AIDecisionDetectTargetRadius3D`** - Enemy detection
- **`AIDecisionLineOfSightToTarget3D`** - LOS checking

**ProjectBlast Components:**
- `Hero.cs` - Orchestration layer (686 lines)
- `HeroDataSO` - Configuration data (ScriptableObject)

### **Combat Flow:**

```
Hero enters Firing zone
  ↓
Hero.StartFiring() called
  ↓
AIBrain.BrainActive = true
  ↓
AIBrain transitions to "Combat" state
  ↓
AIDecisionDetectTargetRadius3D scans for enemies
  ↓
AIDecisionLineOfSightToTarget3D verifies clear shot
  ↓
AIActionAimWeaponAtTarget3D rotates weapon
  ↓
CharacterOrientation3D rotates body
  ↓
AIActionShoot3D fires weapon
  ↓
Ammo consumed → Monitor for depletion
  ↓
Loop continues until zone exit or ammo depleted
```

### **Implementation Pattern (Current):**

```csharp
using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using ProjectBlast.Grid;
using ProjectBlast.Data;

namespace ProjectBlast.Heroes
{
    public enum HeroClass
    {
        Warrior,    // Strong against Rogue
        Mage,       // Strong against Warrior
        Rogue,      // Strong against Mage
        Tank,       // High health
        Support     // Heals/buffs
    }

    /// <summary>
    /// Hero represents a stationary combat unit that auto-fires at enemies.
    /// Uses TopDown Engine's weapon system but remains stationary.
    /// </summary>
    public class Hero : MonoBehaviour
    {
        [Header("Hero Identity")]
        public HeroClass HeroClass;
        public int HeroLevel = 1;
        public string HeroName;

        [Header("Stats")]
        public float MaxHealth = 100f;
        public float CurrentHealth;
        public float AttackDamage = 10f;
        public float AttackRange = 5f;
        public float FireRate = 1f; // Shots per second

        [Header("Components")]
        public Weapon AssignedWeapon;
        public Health HealthComponent;

        [Header("Grid System")]
        public GridPosition CurrentGridPosition;

        [Header("Auto-Combat")]
        public bool IsInFiringZone => CurrentGridPosition?.Zone == GridZone.Firing;
        public Transform CurrentTarget;
        public LayerMask EnemyLayer;

        private float _nextFireTime;

        void Start()
        {
            CurrentHealth = MaxHealth;
            
            // Set up Health component if present
            if (HealthComponent == null)
            {
                HealthComponent = GetComponent<Health>();
            }
            
            if (HealthComponent != null)
            {
                HealthComponent.OnDeath += OnHeroDeath;
            }

            // Configure weapon if present
            if (AssignedWeapon != null)
            {
                AssignedWeapon.Owner = this.gameObject;
            }
        }

        void Update()
        {
            if (!IsInFiringZone) return;

            AutoCombatUpdate();
        }

        void AutoCombatUpdate()
        {
            // Acquire target if needed
            if (CurrentTarget == null || !IsTargetValid(CurrentTarget))
            {
                CurrentTarget = FindNearestEnemy();
            }

            // Fire at target if available
            if (CurrentTarget != null && Time.time >= _nextFireTime)
            {
                FireAtTarget();
                _nextFireTime = Time.time + (1f / FireRate);
            }
        }

        Transform FindNearestEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayer);
            
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

        bool IsTargetValid(Transform target)
        {
            if (target == null) return false;
            
            float dist = Vector3.Distance(transform.position, target.position);
            return dist <= AttackRange;
        }

        void FireAtTarget()
        {
            if (AssignedWeapon == null) return;

            // Aim weapon at target
            Vector3 aimDirection = (CurrentTarget.position - transform.position).normalized;
            transform.forward = aimDirection;

            // Trigger weapon fire
            AssignedWeapon.WeaponInputStart();
            AssignedWeapon.WeaponInputStop();
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
            
            if (HealthComponent != null)
            {
                HealthComponent.Damage(amount, gameObject, 0.1f, 0.1f);
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        void OnHeroDeath()
        {
            Die();
        }

        void Die()
        {
            // Remove from grid
            if (CurrentGridPosition != null)
            {
                CurrentGridPosition.OccupyingHero = null;
            }

            // Trigger death effects
            // TODO: Play death feedback

            Destroy(gameObject);
        }

        public float GetDamageMultiplierAgainst(HeroClass enemyClass)
        {
            // Rock-paper-scissors system
            return (HeroClass, enemyClass) switch
            {
                (HeroClass.Warrior, HeroClass.Rogue) => 1.5f,
                (HeroClass.Mage, HeroClass.Warrior) => 1.5f,
                (HeroClass.Rogue, HeroClass.Mage) => 1.5f,
                _ => 1.0f
            };
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
```

---

## 🔄 Core System 3: Hero Queue Manager

### **Purpose:**
Manages the dynamic queue of heroes moving through Passive → Active zones.

```csharp
using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using YourGame.Grid;

namespace YourGame.Heroes
{
    /// <summary>
    /// Manages the hero queue system where heroes shift through Passive/Active grids
    /// </summary>
    public class HeroQueueManager : MMSingleton<HeroQueueManager>
    {
        [Header("Queue Settings")]
        public float AutoShiftInterval = 3f; // Heroes shift every X seconds
        public bool EnableAutoShift = true;

        [Header("Hero Spawning")]
        public List<GameObject> HeroPrefabs;
        public int InitialQueueSize = 6;

        private List<Hero> _queuedHeroes = new List<Hero>();
        private float _nextShiftTime;

        void Start()
        {
            InitializeQueue();
        }

        void Update()
        {
            if (EnableAutoShift && Time.time >= _nextShiftTime)
            {
                ShiftQueue();
                _nextShiftTime = Time.time + AutoShiftInterval;
            }
        }

        void InitializeQueue()
        {
            // Spawn initial heroes in Passive zone
            var passivePositions = GridManager.Instance.GetAllPositionsInZone(GridZone.Passive);
            
            for (int i = 0; i < Mathf.Min(InitialQueueSize, passivePositions.Count); i++)
            {
                SpawnRandomHero(passivePositions[i]);
            }

            _nextShiftTime = Time.time + AutoShiftInterval;
        }

        void SpawnRandomHero(GridPosition position)
        {
            if (HeroPrefabs.Count == 0) return;

            GameObject prefab = HeroPrefabs[Random.Range(0, HeroPrefabs.Count)];
            GameObject heroObj = Instantiate(prefab, position.WorldPosition, Quaternion.identity);
            
            Hero hero = heroObj.GetComponent<Hero>();
            if (hero != null)
            {
                GridManager.Instance.PlaceHeroAtPosition(hero, position);
                _queuedHeroes.Add(hero);
            }
        }

        void ShiftQueue()
        {
            // Move Active heroes to Firing zone (if space available)
            var activeHeroes = GridManager.Instance.GetOccupiedPositionsInZone(GridZone.Active);
            foreach (var pos in activeHeroes)
            {
                TryMoveToNextZone(pos.OccupyingHero);
            }

            // Move Passive heroes to Active zone (if space available)
            var passiveHeroes = GridManager.Instance.GetOccupiedPositionsInZone(GridZone.Passive);
            foreach (var pos in passiveHeroes)
            {
                TryMoveToNextZone(pos.OccupyingHero);
            }

            // Spawn new heroes in Passive zone
            var emptyPassiveSlots = GridManager.Instance.GetAllPositionsInZone(GridZone.Passive)
                .FindAll(p => !p.IsOccupied);
            
            if (emptyPassiveSlots.Count > 0)
            {
                SpawnRandomHero(emptyPassiveSlots[0]);
            }
        }

        bool TryMoveToNextZone(Hero hero)
        {
            if (hero == null || hero.CurrentGridPosition == null) return false;

            GridZone nextZone = hero.CurrentGridPosition.Zone switch
            {
                GridZone.Passive => GridZone.Active,
                GridZone.Active => GridZone.Firing,
                _ => hero.CurrentGridPosition.Zone // Firing zone heroes don't auto-shift
            };

            if (nextZone == hero.CurrentGridPosition.Zone) return false;

            // Find empty position in next zone
            var emptyPositions = GridManager.Instance.GetAllPositionsInZone(nextZone)
                .FindAll(p => !p.IsOccupied);

            if (emptyPositions.Count > 0)
            {
                GridManager.Instance.PlaceHeroAtPosition(hero, emptyPositions[0]);
                return true;
            }

            return false;
        }

        public void ManualDeployHero(Hero hero, GridPosition targetPosition)
        {
            if (targetPosition.Zone != GridZone.Firing)
            {
                Debug.LogWarning("Can only manually deploy to Firing zone");
                return;
            }

            GridManager.Instance.PlaceHeroAtPosition(hero, targetPosition);
        }
    }
}
```

---

## 🎮 Core System 4: Input & Deployment

### **Purpose:**
Handle player clicks to deploy heroes from Active → Firing grid.

```csharp
using UnityEngine;
using MoreMountains.Tools;
using YourGame.Grid;
using YourGame.Heroes;

namespace YourGame.Input
{
    public class DeploymentInputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        public Camera MainCamera;
        public LayerMask GridLayerMask;
        public LayerMask HeroLayerMask;

        private Hero _selectedHero;

        void Start()
        {
            if (MainCamera == null)
                MainCamera = Camera.main;
        }

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            // Mouse/Touch input
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Ray ray = MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                
                // Try to select a hero
                if (Physics.Raycast(ray, out RaycastHit heroHit, 100f, HeroLayerMask))
                {
                    Hero clickedHero = heroHit.collider.GetComponent<Hero>();
                    if (clickedHero != null && clickedHero.CurrentGridPosition.Zone == GridZone.Active)
                    {
                        _selectedHero = clickedHero;
                        // TODO: Show visual feedback for selection
                        return;
                    }
                }

                // Try to deploy selected hero to firing grid
                if (_selectedHero != null && Physics.Raycast(ray, out RaycastHit gridHit, 100f, GridLayerMask))
                {
                    GridPosition targetPos = GridManager.Instance.GetNearestEmptyPosition(
                        GridZone.Firing, gridHit.point);

                    if (targetPos != null)
                    {
                        HeroQueueManager.Instance.ManualDeployHero(_selectedHero, targetPos);
                        _selectedHero = null;
                        // TODO: Play deployment feedback
                    }
                }
            }
        }
    }
}
```

---

## 🌊 Core System 5: Wave Manager (Using TopDown Engine AI)

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace YourGame.Waves
{
    [System.Serializable]
    public class WaveData
    {
        public int WaveNumber;
        public List<GameObject> EnemyPrefabs;
        public int[] EnemyCounts;
        public float SpawnInterval = 2f;
        public float DelayBeforeNextWave = 5f;
    }

    public class WaveManager : MMSingleton<WaveManager>
    {
        [Header("Wave Configuration")]
        public List<WaveData> Waves;
        public int CurrentWave = 0;

        [Header("Spawn Points")]
        public Transform[] SpawnPoints;

        private bool _waveInProgress = false;

        public void StartNextWave()
        {
            if (_waveInProgress) return;
            
            if (CurrentWave < Waves.Count)
            {
                StartCoroutine(SpawnWave(Waves[CurrentWave]));
            }
        }

        IEnumerator SpawnWave(WaveData wave)
        {
            _waveInProgress = true;

            for (int i = 0; i < wave.EnemyPrefabs.Count; i++)
            {
                GameObject enemyPrefab = wave.EnemyPrefabs[i];
                int count = wave.EnemyCounts[i];

                for (int j = 0; j < count; j++)
                {
                    SpawnEnemy(enemyPrefab);
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }

            // Wait for all enemies to be defeated
            yield return new WaitUntil(() => AreAllEnemiesDefeated());

            _waveInProgress = false;
            CurrentWave++;

            yield return new WaitForSeconds(wave.DelayBeforeNextWave);
            StartNextWave();
        }

        void SpawnEnemy(GameObject prefab)
        {
            if (SpawnPoints.Length == 0) return;

            Transform spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
            GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            // Configure enemy AI to approach player base
            // Use TopDown Engine's AIBrain + AIActionMoveTowardsTarget3D
        }

        bool AreAllEnemiesDefeated()
        {
            // Check if any enemies remain in scene
            return GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
        }
    }
}
```

---

## 🎯 Implementation Priority

### **Phase 1: Grid Foundation (Week 1-2)**
1. Build GridManager with 3 zones
2. Create visual grid in scene
3. Test grid position calculations

### **Phase 2: Hero Basics (Week 2-3)**
4. Create Hero class with basic stats
5. Integrate TopDown Engine Weapon system
6. Test hero spawning on grid

### **Phase 3: Queue System (Week 3-4)**
7. Implement HeroQueueManager
8. Add auto-shifting logic
9. Test queue flow

### **Phase 4: Player Input (Week 4)**
10. Create DeploymentInputHandler
11. Add hero selection
12. Test deployment to Firing grid

### **Phase 5: Combat (Week 5-6)**
13. Implement auto-combat targeting
14. Add weapon firing
15. Integrate enemy spawning
16. Test wave system

### **Phase 6: Advanced Features (Week 7-10)**
17. Merge system (detect 3 matching heroes)
18. Class counter system
19. Resource management
20. Polish with MMFeedbacks

---

## 📝 Next Immediate Steps

1. **Review this architecture** - Does this match your vision?
2. **Decide on grid size** - How many rows/columns per zone?
3. **Choose visual style** - 3D models or sprites?
4. **Start with GridManager** - This is your foundation
5. **Create first hero prefab** - Single test hero with weapon

**Would you like me to:**
- Generate the complete GridManager script as a Unity C# file?
- Create a scene setup checklist?
- Explain any system in more detail?
- Start implementing Phase 1 (Grid Foundation)?
