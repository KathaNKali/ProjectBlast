# ProjectBlast - Game Loop & High-Level Gameplay Design

**Last Updated:** November 21, 2025

---

## 🎮 Core Game Loop

```
┌─────────────────────────────────────────────────────────────┐
│                        MAIN MENU                            │
│  - Continue Story                                           │
│  - Hero Collection                                          │
│  - Deck Builder                                             │
│  - Shop                                                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                      WORLD MAP                              │
│  World 1 → World 2 → World 3 → ... → Final World          │
│  (Unlock worlds by completing previous world)              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    LEVEL SELECT                             │
│  Level 1-1 ★★★  (3 stages)                                │
│  Level 1-2 ★★☆  (4 stages)                                │
│  Level 1-3 🔒    (5 stages) - LOCKED                       │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   DECK BUILDER                              │
│  Select Heroes for Battle (3-10 slots based on progress)   │
│  [Knight] [Archer] [Mage] [Empty] [Empty]                 │
│  ↑ This is your LIMITED POOL for the entire level          │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    BATTLE START                             │
│  Level: Forest Ambush (World 1, Level 2)                   │
│  Stages: 4                                                  │
│  Difficulty: Normal                                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   STAGE 1 - BATTLE                          │
│  ┌─ Hero Pool UI ─────────────────────────────┐           │
│  │ Available: [Knight][Archer][Mage]         │           │
│  │ Deployed:  0/3                             │           │
│  └────────────────────────────────────────────┘           │
│                                                             │
│  [Enemies approach from top]                                │
│  Player deploys heroes: Passive → Active → Firing          │
│  Heroes auto-fire at enemies                                │
│  Enemies move toward base                                   │
│                                                             │
│  Stage Complete: All enemies defeated ✓                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│              STAGE TRANSITION (3-5 sec pause)               │
│                                                             │
│             ★ STAGE 2 ★                                     │
│          "Reinforcements Incoming!"                         │
│                                                             │
│  Heroes remain in Firing Grid                               │
│  Hero Pool shows: Deployed 2/3 (Knight died in Stage 1)   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                   STAGE 2 - BATTLE                          │
│  ┌─ Hero Pool UI ─────────────────────────────┐           │
│  │ Available: [Mage]                          │           │
│  │ Deployed:  2/3 (Knight permanently lost)   │           │
│  └────────────────────────────────────────────┘           │
│                                                             │
│  Harder enemy wave spawns                                   │
│  Continue battling with remaining heroes                    │
│                                                             │
│  If Archer dies → Only Mage left (1/3 deployed)           │
│  If Mage dies → All heroes dead → LEVEL FAILED ❌         │
└─────────────────────────────────────────────────────────────┘
                            ↓
                    ┌───────┴───────┐
                    ↓               ↓
          ┌──────────────┐  ┌──────────────┐
          │   VICTORY    │  │    DEFEAT    │
          │  (All stages │  │ (Base/Heroes │
          │  completed)  │  │  destroyed)  │
          └──────────────┘  └──────────────┘
                    ↓               ↓
          ┌──────────────┐  ┌──────────────┐
          │   REWARDS    │  │   RESTART    │
          │  - Gold      │  │   Level      │
          │  - Hero XP   │  │  from Stage  │
          │  - Chests    │  │      1       │
          │  - Items     │  └──────────────┘
          └──────────────┘
                    ↓
          ┌──────────────────────┐
          │  Return to World Map │
          │  (Next level unlock) │
          └──────────────────────┘
```

---

## 🗂️ Game Structure

### **Meta Layer (Outside Battle)**

#### **1. Hero Collection System**
- **Hero Roster:** Players collect heroes from rewards, chests, shop
- **Hero Progression:** 
  - Level up heroes with XP
  - Evolve/Upgrade heroes with materials
  - Unlock hero abilities
- **Hero Types:** Different classes (Tank, DPS, Support, Ranged, Melee)

#### **2. Deck Building System**
- **Deck Slots:** Start with 3 slots, unlock more through progression
  ```
  Early Game:  [Slot 1][Slot 2][Slot 3][🔒][🔒][🔒][🔒][🔒]
  Mid Game:    [Slot 1][Slot 2][Slot 3][Slot 4][Slot 5][🔒][🔒][🔒]
  End Game:    [Slot 1][Slot 2][...[Slot 8][Slot 9][Slot 10]
  ```
