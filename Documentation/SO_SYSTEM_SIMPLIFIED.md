# Hero ScriptableObject - Simplified Architecture

## The Problem (Before)

The original implementation was confusing because:
- ❌ HeroDataSO had stats
- ❌ Hero.cs ALSO had the same stats as Inspector fields
- ❌ Unclear which was the "source of truth"
- ❌ Duplicate data everywhere
- ❌ Confusing which weapon script to use

## The Solution (Now)

### ✅ Single Source of Truth: HeroDataSO

**Hero.cs Inspector:**
- ✅ Only has: `HeroDataSO HeroData` reference
- ✅ Only has: `WeaponAttachment` transform (physical mount point)
- ✅ All stats READ from HeroDataSO via properties

**Hero.cs Properties (Read-Only):**
```csharp
public string HeroName => HeroData != null ? HeroData.HeroName : gameObject.name;
public float DetectionRange => HeroData != null ? HeroData.DetectionRange : 20f;
public float FireRate => HeroData != null ? HeroData.FireRate : 2f;
// etc...
```

### How It Works Now

```
┌─────────────────────────────────┐
│   HeroDataSO (ScriptableObject) │  ← SINGLE SOURCE OF TRUTH
│                                 │
│  • Hero Name: "Ranger"          │
│  • Fire Rate: 2.0               │
│  • Detection Range: 20m         │
│  • Starting Ammo: 100           │
│  • Weapon Prefab: BasicRifle ───┼──┐
└─────────────────────────────────┘  │
                                      │
       ┌──────────────────────────────┘
       ↓
┌─────────────────────────────────┐
│  Weapon Prefab (BasicRifle)     │
│                                 │
│  Components:                    │
│  • ProjectileWeapon (TDE)       │  ← TopDown Engine component
│  • MMObjectPooler (TDE)         │  ← Handles projectile spawning
│  • WeaponDataHolder (ProjectBlast) ───┐
└─────────────────────────────────┘     │
                                        │
       ┌────────────────────────────────┘
       ↓
┌─────────────────────────────────┐
│   WeaponDataSO                  │
│                                 │
│  • Damage Per Shot: 10          │
│  • Ammo Per Shot: 1             │
└─────────────────────────────────┘
```

## Weapon Setup - Clear Answer

### Question: "Which weapon script should be added to HeroDataSO?"

**Answer: A weapon PREFAB that has these components:**

1. **Weapon** or **ProjectileWeapon** (TopDown Engine)
   - This is the TDE component that handles firing
   
2. **WeaponDataHolder** (ProjectBlast - our custom component)
   - This component holds the WeaponDataSO reference
   - It bridges between TDE weapon and our data system

3. **MMObjectPooler** (TopDown Engine)
   - Handles projectile spawning
   - Configured with the projectile prefab

### Example Weapon Setup

```
BasicRifle.prefab (GameObject)
│
├── ProjectileWeapon (Component)        ← TDE weapon logic
│   └── Owner: Set at runtime
│
├── WeaponDataHolder (Component)        ← OUR bridge component
│   └── Weapon Data: Weapon_BasicRifle.asset (WeaponDataSO)
│
├── MMObjectPooler (Component)          ← TDE pooling
│   └── Pooled Object: Bullet.prefab
│
└── Spawn Point (Child Transform)       ← Where projectiles spawn
```

## Data Flow (Simplified)

```
HERO CREATION
═════════════

Step 1: Create WeaponDataSO
  → Right-click → Create → ProjectBlast → Weapon Data
  → Set: Damage Per Shot, Ammo Per Shot

Step 2: Create Weapon Prefab
  → Add: ProjectileWeapon (TDE)
  → Add: WeaponDataHolder (ProjectBlast)
  → Assign: WeaponDataSO to WeaponDataHolder
  → Configure: MMObjectPooler with projectile

Step 3: Create HeroDataSO
  → Right-click → Create → ProjectBlast → Hero Data
  → Set: Fire Rate, Detection Range, Ammo Pool
  → Assign: Weapon Prefab (from Step 2)

Step 4: Create Hero Prefab
  → Add: Character, Health, CharacterHandleWeapon (TDE)
  → Add: Hero (ProjectBlast)
  → Assign: HeroDataSO to Hero component
  → Create: WeaponAttachment child transform
```

```
RUNTIME FLOW (Current - AIBrain Integration)
════════════════════════════════════════════

Hero.Awake()
  ↓
Find all components:
  • Character, Health, CharacterHandleWeapon
  • CharacterOrientation3D, TopDownController3D
  • AIBrain, AIActionShoot3D, AIActionAim
  • AIDecisionDetectTargetRadius3D, AIDecisionLOS
  ↓
Hero.Start()
  ↓
Hero.ConfigureAI()
  ↓
Apply HeroDataSO stats to AI components:
  • AIDecisionDetect.Radius = HeroData.DetectionRange
  • AIDecisionDetect.TargetLayerMask = HeroData.TargetLayerMask
  • AIDecisionLOS.ObstacleLayerMask = HeroData.ObstacleLayerMask
  • AIActionShoot.TargetHandleWeaponAbility = HandleWeapon
  ↓
Hero.EquipWeapon(HeroData.DefaultWeaponPrefab)
  • Instantiates weapon
  • Weapon attaches to WeaponAttachment transform
  • CharacterHandleWeapon manages weapon
  ↓
[Hero deployed to Firing zone]
  ↓
Hero.StartFiring()
  ↓
AIBrain.BrainActive = true
AIBrain.TransitionToState("Combat")
  ↓
AIBrain executes Combat state:
  • AIDecisionDetectTargetRadius3D scans for enemies
  • AIDecisionLineOfSightToTarget3D verifies LOS
  • AIActionAimWeaponAtTarget3D aims weapon
  • CharacterOrientation3D rotates body
  • AIActionShoot3D fires weapon
  ↓
Projectile spawned → Deals damage via DamageOnTouch
  ↓
Ammo consumed → Hero monitors weapon ammo
  ↓
If ammo depleted → Hero.OnAmmoDepeted() → Remove from grid
```
  • Consumes: GetAmmoConsumptionRate()
    → WeaponDataHolder.GetAmmoPerShot()
    → From WeaponDataSO.AmmoPerShot
