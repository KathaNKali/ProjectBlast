# ProjectBlast - Game Development Plan

## Project Overview
**Project Name:** ProjectBlast  
**Engine:** TopDown Engine v4.4  
**Unity Version:** 6000.2.10f1  
**Start Date:** November 14, 2025  
**Last Updated:** December 17, 2025

---

## 📊 Quick Status Overview

| Metric | Status |
|--------|--------|
| **Overall Progress** | 30% (Core Systems Phase) |
| **Lines of Code** | 4,341 lines (15 scripts) |
| **Documentation** | 20+ files (10,000+ lines) |
| **Development Time** | 4 weeks (Nov 14 - Dec 17) |
| **Phase Status** | ✅ Phase 1 Done, ✅ Phase 2 Done, 🚧 Phase 3 25% |
| **Current Focus** | Enemy AI + Stage Management |
| **Next Milestone** | MVP (February 2026) |

### What's Working Right Now ✅
1. **Grid System** - 3-zone grid with vertical lane queues
2. **Hero Deployment** - Click Active hero → Smooth animation to Firing zone
3. **Auto-Combat** - Heroes auto-target and fire at enemies using TDE AIBrain
4. **Ammo System** - Limited ammo with permanent removal when depleted
5. **Target Priority** - 5 priority modes (Closest, Farthest, Health-based, etc.)
6. **Lane Shifting** - Automatic vertical compacting when heroes deploy
7. **Data Architecture** - ScriptableObject-driven hero/weapon configuration

### What's Next 🚧
1. **Enemy AI** - AIBrain + pathfinding (move toward base)
2. **Base GameObject** - Health component, target for enemies
3. **Victory/Defeat** - Win/loss conditions and UI
4. **Stage Manager** - Multi-stage level progression
5. **Wave Manager** - Coordinated enemy spawning
6. **Battle UI** - Hero pool tracker, base health bar

---

## 🎮 Game Concept

### **Genre**
Tactical Tower Defense / RTS Hybrid with deck-building meta-game

### **Game Description**
ProjectBlast is a fast-paced hybrid of RTS, puzzle strategy, and tower defense. Build your deck of heroes, then deploy them strategically from a limited pool as they enter the battlefield through a vertical lane queue system. Heroes automatically attack enemies as they approach your base, but death is permanent - when a hero falls, they're gone for the entire level. Manage your precious hero pool, time your deployments, and survive multi-stage battles across worlds. Collect rewards, unlock new heroes, and progress through an epic story campaign.

### **Platform Targets**
- Android (Primary)
- iOS (Primary)
- PC (Secondary)

### **Visual Style**
3D with perspective camera, stylized graphics

---

## 🎯 Core Game Loop

**See:** [GAME_LOOP.md](./GAME_LOOP.md) for full game loop documentation.

**Summary:**
```
Collect Heroes → Build Deck (3-10 slots) → Select Level → 
Deploy Heroes in Battle (Passive → Active → Firing) → 
Complete Multi-Stage Level → Earn Rewards (Gold, Heroes, Chests) → 
Upgrade Heroes → Unlock New Worlds → Repeat
```

**Key Systems:**
1. **Hero Collection System** - Gacha-style hero acquisition
2. **Deck Building** - Pre-battle hero selection (limited pool)
3. **World Progression** - 6 worlds with increasing levels
4. **Stage-Based Battles** - Each level has 2-5 stages
5. **Limited Hero Pool** - Deck = all heroes for that level
6. **Permanent Loss** - Dead/depleted heroes lost for level
7. **Dual Rewards** - Currency + Loot chests
8. **Star Rating** - Optional objectives for bonus rewards

---

## 🏗️ Core Mechanics

1. **Vertical Lane Queue System** - 3 independent columns, heroes move within lanes only
2. **Multi-Layered Grid System** - Passive (2+ rows) → Active (1 row) → Firing (no lanes)
3. **Hero Auto-Shift** - When Active hero deploys, Passive heroes in that lane shift up
4. **Automatic Combat System** - Heroes auto-fire at enemies using TDE weapons
5. **Stage Progression** - Multi-wave system with brief pauses between stages
6. **Limited Hero Pool** - No respawns, deck defines available heroes
7. **Permanent Loss Mechanic** - Heroes die or run out of ammo permanently
8. **Dual Loss Conditions** - Base destroyed OR all heroes lost
9. **Hero Classes & Counters** - Different weapon types, ranges, damage
10. **Tactical Deployment** - Choose which lane/hero to deploy and when
11. **Deck Building Strategy** - Pre-battle hero composition matters
12. **Progressive Unlocks** - Expand deck slots through gameplay

---

## 📋 Development Phases

---

## ✅ Phase 1: Foundation & Core Systems (COMPLETED)

**Status:** ✅ FULLY IMPLEMENTED - Core grid and queue systems operational  
**Duration:** Nov 14-21, 2025  
**Lines of Code:** ~1,500 lines

### Grid System ✅ (731 lines)
- [✅] Created `GridManager.cs` (MMSingleton) - Full 3-zone grid management
- [✅] Passive Grid: 3 rows × 3 columns (vertical lane queues)
- [✅] Active Grid: 3 rows × 3 columns (ready deployment area)
- [✅] Firing Grid: 3 rows × 3 columns (combat zone)
- [✅] Lane-based architecture (each column = independent queue)
- [✅] Comprehensive visual gizmos (color-coded zones, occupancy indicators)
- [✅] Slot tracking with `GridSlot` class (84 lines)
- [✅] Lane query methods: GetLaneHeroes, GetLaneSlots, IsLaneEmpty, GetHeroLane
- [✅] GetLeftmostEmptySlot for Firing deployment
- [✅] GetNearestEmptySlot for spatial queries
- [✅] World-to-grid coordinate conversion
- [✅] GetEmptySlots, GetOccupiedSlots zone filtering

**Files Created:**
- `GridManager.cs` (731 lines) - Core grid system with MMSingleton pattern
- `GridSlot.cs` (84 lines) - Slot data structure with occupancy tracking
- `GridZone.cs` (29 lines) - Enum defining Passive/Active/Firing zones

