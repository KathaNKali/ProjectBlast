# Enemy AI Setup Guide - TDE AIBrain Implementation

**Goal:** Configure Enemy_00 prefab to spawn at TOP, detect PlayerBase, and move DOWNWARD toward it.

---

## 🎯 Enemy Behavior Requirements

```
Enemy spawns at Z = +20 (TOP)
    ↓
Detects PlayerBase at Z = -8 (BOTTOM)
    ↓
Moves downward using AIActionMoveTowardsTarget3D
    ↓
Reaches base → Triggers damage logic (future)
```

---

## 📦 Required Components on Enemy Prefab

### **1. Root GameObject: "Enemy_Basic"**

#### **Core TDE Components:**
```
✅ Character
   - Character Type: AI
   - Character Dimension: Type3D
   
✅ TopDownController3D
   - Speed: 3-5 (configurable)
   
✅ CharacterMovement
   - TDE ability for movement
   
✅ Health
   - Current Health: 50-150 (randomized by spawner)
   - Max Health: Same as current
   
✅ Collider (Capsule or Box)
   - Is Trigger: NO
   - For collision detection
   
✅ Rigidbody
   - Use Gravity: NO (top-down game)
   - Is Kinematic: NO
   - Constraints: Freeze Rotation X, Y, Z
```

#### **Layer & Tag:**
```
Layer: Enemy (layer 13)
Tag: Enemy
```

---

### **2. Child GameObject: "AIBrain"**

Create a child GameObject named "AIBrain" and add these components:

#### **AIBrain Component:**
```
Owner: (auto-set to parent)
Brain Active: TRUE (active from spawn)
Current State: MoveToBase (initial state)
Target: (set by spawner at runtime)
```

#### **AI State Configuration:**

**State 1: "MoveToBase"** (Initial State)
- **Actions:**
  - AIActionMoveTowardsTarget3D
- **Transitions:**
  - (None for now - just keeps moving)
- **Update Frequency:** 0.1s

---

## 🔧 Component Configuration Details

### **AIActionMoveTowardsTarget3D Setup**

**Inspector Settings:**
```
Distance to Target: 1.0
  └─ How close enemy gets before stopping
  
Only Run Once: NO
  └─ Continuously move toward target
  
Should Initialize: YES
  
Movement Speed: 3-5
  └─ Or use Character's base speed
```

**How It Works:**
1. Reads `AIBrain.Target` (set by spawner to PlayerBase)
2. Calculates direction: `Vector3.MoveTowards(current, target, speed)`
3. Uses CharacterMovement ability to move enemy
4. Updates every 0.1 seconds

---

## 🎮 Unity Editor Setup (Step-by-Step)

### **Step 1: Open Enemy_00 Prefab**
1. Navigate to `Assets/ProjectBlast/Prefab/Enemy_00.prefab`
2. Double-click to enter Prefab editing mode

### **Step 2: Configure Root GameObject**
1. Select root GameObject
2. **Add Components:**
   - TopDown Engine → Character
     - Set Character Type: **AI**
     - Set Character Dimension: **Type3D**
   - TopDown Engine → Character → Abilities → **CharacterMovement**
   - MoreMountains → TopDown Engine → Health
   - Physics → **Capsule Collider**
   - Physics → **Rigidbody**
     - Uncheck "Use Gravity"
     - Constraints: Check "Freeze Rotation" X, Y, Z

3. **Set Layer & Tag:**
   - Layer: **Enemy** (layer 13)
   - Tag: **Enemy**

### **Step 3: Create AIBrain Child**
1. Right-click root GameObject → Create Empty
2. Rename to **"AIBrain"**
3. Add Component → More Mountains → Tools → AI → **AI Brain**

### **Step 4: Configure AI States**

**Add State 1 - MoveToBase:**
1. In AIBrain component, expand **States** array
2. Set Size = 1
3. State 0:
   - State Name: **"MoveToBase"**
   - **Actions** (size 1):
     - Add Component → TopDown Engine → AI → Actions → **AIActionMoveTowardsTarget3D**
     - Drag component into Actions[0] slot
   - **Transitions**: (size 0 - no transitions yet)

