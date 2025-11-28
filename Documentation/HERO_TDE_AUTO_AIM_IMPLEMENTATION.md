# ⚠️ DEPRECATED - Hero TDE Auto-Aim Implementation

> **WARNING:** This document describes an **obsolete implementation** that has been replaced.
> 
> **Current Implementation:** Heroes now use TDE's **AIBrain system** for combat.
> 
> **See:** [HERO_AIBRAIN_INTEGRATION.md](HERO_AIBRAIN_INTEGRATION.md) for current combat system documentation.
>
> **Last Updated:** November 28, 2025  
> **Status:** DEPRECATED - Kept for historical reference only

---

## Overview (OBSOLETE)

This document describes the old implementation where heroes used **WeaponAutoAim3D** and **WeaponAutoShoot** components that were instantiated at runtime. This approach was replaced with TDE's **AIBrain system** which provides better integration, Inspector configurability, and cleaner architecture.

**Old Approach (Deprecated):**
- WeaponAutoAim3D and WeaponAutoShoot added at runtime
- ConfigureWeaponAutoAim() method in Hero.cs
- Manual component management and cleanup

**New Approach (Current):**
- AIBrain state machine with AI actions/decisions
- All AI components configured in Unity Inspector
- ConfigureAI() method applies HeroDataSO stats to AI components
- Zone-based AIBrain activation/deactivation

---

## Historical Documentation

Heroes previously used **TopDown Engine's built-in auto-aim and auto-shoot system** with **line-of-sight checking** for automatic enemy targeting and firing.

## Flow: Hero Move to Firing Zone → Enemy in Range → Look at Enemy → Line-of-Sight → Shoot

### Complete Combat Flow

```
1. Hero deployed to Firing Zone
   ↓
2. Hero.StartFiring() called
   ↓
3. Weapon equipped with TDE components:
   • WeaponAutoAim3D
   • WeaponAutoShoot
   • WeaponAim3D
   ↓
4. WeaponAutoAim3D scans for enemies
   • Range: DetectionRange (from HeroDataSO)
   • LayerMask: TargetLayerMask (from HeroDataSO)
   • Interval: TargetSearchInterval (from HeroDataSO)
   ↓
5. Enemy detected in range
   ↓
6. WeaponAutoAim3D checks line-of-sight
   • Raycasts from weapon to target
   • ObstacleMask: ObstacleLayerMask (from HeroDataSO)
   • If blocked → target rejected
   • If clear → target acquired
   ↓
7. CharacterOrientation3D rotates hero body toward target
   ↓
8. WeaponAim3D rotates weapon to aim at target
   ↓
9. WeaponAutoShoot fires weapon
   • DelayBeforeShootAfterAcquiringTarget: 0.1s
   • Weapon fires according to fire rate
   ↓
10. Ammo consumed per shot
    ↓
11. Loop back to step 4 (continuous scanning/firing)
```

## TDE Components Used

### 1. WeaponAutoAim3D

**Purpose:** Automatically detects targets within range and verifies line-of-sight.

**Configuration (from HeroDataSO):**
```csharp
_weaponAutoAim.TargetsMask = TargetLayerMask;           // What to target
_weaponAutoAim.ObstacleMask = ObstacleLayerMask;        // What blocks LOS
_weaponAutoAim.ScanRadius = DetectionRange;             // How far to scan
_weaponAutoAim.DurationBetweenScans = TargetSearchInterval; // Scan frequency
_weaponAutoAim.RotationMode = WeaponAim.RotationModes.Free;
_weaponAutoAim.ApplyAutoAimAsLastDirection = true;
_weaponAutoAim.DrawDebugRadius = true;                  // Visual debugging
```

**Line-of-Sight Checking:**
- Automatically raycasts from weapon to target
- Uses `ObstacleMask` to detect blocking objects
- Only acquires targets with clear line-of-sight
- If LOS is blocked, target is rejected

### 2. WeaponAutoShoot

**Purpose:** Automatically fires weapon at acquired targets.