### Hero Queue System ✅ (608 lines)
- [✅] Created `HeroQueueManager.cs` (MMSingleton) - Lane-based queue orchestration
- [✅] Vertical lane shifting (bottom-to-top iteration pattern)
- [✅] BuildLaneShiftPlan - Intelligent multi-hero lane compacting
- [✅] Smooth Lerp-based animations (0.3s duration, 0.2s delay)
- [✅] Input blocking during animations (IsAnimating flag)
- [✅] Auto-shift when Active heroes deploy to Firing
- [✅] Test spawning system (fills Passive and Active zones)
- [✅] Click detection and hero selection
- [✅] Deploy to leftmost Firing slot
- [✅] SpawnHeroesInZone utility

**Files Created:**
- `HeroQueueManager.cs` (608 lines) - Queue management, animations, deployment

### Camera System ✅ (93 lines)
- [✅] `BattlefieldCameraTarget.cs` - Dynamic camera focus point
- [✅] Calculates battlefield center from GridManager
- [✅] Updates camera target as grid state changes
- [✅] Designed for Cinemachine integration
- [✅] Perspective camera setup documented

**Files Created:**
- `BattlefieldCameraTarget.cs` (93 lines)

### Testing Infrastructure ✅ (480 lines)
- [✅] `GridManagerTester.cs` (322 lines) - Comprehensive grid operation testing
- [✅] `TestTarget.cs` (158 lines) - Target dummy for combat testing
- [✅] Debug logging throughout all systems
- [✅] Gizmo visualization for scene debugging
- [✅] Inspector-based test controls

**Files Created:**
- `GridManagerTester.cs` (322 lines) - Grid testing utility
- `TestTarget.cs` (158 lines) - Combat target dummy

### Documentation ✅
- [✅] **GAME_LOOP.md** - Complete game loop and meta-game design
- [✅] **VERTICAL_LANE_QUEUE_SYSTEM.md** - Lane queue specification
- [✅] **GRID_DEFENSE_ARCHITECTURE.md** - TDE integration architecture
- [✅] **CAMERA_SETUP_GUIDE.md** - Camera system setup guide
- [✅] **HERO_QUEUE_SETUP.md** - Unity scene setup instructions
- [✅] **GRIDMANAGER_SETUP.md** - Grid configuration guide
- [✅] **GAME_DEVELOPMENT_PLAN.md** - This file

### Bug Fixes ✅
- [✅] **Lane Shift Bug** (Nov 20): Fixed iteration order (bottom-to-top)
- [✅] **Firing Deployment Bug** (Nov 21): Changed to leftmost slot targeting

---

## ✅ Phase 2: Combat & AI Systems (COMPLETED)

**Status:** ✅ FULLY IMPLEMENTED - TDE AIBrain integration complete, heroes combat-ready  
**Duration:** Nov 21-28, 2025  
**Lines of Code:** ~2,350 lines

### Hero Combat System ✅ (883 lines)
- [✅] Complete `Hero.cs` orchestration layer (883 lines)
- [✅] **TDE AIBrain Integration** - Full AI-driven combat system
  - [✅] AIBrain component for state machine control
  - [✅] AIActionShoot3D for automatic weapon firing
  - [✅] AIActionAimWeaponAtTarget3D for weapon aiming
  - [✅] AIDecisionDetectTargetPriority3D (custom, 411 lines) - Priority-based targeting
  - [✅] AIDecisionLineOfSightToTarget3D for LOS verification
- [✅] **TDE Character System Integration**
  - [✅] Character component (hero type)
  - [✅] Health component with death/revive events
  - [✅] CharacterHandleWeapon for weapon equipping
  - [✅] CharacterOrientation3D for body rotation
- [✅] **Zone-Based Combat Control**
  - [✅] StartFiring() - Activates AIBrain when entering Firing zone
  - [✅] StopFiring() - Deactivates AIBrain when leaving Firing zone
  - [✅] Zone change detection via GridSlot property
- [✅] **ConfigureAI() System**
  - [✅] Applies HeroDataSO stats to AI components at runtime
  - [✅] Detection range, target layers, search intervals
  - [✅] Weapon references, shoot offsets, LOS masks
- [✅] **Weapon System Integration**
  - [✅] Dynamic weapon equipping from HeroDataSO
  - [✅] WeaponAttachment transform management
  - [✅] Weapon state monitoring for ammo tracking
- [✅] **ScriptableObject-Driven Configuration**
  - [✅] HeroDataSO (221 lines) - Complete hero stat definition
  - [✅] WeaponDataSO (141 lines) - Weapon configuration system
  - [✅] WeaponDataHolder (72 lines) - Weapon prefab data carrier
  - [✅] ProjectileDamageConfigurator (172 lines) - Projectile setup

**Files Created:**
- `Hero.cs` (883 lines) - Complete hero orchestration with AIBrain
- `HeroDataSO.cs` (221 lines) - Hero configuration ScriptableObject
- `HeroAmmo.cs` (121 lines) - CharacterAbility for ammo management
- `WeaponDataSO.cs` (141 lines) - Weapon configuration ScriptableObject
- `WeaponDataHolder.cs` (72 lines) - Weapon prefab data component
- `ProjectileDamageConfigurator.cs` (172 lines) - Projectile damage setup
- `AIDecisionDetectTargetPriority3D.cs` (411 lines) - Custom AI decision with priority modes

### Ammo System ✅ (121 lines)
- [✅] **HeroAmmo CharacterAbility** - TDE ability pattern
  - [✅] Follows TDE's `FindAbility<T>()` discovery pattern
  - [✅] InitializeAmmo with max ammo and unlimited flag
  - [✅] ConsumeAmmo with MMAmmoEvent firing
  - [✅] Low ammo threshold tracking
  - [✅] Out-of-ammo state management
- [✅] **Ammo Consumption Tracking**
  - [✅] Weapon state monitoring (WeaponUse transition detection)
  - [✅] Frame-based consumption with cooldown (0.1s)
  - [✅] MMEventManager integration for ammo events
- [✅] **Hero Removal System**
  - [✅] Automatic removal when ammo reaches zero
  - [✅] Delayed removal (1.5s) for visual feedback
  - [✅] GridSlot cleanup on removal
  - [✅] OnHeroRemoved event for HeroQueueManager

**Implementation:**
- Heroes consume ammo per weapon state transition
- MMAmmoEvent broadcasts for UI updates
- Heroes auto-remove when depleted (permanent loss)
- CombatCoordinator tracks all hero ammo pools

