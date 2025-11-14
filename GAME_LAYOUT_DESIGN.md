# Game Layout & Visual Design
## Grid Defense - Screen Layout & Flow

---

## 🎮 Core Layout Concept

### **Camera Perspective Options**

#### **Top-Down View (Your Game Layout)** ⭐
```
    ╔═══════════════════════════════════════════╗
    ║  SCORE: 1,250    WAVE: 3/10    GOLD: 450 ║
    ╠═══════════════════════════════════════════╣
    ║                  TOP                      ║
    ║         ENEMY SPAWN ZONES                 ║
    ║     👹  👹  👹  👹  👹  👹               ║
    ║      ↓   ↓   ↓   ↓   ↓   ↓               ║
    ║   ┌──────────────────────────┐            ║
    ║   │  BATTLEFIELD / LANES     │            ║
    ║   │                          │            ║
    ║   │   🧟 ↓  🧟 ↓  🧟 ↓      │  Enemies   ║
    ║   │      ↓     ↓     ↓      │  move      ║
    ║   │   🧟 ↓  🧟 ↓  🧟 ↓      │  downward  ║
    ║   │      ↓     ↓     ↓      │            ║
    ║   │   🧟 ↓  🧟 ↓  🧟 ↓      │            ║
    ║   │      ↑     ↑     ↑      │            ║
    ║   │    💥↑   💥↑   💥↑      │  Projectiles║
    ║   │    💥↑   💥↑   💥↑      │  fire up   ║
    ║   └──────────────────────────┘            ║
    ║   ╔═════════════════════╗                 ║
    ║   ║   FIRING GRID       ║  ← Heroes here  ║
    ║   ║  [⚔️] [🏹] [🔮]     ║    fire upward  ║
    ║   ║  [🛡️] [💤] [⚔️]     ║    at enemies   ║
    ║   ║  [🏹] [🔮] [⚔️]     ║                 ║
    ║   ╚═════════════════════╝                 ║
    ║   ┌─────────────────────┐                 ║
    ║   │   ACTIVE GRID       │  ← Tap hero     ║
    ║   │  [⚔️] [🏹] [🔮]     │    then tap     ║
    ║   │  [🛡️] [⚔️] [🏹]     │    Firing slot  ║
    ║   └─────────────────────┘                 ║
    ║   ┌─────────────────────┐                 ║
    ║   │  PASSIVE GRID       │  ← Queue shifts ║
    ║   │  [⚔️] [🏹] [🛡️]     │    upward       ║
    ║   │  [🔮] [⚔️] [🏹]     │                 ║
    ║   └─────────────────────┘                 ║
    ║                BOTTOM                     ║
    ║   🏰 BASE HEALTH: ████████░░ 80%          ║
    ╚═══════════════════════════════════════════╝
    
    FLOW:
    👹 Enemies spawn at TOP, move ↓ DOWNWARD
    💥 Heroes fire ↑ UPWARD from Firing Grid
    📦 Queue shifts ↑ UPWARD (Passive→Active→Firing)
    🏰 Base at BOTTOM (enemies reach = damage)
```

#### **Mobile Portrait Layout** (Same top-down principle)
```
    ┌─────────────────┐
    │ Wave 3  💎 450  │  ← Compact HUD
    ├─────────────────┤
    │ 👹👹👹 SPAWNS   │  ← TOP: Enemy spawn
    │   ↓  ↓  ↓      │
    │                 │
    │  🧟  🧟  🧟    │  ← Enemies move down
    │   ↓  ↓  ↓      │
    │  🧟     🧟     │
    │   ↓     ↓      │
    │  💥↑ 💥↑ 💥↑  │  ← Projectiles fire up
    │                 │
    │ ╔═════════════╗ │
    │ ║[⚔️][🏹][🔮]║ │  ← FIRING GRID
    │ ║[🛡️][💤][⚔️]║ │    (heroes attack)
    │ ╚═════════════╝ │
    │ [⚔️][🏹][🔮]   │  ← ACTIVE (tap)
    │ [🛡️][⚔️][🏹]   │
    │                 │
    │ [⚔️][🏹][�️]   │  ← PASSIVE (auto)
    │ [�🔮][⚔️][🏹]   │
    │                 │
    │ 🏰 Base: 80%   │  ← BOTTOM: Your base
    └─────────────────┘
```

