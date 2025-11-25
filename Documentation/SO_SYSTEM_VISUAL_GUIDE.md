# ScriptableObject System - Visual Workflow

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    HERO CREATION WORKFLOW                        │
└─────────────────────────────────────────────────────────────────┘

Step 1: Create WeaponDataSO
┌──────────────────────┐
│  Weapon_BasicRifle   │  → ScriptableObject Asset
│  ==================  │
│  Damage: 10         │
│  Ammo/Shot: 1       │
│  Projectile: Bullet │
└──────────────────────┘
           ↓
Step 2: Create Weapon Prefab
┌──────────────────────┐
│   BasicRifle.prefab  │  → GameObject Prefab
│  ==================  │
│  Components:         │
│  - ProjectileWeapon  │
│  - MMObjectPooler    │
│  - WeaponDataHolder ──→ References Weapon_BasicRifle
└──────────────────────┘
           ↓
Step 3: Create HeroDataSO
┌──────────────────────┐
│    Hero_Ranger       │  → ScriptableObject Asset
│  ==================  │
│  Fire Rate: 2.0     │
│  Ammo: 100          │
│  Detection: 20m     │
│  Weapon: ────────────→ References BasicRifle.prefab
│                     │
│  [Calculated:]      │
│  DPS: 20.0         │  ← Auto-calculated (2.0 × 10)
│  Lifetime: 50s     │  ← Auto-calculated (100 ÷ 2)
└──────────────────────┘
           ↓
Step 4: Create Hero Prefab
┌──────────────────────┐
│   Ranger.prefab      │  → GameObject Prefab
│  ==================  │
│  Components:         │
│  - Character (TDE)   │
│  - Health (TDE)      │
│  - HandleWeapon(TDE) │
│  - Hero ──────────────→ References Hero_Ranger
│  - Collider          │
│                     │
│  Child:             │
│  - WeaponAttachment │
└──────────────────────┘
           ↓
Step 5: Runtime Flow
┌──────────────────────────────────────────────┐
│  RUNTIME INITIALIZATION                       │
│  ──────────────────────────────────────────  │
│  1. Hero.Start()                             │
│  2. InitializeHero()                         │
│  3. InitializeFromData() ←─ HeroDataSO       │
│  4. ApplyToHero()                            │
│     • Load fire rate                         │
│     • Load ammo count                        │
│     • Load detection range                   │
│     • Load target layers                     │
│  5. EquipWeapon(weaponPrefab)                │
│     • Instantiate weapon                     │
│     • WeaponDataHolder.ApplyWeaponData()     │
│  6. StartFiring()                            │
│     • FindTarget()                           │
│     • TryFireWeapon()                        │
│     • ConsumeAmmo() ←─ WeaponDataSO.AmmoPerShot │
└──────────────────────────────────────────────┘
```

## Data Flow Chart

```
COMBAT CYCLE
════════════

HeroDataSO                    WeaponDataSO
    │                             │
    │ Fire Rate: 2.0              │ Damage: 10
    │ Ammo Pool: 100              │ Ammo/Shot: 1
    │ Detection: 20m              │
    ↓                             ↓
┌─────────────────────────────────────┐
│           Hero (Runtime)            │
│                                     │
│  Auto-Targeting Loop:               │
│  ┌──────────────────────┐          │
│  │ FindTarget()         │          │
│  │  • Physics.OverlapSphere(20m) │ ← Detection from HeroDataSO
│  │  • Filter by LayerMask        │
│  │  • Find closest              │
│  └────────┬─────────────┘          │
│           ↓                        │
│  ┌──────────────────────┐          │
│  │ AimAtTarget()        │          │
│  │  • Rotate weapon     │          │
│  └────────┬─────────────┘          │
│           ↓                        │
│  ┌──────────────────────┐          │
│  │ TryFireWeapon()      │          │
│  │  • Check cooldown    │ ← Fire Rate from HeroDataSO (0.5s)
│  │  • ConsumeAmmo()     │ ← Ammo/Shot from WeaponDataSO (1)
│  │  • HandleWeapon.ShootStart() │
│  └────────┬─────────────┘          │
│           ↓                        │
│  Weapon spawns projectile          │
│  Projectile deals damage ──────────┤ ← Damage from WeaponDataSO (10)
│                                     │
│  After 100 shots:                   │
│  • Ammo depleted                    │
│  • RemoveFromGridAfterDelay()       │
│  • Hero removed (1.5s later)        │
└─────────────────────────────────────┘
```

## Stat Calculation Examples

```
DPS CALCULATION
═══════════════

Hero_Ranger:
  Fire Rate: 2.0 shots/sec
  Weapon Damage: 10 per shot
  ────────────────────────
  DPS = 2.0 × 10 = 20.0