### AI Target Priority System ✅ (411 lines)
- [✅] **AIDecisionDetectTargetPriority3D** - Extended TDE AIDecision
  - [✅] 5 priority modes: Closest, Farthest, LowestHealth, HighestHealth, FirstDetected
  - [✅] Lock-on mode for consistent targeting
  - [✅] Health-based sorting with frame-cached lookups
  - [✅] Distance-based sorting
  - [✅] MMConeOfVision integration
  - [✅] SetTargetToNull option for brain cleanup
- [✅] **Design Pattern**
  - [✅] Extends TDE's AIDecision base class
  - [✅] Drop-in replacement for AIDecisionDetectTargetRadius3D
  - [✅] Per-hero priority configuration in Inspector
  - [✅] Performance-optimized with cached Health components

**Benefits:**
- Focus fire on weak enemies (LowestHealth)
- Threat elimination (HighestHealth)
- Consistent damage distribution (Closest)
- Strategic targeting per hero type

### Enemy Spawning System ✅ (295 lines)
- [✅] **SimpleEnemySpawner** - Testing spawner
  - [✅] Configurable spawn count and timing
  - [✅] Randomized enemy health (min/max range)
  - [✅] Spawn area with size configuration
  - [✅] Min distance between enemies
  - [✅] Initial delay and spawn intervals
  - [✅] SpawnOnStart option for testing
  - [✅] Inspector test buttons
  - [✅] Gizmo visualization of spawn area
- [✅] Enemy prefab with Health component
- [✅] Random positioning within spawn bounds
- [✅] Collision detection for spawn placement

**Files Created:**
- `SimpleEnemySpawner.cs` (295 lines)

### Documentation ✅
- [✅] **HERO_AIBRAIN_INTEGRATION.md** (500+ lines) - Complete AI setup guide
- [✅] **HERO_TDE_AUTO_AIM_IMPLEMENTATION.md** - Auto-aim architecture
- [✅] **AI_TARGET_PRIORITY.md** - Priority system documentation
- [✅] **HERO_SCRIPTABLEOBJECT_SYSTEM.md** - SO architecture guide
- [✅] **HERO_FIRING_TEST.md** - Combat testing procedures
- [✅] **AMMO_SYSTEM_IMPLEMENTATION.md** - Ammo system design

### Key Achievement 🎯
Successfully integrated **TDE's AIBrain system** for hero combat:
- **~100 fewer lines** than manual WeaponAutoAim approach
- **Inspector-configurable** AI behavior per hero
- **Extensible architecture** using TDE patterns
- **Zone-aware combat** - heroes only engage in Firing zone
- **Data-driven** - all stats from ScriptableObjects
- **Event-driven** - MMEventManager for decoupled communication

---

## 🚧 Phase 3: Enemy AI & Stage Management (IN PROGRESS)

**Target Duration:** Week 4-6 (Nov 28 - Dec 19, 2025)  
**Status:** 🟡 Enemy spawning implemented, enemy AI and stage system next

### Enemy System 🟡
- [✅] **SimpleEnemySpawner** - Testing spawner (295 lines)
- [✅] Basic enemy prefab with Health component
- [✅] Randomized health system
- [✅] Spawn area configuration
- [ ] **Enemy AI Movement** (Next Priority)
  - [ ] Create Enemy base class (extends TDE Character)
  - [ ] Add AIBrain for pathfinding
  - [ ] Implement AIActionMoveTowardsTarget3D (move to base)
  - [ ] Add pathfinding/navigation system
  - [ ] Test enemy movement toward base
- [ ] **Enemy Types** (Planned)
  - [ ] Fast unit (high speed, low HP)
  - [ ] Tank unit (slow, high HP)
  - [ ] Flying unit (bypass ground defenses)
  - [ ] Armored unit (high defense)
- [ ] **Enemy Combat**
  - [ ] Enemies attack heroes (if in range)
  - [ ] Enemies attack base (on reach)
  - [ ] Attack animations and effects

### Stage System ⏸️
- [ ] Create `StageManager.cs` for multi-stage levels
- [ ] Implement stage progression logic
- [ ] Add stage transition UI (3-5 second pause)
- [ ] Track stage completion conditions:
  - [ ] All enemies defeated
  - [ ] Base still alive
  - [ ] Heroes surviving check
- [ ] Handle hero persistence between stages
- [ ] Test multi-stage flow (3-5 stages per level)

### Wave System ⏸️
- [ ] Create `WaveManager.cs` for enemy spawning coordination
- [ ] Design wave patterns:
  - [ ] Enemy quantity per wave
  - [ ] Enemy type composition
  - [ ] Spawn timing/intervals
- [ ] Implement progressive difficulty per stage
- [ ] Multiple spawn points at battlefield top
- [ ] Wave completion detection
- [ ] Test wave spawning and difficulty curve

### Base Defense System ⏸️
- [ ] Create base/castle GameObject with Health
- [ ] Implement enemy targeting (AIActionMoveTowardsTarget3D)
- [ ] Add base destruction logic (loss condition)
- [ ] Visual feedback for base damage states
- [ ] Base destroyed = level failed
- [ ] Test base defense mechanics

---

## 🎨 Phase 4: Level & Environment (UPCOMING)

**Target Duration:** Week 5-6

### Battlefield Environment
- [ ] Create 3D battlefield scene
- [ ] Add ground plane with textures
- [ ] Create visual boundaries
- [ ] Add decorative elements (rocks, trees, etc.)
- [ ] Set up lighting (dynamic or baked)
- [ ] Add skybox
- [ ] Test camera framing

### Base Design
- [ ] Model/import base/castle structure
- [ ] Add visual feedback for damage states
- [ ] Destruction effects
- [ ] Victory/defeat states
- [ ] Test base visuals

### World Themes
- [ ] World 1: Tutorial Lands (forest/grassland theme)
- [ ] World 2: Desert Kingdom (sand/ruins theme)
- [ ] World 3: Frozen Tundra (ice/snow theme)
- [ ] Plan remaining world themes

---

## 🎯 Phase 5: Meta Game Systems (UPCOMING)

**Target Duration:** Week 6-8

### Hero Collection
- [ ] Design hero rarity system (Common, Rare, Epic, Legendary)
- [ ] Create hero database/ScriptableObjects
- [ ] Implement hero unlocking system
- [ ] Add hero XP and leveling
- [ ] Create hero evolution system
- [ ] Design hero progression UI
- [ ] Test hero collection flow