#### **PC/Tablet Landscape Layout**
```
    ┌─────────────────────────────────────────────────┐
    │ 💎 450  Wave: 3/10  🏰 Base: 80%    [⏸️] [⚙️] │
    ├─────────────────────────────────────────────────┤
    │    TOP                              SIDE PANEL  │
    │  👹👹👹 SPAWNS 👹👹👹             ┌──────────┐│
    │    ↓  ↓  ↓  ↓  ↓                  │ Selected ││
    │                                    │ Hero:    ││
    │  🧟  🧟  🧟  🧟  🧟               │          ││
    │   ↓   ↓   ↓   ↓   ↓               │ ⚔️ Warrior││
    │  🧟     🧟     🧟                 │ Lvl 2    ││
    │   ↓     ↓     ↓                   │          ││
    │  💥↑  💥↑  💥↑                    │ HP: 100  ││
    │  💥↑  💥↑  💥↑                    │ ATK: 25  ││
    │                                    │ Range: 5 ││
    │ ╔═══════════════════╗              │          ││
    │ ║ [⚔️] [🏹] [🔮]    ║ FIRING       │ [Merge]  ││
    │ ║ [🛡️] [💤] [⚔️]    ║ GRID         └──────────┘│
    │ ║ [🏹] [🔮] [⚔️]    ║                          │
    │ ╚═══════════════════╝                          │
    │                                                 │
    │ [⚔️][🏹][🔮]  ACTIVE (Ready to deploy)        │
    │ [🛡️][⚔️][🏹]                                  │
    │                                                 │
    │ [⚔️][🏹][🛡️]  PASSIVE (Waiting queue)         │
    │ [🔮][⚔️][🏹]                                   │
    │    BOTTOM                                       │
    │ 🏰════════════════ YOUR BASE ══════════════🏰  │
    └─────────────────────────────────────────────────┘
```

---

## 🎯 Layout Components Breakdown

### **1. Enemy Spawn & Approach Area**
**Location:** Top of screen  
**Purpose:** Enemies spawn here and move DOWNWARD toward player base

**Visual Elements:**
- Multiple spawn points (3-5 lanes) at top edge
- Vertical lanes/paths showing downward routes
- Particle effects at spawn points
- Distance/progress markers (optional)

**Enemy Movement:**
```
Spawn at Y = +20 (top)
  ↓
Move downward (Y decreases)
  ↓
If reaches Y = -5 (bottom) → Damage base
```

**Unity Implementation:**
```
GameObject: "EnemySpawnManager"
├── SpawnPoint_Lane1 (Transform) - Position: (x: -4, y: 0, z: 20)
├── SpawnPoint_Lane2 (Transform) - Position: (x: 0, y: 0, z: 20)
├── SpawnPoint_Lane3 (Transform) - Position: (x: 4, y: 0, z: 20)
├── LanePathVisualizers (Line Renderers showing downward paths)
└── TargetPoint (Transform at base) - Position: (0, 0, -5)

Enemy AI:
- Use AIActionMoveTowardsTarget3D (TopDown Engine)
- Target = Base position at bottom
- Move speed varies by enemy type
```

---

### **2. Firing Grid (Combat Zone)**
**Location:** Bottom area (just above Active Grid)  
**Purpose:** Heroes stationed here auto-fire UPWARD at enemies approaching from above

**Spatial Layout:**
```
Z-axis layout (top-down view):
  Z = +15 to +5: Enemy battlefield (enemies moving down)
  Z = 0:         Firing Grid (heroes fire upward)
  Z = -3:        Active Grid
  Z = -6:        Passive Grid
  Z = -8:        Base (at bottom)
```

**Grid Size Options:**
- **Small:** 3x2 (6 slots) - Tight, tactical
- **Medium:** 3x3 (9 slots) - Balanced ⭐ **Recommended**
- **Large:** 4x3 or 5x3 (12-15 slots) - More strategic depth

**Visual Design:**
```
Firing Grid (Z = 0, looking down from above):

     Front Row (closest to enemies, Z = +0.5)
┌────────────────────────────────┐
│     [1]    [2]    [3]          │  ← Front line
│     [4]    [5]    [6]          │  ← Mid line  
│     [7]    [8]    [9]          │  ← Back line
└────────────────────────────────┘
     Back Row (furthest from enemies, Z = -0.5)

Row 1 heroes have shorter range but hit enemies first
Row 3 heroes need longer range but safer position

Each slot:
- Size: 1.5x1.5 units
- Spacing: 0.3 unit gap
- Visual: Glowing border when empty
- Visual: Hero model facing UPWARD (toward enemies)
- Visual: Range indicator (circle) extending upward
- Visual: Projectile trails going upward
```