4. **Configure AIActionMoveTowardsTarget3D:**
   - Distance To Target: **1.0**
   - Only Run Once: **NO**
   - Should Initialize: **YES**

### **Step 5: Set Initial State**
1. In AIBrain component:
   - Current State: Select **"MoveToBase"** from dropdown

### **Step 6: Configure TopDownController3D**
1. Select root GameObject
2. Find TopDownController3D component
3. Set Speed: **3.0** (adjust for game feel)

### **Step 7: Save Prefab**
1. Click "Save" in Prefab editor
2. Exit Prefab mode

---

## 🎯 How Spawner Sets Target

The `SimpleEnemySpawner` will automatically configure the enemy's target:

```csharp
// In SimpleEnemySpawner.cs (already implemented)
GameObject enemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);

// Find PlayerBase by tag
GameObject playerBase = GameObject.FindGameObjectWithTag("PlayerBase");

// Set AI target
AIBrain brain = enemy.GetComponentInChildren<AIBrain>();
if (brain != null && playerBase != null)
{
    brain.Target = playerBase.transform;
    Debug.Log($"[Spawner] Set {enemy.name} target to {playerBase.name}");
}
```

---

## 🏰 PlayerBase Setup

### **Create PlayerBase GameObject**

1. **Create Empty GameObject:**
   - Name: **"PlayerBase"**
   - Position: **(0, 0, -8)** ← At BOTTOM of battlefield
   - Tag: **"PlayerBase"**

2. **Add Components:**
   ```
   Health Component:
     - Current Health: 1000
     - Max Health: 1000
     - Invulnerable: NO
   
   Box Collider:
     - Is Trigger: NO
     - Size: (2, 2, 2) - adjust to visual size
   
   Visual Representation:
     - Add Cube mesh renderer (optional)
     - Add particle effects (optional)
   ```

3. **Tag Configuration:**
   - Make sure "PlayerBase" tag exists in Tags & Layers
   - If not: Edit → Project Settings → Tags and Layers → Add "PlayerBase"

---

## 🧪 Testing the Setup

### **Test Scene Setup:**

1. **Place PlayerBase:**
   - Position: (0, 0, -8)
   - Tag: "PlayerBase"

2. **Configure Spawner:**
   - SimpleEnemySpawner GameObject
   - Enemy Prefab: Drag Enemy_00 prefab
   - Spawn Center: Position at (0, 0, +20)
   - Spawn Count: 3 (for testing)
   - Spawn Interval: 3 seconds

3. **Play Mode Test:**
   - Press Play
   - Wait 5 seconds (Initial Delay)
   - **Expected Results:**
     - Enemy spawns at Z = +20
     - Console: "Set Enemy_1 target to PlayerBase"
     - Enemy moves DOWNWARD toward Z = -8
     - Enemy stops when reaching Distance To Target (1.0m from base)

### **Debug Verification:**

Check Console for:
```
[Spawner] Spawned Enemy_1 at (0, 0, 20) with 100 HP
[Spawner] Set Enemy_1 target to PlayerBase at (0, 0, -8)
```

Watch Scene View:
- Enemy should move in negative Z direction
- Speed should match TopDownController3D.Speed
- Enemy should navigate around obstacles (if any)

---

## 🎨 Visual Debugging

### **Gizmos in Scene View:**

Enable these for debugging:
- AIBrain: Shows target line
- CharacterMovement: Shows movement direction
- Colliders: Shows collision boundaries

### **Inspector During Play Mode:**

Select spawned enemy and watch:
- **AIBrain → Current State:** Should show "MoveToBase"
- **AIBrain → Target:** Should show "PlayerBase (Transform)"
- **AIBrain → Time In This State:** Should increase
- **Character → Movement State:** Should show "Walking"

