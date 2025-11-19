# Vertical Lane Queue System - Design Specification

**Core Architecture for ProjectBlast's Hero Queue Management**

---

## 🎯 System Overview

The **Vertical Lane Queue System** is the fundamental mechanic for hero management in ProjectBlast. Each column represents an independent queue where heroes move vertically upward from Passive → Active → Firing zones.

---

## 📐 Grid Architecture

### **Zone Configuration**

```
┌─────────────────────────────────┐
│      FIRING GRID (Combat)       │  ← Zone 3: No lanes
│      [F1][F2][F3][F4][F5][F6]   │     Left-to-right fill
│      [F7][F8][F9][FA][FB][FC]   │     Heroes auto-fire
└─────────────────────────────────┘
              ↑ Deploy
┌─────────────────────────────────┐
│      ACTIVE GRID (Ready)        │  ← Zone 2: 1 row × N columns
│      [A0][A1][A2]               │     Click to deploy
└─────────────────────────────────┘
       ↑      ↑      ↑
    Lane 0  Lane 1  Lane 2
       ↑      ↑      ↑
┌─────────────────────────────────┐
│    PASSIVE GRID (Queue)         │  ← Zone 1: 2+ rows × N columns
│      [P0][P1][P2]               │     Row 0: Next in queue
│      [P3][P4][P5]               │     Row 1+: Waiting
└─────────────────────────────────┘
```

### **Grid Specifications**

- **Active Grid:** 1 row × N columns (typically 3 columns)
- **Passive Grid:** 2+ rows × N columns (typically 2×3 or 3×3)
- **Firing Grid:** 2+ rows × N columns (typically 2×3, no lane restrictions)

---

## 🔄 Lane Rules

### **Rule 1: Strict Lane Enforcement**
Heroes can **only** move within their column (vertical lane). No horizontal movement between lanes.

**Example:**
```
Lane 0:  [A0] → [P0] → [P3]
Lane 1:  [A1] → [P1] → [P4]
Lane 2:  [A2] → [P2] → [P5]

Hero in Lane 0 can NEVER move to Lane 1 or Lane 2
```

### **Rule 2: Active Refill (Same Lane)**
When an Active slot empties, the Passive hero directly below it (same column) moves up.

**Example:**
```
Before:          After:
[A0][A1][A2]    [P0][A1][A2]  ← P0 moved to A0
[P0][P1][P2]    [P3][P1][P2]  ← P3 moved to P0
[P3][P4][P5]    [ ][P4][P5]   ← Empty at bottom

Only Lane 0 shifted. Lanes 1 and 2 unchanged.
```

### **Rule 3: Vertical Compacting**
All heroes in a lane shift up simultaneously when the hero above them leaves.

**Example - Lane 1 deployment:**
```
Before:          After:
[A0][A1][A2]    [A0][P1][A2]  ← P1 moved to A1
[P0][P1][P2]    [P0][P4][P2]  ← P4 moved to P1
[P3][P4][P5]    [P3][ ][P5]   ← Empty at bottom

P1 → A1
P4 → P1 (moved up one row)
Empty slot created at bottom of Lane 1
```

### **Rule 4: Empty Lanes Stay Empty**
If a lane has no heroes, it remains empty. No cross-lane filling.

**Example:**
```
Before:          After (A0 deployed):
[A0][ ][A2]     [ ][ ][A2]
[P0][ ][P2]     [P0][ ][P2]
[P3][ ][P5]     [P3][ ][P5]

Lane 1 is empty and stays empty.
No heroes move from Lane 0 or Lane 2 to fill it.
```

### **Rule 5: Firing Grid Has No Lanes**
Heroes deployed to Firing fill slots left-to-right, top-to-bottom (first available).

**Example:**
```
Active Lane 0 hero → Firing [0,0]
Active Lane 1 hero → Firing [0,1]
Active Lane 2 hero → Firing [0,2]
Active Lane 0 hero → Firing [0,3] (continues left-to-right)

Source lane doesn't matter in Firing grid.
```

---

## ⏱️ Animation Timing

### **Deployment Sequence**

```
t = 0.0s:  Player clicks Active hero
           ↓
           Hero highlights (yellow/orange)
           ↓
           Hero deploys to Firing (instant teleport)
           ↓
t = 0.2s:  Delay before lane shift
           ↓
           Start Lerp animation for lane (0.3s duration)
           ↓
           All heroes in lane move simultaneously:
           - Passive Row 0 → Active
           - Passive Row 1 → Passive Row 0
           - Passive Row 2 → Passive Row 1 (if exists)
           ↓
t = 0.5s:  Animation complete
           ↓
           Re-enable input (allow next click)
```

### **Animation Method**

**Phase 1: Lerp (Initial Implementation)**
```csharp
Vector3.Lerp(startPosition, endPosition, t)
// Linear interpolation over 0.3 seconds
```

**Phase 2: DOTween (Polish - Future)**
```csharp
transform.DOMove(endPosition, 0.3f).SetEase(Ease.OutCubic)
// Smooth easing for professional feel
```

---

## 🚫 Input Blocking

### **During Animation State**

