# Quick Start: Enemy Movement Setup (15 Minutes)

Get enemies moving from TOP to BOTTOM using TDE's AIBrain system.

---

## ✅ Prerequisites Checklist

- [ ] Unity project open with GameScene
- [ ] Enemy_00.prefab exists in `Assets/ProjectBlast/Prefab/`
- [ ] TopDown Engine imported and working
- [ ] SimpleEnemySpawner.cs updated (auto-configures AI target)

---

## 🚀 Setup Steps

### **Step 1: Create PlayerBase (2 minutes)**

1. In Hierarchy: Right-click → Create Empty
2. Name it **"PlayerBase"**
3. Set Position: **(0, 0, -8)** ← BOTTOM of battlefield
4. Set Tag: **"PlayerBase"**
   - If tag doesn't exist: Edit → Project Settings → Tags and Layers → Add "PlayerBase"
5. Add Component → TopDown Engine → **Health**
   - Current Health: **1000**
   - Max Health: **1000**
6. Add Component → Physics → **Box Collider**
   - Size: **(2, 2, 2)**

✅ **Test:** PlayerBase should be at Z = -8 with tag "PlayerBase"

---

### **Step 2: Configure Enemy_00 Prefab (8 minutes)**

1. **Open Prefab:**
   - Navigate to `Assets/ProjectBlast/Prefab/Enemy_00.prefab`
   - Double-click to enter Prefab mode

2. **Add Core Components (on Root GameObject):**
   ```
   Add: TopDown Engine → Character
      ✓ Character Type: AI
      ✓ Character Dimension: Type3D
   
   Add: TopDown Engine → Character → Abilities → CharacterMovement
   
   Add: MoreMountains → TopDown Engine → Health
      ✓ Current Health: 100
      ✓ Max Health: 100
   
   Add: Physics → Capsule Collider
   
   Add: Physics → Rigidbody
      ✓ UNCHECK "Use Gravity"
      ✓ Constraints: CHECK "Freeze Rotation" X, Y, Z
   ```

