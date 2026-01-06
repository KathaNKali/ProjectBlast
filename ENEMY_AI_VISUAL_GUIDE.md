# Enemy AI Visual Reference - Component Hierarchy

Quick visual guide showing how Enemy_00 prefab should be structured.

---

## 🎯 Enemy GameObject Hierarchy

```
Enemy_00 (Root GameObject)
│
├─ Character                              [TDE Component]
│  ├─ Character Type: AI
│  └─ Character Dimension: Type3D
│
├─ TopDownController3D                    [TDE Component]
│  └─ Speed: 3.0
│
├─ CharacterMovement                      [TDE Ability]
│  └─ Enables movement control
│
├─ Health                                 [TDE Component]
│  ├─ Current Health: 100
│  └─ Max Health: 100
│
├─ Capsule Collider                       [Unity Physics]
│  └─ For collision detection
│
├─ Rigidbody                              [Unity Physics]
│  ├─ Use Gravity: NO ✗
│  └─ Freeze Rotation: YES ✓ (X, Y, Z)
│
└─ AIBrain (Child GameObject)
   │
   └─ AIBrain Component                   [TDE AI System]
      ├─ Owner: (auto-set to parent)
      ├─ Brain Active: YES ✓
      ├─ Current State: "MoveToBase"
      ├─ Target: (set by spawner at runtime)
      │
      └─ States Array:
         │
         └─ State[0]: "MoveToBase"
            ├─ State Name: "MoveToBase"
            │
            ├─ Actions[0]:
            │  └─ AIActionMoveTowardsTarget3D
            │     ├─ Distance To Target: 1.0
            │     ├─ Only Run Once: NO ✗
            │     └─ Should Initialize: YES ✓
            │
            └─ Transitions: (empty for now)
```

---

## 📋 Inspector View: Root GameObject

```
┌─────────────────────────────────────────┐
│ Enemy_00                                │
├─────────────────────────────────────────┤
│ Tag: Enemy                      ▼       │
│ Layer: Enemy                    ▼       │
├─────────────────────────────────────────┤
│ Transform                               │
│ Position: (set by spawner at runtime)  │
│ Rotation: (0, 0, 0)                     │
│ Scale: (1, 1, 1)                        │
├─────────────────────────────────────────┤
│ ► Character                             │
│   Character Type: AI             ●      │
│   Character Dimension: Type3D    ●      │
├─────────────────────────────────────────┤
│ ► TopDownController3D                   │
│   Speed: 3.0                            │
├─────────────────────────────────────────┤
│ ► CharacterMovement                     │
│   (TDE Ability - default settings)      │
├─────────────────────────────────────────┤
│ ► Health                                │
│   Current Health: 100                   │
│   Maximum Health: 100                   │
│   Initial Health: 100                   │
├─────────────────────────────────────────┤
│ ► Capsule Collider                      │
│   Radius: 0.5                           │
│   Height: 2.0                           │
├─────────────────────────────────────────┤
│ ► Rigidbody                             │
│   Mass: 1                               │
│   Use Gravity: ☐ UNCHECKED             │
│   Is Kinematic: ☐                       │
│   Constraints:                          │
│     Freeze Position: ☐ ☐ ☐             │
│     Freeze Rotation: ☑ ☑ ☑  (X Y Z)    │
└─────────────────────────────────────────┘
```

---

## 📋 Inspector View: AIBrain GameObject

