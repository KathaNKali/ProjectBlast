# Hero ScriptableObject System

## Overview

The Hero ScriptableObject system provides a data-driven approach to creating and configuring heroes. This system separates hero stats from hero prefabs, making it easy to:

- Create multiple hero variants quickly
- Balance hero stats in the Inspector
- See calculated DPS and ammo lifetime
- Reuse common configurations
- Maintain clean prefab organization

## Architecture

### Core Components

1. **HeroDataSO** - Contains all hero stats and configuration
2. **WeaponDataSO** - Contains weapon-specific stats (damage, ammo consumption)
3. **WeaponDataHolder** - Attaches WeaponDataSO to weapon prefabs
4. **Hero.cs** - Loads stats from HeroDataSO on initialization

### Design Decisions

Based on requirements:
- **No Weapon Sharing**: Each hero has its own unique weapon (no equipment system)
- **Split Stats**: Damage on weapon, fire rate on hero
- **DPS Calculation**: Automatically calculated (Fire Rate × Damage Per Shot)
- **Targeting**: LayerMask + optional tags for flexibility
- **Ammo Model**: Hero defines max ammo pool, weapon defines consumption rate
- **Prefab Approach**: Each hero = unique prefab with its own model

### Stat Priorities

Critical stats to balance:
1. **Ammo Count** - How long hero can fight
2. **Detection Range** - Engagement distance
3. **Fire Rate** - Shots per second
4. **Damage Per Shot** - Damage per hit

## Creating a New Hero

### Step 1: Create HeroDataSO

1. Right-click in Project window
2. Select `Create → ProjectBlast → Hero Data`
3. Name it `Hero_[HeroName]` (e.g., `Hero_Ranger`)
4. Configure stats in Inspector:

**Identity Section:**
- Hero Name: Display name
- Hero Class: Ranged, Tank, Support, etc.
- Icon: UI sprite
- Description: Tooltip text

**Health Section:**
- Max Health: Total HP
- Starting Health: Initial HP

**Ammo Section:**
- Unlimited Ammo: True for infinite ammo
- Starting Ammo: Initial ammo count
- Low Ammo Threshold: When to warn player

**Combat Section:**
- Detection Range: How far hero can see enemies (meters)
- Target Search Interval: How often to search for targets (seconds)
- Fire Rate: Shots per second
- Target Layer Mask: What layers to target
- Target Tags: Optional tag filtering

**Weapon Section:**
- Default Weapon Prefab: Weapon to equip (must have WeaponDataHolder)

**Calculated Stats (Read-Only):**
- DPS: Automatically calculated (Fire Rate × Weapon Damage)
- Ammo Lifetime: How long before ammo runs out (seconds)

### Step 2: Create WeaponDataSO

1. Right-click in Project window
2. Select `Create → ProjectBlast → Weapon Data`
3. Name it `Weapon_[WeaponName]` (e.g., `Weapon_AssaultRifle`)
4. Configure stats:

**Identity Section:**
- Weapon Name
- Weapon Type
- Icon
- Description

**Damage Section:**
- Damage Per Shot: Damage dealt per hit
- Damage Type: Normal, Piercing, Explosive, etc.

**Ammo Section:**
- Ammo Per Shot: How much ammo consumed per shot

**Projectile Section:**
- Projectile Prefab: Projectile GameObject
- Projectile Speed: How fast projectile travels
- Projectile Lifetime: Max duration before despawn
- Max Range: Effective range

**Visual Section:**
- Weapon Model Prefab: Visual representation
- Muzzle Flash Prefab: Fire effect
- Fire Sound: Audio clip

### Step 3: Create Weapon Prefab

1. Create GameObject in scene with weapon model
2. Add `Weapon` or `ProjectileWeapon` component (TopDown Engine)
3. Add `WeaponDataHolder` component
4. Assign your WeaponDataSO to the WeaponDataHolder
5. Configure projectile spawn point
6. Save as prefab in `Assets/ProjectBlast/Prefabs/Weapons/`

### Step 4: Create Hero Prefab

1. Create GameObject in scene with hero model
2. Add required components:
   - `Hero` (ProjectBlast)
   - `Character` (TopDown Engine)
   - `Health` (TopDown Engine)
   - `CharacterHandleWeapon` ability (TopDown Engine)
   - `Collider` (for selection/interaction)
3. Configure Hero component:
   - Assign your HeroDataSO to the `Hero Data` field
   - Create child GameObject named "WeaponAttachment"
   - Assign WeaponAttachment to `Weapon Attachment` field
4. Configure Health component (optional - will be overridden by HeroDataSO)
5. Save as prefab in `Assets/ProjectBlast/Prefabs/Heroes/`

### Step 5: Test Your Hero

1. Add hero prefab to scene
2. Create a TestTarget cube with Health component
3. Set target layer to match hero's TargetLayerMask
4. Enter Play mode
5. Use HeroQueueManager.PlaceHeroInFiringZone() to deploy hero
6. Hero should auto-detect and fire at target

## Example Hero Configurations

### Ranger (Balanced)
- Detection Range: 20m
- Fire Rate: 2 shots/sec
- Damage: 10 per shot
- Ammo: 100 rounds
- **DPS: 20**
- **Ammo Lifetime: 50s**

### Sniper (Long Range, High Damage)
- Detection Range: 30m
- Fire Rate: 0.5 shots/sec
- Damage: 50 per shot
- Ammo: 30 rounds
- **DPS: 25**
- **Ammo Lifetime: 60s**