**Hero Firing Logic:**
```csharp
// Heroes in Firing Grid fire UPWARD (positive Z direction)
Vector3 fireDirection = Vector3.forward; // Toward top of screen

// Target enemies above hero's position
if (enemy.position.z > hero.position.z) 
{
    // Enemy is "above" (upward on screen) - valid target
    hero.FireAtTarget(enemy);
}
```

**Slot States:**
- **Empty:** Glowing outline, can be filled
- **Occupied:** Hero model, health bar, attack effects
- **Under Attack:** Red warning, damage numbers
- **Blocked:** Grayed out (if mechanic needed)

---

### **3. Active Grid (Ready Queue)**
**Location:** Below Firing Grid (Z = -3)  
**Purpose:** Heroes ready for player to deploy to Firing Grid

**Grid Size:** Same as Firing Grid (3x3 recommended)

**Visual Design:**
```
Active Grid (Z = -3, looking down from above):

┌─────────────────────────┐
│  [⚔️] [🏹] [🔮]          │  ← Heroes ready
│  [🛡️] [⚔️] [🏹]          │  ← Tap to select
│  [🔮] [⚔️] [🏹]          │  ← Then tap Firing slot
└─────────────────────────┘

Interaction Flow:
1. Player taps hero in Active Grid → Highlight + selection ring
2. Player taps empty Firing Grid slot → Deploy confirmation
3. Hero slides/jumps UPWARD from Active → Firing
4. Passive heroes automatically shift UP to fill Active
```

**Visual Feedback:**
- Selected hero: Glowing outline, floating animation, upward arrow indicator
- Deployment path: Upward arrow from Active → Firing Grid
- Sound: "Deploy" sound effect + haptic feedback
- Hero orientation: Facing upward (ready to move to battle)

---

### **4. Passive Grid (Waiting Queue)**
**Location:** Bottom area (Z = -6), just above base  
**Purpose:** Heroes waiting to automatically shift UP to Active Grid

**Grid Size:** Same as Active Grid (3x3 recommended)

**Auto-Shift Behavior:**
```
Every 3-5 seconds (or when space opens):
1. Active heroes get deployed → Creates space in Active
2. Passive heroes → shift UPWARD to Active Grid (smooth slide)
3. New heroes spawn in Passive Grid (appear from bottom)

Visual Flow:
- Upward slide animation (Passive → Active)
- "Whoosh" particle effect during shift
- Subtle screen shake for feedback
- Progress bar showing time until next shift
```

**Visual Design:**
- Slightly dimmed (less bright than Active)
- Pulsing glow to indicate "waiting to advance"
- Hero icons/models facing upward (toward Active Grid)
- "Next Shift: 3s" timer visible
- Spawn animation: Heroes emerge from base area at bottom

---

### **5. UI Elements**

#### **Top HUD**
```
╔═══════════════════════════════════════════╗
║ 💎 Gold: 450  ⏱️ 1:23  🌊 Wave 3/10      ║
╚═══════════════════════════════════════════╝
```

#### **Bottom HUD**
```
╔═══════════════════════════════════════════╗
║ 🏰 Base: ████████░░ 80%   ⚔️ Heroes: 7/9 ║
║ 🔥 Combo: x3   💀 Kills: 42               ║
╚═══════════════════════════════════════════╝
```

#### **Side Panel (Optional)**
```
┌─────────────┐
│ 📊 Stats    │
│ ─────────── │
│ DPS: 125    │
│ Damage: 850 │
│ Heals: 200  │
│             │
│ 🎯 Next Wave│
│ [Start] 5s  │
└─────────────┘
```

---

## 📱 Mobile vs PC Layout Considerations

### **Mobile (Portrait)**
```
┌─────────────────┐
│   🌊 Wave 3     │  ← Compact HUD
├─────────────────┤
│                 │
│  ENEMIES        │
│   ↓ ↓ ↓         │
│  [⚔️][🏹][🔮]   │  ← Firing
│  [🛡️][ ][⚔️]   │
│                 │
│  [⚔️][🏹][🔮]   │  ← Active (tap)
│  [🛡️][⚔️][🏹]   │
│                 │
│  [⚔️][🏹][🛡️]   │  ← Passive
│  [🔮][⚔️][🏹]   │
│                 │
│  🏰 Base: 80%   │
└─────────────────┘
```

