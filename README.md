# ProjectBlast

**A tactical tower defense game built on TopDown Engine v4.4**

Unity 6000.2.10f1 | TopDown Engine v4.4 | Feel Feedback System

---

## 🎮 Game Concept

**Genre:** Tactical Tower Defense / RTS Hybrid with Deck-Building

ProjectBlast is a fast-paced hybrid of RTS, puzzle strategy, and tower defense. Build your deck of heroes, deploy them strategically through a vertical lane queue system, and defend your base against waves of enemies. Heroes automatically engage enemies using TDE's AI Brain system, but death is permanent - when a hero falls or runs out of ammo, they're gone for the entire level.

### Core Gameplay Loop
```
Collect Heroes → Build Deck → Deploy from Queue System → 
Auto-Combat with AIBrain → Complete Multi-Stage Levels → 
Earn Rewards → Upgrade Heroes → Progress Through Worlds
```

---

## 🏗️ Current Status

### ✅ Completed Systems

**Phase 1: Foundation (Nov 14-21, 2025)**
- ✅ **Grid System** - 3-zone architecture (Passive → Active → Firing)
- ✅ **Vertical Lane Queue System** - Independent column-based queues with auto-shift
- ✅ **Hero Queue Manager** - Lane-based deployment with smooth animations
- ✅ **Camera System** - Dynamic battlefield targeting with Cinemachine
- ✅ **ScriptableObject Architecture** - HeroDataSO and WeaponDataSO
- ✅ **Grid Manager** - 732-line system with zone management and slot tracking

**Phase 2: Combat Systems (Nov 21-28, 2025)**
- ✅ **Hero Class** - 686-line hero orchestration with TDE integration
- ✅ **AIBrain Integration** - TDE's AI system for automatic combat
- ✅ **Weapon System** - TDE ProjectileWeapon integration
- ✅ **Auto-Targeting** - AIDecisionDetectTargetRadius3D with line-of-sight
- ✅ **Auto-Firing** - AIActionShoot3D with configurable fire rates
- ✅ **Ammo System** - Limited ammo with permanent loss mechanics
- ✅ **Zone-Based Combat** - Heroes only fire when in Firing zone
- ✅ **Health System** - TDE Health component integration

### 🚧 In Progress
- Enemy AI system (using TDE AIBrain)
- Wave spawning system
- Stage progression manager

### 📋 Planned
- UI/HUD system
- Deck building screen
- Meta-game progression
- World map and level selection
- Hero collection and gacha system

---

## 📐 Architecture Overview

### Core Systems

```
┌─────────────────────────────────────────────────────────────┐
│                    GRID SYSTEM (GridManager)                │
│  Passive Grid (Queue) → Active Grid (Ready) → Firing Grid   │
│  3 vertical lanes, heroes move upward through zones         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              HERO QUEUE SYSTEM (HeroQueueManager)           │
│  Lane-based shifting, smooth animations, input blocking     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  HERO COMBAT (Hero.cs + AIBrain)            │
│  • Hero enters Firing zone → AIBrain activates              │
│  • AIDecisionDetectTargetRadius3D finds enemies             │
│  • AIDecisionLineOfSightToTarget3D verifies clear shot      │
│  • AIActionAimWeaponAtTarget3D aims at target               │
│  • AIActionShoot3D fires weapon automatically               │
│  • Ammo consumed → Hero removed when depleted               │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│               DATA LAYER (ScriptableObjects)                │
│  HeroDataSO: Stats, ranges, ammo, weapons                   │
│  WeaponDataSO: Damage, projectiles, effects                 │
│  Single source of truth for all game data                   │
└─────────────────────────────────────────────────────────────┘
```

### Hero Combat Flow

```
1. Hero deployed to Firing Zone
   ↓
2. Hero.StartFiring() → AIBrain.BrainActive = true
   ↓
3. AIBrain executes "Combat" state:
   • AIDecisionDetectTargetRadius3D scans for enemies
   • AIDecisionLineOfSightToTarget3D verifies LOS
   ↓
4. Target acquired:
   • AIActionAimWeaponAtTarget3D rotates weapon
   • CharacterOrientation3D rotates hero body
   ↓
5. AIActionShoot3D fires weapon:
   • ProjectileWeapon fires projectile
   • Ammo consumed per shot
   • Projectile deals damage via DamageOnTouch
   ↓
6. Ammo monitoring:
   • Hero tracks weapon ammo state
   • When depleted → Hero removed from grid
   • Permanent loss for that level
```

