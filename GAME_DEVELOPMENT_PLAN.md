# ProjectBlast - Game Development Plan

## Project Overview
**Project Name:** ProjectBlast  
**Engine:** TopDown Engine v4.4  
**Unity Version:** 6000.2.10f1  
**Start Date:** November 14, 2025

---

## Phase 1: Planning & Design ⏱️ Week 1

### Game Concept
- [ RTS, puzzle, tower-defense battler] Define game genre and style
- [ This game is a fast-paced hybrid of RTS, puzzle strategy, and tower defense. Your heroes enter the battlefield in a shifting queue across Passive and Active grids, and with each tap you deploy them into the Firing Grid where they automatically attack approaching enemies. Different hero classes counter different enemy types, and every deployment becomes a tactical choice as waves push toward your base. Manage your limited hero queue, build the right firing lineup, and survive escalating waves before the enemies breach your defenses.] Write one-paragraph game description
- [ Queue system, Auto aim and auto fire, merge 3 similar troops if in the firing grid] List 3-5 core mechanics
- [ 3D] Decide 2D or 3D
- [ Android, iOs, PC] Define target platform

**Game Description:**
This game is a fast-paced hybrid of RTS, puzzle strategy, and tower defense. Your heroes enter the battlefield in a shifting queue across Passive and Active grids, and with each tap you deploy them into the Firing Grid where they automatically attack approaching enemies. Different hero classes counter different enemy types, and every deployment becomes a tactical choice as waves push toward your base. Manage your limited hero queue, build the right firing lineup, and survive escalating waves before the enemies breach your defenses.```

```

**Core Mechanics:**
1. Multi-Layered Player Grid System 
2. Hero Queue & Deployment
3. Automatic Combat System
4. Enemy Waves & Lane Pressure
5. Hero Classes & Counters
6. Tactical Deployment Decisions
7. Resource & Queue Management
8. Base Defense & Firing Grid Health

---

## Phase 2: Prototype Setup ⏱️ Week 1-2

### Important Note for Your Game Type
⚠️ **Your game is a tactical tower defense/RTS hybrid, NOT a traditional character-controlled game!**
You'll need to heavily adapt TopDown Engine or consider if it's the right fit. 

**TopDown Engine is optimized for:**
- Player-controlled characters (not stationary heroes)
- Real-time movement and combat
- AI that chases and attacks players

**Your game needs:**
- Grid-based positioning system
- Mouse/touch click deployment
- Stationary heroes that auto-fire
- Wave-based enemy spawning
- Queue management system

### Recommended Approach
Consider two paths:
1. **Use TopDown Engine selectively** - Use only weapon system, health, and AI components
2. **Build custom systems** - Create your own grid, queue, and deployment systems

### Scene Structure (Adapted for Your Game)
- [ ] Create main battlefield scene
- [ ] Set up GameManager (score, waves, game state)
- [ ] Create custom GridManager for 3-zone system
- [ ] Create HeroQueueManager
- [ ] Set up UI for hero deployment
- [ ] Test basic scene flow

### Grid System Setup (Custom - Most Important!)
- [ ] Create PassiveGrid zone (visual + data structure)
- [ ] Create ActiveGrid zone
- [ ] Create FiringGrid zone
- [ ] Implement grid position data structure
- [ ] Add visual grid indicators
- [ ] Test grid zone detection

### Hero System (Adapted from Character)
- [ ] Create Hero base class (may extend Character or custom)
- [ ] Add hero stats (damage, fire rate, health)
- [ ] Implement hero classes (different types)
- [ ] Create hero prefabs for each class
- [ ] Add hero visual models
- [ ] Test hero instantiation

### Basic Environment
- [ ] Create battlefield 3D environment
- [ ] Define enemy approach lanes
- [ ] Create base/defense point
- [ ] Add visual elements (ground, barriers)
- [ ] Set up camera position (likely fixed/isometric)
- [ ] Test environment layout

---

## Phase 3: Core Mechanics ⏱️ Week 2-3

### Movement & Controls
- [ ] Implement basic movement
- [ ] Add dash/dodge ability
- [ ] Add jump (if needed)
- [ ] Fine-tune movement speed
- [ ] Add movement feedback (particles, sounds)

### Combat System
- [ ] Create first weapon
- [ ] Add CharacterHandleWeapon ability
- [ ] Create projectiles
- [ ] Implement shooting mechanics
- [ ] Add weapon feedback effects

### Health & Damage
- [ ] Add Health component to player
- [ ] Create damage sources
- [ ] Implement hit reactions
- [ ] Add death and respawn
- [ ] Create health UI

---

## Phase 4: Enemy AI ⏱️ Week 3-4

