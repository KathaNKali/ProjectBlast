# 🚀 Homing/Curved Projectile Implementation - Design Discussion

## 📊 Current TDE Architecture

### **Available Projectile Types:**
TopDown Engine provides several projectile classes out of the box:

1. **`Projectile`** (base class) - Straight-line movement
2. **`PhysicsProjectile`** - Physics-based (uses Rigidbody force)
3. **`BouncyProjectile`** - Bounces off walls
4. **`ThrownObject`** - Arc trajectory (grenades)
5. **`PathedProjectile`** - Moves to fixed destination

### **Key Movement System:**
```csharp
public virtual void Movement()
{
    _movement = Direction * (Speed / 10) * Time.deltaTime;
    
    if (_rigidBody != null) {
        _rigidBody.MovePosition(this.transform.position + _movement);
    }
    if (_rigidBody2D != null) {
        _rigidBody2D.MovePosition(this.transform.position + _movement);
    }
    
    Speed += Acceleration * Time.deltaTime;
}
```

The `Movement()` method is **virtual**, meaning we can override it in custom subclasses!

---

## ✅ YES, Homing Missiles Are Possible!

TopDown Engine fully supports homing projectiles through **class extension**. The RECIPES.md even has an example!

---

## 🎯 Design Options

### **Option 1: Simple Homing (Lerp-Based) ⭐ RECOMMENDED**

**Approach:** Override `Movement()` to gradually rotate toward target using `Vector3.Lerp`

**Implementation:**
```csharp
public class HomingProjectile : Projectile
{
    [Header("Homing Behavior")]
    public Transform Target;               // Set by AIActionShoot3D via AIBrain.Target
    public float TurnSpeed = 5f;          // How quickly it curves (degrees/sec)
    public float HomingDuration = 3f;     // Stop homing after X seconds
    
    protected float _timeAlive = 0f;
    
    public override void Movement()
    {
        _timeAlive += Time.deltaTime;
        
        // Homing phase
        if (Target != null && _timeAlive < HomingDuration)
        {
            Vector3 targetDirection = (Target.position - transform.position).normalized;
            
            // Smoothly rotate Direction toward target
            Direction = Vector3.Lerp(Direction, targetDirection, TurnSpeed * Time.deltaTime);
            
            // Rotate visual to face movement (for 2D)
            transform.right = Direction; // Or transform.up for different sprite orientation
        }
        
        // Standard movement (straight if no target, curved if target exists)
        base.Movement();
    }
    
    protected override void Initialization()
    {
        base.Initialization();
        _timeAlive = 0f;
    }
}
```

**Pros:**
- ✅ Simple to implement (~25 lines)
- ✅ Smooth curves, looks natural
- ✅ Works with existing TDE weapon system
- ✅ Configurable turn speed (gentle curve vs aggressive tracking)
- ✅ No complex math required