---

## 🗂️ Project Structure

```
ProjectBlast/
├── Assets/
│   ├── ProjectBlast/
│   │   ├── Scripts/
│   │   │   ├── Grid/
│   │   │   │   ├── GridManager.cs (732 lines)
│   │   │   │   ├── GridSlot.cs
│   │   │   │   └── GridZone.cs
│   │   │   ├── Heroes/
│   │   │   │   ├── Hero.cs (686 lines)
│   │   │   │   └── HeroQueueManager.cs (504 lines)
│   │   │   ├── Data/
│   │   │   │   ├── HeroDataSO.cs
│   │   │   │   └── WeaponDataSO.cs
│   │   │   ├── Camera/
│   │   │   │   └── BattlefieldCameraTarget.cs
│   │   │   └── Testing/
│   │   │       └── GridManagerTester.cs
│   │   ├── Prefabs/
│   │   │   └── Hero_00.prefab (with AIBrain components)
│   │   └── ScriptableObjects/
│   │       ├── Heroes/
│   │       └── Weapons/
│   ├── TopDownEngine/      (v4.4)
│   └── Feel/               (Feedback system)
├── Documentation/
│   ├── HERO_AIBRAIN_INTEGRATION.md       ⭐ Current combat system
│   ├── HERO_SCRIPTABLEOBJECT_SYSTEM.md
│   ├── SO_SYSTEM_SIMPLIFIED.md
│   └── SO_SYSTEM_CHANGELOG.md
├── GAME_DEVELOPMENT_PLAN.md             📋 Development roadmap
├── GAME_LOOP.md                         🎮 Gameplay design
├── GRID_DEFENSE_ARCHITECTURE.md         🏗️ System architecture
├── VERTICAL_LANE_QUEUE_SYSTEM.md        📐 Queue mechanics
├── TDE_INTEGRATION_GUIDE.md             🔧 TopDown Engine usage
├── QUICK_START.md                       🚀 Setup guide
└── README.md                            📖 This file
```

---

## 📚 Documentation Guide

### Getting Started
1. **[QUICK_START.md](QUICK_START.md)** - First-time setup and creating your first level
2. **[GAME_LOOP.md](GAME_LOOP.md)** - Understanding the gameplay flow and meta-game
3. **[GRID_DEFENSE_ARCHITECTURE.md](GRID_DEFENSE_ARCHITECTURE.md)** - Core system architecture

### Core Systems
4. **[VERTICAL_LANE_QUEUE_SYSTEM.md](VERTICAL_LANE_QUEUE_SYSTEM.md)** - How the queue system works
5. **[TDE_INTEGRATION_GUIDE.md](TDE_INTEGRATION_GUIDE.md)** - Using TopDown Engine components
6. **[Documentation/HERO_AIBRAIN_INTEGRATION.md](Documentation/HERO_AIBRAIN_INTEGRATION.md)** - Hero combat with AIBrain ⭐

### Data & Configuration
7. **[Documentation/SO_SYSTEM_SIMPLIFIED.md](Documentation/SO_SYSTEM_SIMPLIFIED.md)** - ScriptableObject architecture
8. **[Documentation/HERO_SCRIPTABLEOBJECT_SYSTEM.md](Documentation/HERO_SCRIPTABLEOBJECT_SYSTEM.md)** - HeroDataSO guide

### Development
9. **[GAME_DEVELOPMENT_PLAN.md](GAME_DEVELOPMENT_PLAN.md)** - Roadmap and phase breakdown
10. **[RECIPES.md](RECIPES.md)** - Common patterns and code snippets

---

## 🚀 Quick Start

### Prerequisites
- Unity 6000.2.10f1
- TopDown Engine v4.4 (already in project)
- Feel v4.0+ (already in project)

### Running the Project

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd ProjectBlast
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Add project from disk
   - Select Unity 6000.2.10f1