Hero_Sniper:
  Fire Rate: 0.5 shots/sec
  Weapon Damage: 50 per shot
  ────────────────────────
  DPS = 0.5 × 50 = 25.0


Hero_Gunner:
  Fire Rate: 5.0 shots/sec
  Weapon Damage: 5 per shot
  ────────────────────────
  DPS = 5.0 × 5 = 25.0
```

```
AMMO LIFETIME CALCULATION
═════════════════════════

Hero_Ranger:
  Starting Ammo: 100
  Fire Rate: 2.0 shots/sec
  Ammo/Shot: 1
  ────────────────────────
  Shots/Sec = 2.0
  Ammo/Sec = 2.0 × 1 = 2.0
  Lifetime = 100 ÷ 2.0 = 50 seconds


Hero_Sniper:
  Starting Ammo: 30
  Fire Rate: 0.5 shots/sec
  Ammo/Shot: 1
  ────────────────────────
  Shots/Sec = 0.5
  Ammo/Sec = 0.5 × 1 = 0.5
  Lifetime = 30 ÷ 0.5 = 60 seconds


Hero_Gunner:
  Starting Ammo: 200
  Fire Rate: 5.0 shots/sec
  Ammo/Shot: 1
  ────────────────────────
  Shots/Sec = 5.0
  Ammo/Sec = 5.0 × 1 = 5.0
  Lifetime = 200 ÷ 5.0 = 40 seconds
```

## Component Relationships

```
HERO PREFAB STRUCTURE
═════════════════════

Ranger (GameObject)
│
├── Components:
│   ├── Character (TopDown Engine)
│   │   └── CharacterType: Player
│   │   └── CharacterDimension: Type2D
│   │
│   ├── Health (TopDown Engine)
│   │   └── MaxHealth: ← Overridden by HeroDataSO
│   │   └── OnDeath → Hero.OnHeroDeath()
│   │
│   ├── CharacterHandleWeapon (TopDown Engine Ability)
│   │   └── WeaponAttachment: → Child transform
│   │
│   ├── Hero (ProjectBlast)
│   │   ├── HeroData: → Hero_Ranger (SO)
│   │   ├── WeaponAttachment: → Child transform
│   │   └── CurrentWeapon: → Spawned at runtime
│   │
│   └── BoxCollider
│       └── For selection/interaction
│
├── Children:
│   ├── HeroModel
│   │   └── Visual mesh/sprites
│   │
│   └── WeaponAttachment (Transform)
│       └── Weapon spawns here at runtime
│           ├── BasicRifle (Clone)
│           │   ├── ProjectileWeapon
│           │   ├── MMObjectPooler → Bullets
│           │   └── WeaponDataHolder → Weapon_BasicRifle (SO)
│           └── Spawn Point (Child)
└
```

## Inspector Preview

```
HERO COMPONENT (Inspector)
══════════════════════════

┌─────────────────────────────────────┐
│ Hero (Script)                       │
├─────────────────────────────────────┤
│                                     │
│ ▼ Hero Configuration                │
│   Hero Data          [Hero_Ranger]  │ ← Assign SO here
│                                     │
│ ▼ Grid Integration                  │
│   Current Grid Slot  [None]         │
│                                     │
│ ▼ Hero Identity                     │
│   Hero Name          "Ranger"       │ ← Loaded from SO
│   Hero Class         Ranged         │ ← Loaded from SO
│                                     │
│ ▼ TDE Components                    │
│   Character          [Character]    │
│   Health             [Health]       │
│   Handle Weapon      [CharacterH...]│
│                                     │
│ ▼ Weapon Configuration              │
│   Weapon Attachment  [WeaponAtta...]│
│   Weapon Prefab      [BasicRifle]   │ ← Loaded from SO
│   Target Layer Mask  Enemies        │ ← Loaded from SO
│                                     │
│ ▼ Ammo System                       │
│   Unlimited Ammo     ☐              │ ← Loaded from SO
│   Starting Ammo      100            │ ← Loaded from SO
│   Ammo Per Shot      1              │ ← Fallback (uses weapon)
│   Low Ammo Threshold 20             │ ← Loaded from SO
│                                     │
│ ▼ Combat Configuration              │
│   Detection Range    20.0           │ ← Loaded from SO
│   Target Search      0.5            │ ← Loaded from SO
│   Auto Fire Rate     2.0            │ ← Loaded from SO
│                                     │
│ ▼ Lifecycle Settings                │
│   Removal Delay      1.5            │
│   Destroy On Removal ☑              │
└─────────────────────────────────────┘
```

```
HERO DATA SO (Inspector)
════════════════════════

