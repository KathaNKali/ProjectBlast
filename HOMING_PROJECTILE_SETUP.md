# 🚀 Homing Projectile System - Setup Guide

## ✅ Implementation Complete

The homing projectile system has been successfully implemented using **Option 1: Lerp-Based Homing**.

**✨ UPDATED: System now supports BOTH Heroes AND Enemies!**

---

## 📦 Files Created/Updated

### **1. HomingProjectile.cs**
Path: `Assets/ProjectBlast/Scripts/Combat/Weapons/HomingProjectile.cs`

- Extends TDE's `Projectile` class
- Smooth lerp-based steering toward targets
- Configurable turn speed and homing duration
- Target validation (checks if target is alive)
- Debug visualization with gizmos
- Minimum tracking distance to prevent spiraling

**Key Features:**
- `TurnSpeed` (1-20): Controls curve sharpness
- `HomingDuration` (seconds): How long projectile tracks
- `MinTrackingDistance`: Prevents close-range spiraling
- `UseTurnRateLimit`: Optional realistic turn rate limiting

### **2. HomingProjectileWeapon.cs**
Path: `Assets/ProjectBlast/Scripts/Combat/Weapons/HomingProjectileWeapon.cs`

- Extends TDE's `ProjectileWeapon`
- Automatically assigns `AIBrain.Target` to homing projectiles
- **Works with both AI enemies AND heroes**
- Supports both AI and manual target assignment

### **3. Updated EnemyDataSO.cs**

Added homing parameters:
- `UseHomingProjectiles` (bool): Enable/disable homing
- `HomingTurnSpeed` (1-20): Turn rate
- `HomingDuration` (0.5-10s): Tracking time

### **4. Updated HeroDataSO.cs** ⭐ NEW

Added homing parameters for heroes:
- `UseHomingProjectiles` (bool): Enable/disable homing
- `HomingTurnSpeed` (1-20): Turn rate
- `HomingDuration` (0.5-10s): Tracking time

### **5. Updated Hero.cs** ⭐ NEW

Added `ApplyHomingSettings()` method:
- Automatically applies homing settings from HeroDataSO
- Configures projectile prefab when weapon is equipped
- Called after weapon instantiation

### **6. Updated LaneSpawner.cs**

Auto-applies homing settings from EnemyDataSO to spawned enemy projectiles.

---

## 🎯 Unity Setup Instructions

### **STEP 1: Create Homing Projectile Prefab (10 minutes)**

#### **Option A: Modify Existing Projectile**

If you already have a projectile prefab (e.g., `TankCannonBulletAI_Blast`):

```
1. Open projectile prefab in Prefab Mode
2. Select root GameObject
3. Remove existing "Projectile" component (if present)
4. Add Component → ProjectBlast → Combat → Weapons → Homing Projectile
5. Configure Homing Projectile:
   - Speed: 15 (from EnemyDataSO.ProjectileSpeed)
   - Direction: (0, 0, -1) for forward
   - Face Movement: ✓ Yes
   - Movement Vector: Forward (for 3D) or Right (for 2D)
   - Turn Speed: 5 (default, overridden by EnemyDataSO)
   - Homing Duration: 3 (default, overridden by EnemyDataSO)
   - Min Tracking Distance: 0.5
   - Show Debug Gizmos: ✓ Yes (for testing)
6. Save prefab
```

#### **Option B: Create New Homing Projectile**

```
1. Create Empty GameObject: "HomingProjectile"
2. Add Components:
   - Rigidbody (or Rigidbody2D for 2D)
     * Use Gravity: ✗ No
     * Is Kinematic: ✗ No
   - Sphere Collider (or Circle Collider 2D)
     * Radius: 0.2
     * Is Trigger: ✓ Yes
   - ProjectBlast → Combat → Weapons → Homing Projectile
     * Configure as above
   - TopDown Engine → Damage On Touch
     * Damage: 10 (overridden by EnemyDataSO)
     * Target Layer Mask: Player, Base
   - Mesh/Sprite for visual
3. Save as prefab in Assets/ProjectBlast/Prefabs/Projectiles/
```

---

### **STEP 2A: Setup Hero Weapon (For Heroes) (5 minutes)**

#### **Option 1: Use HomingProjectileWeapon Component**

