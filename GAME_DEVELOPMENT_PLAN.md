# ProjectBlast - Game Development Plan

## Project Overview
**Project Name:** ProjectBlast  
**Engine:** TopDown Engine v4.4  
**Unity Version:** 6000.2.10f1  
**Start Date:** November 14, 2025  
**Last Updated:** November 21, 2025

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

**Status:** Core grid and queue systems implemented and tested  
**Duration:** Week 1-2 (Nov 14-21, 2025)

### Grid System ✅
- [✅] Created `GridManager.cs` with 3-zone system (Passive, Active, Firing)
- [✅] Implemented vertical lane-based queue architecture
- [✅] Active Grid: 1 row × N columns (strict lane enforcement)
- [✅] Passive Grid: 2+ rows × N columns (vertical queues)
- [✅] Firing Grid: No lanes (leftmost fill pattern)
- [✅] Added comprehensive visual gizmos (color-coded zones)
- [✅] Slot tracking and occupancy management
- [✅] Lane query methods (GetLaneHeroes, GetLaneSlots, IsLaneEmpty, GetHeroLane)
- [✅] GetLeftmostEmptySlot for Firing deployment
- [✅] GetNearestEmptySlot for spatial queries
- [✅] World-to-grid coordinate conversion

**Files Created:**
- `Assets/ProjectBlast/Scripts/Grid/GridManager.cs` (732 lines)
- `Assets/ProjectBlast/Scripts/Grid/GridSlot.cs`
- `Assets/ProjectBlast/Scripts/Grid/GridZone.cs` (enum)

### Hero Queue System ✅
- [✅] Created `HeroQueueManager.cs` with lane-based shift logic
- [✅] Implemented BuildLaneShiftPlan (bottom-to-top iteration)
- [✅] Animation system with Lerp movement (0.3s duration)
- [✅] Animation delay system (0.2s) for visual clarity
- [✅] Input blocking during animations (IsAnimating flag)
- [✅] Auto-shift logic when Active heroes deploy
- [✅] Vertical compacting within lanes
- [✅] Test hero spawning system
- [✅] Click detection and selection
- [✅] Deploy to Firing with leftmost slot targeting

**Files Created:**
- `Assets/ProjectBlast/Scripts/Heroes/HeroQueueManager.cs` (504 lines)
- `Assets/ProjectBlast/Scripts/Heroes/Hero.cs` (101 lines)

### Camera System ✅
- [✅] Designed camera architecture (TDE + Cinemachine)
- [✅] Created `BattlefieldCameraTarget.cs` for dynamic centering
- [✅] Perspective camera setup guide
- [✅] Camera positioning for 3-zone grid view

**Files Created:**
- `Assets/ProjectBlast/Scripts/Camera/BattlefieldCameraTarget.cs`

### Testing & Utilities ✅
- [✅] Created `GridManagerTester.cs` for testing grid operations
- [✅] Added debug logging throughout systems
- [✅] Gizmo visualization for scene debugging

**Files Created:**
- `Assets/ProjectBlast/Scripts/Testing/GridManagerTester.cs`

### Documentation ✅
- [✅] **GAME_LOOP.md** - Complete game loop and meta-game design
- [✅] **VERTICAL_LANE_QUEUE_SYSTEM.md** - Lane queue specification
- [✅] **GRID_DEFENSE_ARCHITECTURE.md** - TDE integration guide
- [✅] **CAMERA_SETUP_GUIDE.md** - Camera system instructions
- [✅] **HERO_QUEUE_SETUP.md** - Unity setup guide
- [✅] **GAME_DEVELOPMENT_PLAN.md** - This file (updated)

### Bug Fixes ✅
- [✅] **Lane Shift Bug:** Fixed iteration order (bottom-to-top instead of top-to-bottom)
- [✅] **Firing Deployment Bug:** Changed from nearest slot to leftmost slot

---

## 🔄 Phase 2: Combat & Enemy Systems (IN PROGRESS)

**Target Duration:** Week 3-4  
**Status:** Next priority

### Enemy System
- [ ] Design enemy types (Fast, Tank, Flying, Armored)
- [ ] Create Enemy base class (extends TDE Character)
- [ ] Add AIBrain for pathfinding
- [ ] Implement AIActionMoveTowardsTarget3D (move to base)
- [ ] Add Health component to enemies
- [ ] Create enemy prefabs (3-5 types)
- [ ] Add visual models for each enemy type
- [ ] Test enemy spawning and movement

