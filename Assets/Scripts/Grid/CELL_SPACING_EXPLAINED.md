# Understanding Cell Size vs Cell Spacing

## The Behavior You're Seeing

When you change `cellSpacing`, the entire grid appears to "move" or "scale". This is actually **CORRECT behavior**, but might not be what you expect. Let me explain:

## What Cell Spacing Does

### Visual Example:

**cellSize = 1.0, cellSpacing = 0:**
```
[Cell][Cell][Cell][Cell][Cell]
0    1    2    3    4    5
```
Total grid width: 5 units

**cellSize = 1.0, cellSpacing = 0.5:**
```
[Cell] [Cell] [Cell] [Cell] [Cell]
0    1.5   3.0   4.5   6.0   7.0
```
Total grid width: 7 units

**Notice:** The cells themselves are still 1.0 unit wide, but the TOTAL GRID is now 7 units wide instead of 5!

## Why Does The Grid "Move"?

When you use `GridOrigin.Center` (default), the grid is centered at (0,0). When spacing increases:
- The total grid size increases
- To keep it centered, it shifts outward equally in all directions
- This makes it **look** like the grid is scaling, but actually it's just expanding

### Before Spacing (5×5 grid, cellSize=1, spacing=0):
```
Grid bounds: -2.5 to +2.5 (5 units total)
     -2.5         0         +2.5
       [Cell][Cell][Cell][Cell][Cell]
```

### After Spacing (5×5 grid, cellSize=1, spacing=0.5):
```
Grid bounds: -3.5 to +3.5 (7 units total)
     -3.5            0            +3.5
       [Cell] [Cell] [Cell] [Cell] [Cell]
```

The grid **expanded** outward from the center to maintain centering!

## Two Behaviors Available

I've added a `lockOriginWhenSpacing` option to control this:

### Option 1: `lockOriginWhenSpacing = false` (Default)
**Behavior:** Grid expands equally in all directions
**Use when:** You want the grid visually centered at (0,0)
**Result:** Spacing makes grid bigger, shifts outward from center

```
Spacing = 0:     Spacing = 0.5:
    [C][C][C]         [C] [C] [C]
    [C][C][C]  →      [C] [C] [C]
    [C][C][C]         [C] [C] [C]
(centered at 0,0) (still centered, but bigger)
```

### Option 2: `lockOriginWhenSpacing = true`
**Behavior:** Grid expands from the origin point (first cell stays fixed)
**Use when:** You want bottom-left cell to stay at same position
**Result:** Spacing makes grid bigger, expands right/up only

```
Spacing = 0:     Spacing = 0.5:
[C][C][C]        [C] [C] [C]
[C][C][C]   →    [C] [C] [C]
[C][C][C]        [C] [C] [C]
(origin locked)  (expanded from origin)
```

## For Your Game

Since you have:
- Defending Row at top
- Ready Row in middle
- Prep Rows at bottom

You probably want `lockOriginWhenSpacing = true` with `GridOrigin.BottomLeft` or `TopLeft` so rows don't shift when you adjust spacing!

## How To Use

In Unity Inspector, on your Grid2D component:
1. Set `Cell Size` = 1.0 (or whatever you want)
2. Set `Cell Spacing` = 0.2 (adds 0.2 unit gaps)
3. Set `Lock Origin When Spacing` = **TRUE** (prevents shifting)
4. Set `Origin Mode` = BottomLeft or TopLeft (depends on your layout)

## Technical Details

The math is:
```csharp
cellStep = cellSize + cellSpacing;  // Distance between cell centers

// Example: 5 cells, size=1.0, spacing=0.5
// Cell 0: position = 0 * 1.5 = 0.0
// Cell 1: position = 1 * 1.5 = 1.5
// Cell 2: position = 2 * 1.5 = 3.0
// Cell 3: position = 3 * 1.5 = 4.5
// Cell 4: position = 4 * 1.5 = 6.0

// Total grid span = (5-1) * 1.5 + 1.0 = 7.0 units
```

Each cell is still 1.0 units, but they're 1.5 units apart (center to center).

## Summary

✅ **Cell Size**: Controls the visual size of each cell (never changes with spacing)
✅ **Cell Spacing**: Controls the GAP between cells (adds empty space)
✅ **Lock Origin When Spacing**: Prevents grid from shifting when spacing changes

The behavior is now **correct and flexible**!