**Cons:**
- ❌ Not realistic physics (doesn't account for inertia)
- ❌ Can do 180° turns if TurnSpeed too high

**Visual Result:**  
Smooth, gradual curve toward target - like a guided missile with thrusters adjusting course.

---

### **Option 2: Physics-Based Homing (AddForce)**

**Approach:** Extend `PhysicsProjectile` and apply forces toward target

**Implementation:**
```csharp
public class PhysicsHomingProjectile : PhysicsProjectile
{
    [Header("Homing Behavior")]
    public Transform Target;
    public float HomingForce = 50f;       // Force magnitude
    public float MaxTurnRate = 180f;      // Max degrees/sec
    public float HomingDuration = 3f;
    
    protected float _timeAlive = 0f;
    
    public override void Movement()
    {
        _timeAlive += Time.deltaTime;
        
        if (Target != null && _timeAlive < HomingDuration)
        {
            Vector3 targetDirection = (Target.position - transform.position).normalized;
            
            // Apply steering force
            if (_rigidBody != null)
            {
                Vector3 steerForce = targetDirection * HomingForce;
                _rigidBody.AddForce(steerForce, ForceMode.Force);
                
                // Clamp velocity
                if (_rigidBody.velocity.magnitude > Speed)
                {
                    _rigidBody.velocity = _rigidBody.velocity.normalized * Speed;
                }
            }
            
            if (_rigidBody2D != null)
            {
                Vector2 steerForce = targetDirection * HomingForce;
                _rigidBody2D.AddForce(steerForce, ForceMode2D.Force);
                
                if (_rigidBody2D.velocity.magnitude > Speed)
                {
                    _rigidBody2D.velocity = _rigidBody2D.velocity.normalized * Speed;
                }
            }
            
            // Rotate visual
            transform.right = _rigidBody2D.velocity.normalized;
        }
        
        base.Movement(); // Handles physics movement
    }
    
    protected override void Initialization()
    {
        base.Initialization();
        _timeAlive = 0f;
    }
}
```

**Pros:**
- ✅ Realistic physics (momentum, inertia)
- ✅ Natural-looking arc trajectories
- ✅ Can't do instant 180° turns

**Cons:**
- ❌ More complex to tune (force values, drag, max velocity)
- ❌ Requires Rigidbody (slight performance cost)
- ❌ Might overshoot or orbit target if not tuned

**Visual Result:**  
Realistic missile with inertia - takes time to change direction, wide arcs.

---

### **Option 3: Bezier Curve Prediction (Advanced)**

**Approach:** Calculate a Bezier curve from spawn point to predicted target position

**Implementation:**
```csharp
public class BezierHomingProjectile : Projectile
{
    [Header("Homing Behavior")]
    public Transform Target;
    public float CurveHeight = 2f;        // How high the arc goes
    public float PredictionTime = 1f;     // Predict target position X seconds ahead
    
    protected Vector3 _startPosition;
    protected Vector3 _predictedTargetPosition;
    protected Vector3 _controlPoint;
    protected float _journeyTime = 0f;
    protected float _totalDistance;
    
    public override void Movement()
    {
        if (Target == null)
        {
            base.Movement();
            return;
        }
        
        _journeyTime += Time.deltaTime;
        float t = (_journeyTime * Speed) / _totalDistance;
        
        if (t >= 1f)
        {
            base.Movement(); // Fly straight after reaching end
            return;
        }
        
        // Quadratic Bezier: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        Vector3 newPosition = Mathf.Pow(1 - t, 2) * _startPosition +
                              2 * (1 - t) * t * _controlPoint +
                              Mathf.Pow(t, 2) * _predictedTargetPosition;
        
        // Update position and rotation
        Direction = (newPosition - transform.position).normalized;
        
        if (_rigidBody != null) _rigidBody.MovePosition(newPosition);
        else if (_rigidBody2D != null) _rigidBody2D.MovePosition(newPosition);
        else transform.position = newPosition;
        
        transform.right = Direction;
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (Target != null)
        {
            _startPosition = transform.position;
            
            // Predict target position
            Rigidbody targetRb = Target.GetComponent<Rigidbody>();
            Vector3 targetVelocity = targetRb != null ? targetRb.velocity : Vector3.zero;
            _predictedTargetPosition = Target.position + targetVelocity * PredictionTime;
            
            // Calculate control point (arc peak)
            Vector3 midPoint = (_startPosition + _predictedTargetPosition) / 2f;
            _controlPoint = midPoint + Vector3.up * CurveHeight;
            
            _totalDistance = Vector3.Distance(_startPosition, _predictedTargetPosition);
            _journeyTime = 0f;
        }
    }
}
```

**Pros:**
- ✅ Predictable arc (good for visual telegraphing)
- ✅ Looks cinematic (mortar-like trajectory)
- ✅ Performance-friendly (calculated once at spawn)

**Cons:**
- ❌ Can't adjust mid-flight if target moves
- ❌ More complex math
- ❌ Requires target velocity prediction

**Visual Result:**  
Elegant parabolic arc, like a heat-seeking artillery shell.

---

## 🎮 Integration with Your Current System

### **How Target Assignment Works:**

Your existing setup:
```
AIBrain.Target (set by AIDecisionDetectHeroOrWall)
  ↓
AIActionShoot3D shoots projectile
  ↓
ProjectileWeapon spawns Projectile
  ↓
??? HOW TO PASS TARGET TO PROJECTILE ???
```

### **Solution: Custom ProjectileWeapon or SetTarget Method**

#### **Method A: Extend ProjectileWeapon (Cleaner)**
```csharp
public class HomingProjectileWeapon : ProjectileWeapon
{
    protected override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
    {
        GameObject projectile = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);
        
        // Set target from weapon aim
        HomingProjectile homingComp = projectile.GetComponent<HomingProjectile>();
        if (homingComp != null && Owner != null)
        {
            // Get AIBrain from owner character
            AIBrain brain = Owner.GetComponentInParent<AIBrain>();
            if (brain != null && brain.Target != null)
            {
                homingComp.Target = brain.Target;
            }
        }
        
        return projectile;
    }
}
```

#### **Method B: Public SetTarget Method (Simpler)**
```csharp
// In HomingProjectile class
public virtual void SetTarget(Transform target)
{
    Target = target;
}

// In HomingProjectileWeapon (or modify LaneSpawner)
// After spawning projectile:
homingProjectile.SetTarget(AIBrain.Target);
```

---

## 📋 Recommendation: Option 1 with Method B

**Why:**
1. **Simplest implementation** (~30 lines total)
2. **Looks great** - smooth curves, not robotic
3. **Easy to tune** - single TurnSpeed parameter
4. **No physics overhead** - works with standard Projectile
5. **Compatible with pooling** - no complex state management

**Implementation Steps:**
1. Create `HomingProjectile.cs` extending `Projectile`
2. Override `Movement()` to lerp Direction toward Target
3. Add `SetTarget()` method
4. Modify `LaneSpawner.ApplyEnemyData()` or create `HomingProjectileWeapon`
5. Set Target from `AIBrain.Target` after spawn
6. Test with different TurnSpeed values (3-10 recommended)

---

## 🎨 Visual Comparison

### **Straight Projectile (Current):**
```
Enemy ----→----→----→----→ Hero
```

### **Lerp Homing (Option 1):**
```
Enemy ----→---→--↘-↘--↓ Hero
                        ↓
                     (curves smoothly)
```

### **Physics Homing (Option 2):**
```
Enemy ----→---→-→↘-↘-↓-↙-← Hero
                     ↓
              (wide arc, overshoots)
```

### **Bezier (Option 3):**
```
Enemy ----→--↗--⤴--↘--↓ Hero
           /      \
         (perfect arc, calculated)
```

---

## 🔧 Tuning Parameters

### **Gentle Tracking (Slow Curve):**
- TurnSpeed: 2-3
- HomingDuration: 5-10s
- Use case: Anti-air missiles, slow homing

### **Aggressive Tracking (Sharp Curve):**
- TurnSpeed: 8-12
- HomingDuration: 2-3s
- Use case: Heat-seekers, magic missiles

### **Balanced (Recommended):**
- TurnSpeed: 5
- HomingDuration: 3-5s
- Use case: General-purpose homing

---

## 🚀 Performance Considerations

| Approach | CPU Cost | Physics Cost | Pooling Friendly |
|----------|----------|--------------|------------------|
| **Option 1 (Lerp)** | Very Low | None | ✅ Yes |
| **Option 2 (Physics)** | Low | Medium | ✅ Yes |
| **Option 3 (Bezier)** | Medium | None | ⚠️ Requires reset |

**Verdict:** All options are performant for typical enemy counts (20-50 projectiles on screen).

---

## 💡 Additional Features (Optional)

### **1. Max Turn Rate Limiter:**
```csharp
// Prevents unrealistic 180° turns
Vector3 targetDir = (Target.position - transform.position).normalized;
float maxRadians = MaxTurnRate * Mathf.Deg2Rad * Time.deltaTime;
Direction = Vector3.RotateTowards(Direction, targetDir, maxRadians, 0f);
```

### **2. Dead Zone (Don't Track Nearby):**
```csharp
// Prevents spiraling at close range
float distanceToTarget = Vector3.Distance(transform.position, Target.position);
if (distanceToTarget < MinTrackingDistance)
{
    // Fly straight
    base.Movement();
    return;
}
```

### **3. Target Leading (Shoot Where Target Will Be):**
```csharp
// Predict target position based on velocity
Vector3 targetVelocity = Target.GetComponent<Rigidbody>().velocity;
float timeToReach = Vector3.Distance(transform.position, Target.position) / Speed;
Vector3 predictedPosition = Target.position + targetVelocity * timeToReach;
Vector3 targetDirection = (predictedPosition - transform.position).normalized;
```

### **4. Trail Effect:**
```csharp
// Add TrailRenderer component for smoke trail
TrailRenderer trail = GetComponent<TrailRenderer>();
trail.time = 0.5f;
trail.startWidth = 0.2f;
trail.endWidth = 0f;
trail.material = smokeTrailMaterial;
```

---

## 🎯 Final Recommendation

**For ProjectBlast:**

Use **Option 1 (Lerp-Based Homing)** because:
- ✅ Matches your existing straight-line projectiles
- ✅ Easy to debug and tune
- ✅ No physics overhead (your game is grid-based)
- ✅ Works perfectly with AIBrain.Target system
- ✅ Looks good at 20-30 enemies shooting simultaneously

**Suggested Values:**
- TurnSpeed: 5 (medium tracking)
- HomingDuration: 3 seconds (enough to reach heroes at Z=-3)
- Speed: 15 (matches your ProjectileSpeed in EnemyDataSO)
- FaceMovement: true (sprite rotates with curve)

---

## 📝 Next Steps

If you approve Option 1, I will:

1. Create `HomingProjectile.cs` (extending Projectile)
2. Add target assignment via `SetTarget()` method
3. Update projectile prefab to use HomingProjectile component
4. Test with single enemy, verify curve behavior
5. Test with multiple enemies, verify no cross-targeting
6. Add debug gizmos (draw line to target, show turn radius)
7. Document in RECIPES.md

**Estimated time:** 15-20 minutes

---

**What do you think? Would you like to go with Option 1, or explore another approach?** 🚀