### **Mobile (Landscape)**
```
┌─────────────────────────────────────┐
│ Wave 3     🧟→[⚔️🏹🔮]   Base: 80% │
│            🧟→[🛡️ ⚔️]              │
│  [⚔️🏹🔮]  🧟→[🏹🔮⚔️]              │
│  [🛡️⚔️🏹]                          │
│  [🔮⚔️🏹]  Passive↑  Active↑       │
└─────────────────────────────────────┘
```

### **PC (Widescreen)**
```
┌──────────────────────────────────────────────────────────┐
│ Gold: 450  Wave: 3/10  Base: 80%         Pause  Settings│
├──────────────────────────────────────────────────────────┤
│                                                          │
│  👹👹👹 SPAWNS 👹👹👹                    ┌─────────────┐│
│      ↓ ↓ ↓ ↓ ↓                         │ HERO INFO   ││
│   🧟→→→→[⚔️][🏹][🔮]                   │             ││
│   🧟→→→→[🛡️][ ][⚔️]  Firing Grid      │ Warrior Lvl2││
│   🧟→→→→[🏹][🔮][⚔️]                   │ HP: 100/100 ││
│                                         │ ATK: 25     ││
│      [⚔️][🏹][🔮]                       │ Range: 5m   ││
│      [🛡️][⚔️][🏹]  Active Grid         │             ││
│      [🔮][⚔️][🏹]                       │ [Upgrade]   ││
│                                         └─────────────┘│
│      [⚔️][🏹][🛡️]                                      │
│      [🔮][⚔️][🏹]  Passive Grid                        │
│      [🔮][🛡️][⚔️]                                      │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## 🎨 Visual Style Options

### **Option 1: Stylized 3D (Clash Royale-like)**
- Colorful, cartoony 3D models
- Exaggerated proportions (big heads)
- Vibrant particle effects
- Clear silhouettes for readability
- **Pro:** Appeals to wide audience, clear visuals
- **Con:** Requires more art resources

### **Option 2: Low-Poly 3D (Minimalist)**
- Simple geometric shapes
- Flat colors with subtle gradients
- Clean, modern aesthetic
- Easy to produce quickly
- **Pro:** Fast to create, performs well
- **Con:** Less character personality

### **Option 3: 2D Sprite-Based**
- Hand-drawn or pixel art characters
- 2D sprites on 3D grid
- Classic tower defense look
- Can use sprite sheets for animations
- **Pro:** Easier for solo/small team
- **Con:** Less impressive visually in 3D

### **Option 4: Voxel Art**
- Minecraft-style blocky characters
- Modular, easy to create variations
- Retro aesthetic
- Easy to animate
- **Pro:** Unique look, easy to modify
- **Con:** May feel dated to some players

---

## 🎮 Interaction Flow Examples

### **Scenario 1: Deploying a Hero**
```
1. [Auto-Shift] Warrior moves UPWARD: Passive → Active
   └─ Visual: Upward slide animation + particle trail

2. [Player] Taps Warrior in Active Grid
   └─ Visual: Warrior glows, floats, upward arrow appears

3. [Player] Taps empty slot in Firing Grid (position [5])
   └─ Visual: 
      - Green highlight on target Firing slot
      - Upward dotted line from Warrior → slot
      - "Deploy" button appears

4. [Confirm] Warrior deploys UPWARD to Firing Grid
   └─ Visual:
      - Upward jump/slide animation
      - Landing particle burst
      - Warrior rotates to face UPWARD (toward enemies)
      - Range indicator extends upward
      - MMFeedbacks: Camera shake + haptic

5. [Auto] Warrior begins auto-attacking enemies above
   └─ Visual:
      - Projectile fires UPWARD toward enemy
      - Hit impact particles on enemy
      - Damage numbers float up from enemy
```

### **Scenario 2: Hero Takes Damage**
```
1. Enemy reaches Firing Grid and attacks hero
   └─ Visual:
      - Red flash on hero
      - Health bar depletes
      - Damage number "-15" floats up (red)
      - Impact particles
      - MMFeedbacks: Screen shake (subtle)

