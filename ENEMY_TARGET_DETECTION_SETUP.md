# Enemy Target Detection & Shooting - Complete Setup Guide

## 📋 Overview

Enemies now use TDE's standard shooting system with intelligent target detection:
- **Priority 1:** Shoot at heroes if within range
- **Priority 2:** Shoot at wall if no heroes present
- Uses standard TDE components (AIActionShoot3D, AIActionAimWeaponAtTarget3D)
- Only 1 custom component: AIDecisionDetectHeroOrWall

---

## ✅ What Was Changed

### **Removed (Unnecessary):**
- ❌ AIActionShootAtBase.cs (replaced with TDE's AIActionShoot3D)
- ❌ ENEMY_SHOOTING_SETUP.md (outdated)

### **Created:**
- ✅ AIDecisionDetectHeroOrWall.cs (target selection logic)

### **Updated:**
- ✅ LaneSpawner.cs (now configures detection range from EnemyDataSO)

---

## 🎯 STEP-BY-STEP UNITY SETUP

### **STEP 1: Create Base Wall GameObject (5 minutes)**

This is the fallback target when no heroes are in range.

#### **1A. Create Wall GameObject**
```
Hierarchy → Right-click → Create Empty
Name: "BaseWall"
Position: (0, 0, -5)  // Must match BattlefieldConfigSO.BaseWallZ
```

#### **1B. Add Components**
```
Select BaseWall GameObject

1. Add Component → Mesh Filter
   - Mesh: Cube

2. Add Component → Mesh Renderer
   - Material: Any (optional - can be invisible)

3. Add Component → Box Collider
   - Size: Match BattlefieldConfigSO.BaseWallWidth
   - Example: (8, 5, 1) for width=8, height=5

4. Add Component → TopDown Engine → Health
   - Maximum Health: 1000
   - Current Health: 1000
   - Model: BaseWall (drag self)

5. Set Layer: Create new layer "Base" or use existing
```

#### **1C. Configure Collisions**
```
Edit → Project Settings → Physics
Layer Collision Matrix:
- Enable: Enemies × Base (enemies can hit wall)
- Enable: Projectiles × Base (projectiles hit wall)
```

---

### **STEP 2: Setup Enemy Weapon System (10 minutes)**

#### **2A. Add CharacterHandleWeapon Ability**
```
Open: Enemy_00 prefab
Select: Main Character GameObject

Add Component → TopDown Engine → Character → Abilities → Character Handle Weapon

Configure:
- Auto Equip Weapons At Start: ✓ Yes
- Initial Weapon: (leave empty for now)
```

#### **2B. Create Weapon GameObject**
```
In Enemy_00 prefab hierarchy:
Right-click Character GameObject → Create Empty
Name: "Weapon"
Position: (0, 1.8, 0)  // At turret height

Add Components to Weapon:

1. TopDown Engine → Weapons → Projectile Weapon
   Configure:
   - Weapon Name: "Enemy Cannon"
   - Trigger Mode: Auto
   - Delay Before Use: 0
   - Time Between Uses: 1.0 (overridden by EnemyDataSO)
   - Magazine Size: 999
   - Auto Reload: ✓ Yes
   - Projectile Prefab: TankCannonBulletAI_Blast (or your projectile)
   - Projectile Spawn Offset: (0, 0, 0.5)  // In front of turret

2. TopDown Engine → Weapons → Weapon Aim
   Configure:
   - Aim Control: Script
   - Weapon Rotation Speed: 100
   - Lock Vertical Aim: ✓ Yes (for top-down)
```

#### **2C. Create Projectile Spawn Point**
```
In Weapon GameObject:
Right-click → Create Empty
Name: "ProjectileSpawn"
Position: (0, 0, 0.5)  // In front of weapon

Back to Weapon's ProjectileWeapon component:
- Spawn Position: Drag "ProjectileSpawn" here
```

#### **2D. Link Weapon to Ability**
```
Select: Enemy_00 main GameObject
Find: CharacterHandleWeapon ability
Set: Initial Weapon → Drag "Weapon" child GameObject here
```

---

### **STEP 3: Configure AIBrain States (15 minutes)**

#### **3A. Fix Moving → Attacking Transition**
```
Open: Enemy_00 prefab
Select: Enemy_00/AIBrain GameObject

In AIBrain component:
1. Expand: States → "Moving" state
2. Expand: Transitions → Element 0
3. Verify Decision: AIDecisionReachedWall
4. Set TrueState: "Attacking"  // ⚠️ CRITICAL - must match state name!
5. Set FalseState: "" (empty)
```

#### **3B. Add Standard TDE Shooting Actions**
```
Still on AIBrain GameObject

Add Component → TopDown Engine → Character → AI → Actions → AI Action Shoot 3D
Configure:
- Aim At Target: ✓ Yes
- Aim Origin: Transform
- Shoot Offset: (0, 1.8, 0)  // Weapon height
- Lock Vertical Aim: ✓ Yes
- Target Handle Weapon Ability: (leave empty - auto-found)

Add Component → TopDown Engine → Character → AI → Actions → AI Action Aim Weapon At Target 3D
Configure:
- Target Handle Weapon Ability: (leave empty - auto-found)
- Aim At Target: ✓ Yes
```

#### **3C. Add Custom Target Detection Decision**
```
Still on AIBrain GameObject

Add Component → ProjectBlast → Combat → AI → Decisions → AI Decision Detect Hero Or Wall

Configure:
- Wall Target: Drag "BaseWall" GameObject from scene
- Manual Wall Position: (0, 2.5, -5)  // Fallback if no wall object
- Hero Detection Range: 8.0  // Overridden by EnemyDataSO.AttackRange
- Hero Layer Mask: Select "Player" layer (Layer 10)
- Minimum Target Lock Duration: 0.3
- Target Stickyness: 20
- Debug Mode: ✓ Yes (for testing)
```

#### **3D. Configure Attacking State**
```
In AIBrain component → States array:

Find "Attacking" state (or rename "Attack" to "Attacking")

Set Actions array (3 elements):
  Element 0: Drag "AIActionShoot3D" component
  Element 1: Drag "AIActionAimWeaponAtTarget3D" component
  Element 2: Drag "AIDecisionDetectHeroOrWall" component (optional, for continuous checking)

Set Transitions array (0 elements):
  - Empty (enemy stays in Attacking until killed)
  
⚠️ NOTE: AIDecisionDetectHeroOrWall runs continuously while in state
         It's NOT a transition decision!
```

---

### **STEP 4: Configure Enemy Layers (5 minutes)**

#### **4A. Set Enemy Layer**
```
Select: Enemy_00 prefab (all GameObjects)
Set Layer: "Enemies" (Layer 13) - should already be set
```

#### **4B. Verify Layer Collision Matrix**
```
Edit → Project Settings → Physics

Enable these collision pairs:
✓ Enemies × Player (enemies detect heroes)
✓ Enemies × Base (enemies hit wall)
✓ Projectiles × Player (enemy projectiles hit heroes)
✓ Projectiles × Base (enemy projectiles hit wall)
✓ Player × Enemies (heroes hit enemies)
```

---

### **STEP 5: Test Configuration (10 minutes)**

#### **5A. Enable Debug Modes**
```
LaneSpawner: ✓ Debug Mode
AIDecisionDetectHeroOrWall: ✓ Debug Mode
TPSDirector: ✓ Show Debug UI
```

#### **5B. Create Test Scene Setup**
```
Scene must have:
- ✓ BaseWall GameObject at Z=-5
- ✓ TPSDirector with StageConfigSO
- ✓ LaneSpawners (3) at spawn positions
- ✓ GridManager with heroes in Firing zone
- ✓ Camera positioned to see Z=+20 to Z=-5
```

#### **5C. Run Test & Verify**
```
Play Game and check console:

✅ Expected Logs:
"[LaneSpawner] Configured AIDecisionDetectHeroOrWall - Range: 8.0m"
"[AIDecisionDetectHeroOrWall] Initialized on Enemy_00"
"[AIDecisionDetectHeroOrWall] Enemy_00 targeting Wall GameObject: BaseWall"

✅ Expected Behavior:
1. Enemy spawns at Z=+20
2. Moves forward (AIActionMoveForwardInLane)
3. Reaches Z=-5 (AIDecisionReachedWall triggers)
4. Stops moving
5. Transitions to "Attacking" state
6. AIDecisionDetectHeroOrWall runs:
   - Checks for heroes in 8m range
   - If heroes present → Targets closest hero
   - If no heroes → Targets BaseWall
7. AIActionShoot3D shoots at target
8. AIActionAimWeaponAtTarget3D rotates weapon toward target
9. Projectiles spawn and fly toward target
10. If hero gets within range → Enemy switches to hero
11. If hero dies or moves away → Enemy switches back to wall
```

---

## 🎮 BEHAVIOR EXAMPLES

### **Scenario 1: No Heroes in Range**
```
Enemy reaches wall → Detects no heroes → Targets BaseWall
→ Shoots at wall continuously
→ Wall Health decreases
→ If Wall Health reaches 0 → Game Over
```

### **Scenario 2: Hero Enters Range**
```
Enemy shooting wall → Hero moves to Z=-3 (within 8m range)
→ AIDecisionDetectHeroOrWall detects hero
→ Switches target from Wall to Hero
→ Weapon rotates toward hero
→ Shoots at hero
```

### **Scenario 3: Hero Dies**
```
Enemy shooting hero → Hero Health reaches 0
→ Hero dies
→ AIDecisionDetectHeroOrWall re-evaluates
→ Finds no valid heroes
→ Switches back to Wall
→ Resumes shooting wall
```

### **Scenario 4: Multiple Heroes**
```
Enemy at wall → 2 heroes in range (one at 5m, one at 7m)
→ AIDecisionDetectHeroOrWall finds both
→ Calculates closest: 5m hero
→ Targets closest hero
→ Shoots at closest hero
```

---

## 🐛 TROUBLESHOOTING

### **Issue: Enemy doesn't shoot at all**

**Checklist:**
```
[ ] CharacterHandleWeapon ability exists?
[ ] Weapon GameObject exists as child?
[ ] ProjectileWeapon component configured?
[ ] Projectile prefab assigned?
[ ] AIActionShoot3D in Attacking state Actions?
[ ] Moving state TrueState = "Attacking"?
[ ] AIBrain transitions to Attacking state? (check console)
```

**Debug:**
```
Enable DebugMode on AIDecisionDetectHeroOrWall
Check console for: "targeting Wall GameObject" or "targeting Hero"
If no log → AIBrain not entering Attacking state
  → Fix transition: Moving TrueState must be "Attacking"
```

---

### **Issue: Enemy shoots but doesn't hit wall**

**Checklist:**
```
[ ] BaseWall GameObject exists in scene?
[ ] BaseWall has Collider?
[ ] BaseWall Layer set correctly?
[ ] Physics collision matrix: Projectiles × Base enabled?
[ ] Projectile has DamageOnTouch component?
```

**Debug:**
```
Select BaseWall in Hierarchy
Check Health component: Does CurrentHealth decrease?
  Yes → System working!
  No → Collision issue
    → Check Layer Collision Matrix
    → Verify projectile layer and wall layer can collide
```

---

### **Issue: Enemy ignores heroes, only shoots wall**

**Checklist:**
```
[ ] AIDecisionDetectHeroOrWall on AIBrain?
[ ] Hero Layer Mask correct? (Layer 10 = Player)
[ ] Hero Detection Range > 0?
[ ] Heroes have Character component with CharacterType = Player?
[ ] Heroes in actual detection range? (check distance)
```

**Debug:**
```
Enable DebugMode on AIDecisionDetectHeroOrWall
In Scene view:
- Select enemy
- Orange wire sphere = detection radius
- Should overlap hero position
If no overlap → Heroes too far away
If overlap but no targeting → Layer mask wrong
```

---

### **Issue: Enemy shoots wrong direction**

**Checklist:**
```
[ ] AIActionAimWeaponAtTarget3D in Attacking state?
[ ] WeaponAim component on Weapon?
[ ] WeaponAim.Aim Control = Script?
[ ] Target correctly set in AIBrain?
```

**Debug:**
```
Play mode → Select enemy AIBrain
Inspector → AIBrain.Target
Should show either:
- Hero GameObject (when hero in range)
- BaseWall GameObject (when no heroes)

If Target is null → AIDecisionDetectHeroOrWall not running
If Target is set but aim wrong → WeaponAim issue
```

---

## 📊 STATS FROM ENEMYDATASO

The following values are automatically applied:

| EnemyDataSO Field | Applied To | Effect |
|-------------------|-----------|--------|
| `MovementSpeed` | CharacterMovement.WalkSpeed | Enemy forward speed |
| `MaxHealth` | Health.MaximumHealth | Enemy HP |
| `AttackRange` | AIDecisionDetectHeroOrWall.HeroDetectionRange | Hero detection radius |
| `FireRate` | Weapon.TimeBetweenUses | Shots per second |
| `DamagePerShot` | EnemyWeaponData → DamageOnTouch | Damage per hit |
| `ProjectileSpeed` | EnemyWeaponData → Projectile.Speed | Projectile velocity |

**Example:**
```yaml
EnemyDataSO "Grunt":
  AttackRange: 8.0      → Detects heroes within 8m
  FireRate: 1.0         → Shoots 1 projectile/second
  DamagePerShot: 10     → Each hit deals 10 damage
  ProjectileSpeed: 15   → Projectiles fly at 15 units/sec
```

---

## ✅ FINAL CHECKLIST

### **Scripts Ready:**
- [x] AIDecisionDetectHeroOrWall.cs created
- [x] LaneSpawner.cs updated
- [x] AIActionShootAtBase.cs deleted (unnecessary)

### **Scene Setup:**
- [ ] BaseWall GameObject created at (0, 0, -5)
- [ ] BaseWall has Health component (MaxHealth: 1000)
- [ ] BaseWall has Collider
- [ ] BaseWall Layer set (Base or Enemies)
- [ ] Layer collision matrix configured

### **Enemy Prefab:**
- [ ] CharacterHandleWeapon ability added
- [ ] Weapon GameObject created with ProjectileWeapon
- [ ] Weapon has WeaponAim component
- [ ] Projectile prefab assigned
- [ ] AIActionShoot3D added to AIBrain
- [ ] AIActionAimWeaponAtTarget3D added to AIBrain
- [ ] AIDecisionDetectHeroOrWall added to AIBrain
- [ ] AIDecisionDetectHeroOrWall.WallTarget = BaseWall
- [ ] Attacking state Actions includes all 3 components
- [ ] Moving state TrueState = "Attacking"

### **Testing:**
- [ ] Debug modes enabled
- [ ] Enemy spawns and moves forward
- [ ] Enemy stops at wall
- [ ] Enemy transitions to Attacking state
- [ ] Enemy shoots at wall (no heroes)
- [ ] Wall Health decreases
- [ ] Enemy switches to hero when hero in range
- [ ] Enemy switches back to wall when hero dies/leaves
- [ ] Projectiles hit both wall and heroes

---

## 🎯 KEY DIFFERENCES FROM OLD SYSTEM

| Aspect | Old (AIActionShootAtBase) | New (TDE Components) |
|--------|---------------------------|---------------------|
| **Components** | 1 custom action | 3 TDE components + 1 custom decision |
| **Target System** | Hardcoded wall position | Dynamic hero/wall detection |
| **Flexibility** | Wall only | Heroes + Wall |
| **TDE Integration** | Custom | Standard TDE pattern |
| **Aiming** | Manual calculation | TDE's WeaponAim |
| **Shooting** | Custom implementation | TDE's AIActionShoot3D |
| **Extensibility** | Limited | High (can add more target types) |

---

**Once all checkboxes complete, enemies will intelligently shoot at heroes or wall!** 🎯🔫

---

## 🚀 NEXT STEPS

After this works:
1. **Tune detection range** - Adjust EnemyDataSO.AttackRange per enemy type
2. **Add target stickyness** - Prevent flickering between targets
3. **Create BaseHealth UI** - Show wall health bar on HUD
4. **Implement game over** - When BaseWall health reaches 0
5. **Add visual feedback** - Damage effects on wall, muzzle flash on weapon