```csharp
public bool IsAnimating { get; private set; }

void OnHeroClicked(Hero hero)
{
    if (IsAnimating) return; // Block all input
    
    IsAnimating = true;
    DeployHero(hero);
    StartCoroutine(AnimateLaneShift(() => {
        IsAnimating = false; // Re-enable after animation
    }));
}
```

**Why Block Input?**
- Prevents animation conflicts
- Avoids invalid grid states
- Clear feedback to player
- Simpler state management

---

## 🎮 Example Gameplay Flow

### **Scenario: 3-Lane System**

**Initial State:**
```
Active:  [Knight][Archer][Mage]
Passive: [Knight][Archer][Mage]  Row 0
         [Knight][Archer][Mage]  Row 1
Firing:  [ ][ ][ ]
         [ ][ ][ ]

Lane 0: Knight → Knight → Knight
Lane 1: Archer → Archer → Archer
Lane 2: Mage → Mage → Mage
```

**Player clicks Archer (Lane 1):**

**Step 1 - Deploy (t=0.0s):**
```
Active:  [Knight][ ][Mage]
Firing:  [Archer][ ][ ]
```

**Step 2 - Lane Shift (t=0.2s - 0.5s):**
```
Active:  [Knight][Archer][Mage]  ← Passive Archer moved up
Passive: [Knight][Archer][Mage]  ← Row 1 Archer moved to Row 0
         [Knight][ ][Mage]        ← Empty slot at bottom of Lane 1
Firing:  [Archer][ ][ ]

Lane 0: Unchanged
Lane 1: Shifted up
Lane 2: Unchanged
```

**Player clicks Knight (Lane 0):**
```
Active:  [Knight][Archer][Mage]
Passive: [Knight][Archer][Mage]
         [ ][ ][Mage]             ← Lane 0 now empty at bottom
Firing:  [Archer][Knight][ ]     ← Filled left-to-right

Lane 0: Shifted up (now empty at bottom)
Lane 1: Unchanged
Lane 2: Unchanged
```

---

## 🔧 Implementation Checklist

### **GridManager Requirements**
- [✅] Track 3 zones (Passive, Active, Firing)
- [✅] Store GridSlot 2D arrays per zone
- [✅] GridSlot has Zone, Row, Column, OccupyingHero
- [ ] `GetLaneHeroes(GridZone zone, int column)` method
- [ ] `ShiftLaneUp(int column)` method
- [ ] Validate strict lane enforcement

### **HeroQueueManager Requirements**
- [✅] Spawn heroes row-by-row (Active → Passive Row 0 → Row 1)
- [ ] `OnHeroClicked()` with lane detection
- [ ] `DeployToFiring()` with left-to-right fill logic
- [ ] `ShiftLaneUp(int laneIndex)` with Lerp animation
- [ ] `IsAnimating` flag to block input
- [ ] Coroutine for timed animation sequence

### **Hero Requirements**
- [✅] `OnMouseDown()` click detection
- [✅] `IsInActiveZone` property
- [✅] `Highlight()` / `Unhighlight()` methods
- [ ] Lane index tracking (column number)

---

## 📊 Performance Considerations

### **Optimization Strategies**

1. **Pooling:** Pre-instantiate hero objects, reuse instead of Destroy/Instantiate
2. **Batch Updates:** Move all heroes in lane simultaneously (single update)
3. **Cached Queries:** Store `GetLaneHeroes()` results, invalidate on changes
4. **Event-Driven:** Use MMEventManager for grid state changes (avoid polling)

### **Scaling Limits**

- **Max Lanes:** 5-7 (UI/screen real estate limit)
- **Max Passive Rows:** 3-5 (balances queue depth vs screen space)
- **Animation Duration:** 0.2-0.5s (balance responsiveness vs clarity)

---

## 🎨 Visual Feedback

### **Recommended UI Elements**

1. **Lane Indicators:** Vertical lines showing lane boundaries
2. **Queue Numbers:** Display position in queue (1, 2, 3...)
3. **Highlight Active Row:** Glow/pulse on Active grid
4. **Arrow Animations:** Show upward flow during shift
5. **Empty Slot Markers:** Visual indicator for empty lanes

### **Color Coding**

- **Passive Grid:** Green (waiting in queue)
- **Active Grid:** Yellow (ready to deploy)
- **Firing Grid:** Red (in combat)
- **Highlight:** Orange/White (selected hero)

---

## 🚀 Future Enhancements

1. **Express Lane:** Drag hero to front of queue (premium feature)
2. **Lane Swap:** Swap two lanes' positions (strategic option)
3. **Multi-Deploy:** Deploy entire lane at once (power-up)
4. **Lane Locking:** Prevent lane from refilling (tactical choice)
5. **Queue Prediction:** Show next 3 heroes per lane (UI)

---

## ✅ Design Confirmed

This specification reflects the finalized design decisions:

- **Active Grid:** 1 row × N columns
- **Passive Shift:** Vertical within lane only
- **Empty Slots:** No cross-lane filling (Interpretation B)
- **Firing Grid:** No lanes, left-to-right fill
- **Animation:** Parallel waves (Option B)
- **Movement:** Lerp initially (Option A)
- **Input:** Blocked during animation (Option A)
- **Spawn Order:** Row-by-row (Option C)

**Status:** Ready for implementation ✅

---

**Last Updated:** November 19, 2025  
**Next Step:** Implement `HeroQueueManager` lane-shifting logic