### Deck Building System
- [ ] Create Deck Builder UI
- [ ] Implement deck slot unlocking (3 → 10 slots)
- [ ] Add deck saving/loading
- [ ] Validate deck composition
- [ ] Add deck presets
- [ ] Test deck building flow

### Level Progression
- [ ] Create World Map UI
- [ ] Implement level unlocking logic
- [ ] Add star rating system
- [ ] Track level completion
- [ ] Create level select UI
- [ ] Add level intel/preview
- [ ] Test progression flow

### Rewards System
- [ ] Implement post-battle rewards screen
- [ ] Create chest opening system (Bronze, Silver, Gold)
- [ ] Add currency system (Gold, Gems)
- [ ] Implement hero card drops
- [ ] Add equipment drops
- [ ] Create reward calculation logic
- [ ] Test reward distribution

### Shop System
- [ ] Create shop UI
- [ ] Implement hero purchasing
- [ ] Add equipment shop
- [ ] Create daily deals
- [ ] Add premium currency purchases
- [ ] Test shop flow

---

## 🖼️ Phase 6: UI/UX & Polish (UPCOMING)

**Target Duration:** Week 8-9

### Core UI Screens
- [ ] Main Menu
  - [ ] Continue button
  - [ ] World map access
  - [ ] Hero collection
  - [ ] Deck builder
  - [ ] Shop
  - [ ] Settings
- [ ] Battle UI
  - [ ] Hero pool display (Available/Deployed/Lost)
  - [ ] Base health bar
  - [ ] Stage progress indicator
  - [ ] Pause button
- [ ] Victory Screen
  - [ ] Rewards display
  - [ ] Star rating
  - [ ] Continue button
- [ ] Defeat Screen
  - [ ] Retry button
  - [ ] Quit button
- [ ] Pause Menu
  - [ ] Resume
  - [ ] Settings
  - [ ] Quit to map

### HUD Elements
- [ ] Hero pool tracker UI
- [ ] Base health display
- [ ] Stage counter (e.g., "Stage 2/4")
- [ ] Gold/currency display
- [ ] Pause/settings access

### Visual Feedback (MMFeedbacks)
- [ ] Hero deployment effects
- [ ] Hero death effects
- [ ] Enemy death effects
- [ ] Weapon fire effects
- [ ] Hit effects (damage numbers, particles)
- [ ] Base damage effects
- [ ] Victory celebration effects
- [ ] UI button feedback
- [ ] Camera shake on impacts

### Audio
- [ ] Background music (menu, battle, victory)
- [ ] Hero weapon sounds (per hero type)
- [ ] Enemy sounds (movement, attacks, death)
- [ ] UI sounds (clicks, unlocks, rewards)
- [ ] Base damage sounds
- [ ] Ambient battlefield sounds
- [ ] Voice lines (optional)

---

## 📦 Phase 7: Content Creation (UPCOMING)

**Target Duration:** Week 9-11

### Hero Design
- [ ] Create 15-20 unique heroes
- [ ] Design hero classes:
  - [ ] 5x Ranged DPS (different ranges/fire rates)
  - [ ] 3x Tanks (high HP, low damage)
  - [ ] 3x AOE damage dealers
  - [ ] 2x Support (healing/buffs)
  - [ ] 2x Special (unique mechanics)
- [ ] Balance hero stats
- [ ] Create hero art/models
- [ ] Add hero animations
- [ ] Write hero descriptions

### Enemy Design
- [ ] Create 10-15 enemy types
- [ ] Design enemy categories:
  - [ ] Fast units (high speed, low HP)
  - [ ] Tank units (slow, high HP)
  - [ ] Flying units (bypass ground heroes)
  - [ ] Armored units (high defense)
  - [ ] Boss units (5-10 bosses)
- [ ] Balance enemy stats
- [ ] Create enemy art/models
- [ ] Add enemy animations

### Level Design
- [ ] World 1: 5 levels (2-5 stages each)
- [ ] World 2: 6 levels (3-5 stages each)
- [ ] World 3: 7 levels (3-6 stages each)
- [ ] World 4: 8 levels (4-6 stages each)
- [ ] World 5: 10 levels (4-7 stages each)
- [ ] World 6: Final level (12 stages)
- [ ] Balance difficulty curve
- [ ] Design enemy compositions per stage
- [ ] Create level themes and visuals

### Equipment System (Optional)
- [ ] Design equipment types (weapons, armor, accessories)
- [ ] Create equipment stats and effects
- [ ] Add equipment equipping to heroes
- [ ] Create equipment art
- [ ] Balance equipment progression

---

## 🧪 Phase 8: Testing & Balancing (ONGOING)

**Target Duration:** Week 11-13

### Gameplay Testing
- [ ] Balance hero stats (damage, health, fire rate)
- [ ] Balance enemy stats and spawn rates
- [ ] Test difficulty curve (World 1 → World 6)
- [ ] Playtest all levels
- [ ] Test deck compositions
- [ ] Balance rewards (gold, XP, chests)
- [ ] Test meta progression pacing

### Technical Testing
- [ ] Performance optimization (60 FPS target)
- [ ] Memory profiling
- [ ] Fix all bugs
- [ ] Test on target devices (Android/iOS)
- [ ] Input testing (touch, mouse, controller)
- [ ] Save/load testing
- [ ] Edge case testing

### User Testing
- [ ] Internal playtesting
- [ ] External beta testing
- [ ] Collect feedback
- [ ] Iterate on difficulty
- [ ] Polish based on feedback

---

## 🚀 Phase 9: Build & Deploy (UPCOMING)

**Target Duration:** Week 13-14

### Build Preparation
- [ ] Configure Android build settings
- [ ] Configure iOS build settings
- [ ] Configure PC build settings
- [ ] Optimize build size
- [ ] Add splash screens
- [ ] Add app icons
- [ ] Set up version numbers
- [ ] Test builds on devices

### Store Setup
- [ ] Create Google Play listing
- [ ] Create Apple App Store listing
- [ ] Create Steam page (if PC)
- [ ] Write store descriptions
- [ ] Create screenshots
- [ ] Create promotional video
- [ ] Set pricing
- [ ] Add metadata/tags

### Release
- [ ] Submit to stores
- [ ] Monitor reviews
- [ ] Prepare post-launch support
- [ ] Plan updates and content

---

## 📚 Resources & Documentation

