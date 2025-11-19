# Hero Queue System - Unity Setup Guide

Quick guide to set up and test the **vertical lane-based hero queue system** with GridManager.

---

## 📦 What We Built

**System Components:**
1. **Hero.cs** - Enhanced with click detection and visual feedback
2. **HeroQueueManager.cs** - Spawns heroes, handles vertical lane-based queue shifting

**Queue Architecture - Vertical Lanes:**
- Each column is an independent queue
- Heroes only move within their lane (strict lane enforcement)
- Active Grid = 1 row × N columns
- Passive Grid = 2+ rows × N columns
- Firing Grid = No lanes (left-to-right fill)

**Game Flow:**
```
Column 0   Column 1   Column 2
[H1]      [H2]      [H3]       ← Active (1 row)
[H4]      [H5]      [H6]       ← Passive Row 0
[H7]      [H8]      [H9]       ← Passive Row 1

Player clicks [H2] → Deploys to Firing
↓
[H1]      [H5]      [H3]       ← [H5] moves up to Active
[H4]      [H8]      [H6]       ← [H8] moves up in lane
[H7]      [ ]       [H9]       ← Empty slot at bottom

Animation: Parallel waves (0.2s delay, 0.3s duration)
```

---

## 🎮 Unity Setup Steps

### **Step 1: Create TestHero Prefab**

1. **Create 3D GameObject:**
   - In Hierarchy: `Right-click > 3D Object > Capsule`
   - Name: `TestHero`
   - Scale: `(0.8, 0.8, 0.8)` to fit grid cells

2. **Add Hero Component:**
   - Select `TestHero` in Hierarchy
   - `Add Component` → Search "Hero"
   - Configure:
     ```
     Hero Name: "Test Hero"
     Highlight Material: (optional - create a bright colored material)
     ```

3. **Verify Collider:**
   - `TestHero` should have Capsule Collider (auto-added)
   - Make sure it's **not** set to `IsTrigger`
   - This is needed for `OnMouseDown()` to work

4. **Add Visual Material (Optional):**
   - Create material: `Assets/ProjectBlast/Materials/HeroMaterial.mat`
   - Set color: Blue or Green (default state)
   - Drag to `TestHero` MeshRenderer

5. **Create Highlight Material (Optional):**
   - Create material: `Assets/ProjectBlast/Materials/HeroHighlight.mat`
   - Set color: Yellow or Orange (selected state)
   - Assign to Hero component's `Highlight Material` field

6. **Create Prefab:**
   - Drag `TestHero` from Hierarchy into `Assets/ProjectBlast/Prefabs/`
   - Delete from Hierarchy

---

### **Step 2: Setup Scene**

1. **Create New Scene or Use Existing:**
   - `File > New Scene`
   - Or continue with your GridManager test scene

2. **Add GridManager (if not present):**
   - Create Empty GameObject: Name `GridManager`
   - Add Component: `GridManager`
   - Configure grids (example):
     ```
     === Passive Grid ===
     Rows: 2
     Columns: 3
     Grid Center: (0, 0, -6)
     Cell Size: 2
     
     === Active Grid ===
     Rows: 1
     Columns: 3
     Grid Center: (0, 0, -3)
     Cell Size: 2
     
     === Firing Grid ===
     Rows: 2
     Columns: 3
     Grid Center: (0, 0, 0)
     Cell Size: 2
     ```

3. **Add HeroQueueManager:**
   - Create Empty GameObject: Name `HeroQueueManager`
   - Add Component: `HeroQueueManager`
   - Configure:
     ```
     Test Hero Prefab: Drag TestHero prefab here
     Auto Spawn On Start: ✓ (checked)
     Show Debug Logs: ✓ (checked)
     Enable Lane Queue System: ✓ (checked)
     Animation Duration: 0.3
     Animation Delay: 0.2
     ```

4. **Add Camera (if not setup):**
   - Follow `CAMERA_SETUP_GUIDE.md`
   - Or use Scene view for testing

---

### **Step 3: Test the System**

1. **Press Play**

2. **Expected Behavior:**
   - **On Start (Row-by-Row Spawn):**
     - Active grid filled first: [H1][H2][H3] (1 row)
     - Passive Row 0 filled: [H4][H5][H6]
     - Passive Row 1 filled: [H7][H8][H9]
     - Firing grid empty
     - Total: 9 heroes spawned in vertical lanes
   
   - **Console logs:**
     ```
     [HeroQueueManager] Spawned hero at Active (0, 0)
     [HeroQueueManager] Spawned hero at Active (0, 1)
     [HeroQueueManager] Spawned hero at Active (0, 2)
     [HeroQueueManager] Spawned hero at Passive (0, 0)
     ... (repeated for all slots)
     [HeroQueueManager] Spawned 9 test heroes
     ```

3. **Click a Hero in Active Grid (e.g., Column 1):**
   - Hero turns yellow/orange (highlight)
   - Hero deploys to Firing grid (instant)
   - Wait 0.2s delay
   - Heroes in Column 1 shift up (animated over 0.3s):
     * [H5] from Passive Row 0 → Active
     * [H8] from Passive Row 1 → Passive Row 0
     * Empty slot appears at Passive Row 1, Column 1
   - Console shows:
     ```
     [HeroQueueManager] Selected hero: Test Hero at Active (0, 1)
     [HeroQueueManager] Deployed Test Hero → Firing (0, 0)
     [HeroQueueManager] Lane 1 shifting up (2 heroes)
     [HeroQueueManager] Animation complete
     ```

4. **Verify Lane Independence:**
   - Click hero in Column 0 → Only Column 0 heroes shift
   - Click hero in Column 2 → Only Column 2 heroes shift
   - Other lanes remain static (strict lane enforcement)