- **Deck = Battle Pool:** The heroes in your deck are ALL the heroes you have for that level
- **Strategic Choices:** 
  - Balanced deck (Tank + DPS + Support)
  - Counter-specific enemies (All ranged vs flying enemies)
  - High-risk high-reward (All glass cannon DPS)

#### **3. World & Level Progression**
```
World 1: Tutorial Lands (5 levels)
├─ Level 1-1: First Blood (2 stages) ⭐⭐⭐
├─ Level 1-2: Forest Path (3 stages) ⭐⭐☆
├─ Level 1-3: River Crossing (4 stages) ⭐☆☆
├─ Level 1-4: Mountain Pass (4 stages) 🔒
└─ Level 1-5: Boss - Forest Guardian (5 stages) 🔒

World 2: Desert Kingdom (6 levels)
├─ Level 2-1: Sandstorm (3 stages) 🔒
├─ Level 2-2: Oasis Defense (4 stages) 🔒
...

World 3: Frozen Tundra (7 levels)
World 4: Volcanic Depths (8 levels)
World 5: Sky Citadel (10 levels)
World 6: Final Confrontation (12 stages - ONE epic level)
```

**Unlock Logic:**
- Complete Level 1-1 → Unlock Level 1-2
- Complete all of World 1 → Unlock World 2
- Star ratings affect optional content unlocks

---

## ⚔️ Battle System (Inside Level)

### **Pre-Battle Phase**

**1. Deck Selection:**
- Choose heroes from collection to fill available deck slots
- View level intel: Enemy types, stage count, recommended heroes

**2. Level Start:**
- Heroes from deck populate the **Passive Grid** queue
- UI shows **Hero Pool**: "Available: 3/3" or "5/5" depending on deck size
- Battle begins with Stage 1

---

### **Battle Phase (Per Stage)**

#### **Grid System:**
```
     [Enemies spawn and move downward ↓]
            ↓ ↓ ↓ ↓ ↓
┌──────────────────────────────────────┐
│     FIRING GRID (Combat Zone)       │ ← Heroes auto-fire at enemies
│     [H1][H2][  ][  ][  ][  ]        │
│     [  ][  ][  ][  ][  ][  ]        │
└──────────────────────────────────────┘
            ↑ Deploy (tap)
┌──────────────────────────────────────┐
│     ACTIVE GRID (Ready Zone)        │ ← Player taps to deploy
│     [H3][  ][  ]                     │
└──────────────────────────────────────┘
            ↑ Auto-shift from Passive
┌──────────────────────────────────────┐
│   PASSIVE GRID (Queue/Reserve)      │ ← Vertical lane queues
│     [H4][  ][  ]                     │
│     [H5][  ][  ]                     │
└──────────────────────────────────────┘
            ↑
    [Your Base/Castle] ← Enemies target this
```

#### **Hero Flow (Vertical Lane System):**
1. **Start:** All deck heroes in Passive Grid (distributed across lanes)
2. **Auto-Shift:** When Active slot empty → Passive hero moves up (same lane)
3. **Deploy:** Player taps Active hero → Moves to Firing Grid (leftmost slot)
4. **Combat:** Heroes in Firing auto-target and shoot enemies
5. **Limited Pool:** Once all heroes deployed, no more reinforcements

#### **Hero State Tracking:**
```
Hero Pool UI:
┌────────────────────────────────────┐
│ Available: [Knight][Archer][Mage] │ ← In Passive/Active
│ Deployed:  2/5                     │ ← In Firing Grid
│ Lost:      [Tank][Healer]          │ ← Dead (greyed out)
└────────────────────────────────────┘
```

#### **Combat Mechanics:**
- **Auto-Targeting:** Heroes use TDE's `WeaponAutoAim3D` to target nearest enemy
- **Auto-Firing:** Heroes continuously fire at enemies in range
- **Hero Resources:**
  - **Health:** Hero dies when HP reaches 0
  - **Ammo:** Hero becomes useless when ammo depleted (runs out of bullets)
  - Both conditions = **Permanent Loss** for that level

#### **Enemy Mechanics:**
- Spawn at top of battlefield in waves
- Move downward toward base using TDE's AI (AIBrain + AIActionMoveTowardsTarget3D)
- Different enemy types: Fast, Tank, Flying, Armored
- Deal damage to base when reaching it

---

### **Stage Progression Within Level**

#### **Stage Complete Condition:**
All enemies in current stage defeated

#### **Stage Transition (3-5 second pause):**
```
┌────────────────────────────────────┐
│                                    │
│         ★ STAGE 2 / 4 ★           │
│    "Enemy Reinforcements!"         │
│                                    │
│   Heroes remain in positions       │
│   Get ready for next wave...       │
│                                    │
└────────────────────────────────────┘
```

