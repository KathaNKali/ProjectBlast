# Quick Start: Creating Your First Hero with ScriptableObjects

This guide walks you through creating a complete hero using the new SO system.

## Step 1: Create Weapon Data

1. **Right-click** in `Assets/ProjectBlast/Data/Weapons/` folder
2. Select **Create → ProjectBlast → Weapon Data**
3. Name it: `Weapon_BasicRifle`
4. Configure:
   ```
   Weapon Name: "Basic Rifle"
   Weapon Type: Rifle
   Damage Per Shot: 10
   Ammo Per Shot: 1
   Projectile Prefab: [Assign from TopDown Engine demos - e.g., KoalaBullet]
   Projectile Speed: 20
   Projectile Lifetime: 3
   Max Range: 25
   ```

## Step 2: Create Weapon Prefab

1. **Duplicate** an existing weapon prefab from TopDown Engine demos
   - Location: `Assets/TopDownEngine/Demos/[Any Demo]/Weapons/`
   - Example: Duplicate `KoalaRifle.prefab`
2. **Rename** to `BasicRifle.prefab`
3. **Move** to `Assets/ProjectBlast/Prefabs/Weapons/`
4. **Select** the prefab and add component: **WeaponDataHolder**
5. **Assign** `Weapon_BasicRifle` SO to the WeaponDataHolder's `Weapon Data` field
6. **Check** that the weapon has:
   - `ProjectileWeapon` component
   - `MMObjectPooler` component with projectile set
   - Spawn point transform

## Step 3: Create Hero Data

1. **Right-click** in `Assets/ProjectBlast/Data/Heroes/` folder
2. Select **Create → ProjectBlast → Hero Data**
3. Name it: `Hero_Ranger`
4. Configure:
   ```
   === HERO IDENTITY ===
   Hero Name: "Ranger"
   Hero Class: Ranged
   Description: "A versatile ranged fighter."
   
   === HEALTH ===
   Max Health: 100
   Starting Health: 100
   
   === AMMO SYSTEM ===
   Unlimited Ammo: false
   Starting Ammo: 100
   Low Ammo Threshold: 20
   
   === COMBAT STATS ===
   Detection Range: 20
   Target Search Interval: 0.5
   Fire Rate: 2 (2 shots per second)
   Target Layer Mask: [Select layers enemies will be on - e.g., "Enemies"]
   
   === WEAPON ===
   Default Weapon Prefab: [Assign BasicRifle.prefab from Step 2]
   ```
5. **Observe** the Calculated Stats section:
   - DPS should show: 20 (Fire Rate 2 × Damage 10)
   - Ammo Lifetime should show: 50s (100 ammo ÷ 2 shots/sec)

## Step 4: Create Hero Prefab

### Option A: Start from Scratch

1. **Create** empty GameObject in scene: `Ranger`
2. **Add** your hero model as child (or use a cube for testing)
3. **Add components** to Ranger:
   - `Character` (TopDown Engine)
   - `Health` (TopDown Engine)  
   - `CharacterHandleWeapon` (TopDown Engine ability)
   - `Hero` (ProjectBlast)
   - `BoxCollider` (for selection/interaction)
4. **Create child** GameObject named: `WeaponAttachment`
   - Position it where weapon should be held (e.g., at hand)
5. **Configure Hero component**:
   - Hero Data: Assign `Hero_Ranger` SO
   - Weapon Attachment: Drag `WeaponAttachment` child
6. **Configure Character component**:
   - Character Type: Player (or AI if you want)
   - Character Dimension: Type2D or Type3D
7. **Save** as prefab: `Assets/ProjectBlast/Prefabs/Heroes/Ranger.prefab`

### Option B: Duplicate Existing TDE Character

1. **Find** a TopDown Engine demo character prefab
   - Example: `Assets/TopDownEngine/Demos/KoalaDungeon/Prefabs/Characters/KoalaRifle.prefab`
2. **Duplicate** it to your scene
3. **Rename** to `Ranger`
4. **Remove** unnecessary components:
   - Any movement abilities you don't need
   - Demo-specific scripts
5. **Add** `Hero` component
6. **Create** `WeaponAttachment` child if not present
7. **Configure Hero** component:
   - Hero Data: Assign `Hero_Ranger` SO
   - Weapon Attachment: Assign attachment point
   - Leave other fields empty (SO will override them)