**Configuration:**
```csharp
_weaponAutoShoot.DelayBeforeShootAfterAcquiringTarget = 0.1f;
_weaponAutoShoot.OnlyAutoShootIfOwnerIsIdle = false;
```

**Behavior:**
- Waits 0.1s after target acquisition before first shot
- Fires continuously while target is valid
- Respects weapon fire rate
- Stops when target is lost or LOS blocked

### 3. WeaponAim3D

**Purpose:** Rotates weapon to precisely aim at target.

**Configuration:**
```csharp
weaponAim3D.Unrestricted3DAim = true;
weaponAim3D.AimCenter = WeaponAim3D.AimCenters.Weapon;
weaponAim3D.ReticleType = WeaponAim.ReticleTypes.None;
weaponAim3D.DisplayReticle = false;
```

### 4. CharacterOrientation3D

**Purpose:** Rotates hero body to face target direction.

**Configuration (should be on hero prefab):**
```csharp
RotationMode = WeaponDirection;
ShouldRotateToFaceWeaponDirection = true;
WeaponRotationSpeed = Smooth;
```

## Hero.cs Implementation

### Key Methods

#### StartFiring()
```csharp
public virtual void StartFiring()
{
    // Validation checks
    if (!IsInFiringZone) return;
    if (IsOutOfAmmo) return;
    
    // Equip weapon on entering Firing zone
    if (_currentWeapon == null && WeaponPrefab != null)
    {
        EquipWeapon(WeaponPrefab);
    }
    
    // Enable TDE auto-aim and auto-shoot
    _weaponAutoAim.enabled = true;
    _weaponAutoShoot.enabled = true;
}
```

#### ConfigureWeaponAutoAim()
```csharp
protected virtual void ConfigureWeaponAutoAim(Weapon weapon)
{
    // Add WeaponAutoAim3D
    _weaponAutoAim = weapon.gameObject.AddComponent<WeaponAutoAim3D>();
    
    // Configure from HeroDataSO
    _weaponAutoAim.TargetsMask = TargetLayerMask;
    _weaponAutoAim.ObstacleMask = ObstacleLayerMask;
    _weaponAutoAim.ScanRadius = DetectionRange;
    _weaponAutoAim.DurationBetweenScans = TargetSearchInterval;
    
    // Add WeaponAutoShoot
    _weaponAutoShoot = weapon.gameObject.AddComponent<WeaponAutoShoot>();
    _weaponAutoShoot.DelayBeforeShootAfterAcquiringTarget = 0.1f;
    
    // Start disabled (enabled when entering Firing zone)
    _weaponAutoAim.enabled = false;
    _weaponAutoShoot.enabled = false;
}
```

#### Ammo Tracking (Update Loop)
```csharp
void Update()
{
    // Track weapon state transitions for ammo consumption
    if (!UnlimitedAmmo && _currentWeapon != null)
    {
        Weapon.WeaponStates currentState = _currentWeapon.WeaponState.CurrentState;
        
        // Consume ammo only when weapon transitions to firing
        if (currentState == Weapon.WeaponStates.WeaponUse && 
            _lastWeaponState != Weapon.WeaponStates.WeaponUse)
        {
            ConsumeAmmo();
        }
        
        _lastWeaponState = currentState;
    }
}
```

## HeroDataSO Configuration

### Required Fields

```csharp
[Header("=== COMBAT STATS ===")]
public float DetectionRange = 20f;           // How far hero can detect enemies
public float TargetSearchInterval = 0.5f;    // How often to scan for targets
public float FireRate = 2f;                   // Shots per second
public LayerMask TargetLayerMask;            // Layers enemies are on
public LayerMask ObstacleLayerMask;          // Layers that block line-of-sight
```

### Example Configuration

**Ranger Hero:**
```
Detection Range: 20m
Target Search Interval: 0.5s
Fire Rate: 2 shots/sec
Target Layer Mask: Enemies
Obstacle Layer Mask: Obstacles, Walls
```

**Sniper Hero:**
```
Detection Range: 30m
Target Search Interval: 1.0s
Fire Rate: 0.5 shots/sec
Target Layer Mask: Enemies
Obstacle Layer Mask: Obstacles, Walls
```