```
1. Open hero weapon prefab (e.g., Weapon_HeroRifle)
2. Select root GameObject
3. If using standard ProjectileWeapon:
   - Remove "Projectile Weapon" component
   - Add Component → ProjectBlast → Combat → Weapons → Homing Projectile Weapon
4. Configure:
   - Weapon Name: "Hero Homing Rifle"
   - Projectile Prefab: Drag your homing projectile prefab
   - Time Between Uses: (will be overridden by HeroDataSO.FireRate)
   - Magazine Size: 999 (if using ammo system)
   - Auto Reload: ✓ Yes
   - Spawn Position: ProjectileSpawn child transform
   - Auto Assign Target: ✓ Yes
   - Debug Mode: ✓ Yes (for testing)
5. Save prefab
```

#### **Option 2: Keep Standard ProjectileWeapon**

If you want to keep using standard `ProjectileWeapon`, the `Hero.ApplyHomingSettings()` method will still configure homing on the projectile prefab template. Just ensure:
- Projectile prefab has `HomingProjectile` component
- `HeroDataSO.UseHomingProjectiles = true`

---

### **STEP 2B: Setup Enemy Weapon (For Enemies) (5 minutes)**

#### **2A. Replace Weapon Component**

```
1. Open Enemy_00 prefab
2. Select Weapon child GameObject
3. Remove existing "Projectile Weapon" component
4. Add Component → ProjectBlast → Combat → Weapons → Homing Projectile Weapon
5. Configure:
   - Weapon Name: "Enemy Homing Cannon"
   - Projectile Prefab: Drag your homing projectile prefab
   - Time Between Uses: 1.0 (overridden by EnemyDataSO.FireRate)
   - Magazine Size: 999
   - Auto Reload: ✓ Yes
   - Spawn Position: ProjectileSpawn child transform
   - Auto Assign Target: ✓ Yes
   - Debug Mode: ✓ Yes (for testing)
6. Save prefab
```

---

### **STEP 3A: Configure HeroDataSO (For Heroes) (2 minutes)** ⭐ NEW

```
1. Select your HeroDataSO asset (e.g., Hero_Archer)
2. Expand "Weapon" section
3. Find "Homing Behavior" subsection
4. Configure:
   - Use Homing Projectiles: ✓ Yes
   - Homing Turn Speed: 5 (balanced - try 3-8)
   - Homing Duration: 3 (seconds)
5. Save asset
```

**Recommended Turn Speed Values for Heroes:**
- **3-4**: Gentle tracking (skill-based, enemies can dodge)
- **5-7**: Balanced (moderate advantage)
- **8-12**: Strong tracking (powerful heroes, hard to dodge)
- **15+**: Nearly unavoidable (boss heroes, special abilities)

---

### **STEP 3B: Configure EnemyDataSO (For Enemies) (2 minutes)**

```
1. Select your EnemyDataSO asset (e.g., Enemy_Grunt)
2. Expand "Combat Stats" section
3. Find "Homing Behavior" subsection
4. Configure:
   - Use Homing Projectiles: ✓ Yes
   - Homing Turn Speed: 5 (balanced - try 3-8)
   - Homing Duration: 3 (seconds)
5. Save asset
```

**Recommended Turn Speed Values:**
- **3-4**: Gentle curve (anti-air missile style)
- **5-7**: Balanced (general purpose)
- **8-12**: Aggressive (heat-seeker style)
- **15+**: Very sharp (magic missile style)

---

### **STEP 4: Test Homing Behavior (5 minutes)**

#### **4A. Scene Setup**

```
Ensure scene has:
- ✓ BaseWall GameObject at Z=-5
- ✓ TPSDirector with stage config
- ✓ LaneSpawners configured
- ✓ GridManager with heroes in Firing zone (Z=-3 to Z=0)
```

#### **4B. Enable Debug Modes**

```
1. LaneSpawner: ✓ Debug Mode
2. HomingProjectileWeapon on Enemy: ✓ Debug Mode
3. HomingProjectile prefab: ✓ Show Debug Gizmos
4. AIDecisionDetectHeroOrWall: ✓ Debug Mode
```

#### **4C. Run Test**

```
Play Game → Check Console:

✅ Expected Logs:
"[LaneSpawner] Applied homing settings to Enemy_00: TurnSpeed=5, Duration=3"
"[HomingProjectileWeapon] Assigned target Hero_Archer to projectile ..."
"[HomingProjectile] HomingProjectile_Clone acquired target: Hero_Archer at distance 8.2m"

✅ Expected Visual Behavior:
1. Enemy spawns, moves forward
2. Enemy reaches wall, enters Attacking state
3. Enemy shoots projectile at hero (or wall if no heroes)
4. Projectile curves smoothly toward target
5. If hero dies, projectile continues straight
6. Projectile hits target or expires after HomingDuration

✅ Scene View Debug Gizmos:
- Blue line: Current projectile direction
- Green line: Line to target (if homing)
- Red line: Line to target (if not homing anymore)
- Yellow wireframe sphere: Minimum tracking distance around target
- Green wireframe sphere: Homing active indicator on projectile
```