2. Hero health reaches 0
   └─ Visual:
      - Death animation (fall/explode)
      - Slot becomes empty (glowing outline)
      - MMFeedbacks: Dramatic camera shake
      - Sound: Death sound effect
      - Haptic: Strong vibration
      - Enemy continues moving DOWNWARD toward base
```

### **Scenario 3: Enemy Reaches Base**
```
1. Enemy passes through Firing Grid (not killed)
   └─ Visual:
      - Enemy continues moving DOWNWARD
      - Warning flash on screen edges

2. Enemy reaches base at bottom (Z = -8)
   └─ Visual:
      - Base damage animation
      - Base health bar depletes
      - Screen shake
      - Red vignette flash
      - "-50 HP" floats from base
      - Enemy disappears (hit base)
```

### **Scenario 4: Merging Heroes** (if in Firing Grid)
```
1. [Auto-Detect] 3 identical heroes in Firing Grid
   └─ Visual: Glow effect on all 3 heroes

2. [Player] Taps "Merge" button (or automatic)
   └─ Visual:
      - Heroes fly toward center position
      - Spiral particle effect
      - Bright flash
      - New hero appears (upgraded level)
      - Still facing UPWARD, continues firing
      - MMFeedbacks: Satisfying camera punch + particles
      - Sound: Power-up sound
```

---

## 🤔 Remaining Design Questions

### **✅ CONFIRMED:**
- **Layout:** Top-down view, enemies move TOP → BOTTOM
- **Hero firing:** Bottom → Top (upward projectiles)
- **Queue flow:** Passive → Active → Firing (all moving upward)
- **Base location:** Bottom of screen

### **Still Need to Decide:**

**1. Grid Sizing:**
- **3x3 grids (9 slots each)?** ⭐ Balanced, recommended
- **3x2 grids (6 slots)?** Simpler, more tactical
- **4x3 grids (12 slots)?** More complex, strategic depth

**2. Enemy Lanes:**
- **3 lanes?** ⭐ Simple, clear (recommended to start)
- **5 lanes?** More complex, requires strategic placement
- **Dynamic lanes?** Enemies can switch lanes

**3. Merge Mechanics:**
- **Merge anywhere** (Passive/Active/Firing) - Simplest
- **Only in Firing Grid** (during combat) - More exciting/risky ⭐
- **Only in Passive/Active** (pre-deployment) - Safer, planning-focused
- **Auto-merge or manual trigger?**

**4. Camera:**
- **Fixed orthographic** (pure top-down) ⭐ Mobile-friendly
- **Slight angle** (70° instead of 90°) - Shows depth
- **Can zoom in/out?** (Pinch gesture on mobile)

**5. Hero Repositioning:**
- **Can move heroes within Firing Grid?** (Drag to new slot)
- **Can move heroes back to Active Grid?** (Retreat mechanic)
- **Or heroes stay put once deployed?** ⭐ Simpler

**6. Visual Style:**
- **Low-poly 3D** ⭐ Fast to create, performs well
- **Stylized 3D** (Clash Royale-like) - More polished, more work
- **2D sprites on 3D grid** - Easier art pipeline
- **Voxel art** - Unique retro aesthetic

---

## 💡 Recommended Starting Configuration

**For initial prototype (Week 1-2):**

✅ **Layout:** Top-down orthographic camera  
✅ **Grid Size:** 3x3 for all zones (9 slots each = 27 total)  
✅ **Enemy Lanes:** 3 lanes (left, center, right)  
✅ **Platform:** Mobile-first (portrait orientation)  
✅ **Visual Style:** Low-poly 3D (fast to prototype)  
✅ **Merge Location:** Firing Grid only (more exciting)  
✅ **Merge Trigger:** Manual (tap "Merge" button when 3 match)  
✅ **Camera Control:** Fixed (no rotation/zoom initially)  
✅ **Hero Movement:** Heroes stay in Firing Grid (no repositioning)  
✅ **Enemy Direction:** Top to bottom (straight line per lane)  

**This gives you:**
- 27 total hero slots (9 Passive, 9 Active, 9 Firing)
- Clear visual hierarchy (bottom → top = queue → battle)
- Mobile-friendly single-hand play
- Simple spatial logic (Z-axis only, no X movement)
- Easy to expand later with more mechanics

**Spatial Summary:**
```
Z = +20:  Enemy spawns (3 lanes)
          ↓ ↓ ↓