### Project Documentation (15+ files)
**Core Guides:**
- **[GAME_LOOP.md](./GAME_LOOP.md)** - Complete game loop and meta-game design
- **[GAME_ARCHITECTURE.md](./GAME_ARCHITECTURE.md)** - Full production architecture
- **[GRID_DEFENSE_ARCHITECTURE.md](./GRID_DEFENSE_ARCHITECTURE.md)** - TDE integration patterns
- **[ARCHITECTURE_COMPATIBILITY_ANALYSIS.md](./ARCHITECTURE_COMPATIBILITY_ANALYSIS.md)** - System compatibility

**Grid System:**
- **[VERTICAL_LANE_QUEUE_SYSTEM.md](./VERTICAL_LANE_QUEUE_SYSTEM.md)** - Lane queue specification
- **[GRIDMANAGER_SETUP.md](./GRIDMANAGER_SETUP.md)** - Grid configuration guide
- **[GAME_LAYOUT_DESIGN.md](./GAME_LAYOUT_DESIGN.md)** - Battlefield layout

**Hero System:**
- **[HERO_QUEUE_SETUP.md](./HERO_QUEUE_SETUP.md)** - Unity scene setup
- **[Documentation/HERO_AIBRAIN_INTEGRATION.md](./Documentation/HERO_AIBRAIN_INTEGRATION.md)** - AI setup (500+ lines)
- **[Documentation/HERO_TDE_AUTO_AIM_IMPLEMENTATION.md](./Documentation/HERO_TDE_AUTO_AIM_IMPLEMENTATION.md)** - Auto-aim architecture
- **[Documentation/HERO_SCRIPTABLEOBJECT_SYSTEM.md](./Documentation/HERO_SCRIPTABLEOBJECT_SYSTEM.md)** - SO patterns
- **[Documentation/QUICK_START_HERO_SO.md](./Documentation/QUICK_START_HERO_SO.md)** - Quick SO guide
- **[HERO_FIRING_TEST.md](./HERO_FIRING_TEST.md)** - Combat testing

**AI System:**
- **[Documentation/AI_TARGET_PRIORITY.md](./Documentation/AI_TARGET_PRIORITY.md)** - Priority targeting
- **[Documentation/SETUP_TARGET_PRIORITY.md](./Documentation/SETUP_TARGET_PRIORITY.md)** - Priority setup
- **[Documentation/TDE_3D_AIMING_SUMMARY.md](./Documentation/TDE_3D_AIMING_SUMMARY.md)** - 3D aiming guide

**Combat System:**
- **[AMMO_SYSTEM_IMPLEMENTATION.md](./AMMO_SYSTEM_IMPLEMENTATION.md)** - Ammo architecture
- **[Documentation/ZONE_BASED_COMBAT.md](./Documentation/ZONE_BASED_COMBAT.md)** - Zone combat rules
- **[Documentation/COOPERATIVE_ALLOCATION_SYSTEM.md](./Documentation/COOPERATIVE_ALLOCATION_SYSTEM.md)** - Combat coordination

**Setup Guides:**
- **[CAMERA_SETUP_GUIDE.md](./CAMERA_SETUP_GUIDE.md)** - Camera system
- **[TDE_INTEGRATION_GUIDE.md](./TDE_INTEGRATION_GUIDE.md)** - TDE integration
- **[QUICK_START.md](./QUICK_START.md)** - Quick start guide
- **[RECIPES.md](./RECIPES.md)** - Common implementation patterns
- **[README.md](./README.md)** - Project overview

### TopDown Engine Documentation
- **Main Docs:** https://topdown-engine-docs.moremountains.com/
- **API Reference:** https://topdown-engine-docs.moremountains.com/API/
- **Feel Docs:** https://feel-docs.moremountains.com/

### Key TDE Systems We're Using
**Combat:**
- `Weapon.cs`, `ProjectileWeapon.cs` - Weapon system
- `Health.cs`, `DamageOnTouch.cs` - Health and damage
- `CharacterHandleWeapon.cs` - Weapon equipping

**AI:**
- `AIBrain.cs` - State machine controller
- `AIActionShoot3D.cs`, `AIActionAimWeaponAtTarget3D.cs` - Combat actions
- `AIDecisionDetectTargetRadius3D.cs`, `AIDecisionLineOfSightToTarget3D.cs` - Detection

**Character:**
- `Character.cs` - Base character class
- `CharacterAbility.cs` - Ability system base
- `CharacterOrientation3D.cs` - Body rotation

**Core:**
- `MMEventManager.cs` - Event system
- `MMSingleton<T>.cs` - Singleton pattern
- `MMFeedbacks.cs` - Feedback system
- `GameManager.cs`, `LevelManager.cs` - Game management

### TDE Demo Scenes for Reference
- **Minimal3D** - Basic 3D character setup
- **Loft3D** - Complete 3D game with AI
- **KoalaDungeon** - Wave-based gameplay
- **Explodudes** - Grid-based mechanics

---

## 🎯 Success Metrics

### Minimum Viable Product (MVP)
**Target Completion:** February 2026  
**Current Progress:** 30%

**Core Systems (Required for MVP):**
- [✅] Grid system with 3 zones - **DONE**
- [✅] Hero queue with lane shifting - **DONE**
- [✅] Click-to-deploy mechanics - **DONE**
- [✅] Hero auto-combat with AIBrain - **DONE**
- [✅] Ammo system with permanent loss - **DONE**
- [✅] Animation system - **DONE**
- [🚧] Enemy AI with pathfinding - **IN PROGRESS**
- [ ] Base GameObject with Health - **NEXT**
- [ ] Victory/Defeat conditions - **NEXT**
- [ ] Multi-stage battle system - **PLANNED**
- [ ] Basic battle UI (hero pool, base health) - **PLANNED**
- [ ] 1 complete level (World 1-1) with 3 stages - **PLANNED**

**MVP Demo Flow:**
```
1. Click Play → Load GameScene
2. Heroes spawn in Passive/Active zones
3. Click Active hero → Deploys to Firing with animation
4. Hero auto-targets enemies, fires projectiles
5. Enemies spawn, move toward base
6. Enemies reach base → Damage base
7. All enemies defeated → Stage 1 complete
8. Stage 2 starts → More enemies
9. Complete all stages → Victory screen
   OR Base/Heroes lost → Defeat screen
```