### Hero Combat System
- [ ] Integrate TDE Weapon system with Hero class
- [ ] Add `CharacterHandleWeapon` ability to heroes
- [ ] Implement `WeaponAutoAim3D` for auto-targeting
- [ ] Create hero weapon types:
  - [ ] Ranged DPS (ProjectileWeapon)
  - [ ] Tank (low damage, high health)
  - [ ] AOE (area damage)
  - [ ] Support (buffs/heals)
- [ ] Add ammo system (limited ammo = permanent loss condition)
- [ ] Implement hero death handling
- [ ] Add combat feedback (MMFeedbacks for shooting, hits, deaths)
- [ ] Test hero vs enemy combat

### Damage & Health
- [ ] Add Health component to heroes
- [ ] Add Health component to base
- [ ] Create projectiles with DamageOnTouch
- [ ] Implement hit reactions and effects
- [ ] Add death animations
- [ ] Test damage flow (hero → enemy, enemy → base, enemy → hero)

---

## 🌊 Phase 3: Stage & Wave Management (UPCOMING)

**Target Duration:** Week 4-5

### Stage System
- [ ] Create `StageManager.cs` for multi-stage levels
- [ ] Implement stage progression logic
- [ ] Add stage transition UI (3-5 second pause)
- [ ] Track stage completion conditions
- [ ] Handle hero persistence between stages
- [ ] Test multi-stage flow (4-5 stages per level)

### Wave System
- [ ] Create `WaveManager.cs` for enemy spawning
- [ ] Design wave patterns (quantity, timing, types)
- [ ] Implement progressive difficulty per stage
- [ ] Add spawn points at top of battlefield
- [ ] Enemy composition per wave
- [ ] Test wave spawning and difficulty curve

### Base Defense
- [ ] Create base/castle GameObject
- [ ] Add Health component to base
- [ ] Implement enemy targeting (move toward base)
- [ ] Add base destruction logic (loss condition)
- [ ] Visual feedback for base damage
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

### Project Documentation
- **[GAME_LOOP.md](./GAME_LOOP.md)** - Complete game loop and meta-game design
- **[VERTICAL_LANE_QUEUE_SYSTEM.md](./VERTICAL_LANE_QUEUE_SYSTEM.md)** - Lane queue specification  
- **[GRID_DEFENSE_ARCHITECTURE.md](./GRID_DEFENSE_ARCHITECTURE.md)** - TDE integration architecture
- **[CAMERA_SETUP_GUIDE.md](./CAMERA_SETUP_GUIDE.md)** - Camera system setup
- **[HERO_QUEUE_SETUP.md](./HERO_QUEUE_SETUP.md)** - Unity setup instructions

### TopDown Engine Documentation
- Main Docs: https://topdown-engine-docs.moremountains.com/
- API Reference: https://topdown-engine-docs.moremountains.com/API/
- Feel Docs: https://feel-docs.moremountains.com/

### Key TDE Systems We're Using
- **Weapon System:** `Weapon.cs`, `ProjectileWeapon.cs`, `WeaponAutoAim3D.cs`
- **Health System:** `Health.cs`, `DamageOnTouch.cs`
- **AI System:** `AIBrain.cs`, `AIActionMoveTowardsTarget3D.cs`, `AIActionShoot3D.cs`
- **Feedback System:** `MMFeedbacks.cs`, various feedback components
- **Event System:** `MMEventManager.cs`, `TopDownEngineEvent.cs`
- **Managers:** `GameManager.cs`, `LevelManager.cs`, `InputManager.cs`

### Demo Scenes to Reference
- **Minimal3D**: Basic 3D setup
- **Loft3D**: Complete 3D game example
- **KoalaDungeon**: Wave-based gameplay
- **Explodudes**: Grid-based multiplayer

---

## 🎯 Success Metrics

### Minimum Viable Product (MVP) ✅
- [✅] Grid system with 3 zones functional
- [✅] Hero queue system with lane shifting
- [✅] Click-to-deploy mechanics
- [✅] Animation system
- [ ] Hero auto-combat with weapons
- [ ] Enemy AI and pathfinding
- [ ] Multi-stage battle system
- [ ] Basic UI (hero pool, health, stage)
- [ ] 1 complete level (World 1-1) with 3 stages
- [ ] Victory/defeat conditions