Z = +15:  Enemy battlefield
Z = +10:  
Z = +5:   
Z = 0:    Firing Grid (heroes fire upward ↑)
Z = -3:   Active Grid
Z = -6:   Passive Grid
Z = -8:   Player Base 🏰
```

---

## 📐 Unity Scene Hierarchy Example

```
MainGameScene
├── Managers
│   ├── GameManager (at origin)
│   ├── GridManager
│   ├── HeroQueueManager
│   ├── WaveManager
│   └── InputManager
│
├── Camera
│   └── Main Camera 
│       Position: (0, 25, -10)  ← Looking DOWN at battlefield
│       Rotation: (70, 0, 0)    ← Angled downward
│       Orthographic: true      ← Top-down view
│       Size: 15                ← Adjust for screen coverage
│
├── Grids (arranged vertically in Z-axis)
│   ├── PassiveGrid (Z: -6, bottom)
│   │   ├── Slot_0_0 to Slot_2_2 (9 slots)
│   │   └── GridVisuals (plane/lines, green tint)
│   │
│   ├── ActiveGrid (Z: -3, middle-bottom)
│   │   ├── Slot_0_0 to Slot_2_2 (9 slots)
│   │   └── GridVisuals (plane/lines, yellow tint)
│   │
│   └── FiringGrid (Z: 0, middle)
│       ├── Slot_0_0 to Slot_2_2 (9 slots)
│       └── GridVisuals (plane/lines, red tint)
│
├── Battlefield
│   ├── Ground (large plane from Z: -8 to Z: +20)
│   ├── Base (Z: -8, bottom edge)
│   │   └── BaseHealth component
│   └── Environment (decorations, walls, etc.)
│
├── Enemy System
│   ├── SpawnPoints (at top, Z: +20)
│   │   ├── Lane1_Spawn (x: -4, z: 20)
│   │   ├── Lane2_Spawn (x: 0, z: 20)
│   │   └── Lane3_Spawn (x: 4, z: 20)
│   │
│   ├── LaneVisualizers (show downward paths)
│   │   ├── Lane1_Path (line from z:20 to z:-8)
│   │   ├── Lane2_Path
│   │   └── Lane3_Path
│   │
│   └── EnemyContainer (spawned enemies parent)
│
├── UI
│   ├── Canvas (Screen Space - Overlay)
│   │   ├── TopHUD (wave, score, gold)
│   │   │   └── Position: Top of screen
│   │   │
│   │   ├── BottomHUD (base health, hero count)
│   │   │   └── Position: Bottom of screen
│   │   │
│   │   ├── DeploymentFeedback (selection indicators)
│   │   │   ├── SelectedHeroOutline
│   │   │   └── TargetSlotHighlight
│   │   │
│   │   └── PauseMenu
│   │
│   └── WorldSpaceUI (damage numbers, health bars)
│       └── Canvas (World Space)
│
└── Audio
    ├── MusicManager
    └── SFXManager

SPATIAL LAYOUT (Side View):
              Y
              ↑
              │
    Camera ───┼─── (0, 25, -10) Looking down
              │
              │
    ──────────┴────────────── Z (depth)
    
    TOP (Z: +20)
    👹 Enemy Spawns
         ↓
         ↓ Enemies move down
         ↓
    (Z: 0)
    🔫 Firing Grid ← Heroes fire upward
    
    (Z: -3)
    📦 Active Grid
    
    (Z: -6)
    📦 Passive Grid
    
    (Z: -8)
    🏰 Base
    BOTTOM
```

---

## 🎯 Does This Match Your Vision?

**Key Questions:**

1. **Camera angle:** Isometric 3D, top-down 2D, or side view?
2. **Platform priority:** Mobile-first or PC-first?
3. **Visual style:** Stylized 3D, low-poly, 2D sprites, or voxel?
4. **Grid size:** Small (3x2), medium (3x3), or large (4x3+)?
5. **Merge mechanic:** Can merge anywhere or only specific zones?
6. **Hero movement:** Do deployed heroes stay put, or can they be repositioned?

**Let me know:**
- What feels right from the options above?
- What's different from your original vision?
- Any mechanics/visuals I missed?

Once we align on the layout, I can create the actual Unity scene structure and begin implementing! 🚀