### Alpha Build
**Target:** March 2026  
**Requirements:**
- All core MVP systems + polish
- 10 unique heroes with different stats
- 5 enemy types with varied behavior
- World 1 complete (5 levels, 15-25 stages total)
- Basic meta-game (deck building preview, rewards)
- Full battle UI with feedback
- Audio integration (music + SFX)
- Tested on mobile devices

### Beta Build
**Target:** April 2026  
**Requirements:**
- 3 worlds complete (18 levels)
- 15 unique heroes across 4-5 classes
- 10 enemy types + 3 bosses
- Full meta-game (shop, hero collection, progression)
- Polished UI/UX with animations
- Complete audio (music, SFX, voice lines)
- Performance optimized (60 FPS on target devices)
- External playtesting complete

### Full Release
**Target:** May 2026  
**Requirements:**
- 6 worlds complete (40+ levels, 100+ stages)
- 20+ heroes with unique abilities
- 15+ enemy types + 6-10 bosses
- Complete meta-game with all features
- Fully polished with particle effects, screen shake, juicy feedback
- Optimized build size and performance
- Store listing ready (screenshots, video, description)
- Marketing materials prepared

---

## 📅 Development Timeline (Revised)

### ✅ Weeks 1-2 (Nov 14-21, 2025) - COMPLETED
**Phase 1: Foundation**
- Grid system (844 lines)
- Hero queue (608 lines)
- Camera system (93 lines)
- Testing infrastructure (480 lines)
- Documentation (6 files)

### ✅ Weeks 3-4 (Nov 22 - Dec 12, 2025) - COMPLETED
**Phase 2: Combat & AI Systems**
- Hero combat system (883 lines)
- AIBrain integration
- Ammo system (121 lines)
- AI priority targeting (411 lines)
- Data architecture (634 lines)
- Enemy spawner (295 lines)
- Documentation (14 additional files)

### 🚧 Weeks 5-6 (Dec 13-26, 2025) - IN PROGRESS
**Phase 3: Enemy AI & Stage Management**
- [ ] Enemy AI with pathfinding
- [ ] Base GameObject + Health
- [ ] Victory/Defeat conditions
- [ ] Stage Manager implementation
- [ ] Wave Manager implementation
- [ ] Basic battle UI

**Holiday Break:** Dec 25-31, 2025

### ⏸️ Weeks 7-8 (Jan 1-15, 2026) - PLANNED
**Phase 4: Level Environment**
- Battlefield visuals and themes
- Base model and destruction states
- Environmental decorations
- Lighting and skybox
- World 1 level designs

### ⏸️ Weeks 9-11 (Jan 16 - Feb 5, 2026) - PLANNED
**Phase 5: Meta-Game Systems**
- Hero collection system
- Deck building UI
- Level progression and unlocking
- Rewards system (chests, currency)
- Shop system

### ⏸️ Weeks 12-13 (Feb 6-19, 2026) - PLANNED
**Phase 6: UI/UX & Phase 7: Content (Part 1)**
- All UI screens (menu, battle, victory/defeat)
- HUD elements
- Visual feedback (MMFeedbacks)
- Audio integration
- First 10 heroes created
- 5 enemy types created

### ⏸️ Weeks 14-16 (Feb 20 - Mar 12, 2026) - PLANNED
**Phase 7: Content (Part 2) & Phase 8: Testing**
- Remaining 10 heroes
- Remaining 10 enemy types
- All 40+ levels across 6 worlds
- Balancing and playtesting
- Performance optimization

### ⏸️ Weeks 17-18 (Mar 13-26, 2026) - PLANNED
**Phase 9: Build & Deploy**
- Final builds (Android, iOS, PC)
- Store submissions
- Marketing materials
- Launch preparation

**Target Launch:** Late March / Early April 2026

---

## 🔧 TDE Integration Architecture

### **Deep TDE Integration (60% TDE / 40% Custom)**

ProjectBlast is built **on top of** TopDown Engine, not alongside it. The combat and character systems are **pure TDE**, while grid and queue systems are **custom extensions** following TDE patterns.

### TDE Systems Used (Core Combat)

**Character System (100% TDE):**
```
Hero GameObject
├─ Character (TDE) - CharacterTypes.Player
├─ Health (TDE) - Death/damage handling
├─ CharacterHandleWeapon (TDE) - Weapon equipping
├─ CharacterOrientation3D (TDE) - Body rotation
├─ HeroAmmo (Custom CharacterAbility) - Extends TDE ability pattern
└─ Hero.cs (Custom orchestration layer)
```

**AI System (95% TDE):**
```
AIBrain GameObject (child of Hero)
├─ AIBrain (TDE) - State machine controller
├─ AIActionShoot3D (TDE) - Weapon firing
├─ AIActionAimWeaponAtTarget3D (TDE) - Weapon aiming
├─ AIDecisionDetectTargetPriority3D (Custom) - Extends TDE AIDecision
└─ AIDecisionLineOfSightToTarget3D (TDE) - LOS checking
```

**Weapon System (100% TDE):**
- `Weapon.cs` / `ProjectileWeapon.cs` - All weapons are TDE weapons
- `DamageOnTouch.cs` - Projectile damage handling
- `WeaponAmmo.cs` - TDE ammo system (not using built-in, using custom)

**Event System (100% TDE):**
- `MMEventManager` - All events use TDE's event bus
- `MMAmmoEvent` - Custom event type following TDE pattern
- `MMEventListener<T>` - Hero listens to ammo events

### Custom Systems (Following TDE Patterns)

**Grid Management:**
- `GridManager` - MMSingleton<GridManager> (TDE pattern)
- `GridSlot` - Data structure (no TDE equivalent)
- `GridZone` - Enum (custom concept)