### Alpha Build
- [ ] All core systems implemented
- [ ] 10 heroes created
- [ ] 5 enemy types
- [ ] World 1 complete (5 levels)
- [ ] Basic meta-game (deck building, rewards)
- [ ] Full UI flow
- [ ] Audio integration

### Beta Build
- [ ] 3 worlds complete (18 levels)
- [ ] 15 heroes
- [ ] 10 enemy types
- [ ] Full meta-game (shop, progression, equipment)
- [ ] Polished UI/UX
- [ ] Complete audio
- [ ] Tested on devices

### Full Release
- [ ] 6 worlds complete (40+ levels)
- [ ] 20+ heroes
- [ ] 15+ enemy types
- [ ] Complete meta-game systems
- [ ] Fully polished
- [ ] Optimized performance
- [ ] Tested and balanced
- [ ] Marketing materials ready

---

## 📅 Development Timeline

### Weeks 1-2 ✅ (Nov 14-21, 2025)
**Phase 1: Foundation** - COMPLETED
- Grid system, hero queue, camera, documentation

### Weeks 3-4 (Nov 22 - Dec 5, 2025)
**Phase 2: Combat Systems** - IN PROGRESS
- Enemy AI, hero combat, damage system

### Weeks 5-6 (Dec 6-19, 2025)
**Phase 3: Stage Management + Phase 4: Environment**
- Wave spawning, stage progression, battlefield visuals

### Weeks 7-9 (Dec 20 - Jan 9, 2026)
**Phase 5: Meta-Game Systems**
- Hero collection, deck building, rewards, shop

### Weeks 10-11 (Jan 10-23, 2026)
**Phase 6: UI/UX & Polish + Phase 7: Content (Part 1)**
- All UI screens, feedback effects, first 10 heroes

### Weeks 12-13 (Jan 24 - Feb 6, 2026)
**Phase 7: Content (Part 2) + Phase 8: Testing**
- Remaining heroes, all enemies, all levels, balancing

### Week 14 (Feb 7-13, 2026)
**Phase 9: Build & Deploy**
- Final build, store submission

**Target Launch:** Mid-February 2026

---

## 🐛 Known Issues & Fixes

### Resolved Issues ✅
1. **Lane Shift Bug (Nov 20):**
   - **Problem:** Heroes not moving up when Active slot emptied
   - **Cause:** Top-to-bottom iteration in BuildLaneShiftPlan
   - **Fix:** Changed to bottom-to-top iteration
   - **Status:** ✅ FIXED

2. **Firing Deployment Bug (Nov 21):**
   - **Problem:** Heroes not deploying to leftmost Firing slot
   - **Cause:** Using GetNearestEmptySlot (distance-based)
   - **Fix:** Created GetLeftmostEmptySlot (reading order)
   - **Status:** ✅ FIXED

### Current Issues
- None

---

## 💡 Design Decisions Log

### November 14, 2025
- Decided on 3-zone grid system (Passive, Active, Firing)
- Chose vertical lane queue architecture
- Strict lane enforcement (no cross-lane movement)

### November 21, 2025
- **Hero Queue Behavior:** Limited pool (deck = battle pool)
- **Death Mechanic:** Permanent loss (hardcore mode)
- **Stage Transitions:** 3-5 second pause, heroes persist
- **Deck Building:** Variable deck size (3-10 slots, unlock through progression)
- **Loss Conditions:** Base destroyed OR all heroes lost
- **Rewards:** Dual system (currency + loot chests)
- **Win Condition:** Complete all stages, base survives

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

**Completed:**
- ✅ Core grid system (732 lines)
- ✅ Vertical lane queue system (504 lines)
- ✅ Hero base class with click detection (101 lines)
- ✅ Camera architecture
- ✅ Animation system (Lerp-based)
- ✅ Complete documentation (6 major files)
- ✅ Game loop design
- ✅ Bug fixes (2 major bugs resolved)

**In Progress:**
- 🔄 Enemy AI system
- 🔄 Hero combat system
- 🔄 Weapon integration

**Next Priority:**
1. Integrate TDE Weapon system with Hero class
2. Create Enemy base class with AIBrain
3. Implement hero auto-targeting (WeaponAutoAim3D)
4. Test hero vs enemy combat loop
5. Create StageManager for multi-stage battles

**Blockers:** None

---

**Last Updated:** November 21, 2025  
**Next Review:** December 1, 2025