### Gunner (High Fire Rate)
- Detection Range: 15m
- Fire Rate: 5 shots/sec
- Damage: 5 per shot
- Ammo: 200 rounds
- **DPS: 25**
- **Ammo Lifetime: 40s**

### Tank (High HP, Low Damage)
- Detection Range: 15m
- Fire Rate: 1 shot/sec
- Damage: 15 per shot
- Ammo: Unlimited
- Max Health: 200
- **DPS: 15**
- **Ammo Lifetime: Infinite**

### Support (AOE, Slow Fire)
- Detection Range: 20m
- Fire Rate: 0.5 shots/sec
- Damage: 30 per shot (AOE)
- Ammo: 50 rounds
- **DPS: 15 (but hits multiple enemies)**
- **Ammo Lifetime: 100s**

## Data Flow

```
HeroDataSO → Hero.InitializeFromData() → Hero applies stats
                                         ↓
                                    Equips Weapon
                                         ↓
WeaponDataSO → WeaponDataHolder → Weapon configured with damage/ammo
                                         ↓
                                    Hero fires weapon
                                         ↓
                            ConsumeAmmo (from WeaponDataSO)
```

## Stat Calculation Formulas

### DPS (Damage Per Second)
```
DPS = FireRate × DamagePerShot
```

### Ammo Lifetime
```
AmmoLifetime = StartingAmmo ÷ (FireRate × AmmoPerShot)
```

### Total Damage Potential
```
TotalDamage = (StartingAmmo ÷ AmmoPerShot) × DamagePerShot
```

## Best Practices

### Balancing
1. **Start with DPS**: Aim for 15-30 DPS for most heroes
2. **Adjust Trade-offs**:
   - High damage → Low fire rate
   - High fire rate → Low damage
   - Long range → Higher ammo consumption
3. **Use Ammo Lifetime**: Ensure heroes last 30-60s in combat
4. **Test Against Enemies**: Validate time-to-kill feels good

### Organization
- **Folder Structure**:
  ```
  Assets/ProjectBlast/
  ├── Data/
  │   ├── Heroes/
  │   │   ├── Hero_Ranger.asset
  │   │   ├── Hero_Sniper.asset
  │   │   └── ...
  │   └── Weapons/
  │       ├── Weapon_AssaultRifle.asset
  │       ├── Weapon_SniperRifle.asset
  │       └── ...
  ├── Prefabs/
  │   ├── Heroes/
  │   │   ├── Ranger.prefab
  │   │   ├── Sniper.prefab
  │   │   └── ...
  │   └── Weapons/
  │       ├── AssaultRifle.prefab
  │       ├── SniperRifle.prefab
  │       └── ...
  ```

### Naming Conventions
- HeroDataSO: `Hero_[Name]`
- WeaponDataSO: `Weapon_[Name]`
- Hero Prefab: `[Name]` (matches HeroDataSO name without "Hero_")
- Weapon Prefab: `[Name]` (matches WeaponDataSO name without "Weapon_")

### Workflow Tips
1. **Clone Heroes**: Duplicate existing HeroDataSO to create variants
2. **Test Incrementally**: Create one hero at a time
3. **Use Debug Logs**: Hero initialization logs DPS and ammo lifetime
4. **Inspector Fallback**: Hero works without HeroDataSO (uses inspector values)
5. **Iterate Fast**: Modify SO values in play mode to test (won't persist)

## Troubleshooting

### Hero Not Loading Stats
- Ensure HeroData field is assigned in Hero component
- Check console for "Loaded stats from [SO name]" message
- Verify HeroDataSO has all required fields filled

### Weapon Not Firing
- Ensure weapon prefab has WeaponDataHolder component
- Check WeaponDataHolder has WeaponData assigned
- Verify projectile prefab is assigned in WeaponDataSO

### Calculated Stats Show 0
- Ensure weapon prefab reference is assigned in HeroDataSO
- Verify weapon prefab has WeaponDataHolder with valid WeaponDataSO
- Check that WeaponDataSO has DamagePerShot and AmmoPerShot set

### Ammo Not Consuming
- Check weapon has WeaponDataHolder component
- Verify AmmoPerShot is set in WeaponDataSO
- Ensure UnlimitedAmmo is false on HeroDataSO

## Future Enhancements

Potential additions to the system:

1. **Hero Abilities**: Add special ability SOs (dash, shield, etc.)
2. **Upgrade System**: Hero level-up progression
3. **Weapon Equipment**: Allow swapping weapons (separate WeaponDataSO pool)
4. **Status Effects**: Debuff/buff system
5. **Rarity Tiers**: Common, Rare, Epic hero variants
6. **Visual Variants**: Skin/model variations per hero
7. **Audio Sets**: Unique sound profiles per hero class

## Integration with Existing Systems

### GridManager
- Heroes spawn with HeroDataSO reference intact
- Grid placement doesn't modify hero stats

### HeroQueueManager
- Can filter/sort heroes by HeroClass from HeroDataSO
- Display hero stats in queue UI using HeroDataSO

### Combat System
- Auto-targeting uses DetectionRange from HeroDataSO
- Fire rate controlled by AutoFireRate from HeroDataSO
- Ammo consumption reads from WeaponDataHolder

### UI System
- Display hero stats from HeroDataSO
- Show calculated DPS and ammo lifetime
- Use Icon sprite from HeroDataSO

---

**Documentation Version**: 1.0  
**Last Updated**: Initial creation  
**Related Files**: 
- `HeroDataSO.cs`
- `WeaponDataSO.cs`
- `WeaponDataHolder.cs`
- `Hero.cs`