8. **Delete** existing weapon (it will be spawned from SO)
9. **Save** as new prefab: `Assets/ProjectBlast/Prefabs/Heroes/Ranger.prefab`

## Step 5: Test Your Hero

### Setup Test Scene

1. **Create** test target:
   ```
   - Create Cube named "TestEnemy"
   - Add Health component
   - Set layer to match your TargetLayerMask (e.g., "Enemies")
   - Add TestTarget script (optional, for visual feedback)
   - Position at (5, 0, 0)
   ```

2. **Place** your Ranger prefab in scene at (0, 0, 0)

3. **Add GridManager** to scene (if not present):
   - Create empty GameObject: `GridManager`
   - Add GridManager component
   - Configure Firing Zone with at least one slot

4. **Add HeroQueueManager** to scene (if not present):
   - Create empty GameObject: `HeroQueueManager`
   - Add HeroQueueManager component
   - Assign GridManager reference

### Test in Play Mode

1. **Enter Play Mode**
2. **Check Console** for:
   ```
   [Hero] Loaded stats from Hero_Ranger. DPS: 20.0, Ammo Lifetime: 50.0s
   [Hero] Ranger initialized with 100 ammo.
   [Hero] Ranger equipped weapon: BasicRifle
   ```

3. **Deploy Hero**:
   - Use HeroQueueManager Inspector button, or
   - Script: `HeroQueueManager.Instance.PlaceHeroInFiringZone(hero, 0, 0)`

4. **Observe**:
   - Hero should auto-detect TestEnemy
   - Console: `[Hero] Ranger acquired target: TestEnemy at distance X`
   - Hero weapon rotates toward target
   - Hero fires projectiles (2 per second)
   - TestEnemy takes damage
   - Hero ammo decreases

### Expected Behavior

✅ Hero spawns with stats from SO
✅ Weapon equipped automatically  
✅ Auto-targets enemies within 20m
✅ Fires 2 shots per second
✅ Each shot deals 10 damage
✅ Each shot consumes 1 ammo
✅ After 50 seconds (100 shots), hero runs out of ammo
✅ Hero is removed from grid 1.5s after ammo depletion

## Troubleshooting

### "Hero not loading stats"
- Check Hero component has `Hero Data` field assigned
- Console should show: "Loaded stats from Hero_Ranger"

### "Weapon not spawning"
- Check HeroDataSO has `Default Weapon Prefab` assigned
- Check weapon prefab has `Weapon` component
- Console should show: "Ranger equipped weapon: [name]"

### "Hero not detecting enemies"
- Check TestEnemy layer matches hero's TargetLayerMask
- Check Detection Range (should be ≥ distance to enemy)
- Enable console logs: Should see "Ranger acquired target"

### "Hero not firing"
- Check weapon has MMObjectPooler with projectile assigned
- Check hero deployed to Firing zone (StartFiring called)
- Console should show: "Ranger started firing"

### "DPS shows 0 in Inspector"
- Weapon prefab must have WeaponDataHolder component
- WeaponDataHolder must have WeaponData SO assigned
- DPS = Fire Rate × Weapon Damage

## Creating More Heroes

Once you have one working hero, create variants:

### Sniper (High Damage, Slow Fire)
```
Detection Range: 30
Fire Rate: 0.5
Damage Per Shot: 50
Ammo: 30
→ DPS: 25, Lifetime: 60s
```

### Gunner (High Fire Rate, Low Damage)
```
Detection Range: 15
Fire Rate: 5
Damage Per Shot: 5
Ammo: 200
→ DPS: 25, Lifetime: 40s
```

### Tank (Unlimited Ammo, Moderate Damage)
```
Max Health: 200
Detection Range: 15
Fire Rate: 1
Damage Per Shot: 15
Unlimited Ammo: true
→ DPS: 15, Lifetime: Infinite
```

## Next Steps

After you have working heroes:

1. **Create Enemy Base Class** (similar to Hero)
2. **Enemy AI with Pathfinding** (move toward base)
3. **Wave/Stage Management** (spawn enemies in waves)
4. **UI Integration** (show hero stats, queue, ammo)
5. **Polish** (animations, VFX, SFX)

---

**Need Help?**
- Check: `HERO_SCRIPTABLEOBJECT_SYSTEM.md` for detailed docs
- Check: `HERO_FIRING_TEST.md` for debugging guide
- Console logs show detailed initialization and combat info
