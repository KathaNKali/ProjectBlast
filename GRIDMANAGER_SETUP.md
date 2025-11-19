# GridManager Setup Guide

## ✅ Files Created

The following files have been created in your project:

```
Assets/ProjectBlast/Scripts/
├── Grid/
│   ├── GridZone.cs          - Enum for grid zones (Passive/Active/Firing)
│   ├── GridSlot.cs          - Individual grid slot class
│   └── GridManager.cs       - Main grid management system
├── Heroes/
│   └── Hero.cs              - Basic hero class (stub for now)
└── Testing/
    └── GridManagerTester.cs - Test script for validation
```

---

## 🚀 Quick Start - Unity Scene Setup

### **Step 1: Create the Scene**

1. Open Unity
2. Create a new scene: `File > New Scene > Basic (Built-in)`
3. Save as `GridTestScene`

### **Step 2: Add GridManager**

1. Create empty GameObject: `GameObject > Create Empty`
2. Name it: `GridManager`
3. Add component: `Grid Manager` script
4. Configure in Inspector:

**Recommended Starting Values:**
```
=== PASSIVE GRID ===
Passive Rows: 3
Passive Columns: 3
Passive Grid Z: -6

=== ACTIVE GRID ===
Active Rows: 3
Active Columns: 3
Active Grid Z: -3

=== FIRING GRID ===
Firing Rows: 3
Firing Columns: 3
Firing Grid Z: 0

=== CELL SPACING ===
Cell Size: 1.5
Cell Spacing: 0.3

=== VISUAL DEBUG ===
Show Grid Gizmos: ✓ (checked)
Show Slot Labels: ✓ (checked)
Passive Color: Green
Active Color: Yellow
Firing Color: Red
```

### **Step 3: Position the Camera**

1. Select Main Camera
2. Set Transform:
   - Position: `(0, 25, -10)`
   - Rotation: `(70, 0, 0)`
3. Set Camera properties:
   - Projection: `Orthographic`
   - Size: `15`

### **Step 4: Add Test Script (Optional)**

1. Create empty GameObject: `GameObject > Create Empty`
2. Name it: `GridTester`
3. Add component: `Grid Manager Tester` script
4. Configure:
   - Test Hero Prefab: (leave empty for now, will use cube)
   - Auto Run Tests: ✓ (checked for first run)

### **Step 5: Verify Setup**

1. Press **Play**
2. Check Console for test results
3. Check **Scene View** (not Game View) to see grid gizmos
4. You should see:
   - Green wireframes (Passive grid at bottom)
   - Yellow wireframes (Active grid in middle)
   - Red wireframes (Firing grid at top)
   - Slot labels showing coordinates like "P[0,0]", "A[1,2]", "F[2,1]"

---

## 🎮 Testing the Grid System

### **Automated Tests (Auto Run)**

If "Auto Run Tests" is enabled, you'll see console output like:
```
=== GridManager Automated Tests ===

--- Test: Grid Initialization ---
✓ Passive Grid: 9 slots (3x3)
✓ Active Grid: 9 slots (3x3)
✓ Firing Grid: 9 slots (3x3)

--- Test: Placement & Removal ---
✓ Place hero at Firing[0,0]: Success
✓ Place second hero at same slot: Correctly rejected
✓ Remove hero: Success

... (more tests)

=== All Tests Complete ===
```

### **Manual Testing (Keyboard Controls)**

Disable "Auto Run Tests" and use keyboard:

| Key | Action |
|-----|--------|
| **Space** | Place random hero in random grid |
| **R** | Remove random hero |
| **C** | Clear all grids |
| **L** | Log grid status to console |

---

## 🔍 Visual Debugging

### **Scene View Gizmos**

Make sure you're in **Scene View** (not Game View) to see:

1. **Empty Slots:** Wireframe cubes (green/yellow/red by zone)
2. **Occupied Slots:** Small filled orange cube + wireframe
3. **Slot Labels:** Coordinate labels like "F[0,1]" above each slot

### **Understanding Coordinates**

```
Example Firing Grid (3x3):

     Front (toward enemies)
  ┌──────────────────────┐
  │ F[0,0] F[0,1] F[0,2] │  Row 0 = Front
  │ F[1,0] F[1,1] F[1,2] │  Row 1 = Middle
  │ F[2,0] F[2,1] F[2,2] │  Row 2 = Back
  └──────────────────────┘
    Col 0   Col 1  Col 2
    Left   Center  Right
```