3. **Test the Grid System**
   - Open scene: `Assets/ProjectBlast/Scenes/GridTest.unity`
   - Press Play
   - Click heroes in Active grid to deploy them to Firing grid
   - Watch automatic lane shifting

4. **Create Your First Hero**
   - See [QUICK_START.md](QUICK_START.md) for detailed instructions
   - Use [Documentation/HERO_AIBRAIN_INTEGRATION.md](Documentation/HERO_AIBRAIN_INTEGRATION.md) for combat setup

---

## 🎯 Key Features

### Grid System
- **3-Zone Architecture:** Passive (queue) → Active (ready) → Firing (combat)
- **Vertical Lane Queues:** Each column is an independent queue
- **Auto-Shift Logic:** Heroes automatically move up when hero above deploys
- **Smooth Animations:** Lerp-based movement with configurable timing
- **Visual Debugging:** Color-coded gizmos for all zones and slots

### Hero Combat (AIBrain Integration)
- **Automatic Targeting:** AIDecisionDetectTargetRadius3D scans for enemies
- **Line-of-Sight:** AIDecisionLineOfSightToTarget3D prevents shooting through walls
- **Auto-Aim:** AIActionAimWeaponAtTarget3D rotates weapon to target
- **Auto-Fire:** AIActionShoot3D fires weapon based on configured fire rate
- **Zone Control:** Heroes only engage enemies when in Firing zone
- **Ammo System:** Limited ammo with permanent loss when depleted
- **Inspector Configuration:** All AI behavior configured per hero in Unity Inspector

### Data-Driven Design
- **HeroDataSO:** Single source of truth for all hero stats
- **WeaponDataSO:** Weapon configuration and projectile data
- **Inspector-Free Hero Class:** All stats read from ScriptableObjects
- **Easy Balancing:** Change stats in SO without touching code

---

## 🔧 Development Tools

### Testing
- **GridManagerTester.cs** - Test grid operations in editor
- **Visual Gizmos** - Color-coded zone visualization
- **Debug Logging** - Comprehensive logging throughout systems

### Documentation
- **Inline Comments** - Detailed XML documentation in code
- **Architecture Diagrams** - ASCII diagrams in markdown docs
- **Flow Charts** - Combat flow and data flow diagrams

---

## 📝 Recent Updates

### November 28, 2025
- ✅ **Major Refactor:** Integrated AIBrain system into Hero.cs
- ✅ **Removed:** WeaponAutoAim/WeaponAutoShoot approach (~100 lines)
- ✅ **Added:** ConfigureAI() method for applying HeroDataSO stats to AI components
- ✅ **Added:** Zone-based AIBrain activation/deactivation
- ✅ **Added:** Comprehensive documentation (HERO_AIBRAIN_INTEGRATION.md)
- ✅ **Fixed:** All compilation errors - clean build achieved

### November 27, 2025
- ✅ Fixed double weapon spawning bug
- ✅ Implemented WeaponAutoAim3D with line-of-sight checking
- ✅ Added ammo tracking and permanent loss mechanics

### November 21-26, 2025
- ✅ Initial hero combat system with manual targeting
- ✅ ScriptableObject architecture implementation
- ✅ Weapon equipping and auto-aim systems

---

## 🤝 Contributing

This is a personal project, but the architecture is designed to be extensible. Key extension points:

- **Hero Classes:** Extend HeroClass enum and create new HeroDataSO assets
- **Weapon Types:** Create new weapon prefabs with ProjectileWeapon component
- **AI States:** Add new AIBrain states in Unity Inspector for custom behaviors
- **Grid Zones:** Modify GridManager to add additional zones
- **Enemy Types:** Use TDE Character + AIBrain for new enemy behaviors

---

## 📄 License

This project uses TopDown Engine v4.4 (commercial license required) and Feel feedback system. 
Project code in `Assets/ProjectBlast/` is [your license here].

---

## 🔗 Links

- **TopDown Engine Documentation:** https://topdown-engine-docs.moremountains.com/
- **Feel Documentation:** https://feel-docs.moremountains.com/
- **Unity Documentation:** https://docs.unity3d.com/

---

**Last Updated:** November 28, 2025  
**Project Status:** Phase 2 (Combat Systems) - Hero AIBrain Integration Complete
