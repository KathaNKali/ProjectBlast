# ProjectBlast - AI Agent Instructions

This Unity project (Unity 6000.2.10f1) is built on **TopDown Engine v4.4** by MoreMountains, featuring 2D/3D top-down gameplay with the **Feel** haptic feedback system.

## Architecture Overview

### Core Systems

**Persistent Singleton Managers** - Essential game services that persist across scenes:
- `GameManager` (MMPersistentSingleton): Points/score, pause state, persistent characters, level entry/exit tracking
- `LevelManager` (MMSingleton): Character spawning, checkpoints, respawn, level bounds, scene loading
- `InputManager` (MMSingleton): Unified input handling for player controls
- `GridManager` (MMSingleton): Required for grid-based movement systems

Access via `GameManager.Instance`, `LevelManager.Instance`, etc.

### Character System

**Characters** are the foundation - both players and AI enemies:
- `Character` component is mandatory on all controllable entities
- Two types: `CharacterTypes.Player` (player-controlled) or `CharacterTypes.AI` (AI-controlled)
- `CharacterDimension`: Type2D (TopDownController2D) or Type3D (TopDownController3D)
- Uses **MMStateMachine** for state management: `MovementState` and `ConditionState`

**Character Abilities** - Modular capability system:
- All abilities extend `CharacterAbility` base class
- Attached as components to Character GameObjects
- Common abilities: `CharacterMovement`, `CharacterHandleWeapon`, `CharacterInventory`, `CharacterDash2D/3D`, `CharacterJump2D/3D`, `CharacterGridMovement`
- Abilities check `AbilityAuthorized` (considers `AbilityPermitted`, `BlockingMovementStates`, `BlockingConditionStates`, `BlockingWeaponStates`)
- Use `FindAbility<T>()` on Character to locate specific abilities

**State Management Pattern**:
```csharp
// Characters use MMStateMachine for state tracking
_movement.ChangeState(CharacterStates.MovementStates.Walking);
_condition.ChangeState(CharacterStates.CharacterConditions.Normal);

// Abilities access these via:
_movement = _character.MovementState;
_condition = _character.ConditionState;
```

### Weapon System

**Weapon Hierarchy**:
- Base class: `Weapon` (handles rate of fire, ammo, reloading, weapon states)
- `ProjectileWeapon` extends Weapon for projectile-based weapons (guns, launchers)
- `Projectile` class for spawned projectiles with DamageOnTouch
- `WeaponAim` for targeting logic
- `WeaponAmmo` for ammunition management

**Weapon Integration**:
- Characters use `CharacterHandleWeapon` ability to equip/use weapons
- Weapons attach to `WeaponAttachment` transform on character
- Weapons use their own `MMStateMachine<WeaponStates>` (WeaponIdle, WeaponUse, WeaponReload, etc.)

### AI System

**AI Architecture**:
- `AIBrain` component coordinates AI behavior
- `AIAction` classes (e.g., `AIActionShoot2D`, `AIActionShoot3D`) define specific behaviors
- AI actions pilot character abilities - e.g., AIActionShoot finds and controls `CharacterHandleWeapon`
- AI characters still use the same Character + Ability architecture as players

### Event System

**MMEventManager Pattern** - Decoupled communication:
```csharp
// Listening to events
public class MyClass : MMEventListener<TopDownEngineEvent>
{
    void OnMMEvent(TopDownEngineEvent engineEvent) {
        if (engineEvent.EventType == TopDownEngineEventTypes.PlayerDeath) { }
    }
}

// Triggering events
TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, character);
```

Common event types: `TopDownEngineEvent`, `MMGameEvent`, `MMInventoryEvent`, `MMCharacterEvent`, `MMCameraEvent`

### Feel Feedback System

**MMFeedbacks** - Visual, audio, and haptic feedback:
- `MMFeedbacks` components define sequences of feedback effects
- Attached throughout the codebase on abilities, weapons, UI, etc.
- Play via `myFeedback?.PlayFeedbacks(position)`
- Core to game feel - used for dash effects, weapon firing, UI interactions, damage feedback

