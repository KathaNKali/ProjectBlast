# TDE 3D Character Orientation & Weapon Aiming - Summary

## Overview

Your issue: **Bullets are traveling in X-axis of weapon instead of Z-axis (forward).**

This happens because of how TDE handles 3D vs 2D character orientation and weapon aim direction.

---

## TDE Architecture Summary

### 1. Character Orientation System

**CharacterOrientation3D** (Ability Component)
- Controls how 3D characters rotate to face directions
- Has 3 rotation modes:
  - **None**: No automatic rotation
  - **MovementDirection**: Face where moving
  - **WeaponDirection**: Face where weapon aims
  - **Both**: Rotate to movement OR weapon direction

**Key Properties:**
```csharp
public enum RotationModes { 
    None, 
    MovementDirection, 
    WeaponDirection, 
    Both 
}

public RotationModes RotationMode = RotationModes.None;
public bool ShouldRotateToFaceWeaponDirection = true;
public GameObject WeaponRotatingModel; // What rotates (usually character model)
```

### 2. Weapon Aim System

**WeaponAim** (Component on Weapon)
- Controls weapon rotation/aiming
- Multiple control modes:
  - **Off**: No aiming
  - **PrimaryMovement**: Aim based on movement input
  - **SecondaryMovement**: Aim based on second input axis (twin-stick)
  - **Mouse**: Aim toward mouse cursor
  - **Script**: External script controls aim (YOUR CASE)

**Key Properties:**
```csharp
public enum AimControls { 
    Off, 
    PrimaryMovement, 
    SecondaryMovement, 
    Mouse, 
    Script 
}

public AimControls AimControl = AimControls.SecondaryMovement;
public Vector3 CurrentAim { get; } // Current aim direction
```

### 3. Projectile Direction

**ProjectileWeapon.SpawnProjectile()** (line 190-195):
```csharp
if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
{
    // 3D: Uses transform.forward (Z-axis)
    projectile.SetDirection(spread * transform.forward, transform.rotation, true);
}
else // 2D
{
    // 2D: Uses transform.right (X-axis) with flip
    Vector3 newDirection = (spread * transform.right) * (Flipped ? -1 : 1);
    projectile.SetDirection(newDirection, spread * transform.rotation, true);
}
```

**This is the KEY:**
- **3D characters**: Projectiles fire in `transform.forward` (Z-axis)
- **2D characters**: Projectiles fire in `transform.right` (X-axis)

---

## Your Current Setup Problem

### What You Have Now (Hero.cs)

```csharp
// In AimAtTarget():
Vector3 directionToTarget = (target.position - _currentWeapon.transform.position).normalized;

if (directionToTarget != Vector3.zero)
{
    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
    _currentWeapon.transform.rotation = Quaternion.Slerp(
        _currentWeapon.transform.rotation,
        targetRotation,
        Time.deltaTime * 10f
    );
}
```

**Problem:** You're rotating the weapon correctly with `Quaternion.LookRotation`, BUT:
- If your Character is set to **Type2D**, TDE will use `transform.right` (X-axis)
- If your weapon's local axes are misaligned, bullets go wrong direction

---

## Solution: Ensure 3D Character Setup

### Required Setup

1. **Character Component Settings:**
```csharp
Character.CharacterDimension = Character.CharacterDimensions.Type3D;
```

2. **Add CharacterOrientation3D Ability:**
```csharp
// On your hero prefab, add:
CharacterOrientation3D orientation = gameObject.AddComponent<CharacterOrientation3D>();
orientation.RotationMode = CharacterOrientation3D.RotationModes.WeaponDirection;
orientation.ShouldRotateToFaceWeaponDirection = true;
orientation.WeaponRotationSpeed = CharacterOrientation3D.RotationSpeeds.Smooth;
```

3. **WeaponAim Setup (you already have this):**
```csharp
weaponAim.AimControl = WeaponAim.AimControls.Script;
```

4. **Weapon Prefab Orientation:**
- Weapon's **local forward (blue arrow)** should point in firing direction
- When weapon is child of WeaponAttachment, its Z-axis = bullet direction

### Correct Workflow

```
Character (3D)
    └─> CharacterOrientation3D (rotates whole character to face weapon aim)
    └─> CharacterHandleWeapon
            └─> WeaponAttachment (Transform)
                    └─> Weapon (ProjectileWeapon)
                            • WeaponAim: Script mode
                            • transform.forward = firing direction
                            • SpawnProjectile uses transform.forward
```

---

## Code Fix for Hero.cs

### Option A: Rotate Character Instead of Just Weapon

Let TDE handle rotation properly:

```csharp
protected virtual void AimAtTarget(Transform target)
{
    if (_currentWeapon == null) return;
    
    // Calculate direction to target
    Vector3 directionToTarget = (target.position - transform.position).normalized;
    
    // For 3D characters, use CharacterOrientation3D
    if (Character != null && Character.CharacterDimension == Character.CharacterDimensions.Type3D)
    {
        var orientation3D = Character.FindAbility<CharacterOrientation3D>();
        if (orientation3D != null)
        {
            // Tell CharacterOrientation3D to face this direction
            orientation3D.ForcedRotation = true;
            orientation3D.ForcedRotationDirection = directionToTarget;
        }
    }
    
    // Also manually rotate weapon for immediate aim
    if (directionToTarget != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _currentWeapon.transform.rotation = Quaternion.Slerp(
            _currentWeapon.transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );
    }
}

// Remember to reset ForcedRotation when stopping fire
public virtual void StopFiring()
{
    if (!_isFiring) return;
    
    _isFiring = false;
    _currentTarget = null;
    
    // Reset forced rotation
    if (Character != null)
    {
        var orientation3D = Character.FindAbility<CharacterOrientation3D>();
        if (orientation3D != null)
        {
            orientation3D.ForcedRotation = false;
        }
    }
    
    if (HandleWeapon != null)
    {
        HandleWeapon.ShootStop();
    }
    
    Debug.Log($"[Hero] {HeroName} stopped firing.");
}
```

### Option B: Use WeaponAim's CurrentAim Property

Let WeaponAim handle the direction:

```csharp
protected virtual void AimAtTarget(Transform target)
{
    if (_currentWeapon == null) return;
    
    WeaponAim weaponAim = _currentWeapon.GetComponent<WeaponAim>();
    if (weaponAim == null) return;
    
    // Calculate direction to target
    Vector3 directionToTarget = (target.position - _currentWeapon.transform.position).normalized;
    
    // Set weapon aim direction
    if (directionToTarget != Vector3.zero)
    {
        // Rotate weapon transform
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        _currentWeapon.transform.rotation = Quaternion.Slerp(
            _currentWeapon.transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );
        
        // WeaponAim will read from weapon's transform.forward
        // when AimControl is set to Script mode
    }
}
```

---

## Debugging Checklist

### Check These Settings:

1. **Character Component:**
   - [ ] CharacterDimension = **Type3D** (NOT Type2D)
   - [ ] CharacterType = Player (or AI)

2. **CharacterOrientation3D Ability:**
   - [ ] Component exists on hero
   - [ ] RotationMode = **WeaponDirection** or **Both**
   - [ ] ShouldRotateToFaceWeaponDirection = **true**

3. **Weapon Prefab:**
   - [ ] Local forward axis (blue) points in firing direction
   - [ ] WeaponAim.AimControl = **Script**
   - [ ] ProjectileWeapon exists

4. **Weapon Hierarchy:**
   ```
   Hero GameObject
   └─ WeaponAttachment (Transform)
      └─ WeaponPrefab (instantiated)
         └─ Spawn Point (child)
   ```

5. **In Play Mode - Check Transform:**
   - Select weapon in hierarchy
   - Look at blue arrow (forward) in Scene view
   - It should point at target when aiming
   - If it points sideways, that's your issue

---

## Visual Debug

Add this to your Hero.cs to visualize firing direction:

```csharp
void OnDrawGizmos()
{
    // Draw detection range
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, DetectionRange);
    
    // Draw weapon forward direction (firing direction)
    if (_currentWeapon != null)
    {
        Gizmos.color = Color.cyan;
        Vector3 fireDir = _currentWeapon.transform.forward;
        Gizmos.DrawRay(_currentWeapon.transform.position, fireDir * 5f);
        
        // Draw weapon right direction for comparison
        Gizmos.color = Color.red;
        Vector3 rightDir = _currentWeapon.transform.right;
        Gizmos.DrawRay(_currentWeapon.transform.position, rightDir * 3f);
    }
    
    // Draw line to current target
    if (_currentTarget != null && _isFiring)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _currentTarget.position);
    }
}
```

**What to look for:**
- **Cyan arrow** (forward) should point at target
- **Red arrow** (right) should be perpendicular
- If red arrow points at target = you're in 2D mode!

---

## Quick Fix Summary

**Most Likely Issue:** Character is set to **Type2D** instead of **Type3D**

**Quick Fix:**
1. Select your hero prefab
2. Find **Character** component
3. Set **Character Dimension** = `Type3D`
4. Add **CharacterOrientation3D** component
5. Set **Rotation Mode** = `WeaponDirection`
6. Test - bullets should now fire forward (Z-axis)

---

## TDE Design Philosophy

TDE separates:
- **Character rotation** (CharacterOrientation3D) - rotates whole character
- **Weapon rotation** (WeaponAim) - rotates weapon independently
- **Projectile direction** (ProjectileWeapon) - reads weapon's forward

For your tower defense heroes:
- Heroes stand still (no movement rotation)
- Only weapon rotates to track enemies
- Use **WeaponDirection** mode so character body faces weapon aim
- ProjectileWeapon automatically uses `transform.forward` in 3D mode

**Result:** Clean, working auto-targeting that respects TDE's architecture! 🎯