```

## Inspector View (Simplified)

### Before (Confusing)
```
Hero Component:
├── Hero Data SO: [Hero_Ranger]        ← One place
├── Hero Name: "Ranger"                ← DUPLICATE!
├── Fire Rate: 2.0                     ← DUPLICATE!
├── Detection Range: 20                ← DUPLICATE!
├── Starting Ammo: 100                 ← DUPLICATE!
├── Weapon Prefab: [BasicRifle]        ← DUPLICATE!
└── ... (10+ more duplicate fields)
```

### After (Clear)
```
Hero Component:
├── Hero Data SO: [Hero_Ranger]        ← SINGLE SOURCE
├── Weapon Attachment: [Transform]     ← Physical mount point
├── Removal Delay: 1.5                 ← Optional override
├── Destroy On Removal: ☑              ← Optional override
│
└── Read-Only Runtime Info:
    ├── Current Ammo: 85               ← Display only
    └── Is Firing: true                ← Display only
```

All stats are in the SO! Just assign it and you're done.

## Quick Reference

### Creating a Hero

1. **Create WeaponDataSO** (damage, ammo consumption)
2. **Create Weapon Prefab** (ProjectileWeapon + WeaponDataHolder + MMObjectPooler)
3. **Create HeroDataSO** (fire rate, ammo pool, detection, reference weapon prefab)
4. **Create Hero Prefab** (Character + Hero, assign HeroDataSO)

### Key Components

| Component | Location | Purpose |
|-----------|----------|---------|
| **HeroDataSO** | ScriptableObject Asset | All hero stats |
| **WeaponDataSO** | ScriptableObject Asset | Damage, ammo consumption |
| **Hero** | Hero Prefab | References HeroDataSO |
| **WeaponDataHolder** | Weapon Prefab | References WeaponDataSO |
| **ProjectileWeapon** | Weapon Prefab | TDE firing logic |
| **MMObjectPooler** | Weapon Prefab | TDE projectile pooling |

### What Goes Where

| Stat | Defined In | Read By |
|------|------------|---------|
| Fire Rate | HeroDataSO | Hero.AutoFireRate property |
| Detection Range | HeroDataSO | Hero.DetectionRange property |
| Ammo Pool | HeroDataSO | Hero.StartingAmmo property |
| Damage | WeaponDataSO | Projectile DamageOnTouch |
| Ammo Per Shot | WeaponDataSO | Hero.GetAmmoConsumptionRate() |
| DPS | HeroDataSO | Auto-calculated (FireRate × Damage) |

## Benefits

### ✅ No More Confusion
- Only ONE place to edit stats: HeroDataSO
- Hero.cs just reads from SO
- No duplicate fields

### ✅ Clear Weapon Setup
- Weapon prefab needs: ProjectileWeapon + WeaponDataHolder
- HeroDataSO references the prefab
- WeaponDataHolder references WeaponDataSO

### ✅ Easy Balancing
- Edit HeroDataSO stats
- See calculated DPS instantly
- No need to update multiple places

### ✅ Reusable
- Create hero variants by duplicating HeroDataSO
- Change stats, done!
- Prefab stays the same

## Troubleshooting

### "Hero not working"
**Check:** Is HeroDataSO assigned in Hero component?
- Hero requires HeroDataSO to function
- All stats come from the SO

### "DPS shows 0"
**Check:** Does weapon prefab have WeaponDataHolder with WeaponDataSO?
- Weapon prefab needs WeaponDataHolder component
- WeaponDataHolder needs WeaponDataSO assigned
- DPS = FireRate (from HeroDataSO) × Damage (from WeaponDataSO)

### "Hero not consuming ammo"
**Check:** Does WeaponDataSO have AmmoPerShot set?
- WeaponDataSO.AmmoPerShot defines consumption
- Hero reads it via WeaponDataHolder.GetAmmoPerShot()

### "Weapon not firing"
**Check weapon prefab has all 3 components:**
1. ProjectileWeapon (TDE) - firing logic
2. WeaponDataHolder (ProjectBlast) - data bridge
3. MMObjectPooler (TDE) - projectile spawning

## Summary

### Before: Confusing Duplication
```
HeroDataSO → Stats
Hero.cs → Same Stats (duplicated!)
Weapon → ??? (unclear)
```

### After: Single Source of Truth
```
HeroDataSO → All Stats → Hero reads via properties
                ↓
         Weapon Prefab (ProjectileWeapon + WeaponDataHolder)
                ↓
         WeaponDataSO → Damage + Ammo Consumption
```

**Simple Rule:** 
- Hero stats → HeroDataSO
- Weapon stats → WeaponDataSO
- Hero.cs → Just reads from HeroDataSO (no duplicates!)
- Weapon Prefab → Must have WeaponDataHolder that references WeaponDataSO

---

**Result:** Clear, maintainable, no confusion! 🎯