---

## 🎨 Visual Comparison

### **Before (Straight Projectile):**
```
Enemy at Z=-5
    ↓
    Shoots projectile → → → → (flies straight forward)
    ↓
    Misses if hero moves sideways
```

### **After (Homing Projectile):**
```
Enemy at Z=-5
    ↓
    Shoots projectile → → ↘ ↓ (curves toward hero)
    ↓                     ↓
    Tracks hero movement  ↓
                       Hits hero!
```

---

## 🔧 Tuning Examples

### **Scenario 1: Slow, Wide Curve (Anti-Air Style)**
```yaml
EnemyDataSO:
  HomingTurnSpeed: 3
  HomingDuration: 5
  ProjectileSpeed: 12

Result: Gentle arc, gives heroes time to dodge
```

### **Scenario 2: Balanced Tracking**
```yaml
EnemyDataSO:
  HomingTurnSpeed: 5
  HomingDuration: 3
  ProjectileSpeed: 15

Result: Moderate curve, requires tactical movement
```

### **Scenario 3: Aggressive Heat-Seeker**
```yaml
EnemyDataSO:
  HomingTurnSpeed: 10
  HomingDuration: 2
  ProjectileSpeed: 18

Result: Sharp curve, very hard to dodge
```

### **Scenario 4: Mixed Forces (Some Homing, Some Straight)**
```yaml
Enemy_Grunt (EnemyDataSO):
  UseHomingProjectiles: false  ← Shoots straight
  
Enemy_Elite (EnemyDataSO):
  UseHomingProjectiles: true   ← Shoots homing
  HomingTurnSpeed: 8

Result: Varied threat levels, tactical depth
```

---

## 🐛 Troubleshooting

### **Issue: Projectiles Don't Curve**

**Checklist:**
```
[ ] EnemyDataSO.UseHomingProjectiles = true?
[ ] Projectile prefab has HomingProjectile component (not standard Projectile)?
[ ] Enemy weapon is HomingProjectileWeapon (not standard ProjectileWeapon)?
[ ] HomingProjectileWeapon.AutoAssignTarget = true?
[ ] AIBrain.Target is set (check AIDecisionDetectHeroOrWall)?
```

**Debug:**
```
Enable all debug modes, check console:
- "[HomingProjectileWeapon] Assigned target..." should appear
- If missing → Target not being assigned
  → Check AIBrain.Target in Inspector during play mode
  → Verify AIDecisionDetectHeroOrWall is running
```

---

### **Issue: Projectiles Curve Too Much (Spiral/Loop)**

**Cause:** TurnSpeed too high

**Solution:**
```
Reduce TurnSpeed:
- Current: 15 → Try: 5-7
- Or enable UseTurnRateLimit
- Set MaxTurnRate: 180 degrees/sec
```

---

### **Issue: Projectiles Don't Curve Enough**

**Cause:** TurnSpeed too low or HomingDuration too short

**Solution:**
```
Increase values:
- TurnSpeed: 3 → 7
- HomingDuration: 1 → 3 seconds
- Check distance: If hero at Z=0, enemy at Z=-5, projectile needs 5/15 = 0.33s to travel
  → HomingDuration must be > 0.33s
```

---

### **Issue: Projectiles Stop Homing Mid-Flight**

**Possible Causes:**

**1. Target Died**
```
Check console: "[HomingProjectile] target became invalid"
Expected behavior - projectile continues straight
```

**2. HomingDuration Expired**
```
Check console: "[HomingProjectile] homing duration expired"
Solution: Increase HomingDuration in EnemyDataSO
```

**3. Too Close to Target**
```
Check console: "[HomingProjectile] within minimum tracking distance"
This prevents spiraling - expected behavior
Adjust MinTrackingDistance if needed (default 0.5m)
```

---

### **Issue: Projectiles Target Wrong Enemy**

**Cause:** AIDecisionDetectHeroOrWall finding different target

**Debug:**
```
Enable debug on AIDecisionDetectHeroOrWall
Check: "[AIDecisionDetectHeroOrWall] Enemy_00 targeting Hero: ..."
Verify target name matches projectile target
```

---

### **Issue: Projectiles Rotate Wrong Axis**

**Cause:** MovementVector setting mismatch

**Solution:**
```
On HomingProjectile component:
- For 3D: Movement Vector = Forward
- For 2D: Movement Vector = Right (or Up depending on sprite orientation)

If sprite faces wrong way:
- Rotate sprite child GameObject
- Or adjust MovementVector setting
```