3. **Set Layer & Tag:**
   - Layer: **Enemy** (create if doesn't exist)
   - Tag: **Enemy**

4. **Add TopDownController3D:**
   - Add Component → TopDown Engine → Character → Core → **TopDownController3D**
   - Speed: **3.0** (adjust to taste)

5. **Create AIBrain Child:**
   - Right-click root GameObject → Create Empty
   - Name: **"AIBrain"**
   - Add Component → More Mountains → Tools → AI → **AI Brain**

6. **Configure AI State:**
   - Select AIBrain GameObject
   - In AIBrain component:
     - **States** → Size: **1**
     - State[0]:
       - State Name: **"MoveToBase"**
       - **Actions** → Size: **1**
         - Click "+" → Add Component → TopDown Engine → AI → Actions → **AIActionMoveTowardsTarget3D**
         - Drag AIActionMoveTowardsTarget3D into Actions[0] slot
       - **Transitions** → Size: **0** (no transitions yet)

7. **Configure AIActionMoveTowardsTarget3D:**
   - Select AIBrain GameObject
   - Find AIActionMoveTowardsTarget3D component:
     - Distance To Target: **1.0**
     - Only Run Once: **NO** (unchecked)
     - Should Initialize: **YES** (checked)

8. **Set Initial State:**
   - In AIBrain component:
     - Current State: Select **"MoveToBase"** from dropdown

9. **Save Prefab:** Click "Save" button, then exit Prefab mode

✅ **Test:** Enemy_00 prefab should have Character (AI), CharacterMovement, AIBrain child with MoveToBase state

---

### **Step 3: Configure Spawner (2 minutes)**

1. **Find SimpleEnemySpawner in scene:**
   - In Hierarchy: Search for "SimpleEnemySpawner" or "Spawner"

2. **Inspector Settings:**
   ```
   Enemy Prefab: Drag Enemy_00 from Project
   Spawn Count: 3 (for testing)
   Spawn On Start: YES
   
   TARGET CONFIGURATION:
   Target Transform: (leave empty - auto-finds)
   Target Tag: "PlayerBase"
   
   SPAWN AREA:
   Spawn Center: Assign transform at Z = +20 (TOP)
   Spawn Area Size: (10, 0, 10)
   
   TIMING:
   Initial Delay: 2 (seconds)
   Spawn Interval: 3 (seconds)
   
   DEBUG:
   Debug Mode: YES (check console logs)
   ```

3. **Create Spawn Center Point (if needed):**
   - Create Empty GameObject
   - Name: "EnemySpawnCenter"
   - Position: **(0, 0, +20)** ← TOP of battlefield
   - Drag into Spawner's "Spawn Center" field

✅ **Test:** Spawner should have Enemy_00 assigned and Spawn Center at Z = +20

---

### **Step 4: Test in Play Mode (3 minutes)**

1. **Press Play**

2. **Watch Console:** Should see:
   ```
   [SimpleEnemySpawner] Auto-found target: PlayerBase at (0, 0, -8)
   [SimpleEnemySpawner] Spawned Enemy_1 at (0, 0, 20) with 100 HP
   [SimpleEnemySpawner] Set Enemy_1 AIBrain target to: PlayerBase at (0, 0, -8)
   ```

3. **Watch Scene View:**
   - Enemy spawns at Z = +20 (TOP)
   - Enemy moves DOWNWARD (negative Z direction)
   - Enemy approaches Z = -8 (BOTTOM)
   - Enemy stops near PlayerBase (within 1.0m)

4. **Debug in Inspector (select spawned enemy):**
   - AIBrain → Current State: Should show "MoveToBase"
   - AIBrain → Target: Should show "PlayerBase (Transform)"
   - Character → Movement State: Should show "Walking"

✅ **Success Criteria:**
- ✅ Enemy spawns at TOP
- ✅ Console shows target set to PlayerBase
- ✅ Enemy moves DOWNWARD
- ✅ Enemy stops near base
- ✅ No errors in console

---

## 🐛 Troubleshooting

### **Enemy doesn't spawn:**
- Check Enemy Prefab is assigned in spawner
- Check Initial Delay (wait 2 seconds)
- Check Spawn Count > 0

### **Enemy spawns but doesn't move:**
- Check AIBrain.BrainActive = true
- Check AIBrain.Target is set (not null) in Inspector during Play
- Check CharacterMovement ability exists on enemy
- Check TopDownController3D Speed > 0
- Check console for "Set Enemy_X AIBrain target" message

### **"Could not find GameObject with tag 'PlayerBase'" warning:**
- Check PlayerBase exists in scene
- Check PlayerBase Tag is set to "PlayerBase"
- Check tag spelling matches exactly

### **Enemy moves wrong direction:**
- Check Spawn Center is at Z = +20 (positive)
- Check PlayerBase is at Z = -8 (negative)
- Enemy should move from higher Z to lower Z

### **Enemy spins/rotates weirdly:**
- Check Rigidbody → Constraints → Freeze Rotation X, Y, Z are checked

### **Enemy falls through floor:**
- Check Rigidbody → Use Gravity is UNCHECKED
- This is a top-down game, gravity should be disabled

---

## 🎯 What You Should See

**After 2 seconds:**
```
TOP (Z = +20):        🟢 Enemy_1 spawns here
                           ↓
                           ↓ (moves downward)
                           ↓
                      🟢 Enemy_2 spawns
                           ↓
                           ↓
                      🟢 Enemy_3 spawns
                           ↓
                           ↓
BOTTOM (Z = -8):      🏰 PlayerBase
```

**Console Output:**
```
[SimpleEnemySpawner] Auto-found target: PlayerBase at (0, 0, -8)
[SimpleEnemySpawner] Spawned Enemy_1 at (0, 0, 20) with 87 HP
[SimpleEnemySpawner] Set Enemy_1 AIBrain target to: PlayerBase at (0, 0, -8)
[SimpleEnemySpawner] Spawned Enemy_2 at (0.5, 0, 20) with 132 HP
[SimpleEnemySpawner] Set Enemy_2 AIBrain target to: PlayerBase at (0, 0, -8)
...
```

---

## 📈 Next Steps

Once basic movement works:

1. **Add Attack Logic:**
   - Create AIActionDamageBase
   - Add "AttackBase" state
   - Transition when distance < 1.5m

2. **Add Enemy Variety:**
   - Duplicate Enemy_00 → Enemy_Fast (speed 6)
   - Duplicate Enemy_00 → Enemy_Tank (speed 2, HP 300)

3. **Add Visual Feedback:**
   - Enemy health bars
   - Movement trails
   - Spawn effects

4. **Test with Heroes:**
   - Deploy heroes in Firing grid
   - Heroes should auto-target moving enemies
   - Verify projectiles hit enemies

---

## 📁 Files Modified

- ✅ `SimpleEnemySpawner.cs` - Auto-configures enemy AI target
- ✅ `Enemy_00.prefab` - Added AIBrain with MoveToBase state
- ✅ `GameScene.unity` - Added PlayerBase GameObject

## 📚 Related Documentation

- **Full Setup Guide:** `ENEMY_AI_SETUP.md` (detailed explanations)
- **TDE AI Documentation:** TopDown Engine docs → AI Brain system
- **Component Reference:** TDE docs → AIActionMoveTowardsTarget3D

---

**Estimated Time:** 15 minutes
**Difficulty:** Beginner (using TDE built-in components)
**Result:** Enemies spawn at TOP, move to BOTTOM automatically
