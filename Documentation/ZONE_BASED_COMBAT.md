# Zone-Based Combat System

## Architecture Overview

Heroes use **AIBrain state control** to activate/deactivate combat behavior based on grid zones.

### Key Principle
- **Weapon**: Always equipped (on initialization)
- **AIBrain**: Controls when hero can shoot (zone-based)

## How It Works

### 1. **Hero Initialization** (Awake/Start)
```
Hero spawned → Weapon equipped immediately → AIBrain.BrainActive = false
```

**Why equip weapon early?**
- Avoids timing issues (weapon instantiation takes 1 frame)
- AIBrain can activate instantly when entering Firing zone
- No null reference errors in AIActionShoot3D

### 2. **Zone-Based Behavior**

| Zone | AIBrain State | Behavior |
|------|---------------|----------|
| **Passive** | `BrainActive = false` | Hero idle, no combat |
| **Active** | `BrainActive = false` | Hero idle, no combat |
| **Firing** | `BrainActive = true` | Hero scans, aims, shoots |

### 3. **Entering Firing Zone**
```csharp
OnZoneChanged(oldZone: Active, newZone: Firing)
  → StartFiring()
    → AIBrain.BrainActive = true
    → AIBrain.TransitionToState("Combat")
    → AIBrain starts executing AI states:
       - Detect enemies (AIDecisionDetectTargetConeOfVision3D)
       - Aim at target (AIActionAimWeaponAtTarget3D)
       - Shoot weapon (AIActionShoot3D)
```

### 4. **Leaving Firing Zone**
```csharp
OnZoneChanged(oldZone: Firing, newZone: Active)
  → StopFiring()
    → AIBrain.BrainActive = false
    → AIBrain.TransitionToState("Idle" or "Inactive")
    → HandleWeapon.ShootStop()
    → Weapon stays equipped (ready for re-entry)
```

## Code Flow

### Hero.cs
```csharp
// 1. Initialization
InitializeWeaponSystem()
{
    EquipWeapon(WeaponPrefab); // Equip immediately
    AIBrain.BrainActive = false; // Keep inactive
}

// 2. Zone detection
CurrentGridSlot.set
{
    if (newZone != oldZone)
        OnZoneChanged(oldZone, newZone);
}

// 3. Zone change handler
OnZoneChanged(oldZone, newZone)
{
    if (newZone == GridZone.Firing)
        StartFiring();
    else if (oldZone == GridZone.Firing)
        StopFiring();
}

// 4. Combat control
StartFiring()
{
    AIBrain.BrainActive = true; // Activate AI
    AIBrain.TransitionToState("Combat");
}

StopFiring()
{
    AIBrain.BrainActive = false; // Deactivate AI
    HandleWeapon.ShootStop();
}
```

### AIActionShoot3D.cs (Modified)
```csharp
// Safety checks prevent null reference errors
PerformAction()
{
    if (TargetHandleWeaponAbility == null || 
        TargetHandleWeaponAbility.CurrentWeapon == null)
    {
        return; // Don't shoot if no weapon
    }
    
    MakeChangesToTheWeapon();
    TestAimAtTarget();
    Shoot();
}

OnEnterState()
{
    if (CurrentWeapon != null)
    {
        _weaponAim = CurrentWeapon.GetComponent<WeaponAim>();
        _projectileWeapon = CurrentWeapon.GetComponent<ProjectileWeapon>();
    }
    else
    {
        Debug.LogWarning("Weapon not equipped yet!");
    }
}
```

## Benefits

### ✅ Clean Separation
- **Weapon system**: Handles equipping/shooting mechanics
- **AIBrain system**: Handles combat behavior/decisions
- **Hero.cs**: Orchestrates both based on zone

### ✅ Performance
- AI only runs in Firing zone (saves CPU in Passive/Active)
- No unnecessary target detection when hero can't shoot
- No wasted state transitions

### ✅ Reliability
- Weapon always equipped and ready
- No timing issues with async weapon instantiation
- Null checks prevent crashes

### ✅ Flexibility
- Easy to add zone-specific behaviors
- AI states fully customizable in Inspector
- Can add "Idle" state for animations in non-Firing zones

## Safety Mechanisms

### 1. **Triple-Layer Protection**
```csharp
// Layer 1: Awake() - Disable on spawn
AIBrain.BrainActive = false;

// Layer 2: OnZoneChanged() - Zone-based control
if (newZone == Firing) StartFiring();
else StopFiring();

// Layer 3: Update() - Runtime check
if (AIBrain.BrainActive && !IsInFiringZone)
    StopFiring(); // Emergency stop
```

### 2. **Null Safety in AIActionShoot3D**
- `PerformAction()`: Checks weapon exists before shooting
- `OnEnterState()`: Checks weapon exists before caching components
- Prevents crashes if weapon somehow missing

### 3. **Coroutine Fallback**
```csharp
// If weapon not equipped on StartFiring() (shouldn't happen)
if (_currentWeapon == null)
{
    EquipWeapon(WeaponPrefab);
    StartCoroutine(ActivateAIAfterWeaponEquip()); // Wait for equip
    return;
}
```

## Configuration

### Inspector Setup (Hero Prefab)
1. **Root GameObject**:
   - `Character` (Type3D, AI)
   - `Hero.cs` (HeroDataSO assigned)
   - `CharacterHandleWeapon`
   - `Health`

2. **AIBrain Child**:
   - `AIBrain` (BrainActive = false initially)
   - AI States: Seeking, WaitToShoot, Destroying, BackToSeeking
   - AI Actions: AIActionShoot3D, AIActionAimWeaponAtTarget3D
   - AI Decisions: DetectTargetConeOfVision3D, LineOfSightToTarget3D

3. **Abilities Child**:
   - `CharacterOrientation3D`
   - `CharacterConeOfVision` + `MMConeOfVision`
   - `CharacterHandleWeapon`

### HeroDataSO
```
DefaultWeaponPrefab: Weapon prefab reference
DetectionRange: 17.88
TargetLayerMask: Enemy
StartingAmmo: 100
UnlimitedAmmo: false
```

## Testing Checklist

- [ ] Hero spawns with weapon equipped
- [ ] AIBrain inactive in Passive zone
- [ ] AIBrain inactive in Active zone
- [ ] AIBrain activates when moved to Firing zone
- [ ] Hero shoots at enemies in Firing zone
- [ ] AIBrain deactivates when moved out of Firing zone
- [ ] No null reference errors in console
- [ ] Weapon stays equipped across zone changes
- [ ] Can re-enter Firing zone and resume shooting

## Troubleshooting

### "NullReferenceException in AIActionShoot3D"
**Cause**: Weapon not equipped when AIBrain activated  
**Fix**: Weapon now equipped on initialization (before AIBrain activation)

### "Hero shooting in Passive/Active zone"
**Cause**: AIBrain.BrainActive = true outside Firing zone  
**Fix**: Triple-layer protection ensures AIBrain only active in Firing zone

### "Weapon equipping delay"
**Cause**: TDE's ChangeWeapon() instantiates async (takes 1 frame)  
**Fix**: Weapon equipped in InitializeWeaponSystem(), AIBrain waits for ready weapon

### "AI not shooting in Firing zone"
**Check**:
1. AIBrain.BrainActive = true? (Should be)
2. CurrentWeapon != null? (Should be)
3. Target detected? (Check cone of vision radius/angle)
4. Line of sight clear? (Check obstacles)

---

**Last Updated**: December 3, 2025  
**Related Docs**: `HERO_AIBRAIN_INTEGRATION.md`, `GRID_SYSTEM.md`