┌─────────────────────────────────────┐
│ Hero_Ranger (Hero Data SO)          │
├─────────────────────────────────────┤
│                                     │
│ ▼ HERO IDENTITY                     │
│   Hero Name          "Ranger"       │
│   Hero Class         Ranged         │
│   Icon               [RangerIcon]   │
│   Description        "A versatile  │
│                      ranged fighter"│
│                                     │
│ ▼ HEALTH                            │
│   Max Health         100            │
│   Starting Health    100            │
│                                     │
│ ▼ AMMO SYSTEM                       │
│   Unlimited Ammo     ☐              │
│   Starting Ammo      100            │
│   Low Ammo Threshold 20             │
│                                     │
│ ▼ COMBAT STATS                      │
│   Detection Range    20.0           │
│   Target Search      0.5            │
│   Fire Rate          2.0            │
│   Target Layer Mask  Enemies        │
│   Target Tags        [Empty]        │
│                                     │
│ ▼ WEAPON                            │
│   Default Weapon     [BasicRifle]   │
│                                     │
│ ▼ CALCULATED STATS (Read-Only)     │
│   Cached DPS         20.0           │ ← Auto-calculated
│   Cached Ammo Life   50.0           │ ← Auto-calculated
└─────────────────────────────────────┘
```

## File Organization

```
Assets/ProjectBlast/
│
├── Scripts/
│   ├── Data/
│   │   ├── HeroDataSO.cs           ← ScriptableObject definition
│   │   ├── WeaponDataSO.cs         ← ScriptableObject definition
│   │   └── WeaponDataHolder.cs     ← Component for prefabs
│   │
│   └── Heroes/
│       └── Hero.cs                  ← Updated with SO support
│
├── Data/                            ← Create these folders
│   ├── Heroes/
│   │   ├── Hero_Ranger.asset       ← Create via menu
│   │   ├── Hero_Sniper.asset
│   │   ├── Hero_Gunner.asset
│   │   └── Hero_Tank.asset
│   │
│   └── Weapons/
│       ├── Weapon_BasicRifle.asset ← Create via menu
│       ├── Weapon_SniperRifle.asset
│       ├── Weapon_MachineGun.asset
│       └── Weapon_Shotgun.asset
│
└── Prefabs/                         ← Create these folders
    ├── Heroes/
    │   ├── Ranger.prefab           ← References Hero_Ranger.asset
    │   ├── Sniper.prefab
    │   ├── Gunner.prefab
    │   └── Tank.prefab
    │
    └── Weapons/
        ├── BasicRifle.prefab       ← Has WeaponDataHolder
        ├── SniperRifle.prefab      ← References Weapon_*.asset
        ├── MachineGun.prefab
        └── Shotgun.prefab
```

## Quick Reference Menu

```
RIGHT-CLICK CONTEXT MENU
════════════════════════

Project Window → Right-Click

Create →
  ├── ProjectBlast →
      ├── Hero Data        ← Creates HeroDataSO
      └── Weapon Data      ← Creates WeaponDataSO
```

## Balancing Matrix

```
HERO COMPARISON TABLE
═════════════════════

╔═══════════╦══════╦════════╦═══════╦═══════╦═════════╦═════════════╗
║ Hero      ║ Fire ║ Damage ║ Ammo  ║ Range ║   DPS   ║   Lifetime  ║
║           ║ Rate ║ /Shot  ║ Pool  ║  (m)  ║         ║    (sec)    ║
╠═══════════╬══════╬════════╬═══════╬═══════╬═════════╬═════════════╣
║ Ranger    ║ 2.0  ║   10   ║  100  ║  20   ║  20.0   ║    50       ║
╠═══════════╬══════╬════════╬═══════╬═══════╬═════════╬═════════════╣
║ Sniper    ║ 0.5  ║   50   ║   30  ║  30   ║  25.0   ║    60       ║
╠═══════════╬══════╬════════╬═══════╬═══════╬═════════╬═════════════╣
║ Gunner    ║ 5.0  ║    5   ║  200  ║  15   ║  25.0   ║    40       ║
╠═══════════╬══════╬════════╬═══════╬═══════╬═════════╬═════════════╣
║ Tank      ║ 1.0  ║   15   ║  ∞    ║  15   ║  15.0   ║    ∞        ║
╠═══════════╬══════╬════════╬═══════╬═══════╬═════════╬═════════════╣
║ Support   ║ 0.5  ║   30   ║   50  ║  20   ║  15.0*  ║   100       ║
║ (AOE)     ║      ║ (AOE)  ║       ║       ║ (Multi) ║             ║
╚═══════════╩══════╩════════╩═══════╩═══════╩═════════╩═════════════╝

* Support hits multiple enemies, effective DPS is higher
```

---

**Quick Links:**
- Full Docs: `HERO_SCRIPTABLEOBJECT_SYSTEM.md`
- Quick Start: `QUICK_START_HERO_SO.md`
- Implementation: `SO_SYSTEM_IMPLEMENTATION_SUMMARY.md`