## Key Conventions

**Namespace**: All TopDown Engine code lives in `MoreMountains.TopDownEngine`, utilities in `MoreMountains.Tools`

**Component Discovery**: Use `MMGetComponentNoAlloc<T>()` instead of GetComponent for performance

**Character Type Checks**:
```csharp
if (_character.CharacterType != Character.CharacterTypes.Player) return;
```

**Ability Pattern**:
- Override `Initialization()` for setup
- Override `HandleInput()` for input processing
- Override `ProcessAbility()` for per-frame logic
- Use `PlayAbilityStartFeedbacks()` and `PlayAbilityStopFeedbacks()`

**Animation Parameters**: Defined per-weapon in inspector, automatically set by CharacterHandleWeapon

## Project Structure

```
Assets/
├── TopDownEngine/
│   ├── Common/
│   │   ├── Scripts/
│   │   │   ├── Managers/          # GameManager, LevelManager, InputManager
│   │   │   ├── Characters/
│   │   │   │   ├── Core/          # Character, TopDownController
│   │   │   │   ├── CharacterAbilities/  # All ability classes
│   │   │   │   ├── AI/            # AI brain and actions
│   │   │   │   ├── Weapons/       # Weapon, ProjectileWeapon, Projectile
│   │   │   │   ├── Health/        # Health, DamageOnTouch
│   │   │   ├── Environment/       # Interactive objects, moving platforms
│   │   │   ├── GUI/               # UI managers and displays
│   │   │   ├── Items/             # Pickable items
│   │   ├── ScriptsInputSystem/    # New Input System integration
│   ├── Demos/                      # Demo scenes and assets
│   ├── ThirdParty/                 # DOTween, other helpers
├── Feel/
│   ├── MMFeedbacks/                # Feedback system
│   ├── MMTools/                    # Utility scripts
│   ├── NiceVibrations/             # Haptic feedback for mobile
├── Plugins/
│   ├── Febucci/                    # Text animation
│   ├── Demigiant/                  # DOTween
```

## Development Workflows

**Creating a New Ability**:
1. Extend `CharacterAbility`
2. Override `Initialization()` to cache components
3. Override `HandleInput()` for input checks
4. Override `ProcessAbility()` for per-frame updates
5. Add AddComponentMenu attribute: `[AddComponentMenu("TopDown Engine/Character/Abilities/My Ability")]`

**Adding Character Functionality**:
- Don't modify Character directly
- Create/configure abilities as components
- Use `Character.FindAbility<T>()` for cross-ability communication

**Scene Setup Requirements**:
- Every playable scene needs a GameManager (or reference to persistent one)
- Every level needs a LevelManager
- Player characters need InputManager (usually on GameManager)
- Grid-based games need GridManager

**Persistent Characters**: Use `GameManager.SetPersistentCharacter()` to maintain character across scene loads

## Common Patterns

**Triggering Ability Permissions**:
```csharp
character.FindAbility<CharacterDash2D>()?.PermitAbility(true);
```

**Weapon Switching**:
```csharp
var handleWeapon = character.FindAbility<CharacterHandleWeapon>();
handleWeapon?.ChangeWeapon(newWeapon, weaponID);
```

**Damage System**:
- `Health` component on damageable entities
- `DamageOnTouch` on damaging objects
- Health.Damage(amount, instigator, flickerDuration, invincibilityDuration)

## Testing & Debugging

**Demo Scenes**: TopDownEngine includes extensive demos - reference these for implementation patterns

**Console Commands**: GameManager supports pause/unpause, time control, character swapping

**State Visualization**: Enable `SendStateChangeEvents` on Character to see state changes in console

---

**Documentation**: 
- TopDown Engine: https://topdown-engine-docs.moremountains.com/
- Feel: https://feel-docs.moremountains.com/
- API: https://topdown-engine-docs.moremountains.com/API/
