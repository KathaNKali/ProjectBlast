# Hero Firing Test - Quick Setup Guide

**Goal:** Test hero weapon system, auto-targeting, and firing logic with simple cube targets.

---

## 📋 Quick Setup Steps

### **Step 1: Create Test Target**

1. **Create a Cube**
   - Hierarchy → Right-click → 3D Object → Cube
   - Name it "TestTarget"
   - Position: `(0, 0.5, 5)` (in front of Firing grid)
   - Scale: `(1, 1, 1)`

2. **Add TestTarget Script**
   - Select the cube
   - Add Component → Scripts → ProjectBlast.Testing → TestTarget
   - Configure:
     - Target Name: "Test Target"
     - Starting Health: 100
     - Show Damage Logs: ✓

3. **Add Health Component**
   - Add Component → TopDown Engine → Health
   - Initial Health: 100
   - Maximum Health: 100
   - Can Take Damage: ✓

4. **Set Layer to "Enemy"**
   - Top of Inspector → Layer dropdown
   - If "Enemy" layer doesn't exist, create it:
     - Add Layer → User Layer 8 → "Enemy"
   - Set cube's layer to "Enemy"

5. **Add Collider** (should already exist)
   - Cube has BoxCollider by default
   - Ensure it's enabled

---

### **Step 2: Setup Hero for Testing**

#### **Option A: Use Existing Hero in Grid**

If you already have heroes in Passive/Active grids from previous testing:

1. **Select a Hero GameObject**
2. **Configure Hero Component:**
   - Hero Name: "Test Hero"
   - Hero Class: Ranged
   - **Weapon Configuration:**
     - Weapon Prefab: (We'll create this next)
     - Target Layer Mask: Select "Enemy" layer
   - **Ammo System:**
     - Unlimited Ammo: ✓ (for easy testing)
     - Starting Ammo: 100
   - **Auto-Targeting:**
     - Detection Range: 20
     - Target Search Interval: 0.5
     - Auto Fire Rate: 2

#### **Option B: Create New Test Hero**

1. **Create Empty GameObject**
   - Hierarchy → Right-click → Create Empty
   - Name: "TestHero"
   - Position: Firing Grid slot (e.g., `(0, 0.5, 0)`)

2. **Add Visual (Cube/Capsule)**
   - Right-click TestHero → 3D Object → Capsule
   - Name: "HeroModel"
   - Local Position: `(0, 0, 0)`
   - Scale: `(0.5, 1, 0.5)`

3. **Add Components to TestHero:**
   - Add Component → Hero (ProjectBlast.Heroes)
   - Add Component → Character (TopDown Engine)
   - Add Component → Health (TopDown Engine)
   - Add Component → Character Handle Weapon (TopDown Engine)
   - Add Component → Box Collider (for click detection)

4. **Configure Hero:**
   (Same as Option A above)

---

### **Step 3: Create Simple Weapon Prefab**

Since TDE weapons are complex, let's use a TopDown Engine demo weapon:

#### **Quick Option: Copy from TDE Demos**

1. **Find Demo Weapon:**
   - Project → Assets → TopDownEngine → Demos → **Loft3D** or **Minimal3D**
   - Navigate to Weapons folder
   - Find a ProjectileWeapon prefab (e.g., "AssaultRifle3D")

2. **Duplicate to Your Project:**
   - Right-click weapon prefab → Duplicate
   - Drag duplicate to `Assets/ProjectBlast/Prefabs/Weapons/` (create folder if needed)
   - Rename to "TestWeapon"

3. **Simplify Weapon:**
   - Open TestWeapon prefab
   - Weapon component settings:
     - Trigger Mode: Auto
     - Delay Before Use: 0
     - Time Between Uses: 0.5 (fire rate)
   - ProjectileWeapon settings:
     - Check that Projectile prefab is assigned
   - Projectile settings:
     - Damage Caused: 10
     - Speed: 20
     - Lifetime: 5

4. **Assign to Hero:**
   - Select your TestHero
   - Drag TestWeapon prefab into "Weapon Prefab" field

#### **Alternative: Create Minimal Weapon**

If copying is difficult, we can create one from scratch later. For now, try the copy method.

---

### **Step 4: Setup GridManager (if not already)**

1. **Create GridManager GameObject:**
   - Hierarchy → Create Empty
   - Name: "GridManager"
   - Position: `(0, 0, 0)`
   - Add Component → GridManager (ProjectBlast.Grid)

2. **Configure Grids:**
   - **Passive Grid:**
     - Rows: 2, Columns: 3
     - Center: `(0, 0, -6)`
   - **Active Grid:**
     - Rows: 1, Columns: 3
     - Center: `(0, 0, -3)`
   - **Firing Grid:**
     - Rows: 2, Columns: 3
     - Center: `(0, 0, 0)`
   - **Cell Size:** 1.5
   - **Show Grid Gizmos:** ✓

---

### **Step 5: Setup HeroQueueManager**

1. **Create HeroQueueManager GameObject:**
   - Hierarchy → Create Empty
   - Name: "HeroQueueManager"
   - Add Component → HeroQueueManager

2. **Configure:**
   - Auto Spawn On Start: ✗ (we'll manually place hero)
   - Show Debug Logs: ✓

---

### **Step 6: Position Hero in Firing Grid**

**Manual Placement:**

1. Select your TestHero
2. Position in Firing Grid slot: `(0, 0.5, 0)` or similar
3. Run the scene to register with GridManager

**OR via Script:**

```csharp
// In Unity console or test script:
GridManager.Instance.PlaceHero(hero, GridZone.Firing, 0, 0);
hero.StartFiring();
```

---

## 🎮 Testing Procedure

### **Important: Heroes Auto-Start Firing Now!**

When a hero is deployed to the Firing zone (via clicking in Active zone), they **automatically start firing**. You don't need to manually call `StartFiring()` anymore!

### **Option A: Test via Active → Firing Deployment (Recommended)**

1. **Setup heroes in Active zone** (if not already)
2. **Play the scene**
3. **Click on a hero in Active zone**
4. **Hero deploys to Firing zone AND automatically starts firing**
5. **Watch console for:**
   ```
   [HeroQueueManager] Deployed Hero → Firing (0, 0)
   [Hero] Hero started firing with unlimited ammo
   [Hero] Hero found 1 potential targets in range
   [Hero] Hero acquired target: TestTarget at distance 5.00m
   ```

### **Option B: Manual Testing (Quick Test)**

If you want to quickly test a hero without the queue system:

1. **Create a hero in the scene** (or select existing)
2. **Position manually** in Firing zone: `(0, 0.5, 0)`
3. **Open Console Window** (Ctrl/Cmd + Shift + C)
4. **Type in Console:**
   ```csharp
   // C# Interactive Console or via script:
   var hero = GameObject.Find("YourHeroName").GetComponent<ProjectBlast.Heroes.Hero>();
   ProjectBlast.Heroes.HeroQueueManager.Instance.PlaceHeroInFiringZone(hero, 0, 0);
   ```
   
   OR use the helper method in a test script:
   ```csharp
   HeroQueueManager.Instance.PlaceHeroInFiringZone(heroReference, 0, 0);
   ```

### **Test 1: Basic Detection**

1. **Play the scene**
2. **Position TestHero in Firing zone** (if not already)
3. **Call `hero.StartFiring()` in console or via script**
4. **Check console for:**
   - `[Hero] Test Hero started firing`
   - `[Hero] Test Hero searching for targets...`

### **Test 2: Auto-Targeting**

1. **In Scene view, select TestHero**
2. **Look at Gizmos:**
   - Red sphere = detection range (should be 20m)
   - Yellow line = line to target (if found)
3. **Check console:**
   - Should see "Target found: TestTarget" or similar
   - If no target found, check:
     - Target layer is "Enemy"
     - Hero's TargetLayerMask includes "Enemy"
     - Target is within Detection Range

### **Test 3: Weapon Firing**

1. **Watch for projectiles:**
   - Hero should fire projectiles toward target
   - Projectiles should spawn from weapon attachment point
2. **Check console:**
   - `[Hero] Firing at target...`
   - `[TestTarget] Test Target HIT! HP: 90/100`
3. **Watch target health bar:**
   - Green bar above target should decrease
   - Target should flash red on hit (if DamageMaterial assigned)

### **Test 4: Ammo Consumption**

If using limited ammo:

1. **Watch console for ammo count:**
   - `[Hero] Ammo: 99/100`
   - `[Hero] Ammo: 98/100`
   - ...
   - `[Hero] Ammo LOW! 20 remaining`
   - `[Hero] OUT OF AMMO!`
2. **Verify hero stops firing when ammo depletes**

### **Test 5: Target Destruction**

1. **Wait for target HP to reach 0**
2. **Check console:**
   - `[TestTarget] Test Target DESTROYED!`
3. **Target should disappear after 0.5s**
4. **Hero should continue searching for new targets**

---

## 🔧 Troubleshooting

### **NullReferenceException: MMUIFollowMouse**
**Error:** `MMUIFollowMouse.LateUpdate() NullReferenceException`

**Cause:** Weapon prefab has reticle/cursor components meant for player-controlled characters.

**Fix:** The Hero script now automatically disables these components. If you still see this error:
1. Select the weapon prefab
2. Find any `MMUIFollowMouse` components
3. Disable or remove them
4. Also disable/remove any `Reticle` GameObjects

**Alternative:** In weapon's `WeaponAim` component:
- Set `Reticle Type` to "None"
- Uncheck `Display Reticle`
- Set `Aim Control` to "Script"

### **"No weapon equipped"**
- Assign a weapon prefab to Hero's "Weapon Prefab" field
- Ensure weapon prefab has Weapon component

### **"Cannot start firing - no weapon equipped"**
- Check Hero has CharacterHandleWeapon ability
- Check weapon attached correctly in Awake/Start

### **"Target not found"**
- **Check console for detection logs:**
  - `[Hero] found 0 potential targets` = Nothing in range
  - `[Hero] found X potential targets` = Targets detected but layer might be wrong
- **Verify target layer is "Enemy":**
  - Select the TestTarget cube
  - Top of Inspector → Layer should say "Enemy"
- **Check Hero's TargetLayerMask includes "Enemy" layer:**
  - Select Hero
  - In Inspector → Hero component → Target Layer Mask
  - Make sure "Enemy" is checked
- **Ensure target is within DetectionRange:**
  - Select Hero in Scene view
  - Red sphere gizmo shows detection range
  - Target cube must be inside this sphere
  - Try increasing Detection Range to 30 or 50 for testing
- **Check target has a Collider component:**
  - Cube should have BoxCollider (enabled by default)
  - Collider must be enabled

### **"Projectiles not spawning"**
- Check weapon has ProjectileWeapon component
- Verify Projectile prefab is assigned to weapon
- Check weapon's SpawnPosition is configured

### **"Target not taking damage"**
- Ensure target has Health component
- Check projectile has DamageOnTouch component
- Verify layers allow collision (Physics collision matrix)

### **"Hero firing but projectiles missing"**
- Check weapon rotation/aim is correct
- Verify projectile speed is reasonable (10-30)
- Check projectile lifetime isn't too short

---

## 📊 Expected Console Output

```
[GridManager] Initialized grids: Passive(2x3), Active(1x3), Firing(2x3)
[Hero] Test Hero initialized
[Hero] Test Hero equipped weapon: TestWeapon
[TestTarget] Test Target initialized with 100 HP on layer Enemy
[Hero] Test Hero started firing with unlimited ammo
[Hero] Test Hero searching for targets in range 20m
[Hero] Target found at (0, 0.5, 5)
[TestTarget] Test Target HIT! HP: 90/100
[TestTarget] Test Target HIT! HP: 80/100
[TestTarget] Test Target HIT! HP: 70/100
...
[TestTarget] Test Target HIT! HP: 10/100
[TestTarget] Test Target DESTROYED!
[Hero] Test Hero searching for targets... (no target found)
```

---

## 🎯 Success Criteria

✅ Hero detects target within range  
✅ Hero aims weapon at target  
✅ Hero fires projectiles automatically  
✅ Projectiles hit target  
✅ Target takes damage  
✅ Target health decreases  
✅ Target destroyed when HP = 0  
✅ Ammo decreases with each shot (if limited)  
✅ Hero stops when out of ammo (if limited)  

---

## 🚀 Next Steps After Testing

Once hero firing works with TestTarget:

1. **Create proper Enemy base class**
2. **Add enemy AI (move toward base)**
3. **Add enemy attacks**
4. **Create multiple enemy types**
5. **Implement wave spawning**
6. **Test full combat loop**

---

**Note:** This is a minimal test setup. Full implementation will include:
- Proper weapon prefabs with effects
- Enemy AI with pathfinding
- Wave spawning system
- UI for health/ammo
- Combat feedback (particles, sounds)