## Line-of-Sight System

### How It Works

1. **WeaponAutoAim3D** performs Physics.Raycast from weapon to target
2. Raycast uses `ObstacleMask` LayerMask
3. If raycast hits obstacle before target → LOS blocked
4. If raycast hits target directly → LOS clear

### Setup Requirements

**Obstacle GameObjects:**
- Must be on layers included in `ObstacleLayerMask`
- Should have Colliders
- Examples: Walls, Pillars, Cover

**Enemy GameObjects:**
- Must be on layers included in `TargetLayerMask`
- Should have Colliders
- Examples: Enemy units, bosses

### Visual Debugging

**In Scene View:**
- Red wire sphere = Detection range
- Yellow line = Line to current target
- Cyan arrow = Weapon forward direction (where bullets go)

**Enable Debug Drawing:**
```csharp
_weaponAutoAim.DrawDebugRadius = true;
```

## Weapon Prefab Requirements

### Required Components

1. **ProjectileWeapon** (TopDown Engine)
   - Handles projectile spawning
   - Uses MMObjectPooler for bullets

2. **WeaponDataHolder** (ProjectBlast)
   - References WeaponDataSO
   - Provides damage and ammo consumption

3. **WeaponAutoAim3D** (Added at runtime)
   - Auto-configured by Hero.cs
   - Handles target detection and LOS

4. **WeaponAutoShoot** (Added at runtime)
   - Auto-configured by Hero.cs
   - Handles automatic firing

5. **WeaponAim3D** (TopDown Engine)
   - Should be on weapon prefab
   - Handles weapon rotation

### Weapon Hierarchy

```
WeaponPrefab (e.g., BasicRifle)
├── ProjectileWeapon         (TDE - already on prefab)
├── WeaponDataHolder          (ProjectBlast - already on prefab)
├── WeaponAim3D               (TDE - already on prefab)
├── MMObjectPooler            (TDE - already on prefab)
├── WeaponAutoAim3D           (Added by Hero.cs at runtime)
└── WeaponAutoShoot           (Added by Hero.cs at runtime)
```

## Character Prefab Requirements

### Required Components

1. **Character** (TopDown Engine)
   - CharacterDimension = **Type3D**
   - CharacterType = Player

2. **CharacterHandleWeapon** (TopDown Engine Ability)
   - Equips and manages weapons

3. **CharacterOrientation3D** (TopDown Engine Ability)
   - RotationMode = **WeaponDirection**
   - ShouldRotateToFaceWeaponDirection = **true**
   - Rotates hero body toward weapon aim

4. **Health** (TopDown Engine)
   - Handles damage and death

5. **Hero** (ProjectBlast)
   - References HeroDataSO
   - Configures auto-aim system

6. **Collider** (Unity)
   - For selection/interaction

### Hero Hierarchy

```
HeroGameObject (e.g., Ranger)
├── Character                    (TDE)
├── CharacterHandleWeapon        (TDE Ability)
├── CharacterOrientation3D       (TDE Ability)
├── Health                       (TDE)
├── Hero                         (ProjectBlast)
├── BoxCollider                  (Unity)
└── WeaponAttachment (Child)     (Empty Transform)
    └── Weapon (instantiated at runtime)
```

## Advantages Over Manual Implementation

### ✅ TDE Built-in Features

| Feature | Manual | TDE Auto-Aim |
|---------|--------|--------------|
| Target Detection | Custom Physics.OverlapSphere | Built-in with optimization |
| Line-of-Sight | Manual Raycast | Automatic with ObstacleMask |
| Weapon Rotation | Manual Quaternion.Slerp | Automatic smooth rotation |
| Character Rotation | Manual rotation code | CharacterOrientation3D handles it |
| Target Priority | Custom logic | Closest target automatically |
| Performance | Unknown | TDE optimized |
| Debug Visualization | Manual Gizmos | Built-in debug drawing |

### ✅ Benefits