---

## ⚙️ Configuration Options

### **Enemy Speed Tuning:**
```
TopDownController3D → Speed:
  - Slow enemy: 2.0
  - Normal enemy: 3.0-4.0
  - Fast enemy: 5.0-7.0
```

### **Stop Distance Tuning:**
```
AIActionMoveTowardsTarget3D → Distance To Target:
  - 0.5: Enemy gets very close to base
  - 1.0: Standard melee range
  - 2.0: Stops farther away (ranged enemy)
```

### **Movement Smoothness:**
```
TopDownController3D → Smoothing:
  - Higher = smoother but less responsive
  - Lower = snappier but more robotic
  - Recommended: 0.1-0.2
```

---

## 🔄 Future Enhancements (Phase 4)

### **Add Attack State:**
When enemy reaches base, transition to "AttackBase" state:

**State 2: "AttackBase"**
- **Transition FROM MoveToBase:**
  - Decision: AIDecisionDistanceToTarget
  - Comparison: Less Than
  - Distance: 1.5
  - If True → Transition to "AttackBase"

- **Actions:**
  - AIActionDamageBase (custom script)
  - Plays attack animation
  - Damages PlayerBase Health component
  - Destroys enemy after delay

---

## 📋 Checklist

Before testing, verify:

- [ ] Enemy_00 prefab has Character (AI, Type3D)
- [ ] Enemy_00 prefab has TopDownController3D
- [ ] Enemy_00 prefab has CharacterMovement ability
- [ ] Enemy_00 prefab has Health component
- [ ] Enemy_00 prefab has Collider + Rigidbody
- [ ] Enemy_00 prefab has AIBrain child GameObject
- [ ] AIBrain has state "MoveToBase" configured
- [ ] AIBrain has AIActionMoveTowardsTarget3D in Actions
- [ ] PlayerBase exists in scene at (0, 0, -8)
- [ ] PlayerBase has tag "PlayerBase"
- [ ] PlayerBase has Health component
- [ ] SimpleEnemySpawner references Enemy_00 prefab
- [ ] Spawner has Spawn Center at Z = +20

---

## 🐛 Troubleshooting

### **Enemy doesn't move:**
- Check AIBrain.BrainActive = true
- Check AIBrain.Target is set (not null)
- Check Character has CharacterMovement ability
- Check TopDownController3D.Speed > 0

### **Enemy moves wrong direction:**
- Check PlayerBase position (should be Z = -8)
- Check spawn position (should be Z = +20)
- Enemy should move from higher Z to lower Z

### **Enemy spins in place:**
- Check Rigidbody constraints (Freeze Rotation XYZ)
- Check CharacterOrientation3D is NOT on enemy (heroes use it, enemies don't need it for basic movement)

### **"Target not set" console error:**
- Check PlayerBase tag is "PlayerBase"
- Check spawner auto-finds target (SimpleEnemySpawner.Start)
- Check PlayerBase exists in scene before spawning

### **Enemy passes through PlayerBase:**
- Check both have non-trigger colliders
- Check collision matrix (Enemy layer vs Default layer)
- Check Distance To Target in AIActionMoveTowardsTarget3D

---

## 📚 Next Steps

Once basic movement works:

1. **Add Attack Logic** - Damage base when reached
2. **Add Enemy Variety** - Fast, tank, flying types
3. **Add Wave Management** - Coordinated spawning
4. **Add Visual Feedback** - Enemy health bars, damage numbers
5. **Add Death Effects** - Explosions, rewards
6. **Add Stage Progression** - Multiple waves per level

---

**Related Files:**
- `SimpleEnemySpawner.cs` - Spawns enemies and sets target
- `Enemy_00.prefab` - Enemy prefab to configure
- `GameScene.unity` - Test scene with PlayerBase

**Documentation:**
- TDE AI System: TopDown Engine docs → AI Brain
- AIActionMoveTowardsTarget3D: TDE docs → AI Actions