```
┌─────────────────────────────────────────┐
│ AIBrain (Child of Enemy_00)            │
├─────────────────────────────────────────┤
│ Tag: Untagged                           │
│ Layer: Default                          │
├─────────────────────────────────────────┤
│ Transform                               │
│ Position: (0, 0, 0)  [Local]           │
│ Rotation: (0, 0, 0)                     │
│ Scale: (1, 1, 1)                        │
├─────────────────────────────────────────┤
│ ► AI Brain                              │
│ ┌─────────────────────────────────────┐ │
│ │ Owner: Enemy_00 (GameObject)        │ │
│ │ Brain Active: ☑ YES                 │ │
│ │ Current State: MoveToBase     ▼     │ │
│ │ Target: (set at runtime)            │ │
│ │ Last Known Target Position: (...)   │ │
│ ├─────────────────────────────────────┤ │
│ │ ► States                            │ │
│ │   Size: 1                           │ │
│ │   ┌───────────────────────────────┐ │ │
│ │   │ Element 0: MoveToBase         │ │ │
│ │   │                               │ │ │
│ │   │ State Name: "MoveToBase"      │ │ │
│ │   │                               │ │ │
│ │   │ ► Actions                     │ │ │
│ │   │   Size: 1                     │ │ │
│ │   │   Element 0: AIActionMove...  │ │ │
│ │   │                               │ │ │
│ │   │ ► Transitions                 │ │ │
│ │   │   Size: 0                     │ │ │
│ │   └───────────────────────────────┘ │ │
│ └─────────────────────────────────────┘ │
├─────────────────────────────────────────┤
│ ► AIActionMoveTowardsTarget3D           │
│ ┌─────────────────────────────────────┐ │
│ │ Should Initialize: ☑ YES            │ │
│ │ Only Run Once: ☐ NO                 │ │
│ │ Distance To Target: 1.0             │ │
│ │                                     │ │
│ │ (Component pulls from AIBrain.Target)│ │
│ │ (Uses CharacterMovement to move)    │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

---

## 🎮 Runtime View (During Play Mode)

Select spawned enemy in Hierarchy and watch these values update:

```
┌─────────────────────────────────────────┐
│ Enemy_1 (Clone)                         │
├─────────────────────────────────────────┤
│ Transform                               │
│ Position: (0.2, 0, 15.3)  ← Moves!     │
├─────────────────────────────────────────┤
│ ► Character                             │
│   Movement State: Walking        ✓      │
│   Condition State: Normal        ✓      │
├─────────────────────────────────────────┤
│ ► Health                                │
│   Current Health: 87/100         ✓      │
├─────────────────────────────────────────┤
│                                         │
│ AIBrain (Child)                         │
├─────────────────────────────────────────┤
│ ► AI Brain                              │
│   Owner: Enemy_1                 ✓      │
│   Brain Active: ☑ TRUE           ✓      │
│   Current State: MoveToBase      ✓      │
│   Time In This State: 3.2s       ✓      │
│   Target: PlayerBase (Transform) ✓      │
│   Last Known Target Pos: (0,0,-8)✓      │
└─────────────────────────────────────────┘
```

**What to look for:**
- ✅ Transform Position Z value **decreasing** (moving toward -8)
- ✅ Movement State showing **"Walking"**
- ✅ Target showing **"PlayerBase (Transform)"** (not null!)
- ✅ Time In This State **increasing**

---

## 🗺️ Scene Layout

```
                    WORLD SPACE
                    
    Z = +20  ┌────────────────────────┐
             │  ✦ SPAWN ZONE (TOP)   │
             │                        │
             │    🟢 Enemy spawns     │
             │         ↓              │
    Z = +15  │         ↓              │
             │         ↓              │
    Z = +10  │    🟢 Moving down      │
             │         ↓              │
    Z = +5   │         ↓              │
             │         ↓              │
    Z = 0    │  ⚔️ FIRING GRID        │
             │  [Heroes here]         │
    Z = -3   │  📦 ACTIVE GRID        │
             │                        │
    Z = -6   │  📦 PASSIVE GRID       │
             │                        │
    Z = -8   │  🏰 PLAYERBASE         │
             │     (Enemy target)     │
             └────────────────────────┘
```

**Key Positions:**
- **Spawn Center:** (0, 0, +20) ← TOP
- **PlayerBase:** (0, 0, -8) ← BOTTOM
- **Movement Direction:** From +Z to -Z (downward)

---

## 🔧 Configuration Cheat Sheet

### **Speed Presets:**
```
AIBrain → TopDownController3D → Speed:

Slow Enemy (Tank):    2.0
Normal Enemy:         3.0 - 4.0
Fast Enemy (Scout):   6.0 - 8.0
Boss:                 1.5
```

### **Stop Distance:**
```
AIActionMoveTowardsTarget3D → Distance To Target:

Melee Enemy:          0.5 - 1.0
Ranged Enemy:         2.0 - 3.0
Avoid exact contact:  1.0 (recommended)
```

### **Health Randomization:**
```
SimpleEnemySpawner:

Min Health: 50
Max Health: 150
  └─ Spawner randomizes between these values
```

---

## 🎨 Visual Debug Indicators

### **Enable in Scene View:**
1. **Gizmos button** (top-right of Scene View)
2. Check these:
   - ✓ Colliders (green outlines)
   - ✓ Physics (shows rigidbody)
   - ✓ AI (shows target line from enemy to base)

### **AI Debug Line:**
When enemy is selected in Hierarchy:
```
Enemy position ──────────────> PlayerBase
    (yellow line shows AI target direction)
```

---

## 📊 Verification Checklist

Before testing, ensure ALL these are correct:

**Enemy Prefab:**
- [ ] Root GameObject has Character (AI, Type3D)
- [ ] Root GameObject has TopDownController3D
- [ ] Root GameObject has CharacterMovement
- [ ] Root GameObject has Health component
- [ ] Root GameObject has Capsule Collider
- [ ] Root GameObject has Rigidbody (gravity OFF, rotation frozen)
- [ ] Layer set to "Enemy", Tag set to "Enemy"
- [ ] Child GameObject named "AIBrain" exists
- [ ] AIBrain has AI Brain component
- [ ] AIBrain has AIActionMoveTowardsTarget3D
- [ ] State "MoveToBase" configured with Action
- [ ] Current State set to "MoveToBase"

**Scene Setup:**
- [ ] PlayerBase GameObject exists at (0, 0, -8)
- [ ] PlayerBase has Tag "PlayerBase"
- [ ] PlayerBase has Health component
- [ ] SimpleEnemySpawner has Enemy_00 prefab assigned
- [ ] Spawner Spawn Center is at Z = +20 (TOP)
- [ ] Spawner Target Tag is "PlayerBase"

**Spawner Configuration:**
- [ ] Enemy Prefab field has Enemy_00
- [ ] Spawn Count > 0
- [ ] Spawn On Start is checked
- [ ] Target Tag is "PlayerBase"
- [ ] Debug Mode is checked (for console logs)

---

## 🎬 Expected Console Output

```
Frame 1:
[SimpleEnemySpawner] Auto-found target: PlayerBase at (0, 0, -8)

Frame 120 (after 2s initial delay):
[SimpleEnemySpawner] Spawned Enemy_1 at (0.12, 0, 20) with 87 HP
[SimpleEnemySpawner] Set Enemy_1 AIBrain target to: PlayerBase at (0, 0, -8)

Frame 300 (after 3s spawn interval):
[SimpleEnemySpawner] Spawned Enemy_2 at (-0.5, 0, 19.8) with 132 HP
[SimpleEnemySpawner] Set Enemy_2 AIBrain target to: PlayerBase at (0, 0, -8)

Frame 480:
[SimpleEnemySpawner] Spawned Enemy_3 at (0.8, 0, 20.2) with 65 HP
[SimpleEnemySpawner] Set Enemy_3 AIBrain target to: PlayerBase at (0, 0, -8)
[SimpleEnemySpawner] Finished spawning 3 enemies
```

**No errors!** If you see errors, check Troubleshooting section.

---

## 🔍 Common Mistakes

❌ **Forgot to add CharacterMovement ability**
   → Enemy won't move, AIAction can't control movement

❌ **Brain Active is false**
   → AI won't execute, enemy stays still

❌ **Target not set (null)**
   → AIAction has no destination, enemy won't move

❌ **Gravity enabled on Rigidbody**
   → Enemy falls through floor (this is top-down, not 3D platformer!)

❌ **Rotation not frozen**
   → Enemy spins/tilts weirdly while moving

❌ **PlayerBase tag misspelled**
   → Spawner can't find target, prints warning

❌ **Spawn Center at wrong Z**
   → Enemies spawn near base instead of at top

❌ **Distance To Target = 0**
   → Enemy tries to reach exact position, may jitter

---

**Next:** Once enemies move correctly, add attack logic with AIActionDamageBase!