---

## 📊 Performance Notes

### **Cost Analysis:**

| Component | CPU Cost | Notes |
|-----------|----------|-------|
| **HomingProjectile** | Very Low | Simple Vector3.Lerp + direction calc |
| **Target Validation** | Negligible | 1 Health check per frame per projectile |
| **Debug Gizmos** | Low | Only in Editor, disabled in build |

**Verdict:** Safe for 20-50 simultaneous homing projectiles (typical enemy count).

### **Optimization Tips:**

1. **Disable debug gizmos in production**
   ```csharp
   HomingProjectile.ShowDebugGizmos = false
   ```

2. **Use shorter HomingDuration for distant targets**
   - Reduces computation time
   - Projectiles fly straight after initial curve

3. **Limit projectile lifetime**
   - Add LifeTime field to Projectile
   - Prevents off-screen projectiles from tracking forever

---

## 🎮 Gameplay Impact

### **Player Experience:**

**Without Homing:**
- Sidestep to dodge
- Static positioning safe

**With Homing:**
- Must constantly reposition
- Encourages tactical retreats
- Rewards aggressive play (kill enemy before missiles hit)

### **Difficulty Scaling:**

**Easy Waves:**
```yaml
UseHomingProjectiles: false  # All straight projectiles
```

**Medium Waves:**
```yaml
50% enemies: UseHomingProjectiles: false
50% enemies: UseHomingProjectiles: true (TurnSpeed: 5)
```

**Hard Waves:**
```yaml
100% enemies: UseHomingProjectiles: true (TurnSpeed: 8+)
```

---

## 🚀 Advanced Features (Optional)

### **Feature 1: Target Leading (Predictive Aiming)**

```csharp
// In HomingProjectile.Movement()
Vector3 targetDirection;

if (PredictTargetMovement)
{
    // Predict where target will be
    Rigidbody targetRb = Target.GetComponent<Rigidbody>();
    if (targetRb != null)
    {
        float timeToReach = Vector3.Distance(transform.position, Target.position) / Speed;
        Vector3 predictedPos = Target.position + targetRb.velocity * timeToReach;
        targetDirection = (predictedPos - transform.position).normalized;
    }
    else
    {
        targetDirection = (Target.position - transform.position).normalized;
    }
}
```

### **Feature 2: Multi-Stage Homing**

```csharp
// Phase 1: Rapid turn for 1 second
// Phase 2: Gentle tracking for 2 seconds
// Phase 3: Fly straight

if (_timeAlive < 1f)
{
    currentTurnSpeed = TurnSpeed * 2f; // Aggressive initial turn
}
else if (_timeAlive < 3f)
{
    currentTurnSpeed = TurnSpeed * 0.5f; // Gentle correction
}
else
{
    _isHoming = false; // Stop tracking
}
```

### **Feature 3: Proximity Fuse**

```csharp
// Explode when near target (not just on collision)
float distanceToTarget = Vector3.Distance(transform.position, Target.position);
if (distanceToTarget < ProximityFuseRadius)
{
    // Trigger explosion
    Explode();
}
```

---

## ✅ Final Checklist

### **Code:**
- [x] HomingProjectile.cs created
- [x] HomingProjectileWeapon.cs created
- [x] EnemyDataSO updated with homing params
- [x] LaneSpawner applies homing settings

### **Unity Setup:**
- [ ] Homing projectile prefab created
- [ ] Enemy weapon uses HomingProjectileWeapon component
- [ ] EnemyDataSO.UseHomingProjectiles enabled
- [ ] Test scene with heroes and enemies

### **Testing:**
- [ ] Projectiles curve toward heroes
- [ ] Projectiles fall back to wall if no heroes
- [ ] Projectiles stop tracking after duration
- [ ] Target validation works (dead heroes ignored)
- [ ] Multiple enemies don't interfere with each other

### **Polish:**
- [ ] Disable debug gizmos for production
- [ ] Tune TurnSpeed values per enemy type
- [ ] Add projectile trail effect (optional)
- [ ] Add homing sound effect (optional)

---

## 🎯 What's Next?

Once homing works:

1. **Balance Testing**
   - Test with multiple enemy types
   - Adjust TurnSpeed per difficulty
   - Ensure heroes can still survive

2. **Visual Polish**
   - Add TrailRenderer to projectiles
   - Smoke effect during turns
   - Sound effect for missile lock-on

3. **Advanced Behaviors**
   - Some enemies shoot straight, some homing
   - Boss enemies with multi-stage homing
   - Elite enemies with predictive aiming

---

**Implementation complete! Follow Unity setup steps to enable homing projectiles.** 🚀
