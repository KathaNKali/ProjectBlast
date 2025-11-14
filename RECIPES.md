# 🔧 TopDown Engine - Common Recipes & Patterns

Quick reference for common tasks and patterns in TopDown Engine.

---

## 🎮 Character Recipes

### Recipe 1: Create a Fast Dash Character
```csharp
// On your player GameObject:
1. Add CharacterDash2D (or 3D)
2. Configure:
   - Dash Mode: MainMovement
   - Dash Distance: 5
   - Dash Duration: 0.15
   - Cooldown: 1 second
   - Invincible While Dashing: true
3. Add MMFeedbacks for dash effect
```

### Recipe 2: Create a Double-Jump Character
```csharp
// Add CharacterJump3D
CharacterJump3D jump = GetComponent<CharacterJump3D>();
jump.NumberOfJumps = 2; // Allow double jump
jump.JumpHeight = 3.5f;
```

### Recipe 3: Make Character Invincible Temporarily
```csharp
Health health = character.GetComponent<Health>();
health.Invulnerable = true;

// After 3 seconds:
StartCoroutine(RemoveInvincibility(3f));

IEnumerator RemoveInvincibility(float delay)
{
    yield return new WaitForSeconds(delay);
    health.Invulnerable = false;
}
```

### Recipe 4: Change Character Speed
```csharp
CharacterMovement movement = character.FindAbility<CharacterMovement>();
movement.WalkSpeed = 8f; // Faster
movement.RunSpeed = 12f;
```

---

## 🔫 Weapon Recipes

### Recipe 5: Create a Rapid-Fire Weapon
```csharp
// On ProjectileWeapon:
- TriggerMode: Auto
- TimeBetweenUses: 0.1 (10 shots/second)
- MagazineSize: 100
- ReloadTime: 2
```

### Recipe 6: Create a Shotgun (Multiple Projectiles)
```csharp
// On ProjectileWeapon:
- ProjectilesPerShot: 8
- Spread: 30 degrees
- ProjectileSpread: { min: -15, max: 15 }
```

### Recipe 7: Create Homing Missiles
```csharp
// On Projectile component:
public class HomingProjectile : Projectile
{
    public Transform Target;
    public float TurnSpeed = 5f;
    
    protected override void Movement()
    {
        if (Target != null)
        {
            Vector3 direction = (Target.position - transform.position).normalized;
            transform.up = Vector3.Lerp(transform.up, direction, TurnSpeed * Time.deltaTime);
        }
        base.Movement();
    }
}
```

### Recipe 8: Switch Weapons Programmatically
```csharp
CharacterHandleWeapon handleWeapon = character.FindAbility<CharacterHandleWeapon>();
handleWeapon.ChangeWeapon(newWeaponPrefab, "WeaponID");
```

---

## 🤖 AI Recipes

### Recipe 9: Create Patrol → Chase → Attack AI
```csharp
// On AI Enemy:
1. Add AIBrain
2. Add AIActionPatrol2D (set waypoints)
3. Add AIDecisionDetectTargetRadius2D
   - Detection Radius: 10
4. Add AIActionMoveTowardsTarget2D
5. Add AIActionShoot2D (when close enough)
6. Set up brain states in inspector
```

### Recipe 10: Make AI Flee When Low Health
```csharp
public class AIActionFleeWhenLowHealth : AIAction
{
    public float HealthThreshold = 30f;
    public float FleeSpeed = 8f;
    
    public override void PerformAction()
    {
        Health health = GetComponentInParent<Health>();
        if (health.CurrentHealth < HealthThreshold)
        {
            // Move away from target
            Vector3 fleeDirection = (transform.position - _brain.Target.position).normalized;
            _characterMovement.SetMovement(fleeDirection * FleeSpeed);
        }
    }
}
```

### Recipe 11: Create Boss with Multiple Phases
```csharp
public class BossAI : AIBrain
{
    public float Phase2HealthThreshold = 50f;
    private bool _inPhase2 = false;
    
    public override void Update()
    {
        base.Update();
        
        Health health = GetComponent<Health>();
        if (health.CurrentHealth <= Phase2HealthThreshold && !_inPhase2)
        {
            EnterPhase2();
        }
    }
    
    void EnterPhase2()
    {
        _inPhase2 = true;
        // Enable more aggressive AI actions
        // Change weapon
        // Spawn minions
    }
}
```