---

## 📊 GridManager API Overview

### **Placement**
```csharp
// Place hero in specific slot
GridManager.Instance.PlaceHero(hero, GridZone.Firing, row: 0, col: 1);

// Place hero using GridSlot reference
var slot = GridManager.Instance.GetSlot(GridZone.Active, 1, 1);
GridManager.Instance.PlaceHero(hero, slot);

// Place hero in first available slot
GridManager.Instance.TryPlaceHeroInZone(hero, GridZone.Passive);
```

### **Removal**
```csharp
// Remove hero by reference
GridManager.Instance.RemoveHero(hero);

// Remove hero by position
GridManager.Instance.RemoveHero(GridZone.Firing, row: 0, col: 1);
```

### **Queries**
```csharp
// Get specific slot
var slot = GridManager.Instance.GetSlot(GridZone.Firing, 0, 0);

// Get all slots in zone
var allSlots = GridManager.Instance.GetAllSlots(GridZone.Active);

// Get occupied/empty slots
var occupied = GridManager.Instance.GetOccupiedSlots(GridZone.Firing);
var empty = GridManager.Instance.GetEmptySlots(GridZone.Active);

// Get heroes in zone
var heroes = GridManager.Instance.GetAllHeroesInZone(GridZone.Firing);

// Get nearest empty slot to position
var nearest = GridManager.Instance.GetNearestEmptySlot(GridZone.Firing, worldPos);
```

### **Validation**
```csharp
// Check if position is valid
bool valid = GridManager.Instance.IsValidPosition(GridZone.Firing, row: 0, col: 5);

// Check slot state
bool empty = GridManager.Instance.IsSlotEmpty(GridZone.Active, 1, 1);
bool occupied = GridManager.Instance.IsSlotOccupied(GridZone.Firing, 0, 0);
```

### **Spatial Conversion**
```csharp
// Grid to world
Vector3 worldPos = GridManager.Instance.GridToWorldPosition(GridZone.Firing, 1, 1);

// World to grid
var (row, col) = GridManager.Instance.WorldToGridPosition(GridZone.Active, worldPos);
```

---

## 🎯 Next Steps

Once the GridManager is working:

1. ✅ **Grid System** - Complete!
2. ⏭️ **Create Hero Prefab** - With visuals and TDE Weapon component
3. ⏭️ **Build HeroQueueManager** - Auto-shift heroes between zones
4. ⏭️ **Implement Deployment Input** - Click to deploy heroes
5. ⏭️ **Add Enemy System** - Enemies moving from top to bottom
6. ⏭️ **Auto-Combat Logic** - Heroes firing at enemies

---

## ❓ Troubleshooting

### **"GridManager instance not found"**
- Make sure GridManager GameObject exists in scene
- Verify GridManager script is attached
- Check Console for initialization errors

### **"Grids not visible in Scene View"**
- Ensure "Show Grid Gizmos" is checked
- Switch to Scene View (not Game View)
- Make sure Gizmos are enabled (top right of Scene View)

### **"Invalid position" warnings**
- Check row/column values are within grid bounds
- Passive/Active/Firing grids can have different sizes
- Row/Column indices start at 0

### **"Slot already occupied" warnings**
- This is expected behavior - can't place two heroes in same slot
- Use `GetEmptySlots()` to find available positions
- Or use `TryPlaceHeroInZone()` which auto-finds empty slot

---

## 📝 Configuration Tips

### **Adjusting Grid Sizes**

You can configure each grid independently:

**Tight Tactical (6-9 heroes total):**
- All grids: 2x3 or 3x3

**Balanced (9-12 heroes):**
- Passive: 3x3, Active: 3x2, Firing: 3x3

**Strategic Depth (12-18 heroes):**
- All grids: 3x4 or 4x3

### **Adjusting Spacing**

- **Cell Size:** Distance between slot centers (1.5 recommended)
- **Cell Spacing:** Visual gap between cells (0.3 recommended)
- Larger values = more spread out grids
- Smaller values = tighter, compact grids

### **Adjusting Z Positions**

Current layout (top to bottom):
- Enemies: Z = +20 to +5
- Firing Grid: Z = 0
- Active Grid: Z = -3
- Passive Grid: Z = -6
- Base: Z = -8

Adjust these to change battlefield size.

---

**Grid System is now ready for testing!** 🎉

Open Unity, follow the setup steps, and press Play to see it in action.
