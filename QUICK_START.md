# 🚀 Quick Start Guide - ProjectBlast Setup

This guide covers both **generic TopDown Engine setup** and **ProjectBlast-specific systems**.

**Choose Your Path:**
- 📘 **New to TDE?** Start with [Generic TDE Setup](#generic-tde-setup) below
- 🎮 **ProjectBlast Specific?** Jump to [ProjectBlast Grid Defense Setup](#projectblast-grid-defense-setup)

---

## 🎯 ProjectBlast Grid Defense Setup

### Quick Overview
ProjectBlast uses a **3-zone grid system** with **vertical lane queues** where heroes automatically engage enemies using **TDE's AIBrain system**.

**Key Systems:**
- `GridManager` - 3-zone grid (Passive → Active → Firing)
- `HeroQueueManager` - Lane-based hero movement
- `Hero.cs` - AIBrain integration for auto-combat
- `HeroDataSO` - ScriptableObject-based configuration

### Step 1: Test Existing Grid System (5 minutes)

```
1. Open ProjectBlast in Unity 6000.2.10f1
2. Open scene: Assets/ProjectBlast/Scenes/GridTest.unity
3. Press Play
4. Click heroes in Active grid (middle row)
5. Watch them deploy to Firing grid (top rows)
6. Observe automatic lane shifting
```

**✅ If heroes move smoothly, the core system works!**

### Step 2: Create Your First Hero (15 minutes)

#### A. Create HeroDataSO Asset
```
1. Project → Right-click → Create → ProjectBlast → Hero Data
2. Name: "Hero_Ranger"
3. Configure:
   - Hero Name: "Ranger"
   - Hero Class: Ranged
   - Detection Range: 20
   - Target Search Interval: 0.5
   - Starting Ammo: 100
   - Fire Rate: 2.0
   - Target Layer Mask: Select "Enemy" layer
   - Obstacle Layer Mask: Select "Obstacles" layer
```

#### B. Create Weapon Prefab (or use existing)
```
1. Use existing: Assets/TopDownEngine/Demos/Loft3D/Weapons/AssaultRifle3D
2. Or create new weapon - see Documentation/HERO_AIBRAIN_INTEGRATION.md
```

#### C. Create Hero Prefab
```
1. Create Empty GameObject → Name: "Hero_Ranger"
2. Add TDE Components:
   - Character (CharacterTypes.Player, Dimension: Type3D)
   - Health
   - CharacterHandleWeapon
   - CharacterOrientation3D
   - TopDownController3D
3. Add AI Components (create child GameObject "AIBrain"):
   - AIBrain
   - AIActionShoot3D
   - AIActionAimWeaponAtTarget3D
   - AIDecisionDetectTargetRadius3D
   - AIDecisionLineOfSightToTarget3D
4. Add ProjectBlast Component:
   - Hero (assign HeroDataSO)
5. Create child transform: "WeaponAttachment"
6. Add visual model (Capsule or 3D model)
```

#### D. Configure AIBrain States in Inspector
```
1. Select Hero_Ranger
2. Find AIBrain component
3. Configure States:
   
   State 0: "Inactive"
   - Actions: (empty)
   - Transitions: (none)
   
   State 1: "Combat"
   - Actions: Add AIActionShoot3D, AIActionAimWeaponAtTarget3D
   - Transitions: (none - controlled by Hero.cs)
   
4. Set BrainActive = false (Hero.cs controls this)
```

**📖 Full Guide:** See `Documentation/HERO_AIBRAIN_INTEGRATION.md` for complete setup

### Step 3: Test Hero Combat (10 minutes)

#### A. Create Test Enemy
```
1. Create Cube → Name: "TestEnemy"
2. Set Layer: "Enemy"
3. Add Component: Health (Initial: 100)
4. Position at (0, 0.5, 10) - in front of Firing grid
```

#### B. Test in Play Mode
```
1. Place Hero_Ranger in Firing grid slot
2. Press Play
3. Hero should:
   ✅ Detect enemy (AIDecisionDetectTargetRadius3D)
   ✅ Verify line-of-sight (AIDecisionLineOfSightToTarget3D)
   ✅ Rotate toward target (CharacterOrientation3D)
   ✅ Aim weapon (AIActionAimWeaponAtTarget3D)
   ✅ Fire automatically (AIActionShoot3D)
```

**🐛 Troubleshooting:** See `HERO_FIRING_TEST.md` for detailed testing guide

### Step 4: Understand Data Flow

```
┌─────────────────┐
│   HeroDataSO    │  ← Single source of truth
└────────┬────────┘
         ↓
┌─────────────────┐
│   Hero.cs       │  ← Orchestration layer
│  ConfigureAI()  │  ← Applies SO stats to AI
└────────┬────────┘
         ↓
┌─────────────────┐
│   AIBrain       │  ← TDE behavior layer
│  AI Components  │  ← Auto-combat execution
└─────────────────┘
```

**Key Files to Study:**
- `Assets/ProjectBlast/Scripts/Heroes/Hero.cs` (686 lines)
- `Assets/ProjectBlast/Scripts/Grid/GridManager.cs` (732 lines)
- `Assets/ProjectBlast/Scripts/Heroes/HeroQueueManager.cs` (504 lines)

**Documentation:**
- `GRID_DEFENSE_ARCHITECTURE.md` - System architecture
- `VERTICAL_LANE_QUEUE_SYSTEM.md` - Queue mechanics
- `Documentation/SO_SYSTEM_SIMPLIFIED.md` - Data architecture

---

# Generic TDE Setup

---

## ⚡ Quick Setup (5 minutes)

### Step 1: Choose Your Template
Open one of these demo scenes to understand the structure:
- **2D Game:** `Assets/TopDownEngine/Demos/Minimal2D/MinimalScene2D.unity`
- **3D Game:** `Assets/TopDownEngine/Demos/Minimal3D/MinimalScene3D.unity`

### Step 2: Create Your Scene
1. **File → New Scene**
2. **File → Save As** → `Assets/ProjectBlast/Scenes/Level01.unity`
3. Create the `Scenes` folder if needed

---

## 🎮 Scene Setup (10 minutes)

### Step 3: Add Core Managers

#### Create UICamera GameObject
```
1. Create Empty GameObject → Name: "UICamera"
2. Add Component: Camera
3. Set:
   - Clear Flags: Depth Only
   - Culling Mask: UI only
   - Depth: 1
```

#### Create GameManager
```
1. Create Empty GameObject → Name: "GameManager"
2. Add Component: GameManager (TopDown Engine)
3. Add Component: InputManager (or InputSystemManager)
4. Add Component: GUIManager
5. Configure:
   - Link UICamera to GUIManager
```

#### Create LevelManager
```
1. Create Empty GameObject → Name: "LevelManager"
2. Add Component: LevelManager
3. Add Component: BoxCollider (2D or 3D for level bounds)
4. Adjust collider to cover your level area
```

### Step 4: Create Spawn Point
```
1. Create Empty GameObject → Name: "SpawnPoint"
2. Position where player should start
3. Drag to LevelManager → Initial Spawn Point field
```

---

## 🏃 Create Player (10 minutes)

### Step 5: Player Setup (2D Example)

#### Create Player GameObject
```
1. Create Empty GameObject → Name: "Player"
2. Add Component: Character
   - Character Type: Player
   - Player ID: "Player1"
3. Add Component: TopDownController2D
4. Add Component: BoxCollider2D
5. Add Component: Rigidbody2D
   - Gravity Scale: 0
   - Constraints: Freeze Rotation Z
```

#### Add Movement Abilities
```
Add these components to Player:
1. CharacterMovement
   - Walk Speed: 6
2. CharacterOrientation2D
   - Rotation Mode: MovementDirection
3. CharacterDash2D (optional)
   - Dash Distance: 3
   - Dash Duration: 0.2
```

#### Add Visual
```
1. Create child GameObject → Name: "Model"
2. Add SpriteRenderer (2D) or Model (3D)
3. Assign sprite/model
```

### Step 6: Create Player Prefab
```
1. Drag Player from Hierarchy to Project → Assets/ProjectBlast/Prefabs/
2. Delete Player from scene
3. Drag Player prefab to LevelManager → Player Prefabs array
```

---

## 🗺️ Build Level Geometry (5 minutes)

### Step 7: Create Ground/Floor

#### For 2D:
```
1. Create → 2D Object → Tilemap
2. Open Tile Palette (Window → 2D → Tile Palette)
3. Paint your level layout
```

#### For 3D:
```
1. Create → 3D Object → Plane (for floor)
2. Create → 3D Object → Cube (for walls)
3. Scale and position to create level
```

### Step 8: Set Collision Layers
```
1. Select ground/walls
2. Set Layer to "Ground" or "Obstacles"
3. In TopDownController, set Obstacle Layer Mask
```

---

## 🎯 Test Your Game!

### Step 9: Play Test
```
1. Press Play in Unity
2. Player should spawn at SpawnPoint
3. Use WASD or Arrow Keys to move
4. Use Space for dash (if added)
```

**✅ If player moves, you're successful!**

---

## 🔫 Add Weapon (Bonus - 10 minutes)

### Step 10: Create a Simple Gun

#### Create Weapon GameObject
```
1. Create Empty → Name: "BasicGun"
2. Add Component: ProjectileWeapon
3. Configure:
   - Weapon Name: "Basic Gun"
   - Time Between Uses: 0.3
   - Magazine Based: true
   - Magazine Size: 30
```

#### Create Projectile
```
1. Create → Sphere (3D) or Sprite (2D) → Name: "Bullet"
2. Scale to small size (0.1, 0.1, 0.1)
3. Add Component: Projectile
   - Speed: 20
   - Lifetime: 2
4. Add Component: DamageOnTouch
   - Damage Caused: 10
5. Save as Prefab → Assets/ProjectBlast/Prefabs/Bullet
```

#### Link Projectile to Weapon
```
1. Select BasicGun
2. Drag Bullet prefab to ProjectileWeapon → Projectile
```

### Step 11: Give Player the Weapon

#### Add Weapon Ability
```
1. Select Player prefab
2. Add Component: CharacterHandleWeapon
3. Create child Empty → Name: "WeaponAttachment"
4. Drag WeaponAttachment to CharacterHandleWeapon → Weapon Attachment
```

#### Equip Weapon
```
1. Drag BasicGun into scene as child of Player
2. Position near player
3. Save Player prefab
```

**🎉 Press Play - Use Left Click or Shoot button to fire!**

---

## 🤖 Add Simple Enemy (Bonus - 10 minutes)

### Step 12: Create Enemy

#### Basic Enemy Setup
```
1. Create Empty → Name: "Enemy"
2. Add Component: Character
   - Character Type: AI
3. Add Component: TopDownController2D (or 3D)
4. Add Component: BoxCollider2D
5. Add Component: Rigidbody2D (if 2D)
6. Add visual (sprite or model)
```

#### Add AI
```
1. Add Component: AIBrain
2. Add Component: AIActionMoveTowardsTarget2D (or 3D)
3. Add Component: CharacterMovement
```

#### Add Health
```
1. Add Component: Health
   - Initial Health: 100
   - Maximum Health: 100
2. Add Component: DamageOnTouch (to damage player)
   - Damage Caused: 10
```

### Step 13: Test Combat
```
1. Place Enemy in scene
2. Press Play
3. Shoot enemy - it should take damage
4. Touch enemy - you should take damage
```

---

## 🎨 Next Steps

Now that you have a working prototype, enhance it:

### Immediate Improvements
- [ ] Add Health UI bar for player
- [ ] Add death and respawn logic
- [ ] Create multiple enemy types
- [ ] Add pickups (health, ammo)
- [ ] Add more levels

### Study These Demo Scenes
- **Minimal2D/3D** - Basic setup (you just did this!)
- **Koala2D** - Complete 2D game with multiple levels
- **Loft3D** - 3D game with advanced features
- **Explodudes** - Twin-stick shooter mechanics

### Learn These Systems
1. **MMFeedbacks** - Add juice to your game
2. **Inventory System** - Item collection
3. **Save/Load** - Persist player progress
4. **Dialogue** - Add story elements

---

## 🐛 Troubleshooting

### Player Not Moving?
- Check InputManager is in scene
- Verify CharacterMovement is on player
- Check player has TopDownController

### Weapon Not Shooting?
- Verify CharacterHandleWeapon is added
- Check weapon is child of WeaponAttachment
- Ensure projectile prefab is linked

### Collisions Not Working?
- Check collision layers in Physics Settings
- Verify colliders are on objects
- Check TopDownController obstacle mask

### Camera Not Following?
- Check camera has Cinemachine Virtual Camera
- Set Follow target to player
- Adjust camera settings

---

## 📚 Resources

**Documentation:**
- Main: https://topdown-engine-docs.moremountains.com/
- API: https://topdown-engine-docs.moremountains.com/API/
- Feel: https://feel-docs.moremountains.com/

**Community:**
- Discord: Check MoreMountains website
- Forum: Unity Asset Store page

**Your Files:**
- `GAME_DEVELOPMENT_PLAN.md` - Full roadmap
- `.github/copilot-instructions.md` - AI coding guide

---

## 🎯 Success Checklist

After following this guide, you should have:
- ✅ Playable scene with GameManager and LevelManager
- ✅ Player character that moves with keyboard/controller
- ✅ Level geometry with collision
- ✅ Working weapon that shoots projectiles
- ✅ Basic enemy that chases and damages player

**🚀 You're ready to build your game!**

Next: Open `GAME_DEVELOPMENT_PLAN.md` and start Phase 3: Core Mechanics!