---

## 💥 Damage & Health Recipes

### Recipe 12: Create Damage Zones (Lava, Spikes)
```csharp
// Create GameObject with:
1. Collider (Is Trigger: true)
2. DamageOnTouch component
   - Damage Caused: 10
   - Damage Caused Every X Seconds: 0.5
3. Set to correct layer
```

### Recipe 13: Heal Player Over Time
```csharp
public class HealthRegeneration : MonoBehaviour
{
    public float RegenRate = 5f; // HP per second
    private Health _health;
    
    void Start()
    {
        _health = GetComponent<Health>();
    }
    
    void Update()
    {
        if (_health.CurrentHealth < _health.MaximumHealth)
        {
            _health.CurrentHealth += RegenRate * Time.deltaTime;
            _health.CurrentHealth = Mathf.Min(_health.CurrentHealth, _health.MaximumHealth);
        }
    }
}
```

### Recipe 14: Create Knockback on Hit
```csharp
// On weapon or projectile DamageOnTouch:
- Knockback Force: 5
- Direction: Based On Speed or Based On Owner Position
```

---

## 🎨 Feel & Feedback Recipes

### Recipe 15: Add Camera Shake on Damage
```csharp
// On Health component:
1. Create MMFeedbacks component
2. Add MMF_CameraShake feedback
3. Link to Health → Damage MM Feedbacks
```

### Recipe 16: Add Screen Flash on Player Hit
```csharp
// Create MMFeedbacks with:
1. MMF_Flash
   - Target: Main Camera
   - Color: Red
   - Duration: 0.1s
```

### Recipe 17: Add Particles on Enemy Death
```csharp
// On Health component:
1. Death MM Feedbacks → Add MMF_Particles
2. Set particle system prefab
3. Set position to character center
```

### Recipe 18: Add Haptic Feedback (Mobile)
```csharp
// In MMFeedbacks:
1. Add MMF_NiceVibration
2. Set Haptic Type: Medium Impact
3. Play on weapon fire or damage
```

---

## 🎯 Pickups & Items Recipes

### Recipe 19: Create Health Pickup
```csharp
// Create GameObject:
1. Add PickableHealth component
   - Health To Give: 25
2. Add visual (sprite/model)
3. Add trigger collider
4. Set layer to "Item"
```

### Recipe 20: Create Ammo Pickup
```csharp
// Create GameObject:
1. Add PickableAmmo
   - Ammo Amount: 30
   - Weapon ID: (match weapon)
2. Configure as item
```

### Recipe 21: Create Temporary Power-Up
```csharp
public class SpeedBoostPickup : PickableItem
{
    public float SpeedMultiplier = 2f;
    public float Duration = 5f;
    
    protected override void Pick(GameObject picker)
    {
        Character character = picker.GetComponent<Character>();
        CharacterMovement movement = character.FindAbility<CharacterMovement>();
        
        StartCoroutine(ApplySpeedBoost(movement));
    }
    
    IEnumerator ApplySpeedBoost(CharacterMovement movement)
    {
        float originalSpeed = movement.WalkSpeed;
        movement.WalkSpeed *= SpeedMultiplier;
        
        yield return new WaitForSeconds(Duration);
        
        movement.WalkSpeed = originalSpeed;
    }
}
```

---

## 🎬 Scene & Level Recipes

### Recipe 22: Load Next Level
```csharp
// On trigger or button:
public void LoadNextLevel()
{
    LevelManager.Instance.GotoLevel("Level02");
}
```

### Recipe 23: Create Checkpoint System
```csharp
// Use built-in CheckPoint component:
1. Create Empty → Add CheckPoint
2. Position in level
3. Configure respawn settings
4. Assign to LevelManager
```

### Recipe 24: Create Level Exit/Door
```csharp
// Create GameObject:
1. Add Collider (Is Trigger: true)
2. Add FinishLevel component
   - Level Name: "Level02"
3. Add visual (door sprite/model)
```