**Hero Queue:**
- `HeroQueueManager` - MMSingleton<HeroQueueManager> (TDE pattern)
- Lerp animations (inspired by TDE's MMMovement)

**Data Architecture:**
- `HeroDataSO` - ScriptableObject (TDE standard)
- `WeaponDataSO` - ScriptableObject (TDE standard)
- Apply-on-initialize pattern (TDE standard)

### Why This Integration Works

✅ **Not Fighting TDE** - Custom systems add features, don't replace core  
✅ **TDE Patterns** - MMSingleton, ScriptableObjects, CharacterAbility, Events  
✅ **Deep Combat Integration** - Using AIBrain instead of reinventing auto-aim  
✅ **Extensible** - Custom AIDecision extends TDE's base class properly  
✅ **Inspector-Friendly** - All TDE benefits (visual config, drag-drop)  

### What Makes This Different from Pure TDE

| Feature | Pure TDE Project | ProjectBlast |
|---------|------------------|--------------|
| **Player Control** | WASD/joystick character movement | No player character - tap to deploy |
| **Movement** | Free movement with TopDownController | Heroes placed on grid, stationary |
| **Combat Initiation** | Player aims and shoots | Heroes auto-engage via AIBrain |
| **Character Lifecycle** | Persistent across scenes | Spawned per level, permanent loss |
| **Level Structure** | Exploration levels | Wave-based defense stages |
| **Camera** | Follows player | Static battlefield view |
| **Input** | Continuous input handling | Tap-based deployment only |

**Key Insight:** ProjectBlast uses TDE's **combat engine** (weapons, health, AI, damage) while replacing the **movement/control layer** (grid instead of free movement, tap instead of WASD).

---

### ✅ Completed Systems (100%)

| System | Lines | Status | Files |
|--------|-------|--------|-------|
| **Grid Management** | 844 | ✅ Complete | GridManager (731), GridSlot (84), GridZone (29) |
| **Hero Queue** | 608 | ✅ Complete | HeroQueueManager (608) |
| **Hero Combat** | 883 | ✅ Complete | Hero (883) |
| **Hero Data** | 221 | ✅ Complete | HeroDataSO (221) |
| **Ammo System** | 121 | ✅ Complete | HeroAmmo (121) |
| **AI Priority** | 411 | ✅ Complete | AIDecisionDetectTargetPriority3D (411) |
| **Weapon Data** | 213 | ✅ Complete | WeaponDataSO (141), WeaponDataHolder (72) |
| **Damage Config** | 172 | ✅ Complete | ProjectileDamageConfigurator (172) |
| **Enemy Spawn** | 295 | ✅ Complete | SimpleEnemySpawner (295) |
| **Camera** | 93 | ✅ Complete | BattlefieldCameraTarget (93) |
| **Testing** | 480 | ✅ Complete | GridManagerTester (322), TestTarget (158) |
| **TOTAL** | **4,341** | ✅ Complete | 15 scripts |

### 🚧 In Progress (0-50%)

| System | Status | Priority | Notes |
|--------|--------|----------|-------|
| **Enemy AI** | 10% | High | Need AIBrain + pathfinding |
| **Stage Manager** | 0% | High | Multi-stage level flow |
| **Wave Manager** | 0% | High | Coordinated enemy spawning |
| **Base Defense** | 0% | High | Base GameObject + Health |

### ⏸️ Planned (Not Started)

- Level & Environment (battlefields, themes)
- Meta Game Systems (hero collection, deck building, progression)
- UI/UX & Polish (all screens, HUD, effects)
- Content Creation (heroes, enemies, levels)
- Testing & Balancing
- Build & Deploy

---

## 📈 Development Progress

**Overall Completion:** ~30% (Core Systems Phase)

### Phase Breakdown
- ✅ **Phase 1: Foundation** - 100% Complete (1,500 lines)
- ✅ **Phase 2: Combat & AI** - 100% Complete (2,350 lines)
- 🚧 **Phase 3: Enemy AI & Stages** - 25% Complete (enemy spawning only)
- ⏸️ **Phase 4: Environment** - 0%
- ⏸️ **Phase 5: Meta Game** - 0%
- ⏸️ **Phase 6: UI/UX** - 0%
- ⏸️ **Phase 7: Content** - 0%
- ⏸️ **Phase 8: Testing** - 0%
- ⏸️ **Phase 9: Deploy** - 0%

---

## 🎯 Current State Analysis

### What's Working ✅
1. **Grid System** - Fully operational 3-zone grid with lane queues
2. **Hero Deployment** - Click-to-deploy with smooth animations
3. **Hero Combat** - AIBrain-driven auto-targeting and firing
4. **Ammo System** - Limited ammo with permanent removal
5. **Data Architecture** - ScriptableObject-driven configuration
6. **Zone Control** - Heroes only fire in Firing zone
7. **Target Priority** - 5 priority modes for strategic targeting
8. **Event System** - MMEventManager integration for ammo tracking

### What's Missing 🚧
1. **Enemy AI** - Enemies spawn but don't move toward base
2. **Stage System** - No multi-stage level progression
3. **Wave Coordination** - SimpleEnemySpawner is basic, needs WaveManager
4. **Base GameObject** - No base to defend yet
5. **Victory/Defeat** - No win/loss conditions implemented
6. **UI/HUD** - No battle interface (hero pool, base health, etc.)
7. **Meta Game** - No deck building, hero collection, or progression

### Immediate Next Steps (Priority Order)
1. **Enemy AI Movement** - AIBrain + AIActionMoveTowardsTarget3D
2. **Base GameObject** - Health component, target for enemies
3. **Victory Condition** - Detect when all enemies defeated
4. **Defeat Conditions** - Base destroyed OR all heroes lost
5. **Stage Manager** - Multi-stage level flow
6. **Wave Manager** - Coordinated enemy spawning
7. **Basic Battle UI** - Hero pool count, base health bar

---

## 🐛 Known Issues & Technical Debt

### Resolved Issues ✅
1. **Lane Shift Bug (Nov 20, 2025)**
   - **Problem:** Heroes not moving up when Active slot emptied
   - **Cause:** Top-to-bottom iteration in BuildLaneShiftPlan created conflicts
   - **Fix:** Changed to bottom-to-top iteration to process heroes in correct order
   - **Status:** ✅ FIXED - Verified working

2. **Firing Deployment Bug (Nov 21, 2025)**
   - **Problem:** Heroes not deploying to leftmost Firing slot
   - **Cause:** Using GetNearestEmptySlot (distance-based) instead of reading order
   - **Fix:** Created GetLeftmostEmptySlot (left-to-right, top-to-bottom)
   - **Status:** ✅ FIXED - Verified working

### Current Issues 🔴
**None** - All systems operational

### Technical Debt 🟡
1. **Enemy System** - Needs full AI implementation (movement, targeting, combat)
2. **Stage System** - No multi-stage progression implemented yet
3. **UI System** - No HUD or battle interface
4. **Performance** - No object pooling for projectiles/enemies yet
5. **Sound** - No audio integration
6. **Feedback** - MMFeedbacks configured but minimal usage
7. **Save System** - No persistence implemented

---

## 💡 Design Decisions Log

### November 14, 2025 - Foundation Decisions
**Grid Architecture:**
- ✅ 3-zone system: Passive → Active → Firing
- ✅ Vertical lane-based queues (each column = independent queue)
- ✅ Strict lane enforcement (heroes don't cross lanes)
- ✅ 3×3 grid per zone for initial implementation
- ✅ Leftmost fill pattern for Firing zone

**Rationale:** Vertical lanes create intuitive queue visualization and simplify logic

### November 15-20, 2025 - Queue System Decisions
**Animation System:**
- ✅ Lerp-based smooth movement (0.3s duration)
- ✅ Animation delays (0.2s) for visual clarity
- ✅ Input blocking during animations
- ✅ Bottom-to-top iteration for lane shifting

**Rationale:** Smooth animations improve feel; input blocking prevents conflicts

### November 21, 2025 - Combat Architecture Decisions
**Hero Combat:**
- ✅ Use TDE's AIBrain instead of manual WeaponAutoAim
- ✅ Zone-based combat (only fire in Firing zone)
- ✅ ScriptableObject-driven hero configuration
- ✅ Limited ammo with permanent loss

**Rationale:** AIBrain provides Inspector configurability and follows TDE patterns; zone control adds strategic layer

**Target Priority:**
- ✅ 5 priority modes (Closest, Farthest, Lowest/HighestHealth, FirstDetected)
- ✅ Lock-on mode option
- ✅ Per-hero configuration in Inspector

**Rationale:** Different heroes need different targeting strategies for tactical depth

### November 25-28, 2025 - Data Architecture Decisions
**ScriptableObject System:**
- ✅ HeroDataSO as single source of truth for hero stats
- ✅ WeaponDataSO for weapon configuration
- ✅ Apply-on-initialize pattern
- ✅ Inspector-first design (minimize code changes)

**Rationale:** SO pattern enables rapid iteration and designer-friendly workflows

**Ammo System:**
- ✅ CharacterAbility pattern for HeroAmmo
- ✅ MMEventManager for ammo tracking
- ✅ Weapon state monitoring (WeaponUse transition detection)
- ✅ Frame-based consumption with 0.1s cooldown

**Rationale:** CharacterAbility follows TDE discovery pattern; events enable UI updates

### December 2025 - Ongoing Decisions
**Game Loop:**
- ✅ Limited pool (deck = all heroes for level)
- ✅ Permanent loss (heroes die/deplete, gone for level)
- ✅ Stage transitions (3-5 second pause, heroes persist)
- ✅ Deck building (variable deck size 3-10 slots)
- ✅ Dual loss conditions (base destroyed OR all heroes lost)
- ✅ Dual rewards (currency + loot chests)

**Rationale:** Permanent loss creates tension; limited pool forces strategic deployment

---

## 📝 Notes & Ideas

### Future Features (Post-Launch)
- PvP Arena (asynchronous battles)
- Daily Challenges
- Guild System
- Hero Fusion mechanic
- Equipment Crafting
- Endless Survival Mode
- Story cutscenes
- Seasonal events

### Monetization Ideas
- Hero gacha (free-to-play model)
- Premium currency (Gems)
- Cosmetic skins
- Battle Pass
- Remove ads option
- Special hero bundles

---

## 🔧 Troubleshooting

### Common Setup Issues
1. **GridManager not found:** Ensure GridManager is in scene and initialized
2. **Heroes not spawning:** Check TestHeroPrefab is assigned in HeroQueueManager
3. **Lane shifting not working:** Verify lane indices are correct (0-indexed)
4. **Animations stuttering:** Check AnimationDuration and AnimationDelay values

### Performance Tips
- Use object pooling for projectiles and enemies
- Optimize collision layers (heroes vs enemies only)
- Disable `ShowGridGizmos` in builds
- Batch hero movements in same lane
- Use LOD for distant models

---

## ✅ Current Status Summary

### 🎯 **Project Milestone: Core Combat Systems Complete**

**Completed Components (4,341 lines):**
- ✅ **Grid System** (844 lines) - 3-zone lane-based grid with occupancy tracking
- ✅ **Hero Queue System** (608 lines) - Vertical lane queues with smooth animations
- ✅ **Hero Combat System** (883 lines) - TDE AIBrain integration with zone control
- ✅ **Data Architecture** (634 lines) - ScriptableObject-driven hero/weapon configuration
- ✅ **Ammo System** (121 lines) - Limited ammo with CharacterAbility pattern
- ✅ **AI Priority System** (411 lines) - 5 priority modes for strategic targeting
- ✅ **Enemy Spawning** (295 lines) - Configurable test spawner
- ✅ **Testing Infrastructure** (480 lines) - Comprehensive testing utilities
- ✅ **Camera System** (93 lines) - Dynamic battlefield centering

**What Works Right Now:**
1. Click hero in Active zone → Deploys to Firing zone with animation
2. Hero enters Firing zone → AIBrain activates → Auto-targets enemies
3. Hero detects enemy in range → Aims weapon → Fires automatically
4. Weapon fires → Ammo consumed → MMAmmoEvent broadcast
5. Ammo reaches 0 → Hero removed → Lane compacts upward
6. Empty Active slot → Passive hero shifts up automatically

**What's Next:**
1. Enemy AI movement (AIBrain + pathfinding to base)
2. Base GameObject with Health component
3. Victory/Defeat conditions
4. Stage Manager for multi-stage levels
5. Wave Manager for coordinated spawning
6. Basic battle UI (hero pool, base health)

**Blockers:** None - All required systems functional

---

## 📊 Development Metrics

**Total Code Written:** 4,341 lines (15 scripts)  
**Total Documentation:** 20+ files (10,000+ lines)  
**Development Time:** ~4 weeks (Nov 14 - Dec 17, 2025)  
**Bugs Fixed:** 2 major (lane shift, firing deployment)  
**TDE Integration:** Deep (60% TDE, 40% custom)  

**Code Quality:**
- ✅ MMSingleton pattern for managers
- ✅ ScriptableObject architecture for data
- ✅ CharacterAbility pattern for hero features
- ✅ Event-driven communication (MMEventManager)
- ✅ Comprehensive Inspector configuration
- ✅ Debug gizmos and logging throughout
- ✅ Well-documented with inline comments

---

**Last Updated:** December 17, 2025  
**Next Review:** January 1, 2026  
**Target MVP Completion:** February 2026