1. **Less Code:** ~300 lines removed from Hero.cs
2. **Better Performance:** TDE's optimized scanning
3. **Proven System:** TDE's battle-tested auto-aim
4. **Easy Configuration:** All settings in HeroDataSO Inspector
5. **Line-of-Sight:** Built-in raycast checking
6. **Visual Debugging:** Built-in debug sphere drawing
7. **Maintainable:** Standard TDE components

## Testing Checklist

### Scene Setup

- [ ] Hero prefab has Character (Type3D)
- [ ] Hero prefab has CharacterOrientation3D
- [ ] Hero prefab has CharacterHandleWeapon
- [ ] Hero prefab has Health
- [ ] Hero prefab has Hero component
- [ ] Hero has HeroDataSO assigned

### HeroDataSO Setup

- [ ] Detection Range set (e.g., 20m)
- [ ] Target Layer Mask includes enemy layer
- [ ] Obstacle Layer Mask includes obstacle layers
- [ ] Target Search Interval set (e.g., 0.5s)
- [ ] Fire Rate set (e.g., 2 shots/sec)
- [ ] Weapon Prefab assigned

### Weapon Prefab Setup

- [ ] Has ProjectileWeapon component
- [ ] Has WeaponDataHolder with WeaponDataSO
- [ ] Has WeaponAim3D component
- [ ] Has MMObjectPooler with projectile
- [ ] Weapon forward axis points in firing direction

### Test Enemies Setup

- [ ] On layer included in TargetLayerMask
- [ ] Has Collider
- [ ] Has Health component (optional)

### Obstacle Setup

- [ ] On layer included in ObstacleLayerMask
- [ ] Has Collider
- [ ] Positioned between hero and some enemies

### Expected Behavior

1. ✅ Hero equipped with weapon when entering Firing zone
2. ✅ Red wire sphere visible in Scene view (detection range)
3. ✅ Hero body rotates to face nearby enemies
4. ✅ Weapon rotates to aim at target
5. ✅ Hero shoots enemies with clear line-of-sight
6. ✅ Hero does NOT shoot enemies blocked by obstacles
7. ✅ Yellow line drawn to current target
8. ✅ Ammo decreases with each shot
9. ✅ Hero stops firing when out of ammo

## Troubleshooting

### "Hero not detecting enemies"
- Check: TargetLayerMask includes enemy layer
- Check: DetectionRange is large enough
- Check: Enemies have Colliders

### "Hero shoots through walls"
- Check: ObstacleLayerMask includes wall/obstacle layers
- Check: Obstacles have Colliders
- Check: WeaponAutoAim3D enabled

### "Hero not rotating toward enemies"
- Check: CharacterOrientation3D exists on hero
- Check: RotationMode = WeaponDirection
- Check: ShouldRotateToFaceWeaponDirection = true

### "Weapon not aiming at target"
- Check: WeaponAim3D exists on weapon prefab
- Check: Unrestricted3DAim = true
- Check: Weapon forward axis (blue) points forward

### "Bullets go wrong direction"
- Check: Character.CharacterDimension = **Type3D** (NOT Type2D)
- Check: Weapon transform.forward points in firing direction

### "Hero shoots too fast/slow"
- Check: Weapon.TimeBetweenUses (fire rate cooldown)
- Check: WeaponDataSO.FireRate doesn't affect actual firing
- Fire rate is controlled by weapon's internal timer

## Summary

**Before:** Manual targeting with ~800 lines of custom code

**Now:** TDE's proven auto-aim system with ~400 lines of configuration code

**Result:** 
- ✅ Automatic enemy detection
- ✅ Line-of-sight checking built-in
- ✅ Smooth character and weapon rotation
- ✅ Optimized performance
- ✅ Easy to configure
- ✅ Visual debugging tools
- ✅ Works with TDE's complete system

**The hero will now:**
1. Detect enemies in range
2. Check line-of-sight (raycasts)
3. Rotate body toward target
4. Aim weapon at target
5. Shoot automatically if LOS is clear
6. Consume ammo per shot
7. Stop when ammo depleted

All controlled by HeroDataSO! 🎯