**What Happens:**
- ✅ Heroes stay in Firing Grid
- ✅ Dead heroes stay dead (not respawned)
- ✅ Hero Pool UI updates: "Available: 1/5, Deployed: 2/5, Lost: 2/5"
- ✅ Brief pause to catch breath
- ✅ Next stage enemies spawn after countdown

---

### **Victory Conditions**

**Level Victory (Win):**
- ✅ All stages in level completed
- ✅ All enemies in final stage defeated
- ✅ Base still standing (HP > 0)

**Does NOT require:**
- ❌ All heroes alive (heroes can die)
- ❌ Perfect performance
- ❌ Time limit
- ❌ Special objectives (those are optional)

**Proceed to Rewards Screen**

---

### **Defeat Conditions (Fail)**

**Level Defeat happens if EITHER:**

1. **Base Destroyed:**
   - Enemies reach base and reduce Base HP to 0
   - Game Over → Restart from Stage 1

2. **All Heroes Lost:**
   - All heroes in deck either:
     * Killed (HP = 0)
     * Out of ammo (can't fight anymore)
   - No heroes in Passive/Active queue
   - No heroes in Firing Grid (or all useless)
   - Game Over → Restart from Stage 1

**Penalty:**
- No checkpoints within level
- Must restart entire level from Stage 1
- No rewards for partial completion

---

## 🎁 Rewards & Progression System

### **Victory Rewards**

**Earned After Level Completion:**

#### **1. Guaranteed Rewards:**
```
Gold:     100-500 (scales with level difficulty)
Hero XP:  Split among heroes used in battle
          - Heroes that survived get MORE XP
          - Dead heroes get reduced XP (50%)
```

#### **2. Star Rating (Optional Objectives):**
```
⭐ Basic Victory: Complete all stages
⭐⭐ Good Victory: + No heroes lost
⭐⭐⭐ Perfect Victory: + No base damage + Under time par
```

**Star Benefits:**
- Unlock bonus chests
- Required for optional side content
- Bragging rights

#### **3. Chests (Loot Boxes):**

**Bronze Chest** (Common reward):
- Common hero cards
- Small gold amount
- Basic equipment

**Silver Chest** (2-star victory):
- Rare hero cards
- Medium gold amount
- Uncommon equipment

**Gold Chest** (3-star victory):
- Epic hero cards
- Large gold amount
- Rare equipment
- Hero evolution materials

#### **4. First-Time Clear Bonus:**
- Extra large gold bonus
- Guaranteed hero card
- Story progression unlock

---

### **Reward Types Detail**

#### **A. Hero Cards:**
- **New Heroes:** Unlock heroes you don't own
- **Duplicate Heroes:** Convert to hero shards/XP for upgrading existing heroes

#### **B. Currency:**
- **Gold:** Buy items in shop, upgrade heroes
- **Gems:** Premium currency for special purchases, continues, deck slots

#### **C. Equipment:**
- Weapons (increase damage)
- Armor (increase health)
- Accessories (special effects: +range, +fire rate, +ammo)

#### **D. Evolution Materials:**
- Hero-specific items needed to evolve heroes to next tier
- Example: Knight Emblem (evolve Knight to Paladin)

---

## 📈 Progression Systems

### **1. Player Level**
- Gain XP from completing levels
- Unlock features as player level increases:
  - Level 5: Unlock equipment system
  - Level 10: Unlock 4th deck slot
  - Level 15: Unlock hero evolution
  - Level 20: Unlock 5th deck slot
  - Etc.

### **2. Hero Progression**
- **Hero Level:** Level 1-50, gained through battle XP
- **Hero Evolution:** Tier 1 → Tier 2 → Tier 3 (requires materials)
- **Hero Skills:** Unlock special abilities at levels 10, 20, 30

### **3. Deck Expansion**
```
Starting Deck: 3 slots
- Complete World 1 → 4th slot
- Player Level 10 → 5th slot
- Complete World 2 → 6th slot
- Player Level 20 → 7th slot
- Complete World 3 → 8th slot
- Player Level 30 → 9th slot
- Complete World 4 → 10th slot (max)
```

### **4. World Unlocks**
- Linear progression through worlds
- Must complete previous world to unlock next
- Each world introduces new mechanics, enemies, heroes

---

## 🎯 Strategic Depth

### **Deck Building Strategy**

**Example Scenarios:**

**Scenario 1: Boss Level with Single Target**
```
Deck: [5x High DPS Heroes]
Strategy: All damage dealers to burn down boss quickly
Risk: If heroes die, no backup/sustain
```

**Scenario 2: Swarm Level with Many Weak Enemies**
```
Deck: [2x AOE Heroes][2x Ranged][1x Tank]
Strategy: AOE clears groups, tank protects backline
Risk: Weak against armored single targets
```

**Scenario 3: Mixed Enemy Types**
```
Deck: [Tank][Healer][Ranged DPS][Melee DPS][AOE Mage]
Strategy: Balanced, adapts to any situation
Risk: Jack of all trades, master of none
```

---

### **Resource Management During Battle**

**Considerations:**

1. **When to Deploy?**
   - Deploy early → More firepower but heroes exposed longer
   - Deploy late → Save heroes for harder stages but might lose base

2. **Which Lane to Deploy From?**
   - Each lane has different heroes in queue
   - Strategic choice of which lane to pull from

3. **Hero Positioning in Firing Grid**
   - Leftmost fill = predictable
   - Tanks in front, DPS in back?

4. **Ammo Conservation:**
   - Heroes with limited ammo might not last entire level
   - Need to manage when they're deployed

---

## 🔄 Session Loop (Play Session)

**Typical 30-Minute Session:**

```
1. Player opens game (0:00)
2. Collect daily rewards (0:30)
3. Check new heroes/chests (1:00)
4. Build deck for next level (2:00)
5. Play Level 2-3 (5:00-10:00)
   - Stage 1: Deploy 2 heroes, clear wave
   - Stage 2: Deploy 1 more hero, tougher enemies
   - Stage 3: 1 hero dies, struggling
   - Stage 4: Victory with 2 heroes remaining!
6. Collect rewards screen (10:30)
7. Open silver chest (11:00)
8. Get new Epic hero! (11:30)
9. Upgrade heroes with gold/XP (13:00)
10. Play Level 2-4 (15:00-22:00)
    - DEFEAT on Stage 3 (all heroes lost)
11. Retry Level 2-4 with different deck (22:00-28:00)
    - Victory!
12. Save and quit (30:00)
```

---

## 🚀 Future Expansion Ideas

### **Additional Systems (To Be Decided):**

1. **PvP Arena:**
   - Players compete with their decks
   - Asynchronous battles or live matches?

2. **Daily Challenges:**
   - Special levels with unique modifiers
   - Time-limited rewards

3. **Guild System:**
   - Join clans, share heroes
   - Cooperative raid bosses

4. **Hero Fusion:**
   - Combine duplicate heroes for power-ups
   - Merge mechanic in Firing Grid?

5. **Equipment Crafting:**
   - Collect materials, craft gear
   - Enhance equipment with upgrades

6. **Endless Mode:**
   - Survive infinite waves
   - Leaderboards for highest stage reached

7. **Story Cutscenes:**
   - Narrative between worlds
   - Hero backstories

---

## 📊 Key Design Pillars

### **1. Tactical Decision Making**
Every deployment matters since heroes are limited and permanent when lost.

### **2. Risk vs Reward**
Deploy heroes early for safety, or save them for harder stages?

### **3. Deck Building Mastery**
Success requires smart pre-battle deck composition.

### **4. Progressive Challenge**
Each stage within a level gets progressively harder.

### **5. Meaningful Losses**
No mid-level checkpoints means failure has weight.

### **6. Collection & Upgrades**
Long-term progression through hero collection and upgrades.

---

## 🎮 Core Loop Summary

```
Collect Heroes → Build Deck → Play Level → 
Complete Stages → Earn Rewards → Unlock Content → 
Upgrade Heroes → Build Better Deck → Play Harder Level → 
Repeat → Complete Story
```

**Session Goals:**
- Short term: Clear next level
- Medium term: Complete current world
- Long term: Collect all heroes, complete story

**Engagement Hooks:**
- Hero collection (gacha-style)
- Deck experimentation
- Level mastery (3-star ratings)
- Story progression
- Hero upgrades

---

**This game loop balances:**
- Strategic depth (deck building, deployment timing)
- Action gameplay (tower defense combat)
- Progression systems (hero collection, upgrades)
- Risk management (limited hero pool, permanent loss)
- Replayability (different decks, star challenges)

---

**Status:** ✅ Core loop defined, ready for implementation planning
**Next Steps:** Prioritize features for MVP, create detailed implementation plan