---

## 🎨 Visual Verification

**Scene View (with gizmos enabled) - Vertical Lane System:**
```
     [Enemy Spawn Area] (top)
            ↑
            |
┌─────────────────────┐
│   Firing Grid       │  RED wireframe (no lanes - left-to-right fill)
│   [ ][ ][ ]         │
│   [ ][ ][ ]         │
└─────────────────────┘
            ↑
┌─────────────────────┐
│   Active Grid       │  YELLOW wireframe (1 row - strict lanes)
│   [H1][H2][H3]      │  Column 0 | Column 1 | Column 2
└─────────────────────┘
            ↑
┌─────────────────────┐
│   Passive Grid      │  GREEN wireframe (2 rows - vertical queues)
│   [H4][H5][H6]      │  Row 0: Next in queue per lane
│   [H7][H8][H9]      │  Row 1: Bottom of queue
└─────────────────────┘
     (base/bottom)

Lane 0: H1 → H4 → H7
Lane 1: H2 → H5 → H8
Lane 2: H3 → H6 → H9
```

**After clicking H2 (Column 1) in Active:**
```
┌─────────────────────┐
│   Firing Grid       │
│   [H2][ ][ ]        │  H2 deployed (left-to-right fill)
│   [ ][ ][ ]         │
└─────────────────────┘
┌─────────────────────┐
│   Active Grid       │
│   [H1][H5][H3]      │  H5 moved up from Passive Row 0
└─────────────────────┘
┌─────────────────────┐
│   Passive Grid      │
│   [H4][H8][H6]      │  H8 moved up to Row 0
│   [H7][ ][H9]       │  Empty slot at bottom of Lane 1
└─────────────────────┘

Lane 0: H1 → H4 → H7 (unchanged)
Lane 1: H5 → H8 → [ ] (shifted up)
Lane 2: H3 → H6 → H9 (unchanged)
```

---

## 🔍 Troubleshooting

### **Heroes don't spawn**
- Check HeroQueueManager has `TestHeroPrefab` assigned
- Check GridManager is in scene and initialized
- Look for errors in Console
- Verify `Auto Spawn On Start` is checked

### **Can't click heroes**
- Main Camera needs `Physics Raycaster` component (usually auto-added)
- TestHero must have Collider (not trigger)
- Hero must be in Active zone (can't click Passive heroes)
- Check layers - hero shouldn't be on Ignore Raycast layer

### **Heroes spawn but don't move when clicked**
- Check Console for error messages
- Verify Firing grid has empty slots
- Check GridManager's `PlaceHero()` and `RemoveHero()` are working
- Enable `Show Debug Logs` on HeroQueueManager

### **Heroes spawn at wrong position**
- Check GridManager's Grid Center positions
- Verify Cell Size matches your prefab scale
- Check Grid Spacing values
- Enable Gizmos in Scene view to visualize grids

### **Highlight doesn't work**
- Assign `Highlight Material` in Hero component
- Check if material is different from default
- Verify Renderer component exists on TestHero

---

## 🧪 Testing Checklist

- [ ] All Active slots filled on start (row-by-row spawn)
- [ ] All Passive Row 0 slots filled
- [ ] All Passive Row 1 slots filled  
- [ ] Firing grid empty on start
- [ ] Clicking Active hero highlights it (if material set)
- [ ] Clicked hero deploys to Firing grid (instant)
- [ ] Only heroes in that column shift up (lane independence)
- [ ] Passive hero moves to Active slot (0.2s delay + 0.3s animation)
- [ ] All heroes in lane shift up simultaneously
- [ ] Empty slot appears at bottom of lane
- [ ] Other lanes remain unchanged (strict lane enforcement)
- [ ] Hero's GameObject position updates smoothly (Lerp)
- [ ] GridManager's slot states update correctly
- [ ] Console shows correct deployment + shift logs
- [ ] Can't click heroes during animation (input blocked)
- [ ] Multiple lanes can be deployed sequentially
- [ ] Empty lanes stay empty (no cross-lane filling)
- [ ] Warning shown when Firing grid is full

---

## 🚀 Next Steps

Once vertical lane queue system is working:

1. **Polish Animations** - Upgrade Lerp to DOTween with easing
2. **Visual Feedback** - Add lane indicators, queue numbers on heroes
3. **Enemy System** - Spawn enemies at top, move toward base
4. **Combat System** - Heroes auto-fire at enemies using TDE weapons (WeaponAutoAim3D)
5. **Wave Manager** - Spawn enemies in waves with difficulty scaling
6. **UI System** - Show lane status, wave info, score
7. **Hero Classes** - Different weapon types with unique ranges/damage
8. **Merge System** - Combine 3 matching heroes in Firing grid

---

## 🎯 Vertical Lane System Summary

**Core Rules:**
- Each column = independent queue
- Heroes only move within their lane
- Active (1 row) ← Passive (2+ rows)
- Firing = no lanes (left-to-right fill)
- Animation: 0.2s delay + 0.3s Lerp
- Input blocked during animation

---

## 📝 Quick Commands (for Testing)

Add these keyboard shortcuts to HeroQueueManager for testing:

```csharp
void Update()
{
    // Press R to respawn all heroes
    if (Input.GetKeyDown(KeyCode.R))
    {
        ClearAllHeroes();
        SpawnTestHeroes();
    }
    
    // Press C to clear all heroes
    if (Input.GetKeyDown(KeyCode.C))
    {
        ClearAllHeroes();
    }
}
```

---

**Ready to test! Let me know if you encounter any issues.** 🎮