### Basic Enemies
- [ ] Create enemy character prefab
- [ ] Add AIBrain component
- [ ] Implement patrol AI
- [ ] Add chase behavior
- [ ] Add attack behavior

### Enemy Types
- [ ] Melee enemy
- [ ] Ranged enemy
- [ ] Tank enemy (optional)
- [ ] Boss enemy (optional)

---

## Phase 5: Level Design ⏱️ Week 4-5

### Level 1 - Tutorial
- [ ] Design level layout
- [ ] Place enemies strategically
- [ ] Add collectibles
- [ ] Create checkpoints
- [ ] Add tutorial prompts

### Level 2-3
- [ ] Increase difficulty
- [ ] Introduce new enemy types
- [ ] Add environmental hazards
- [ ] Create level goals

---

## Phase 6: Systems & Polish ⏱️ Week 5-6

### UI/UX
- [ ] Main menu
- [ ] Pause menu
- [ ] HUD (health, ammo, score)
- [ ] Level complete screen
- [ ] Game over screen

### Audio
- [ ] Background music
- [ ] Weapon sounds
- [ ] Hit sounds
- [ ] UI sounds
- [ ] Ambient sounds

### Feel & Juice
- [ ] Camera shake
- [ ] Screen flash
- [ ] Hit particles
- [ ] Death explosions
- [ ] Victory effects

---

## Phase 7: Content Creation ⏱️ Week 6-8

### Weapons
- [ ] Weapon #1: [Name]
- [ ] Weapon #2: [Name]
- [ ] Weapon #3: [Name]
- [ ] Balance weapon stats

### Power-ups
- [ ] Health pickup
- [ ] Ammo pickup
- [ ] Speed boost
- [ ] Shield/Invincibility

### Levels
- [ ] Complete Level 1
- [ ] Complete Level 2
- [ ] Complete Level 3
- [ ] Add more as needed

---

## Phase 8: Testing & Balancing ⏱️ Week 8-9

### Gameplay Testing
- [ ] Test all levels
- [ ] Balance difficulty
- [ ] Fix bugs
- [ ] Optimize performance
- [ ] Get playtester feedback

### Polish Pass
- [ ] Improve visuals
- [ ] Enhance audio
- [ ] Add more feedback
- [ ] Improve UI/UX

---

## Phase 9: Build & Deploy ⏱️ Week 9-10

### Build Setup
- [ ] Configure build settings
- [ ] Test builds on target platform
- [ ] Optimize build size
- [ ] Add splash screens

### Release
- [ ] Create promotional materials
- [ ] Write game description
- [ ] Prepare screenshots/videos
- [ ] Publish to platform

---

## Resources & References

### TopDown Engine Documentation
- Main Docs: https://topdown-engine-docs.moremountains.com/
- API Reference: https://topdown-engine-docs.moremountains.com/API/
- Feel Docs: https://feel-docs.moremountains.com/

### Demo Scenes to Study
- **Minimal2D/3D**: Basic setup examples
- **Koala2D**: Complete 2D game example
- **Loft3D**: Complete 3D game example
- **Explodudes**: Twin-stick shooter
- **Deadline**: Story-based game with progression

### Key Scripts to Understand
- `Character.cs` - Core character system
- `CharacterAbility.cs` - Ability base class
- `GameManager.cs` - Game state management
- `LevelManager.cs` - Level and spawn management
- `Weapon.cs` - Weapon system
- `Health.cs` - Health and damage

---

## Daily Development Log

### Week 1
**Day 1:**
- 

**Day 2:**
- 

**Day 3:**
- 

(Continue logging your progress)

---

## Notes & Ideas
```
[Add your notes, ideas, and discoveries here]
```

---

## Troubleshooting

### Common Issues
1. **Player not moving:** Check InputManager is set up, Character has movement ability
2. **Weapons not shooting:** Verify CharacterHandleWeapon is added, weapon is equipped
3. **AI not working:** Ensure AIBrain and AIActions are configured
4. **Scenes not loading:** Check scene is added to Build Settings

### Performance Tips
- Use object pooling for projectiles
- Disable `RunAnimatorSanityChecks` in production
- Optimize collision layers
- Use occlusion culling for 3D

---

## Success Metrics

### Minimum Viable Product (MVP)
- [ ] Player can move and shoot
- [ ] Enemies spawn and attack
- [ ] Health and damage work
- [ ] At least 1 complete level
- [ ] Basic UI and menus

### Full Release
- [ ] 3-5 polished levels
- [ ] 3+ enemy types
- [ ] 2-3 weapons
- [ ] Complete UI/menus
- [ ] Background music and SFX
- [ ] Tested and bug-free