### Recipe 25: Create Persistent Character Across Scenes
```csharp
// In character selection or level start:
GameManager.Instance.SetPersistentCharacter(selectedCharacter);

// Character will automatically appear in next scene
// To clear:
GameManager.Instance.DestroyPersistentCharacter();
```

---

## 📊 UI & HUD Recipes

### Recipe 26: Display Health Bar
```csharp
// Use built-in MMHealthBar:
1. Add to Canvas
2. Link to player's Health component
3. Configure appearance
```

### Recipe 27: Show Ammo Count
```csharp
// Create UI Text:
1. Add to HUD Canvas
2. Update in script:

WeaponAmmo ammo = weapon.GetComponent<WeaponAmmo>();
ammoText.text = ammo.CurrentAmmoLoaded + " / " + ammo.MaxAmmo;
```

### Recipe 28: Create Pause Menu
```csharp
// Use built-in PauseManager:
1. Create Canvas with pause UI
2. Add buttons for Resume/Quit
3. Link buttons:
   - Resume: GameManager.Instance.UnPause()
   - Quit: LevelManager.Instance.GotoLevel("MainMenu")
```

---

## 🎮 Input Recipes

### Recipe 29: Add Custom Input Action
```csharp
// In custom ability:
protected override void HandleInput()
{
    if (_inputManager.InteractButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
    {
        PerformCustomAction();
    }
}
```

### Recipe 30: Support Gamepad Aim
```csharp
// On CharacterHandleWeapon:
- Weapon Aim Control: SecondaryThenPrimaryMovement
// Now right stick controls aim direction
```

---

## 🔧 Performance Recipes

### Recipe 31: Object Pooling for Projectiles
```csharp
// ProjectileWeapon automatically uses pooling!
// Just make sure to:
1. Use same prefab for all bullets
2. Don't modify hierarchy at runtime
3. Set appropriate Lifetime on projectiles
```

### Recipe 32: Optimize Many Enemies
```csharp
// On each enemy:
1. Disable SendStateChangeEvents on Character
2. Set RunAnimatorSanityChecks = false
3. Use simple colliders
4. Disable unnecessary abilities when far from player
```

---

## 🐛 Debugging Recipes

### Recipe 33: Visualize AI Detection Range
```csharp
// On AIDecisionDetectTargetRadius:
- Enable ShowGizmos in inspector
// Circles will show in Scene view
```

### Recipe 34: Log Character States
```csharp
// On Character:
- SendStateChangeEvents = true
// States will log to console

// Or in your code:
Debug.Log($"Movement: {character.MovementState.CurrentState}");
Debug.Log($"Condition: {character.ConditionState.CurrentState}");
```

### Recipe 35: Test Without Building
```csharp
// Use these shortcuts in Play mode:
- P: Pause game
- K: Kill player (test respawn)
- R: Reload scene
```

---

## 💡 Pro Tips

1. **Always Start Simple:** Get basic movement working before adding complex features
2. **Use Prefabs:** Create prefabs for everything (player, enemies, pickups)
3. **Study Demos:** The engine demos have solutions for most problems
4. **Layer Everything:** Use proper layers for collision optimization
5. **Test Early:** Press Play frequently to catch issues early
6. **Use MMFeedbacks:** They make your game feel 10x better with minimal effort
7. **Read the Docs:** The documentation is comprehensive and well-written
8. **Ask for Help:** The MoreMountains community is helpful

---

## 📚 Next Steps

**Learn More:**
- Check `QUICK_START.md` for your first level
- Follow `GAME_DEVELOPMENT_PLAN.md` for complete roadmap
- Study demo scenes in `Assets/TopDownEngine/Demos/`

**Advanced Topics:**
- Inventory system integration
- Save/Load system
- Multiplayer setup
- Custom abilities
- Advanced AI behaviors

**Resources:**
- Docs: https://topdown-engine-docs.moremountains.com/
- API: https://topdown-engine-docs.moremountains.com/API/
- Feel: https://feel-docs.moremountains.com/

---

🎮 **Happy Game Development!**
