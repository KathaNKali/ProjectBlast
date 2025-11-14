# Flexible Grid System for Unity

A highly portable, extensible grid system that can be easily moved between Unity projects. Supports 2D, 3D, and Isometric grids out of the box.

## Features

- ✅ **Multiple Grid Types**: 2D, 3D, Isometric (easily extensible for Hex, etc.)
- ✅ **Interface-Based**: `IGridSystem` interface for maximum flexibility
- ✅ **Component-Based**: Modular design with reusable components
- ✅ **Event-Driven**: Cell-level and grid-level events for reactive programming
- ✅ **Mobile-Optimized**: Efficient memory usage, no LINQ, cache-friendly
- ✅ **Debug Visualization**: Built-in Gizmo drawing for development
- ✅ **Fully Documented**: Comprehensive XML documentation
- ✅ **Zero Dependencies**: Only uses Unity's core libraries

## Quick Start

### 1. Basic 2D Grid Setup

```csharp
using ProjectBlast.Grid;

// Add Grid2D component to a GameObject
public class GameSetup : MonoBehaviour
{
    [SerializeField] private Grid2D grid;
    
    void Start()
    {
        // Grid auto-initializes in Awake
        
        // Get a cell
        GridCell cell = grid.GetCell(new Vector2Int(5, 5));
        
        // Place something
        cell.Occupy(myGameObject);
        
        // Convert coordinates
        Vector3 worldPos = grid.GridToWorld(new Vector2Int(3, 3));
        Vector2Int gridPos = grid.WorldToGrid(transform.position);
    }
}
```

### 2. Configure in Inspector

- **Width/Height**: Grid dimensions
- **Cell Size**: Size of each cell in world units
- **Cell Spacing**: Gap between cells
- **Origin Mode**: BottomLeft, Center, or TopLeft

### 3. Add Visualizer (Optional)

```csharp
// Add GridVisualizer component to same GameObject as Grid2D
// Configure colors and what to show in inspector
// See grid lines in Scene view while developing
```

## Grid Types

### Grid2D
Perfect for top-down 2D games, tower defense, roguelikes.

```csharp
Grid2D grid = gameObject.AddComponent<Grid2D>();
```

### Grid3D
For 3D top-down games where Y-axis is up.

```csharp
Grid3D grid = gameObject.AddComponent<Grid3D>();
```

### GridIsometric
For isometric perspective games.

```csharp
GridIsometric grid = gameObject.AddComponent<GridIsometric>();
```

## Common Operations

### Cell Manipulation

```csharp
// Get cell
GridCell cell = grid.GetCell(new Vector2Int(x, y));

// Check state
bool isEmpty = !cell.IsOccupied;
bool canWalk = cell.IsWalkable;

// Place entity
cell.Occupy(myEntity);

// Remove entity
object entity = cell.Clear();

// Change type
cell.SetType(CellType.Buildable);
```

### Area Operations

```csharp
// Get cells in radius
GridCell[] neighbors = grid.GetCellsInRadius(center, radius);

// Get rectangular area
GridCell[] area = grid.GetCellsInArea(bottomLeft, topRight);

// Set multiple cells
grid.SetCellTypeInArea(bottomLeft, topRight, CellType.Path);
```

### Input Handling

```csharp
// For 2D games
void OnMouseClick(Vector2 screenPos)
{
    GridCell cell = grid.GetCellAtScreenPosition(screenPos);
    if (cell != null && !cell.IsOccupied)
    {
        PlaceTower(cell);
    }
}

// For 3D games (raycast)
void OnMouseClick(Vector2 screenPos)
{
    GridCell cell = grid3D.GetCellAtScreenPosition(screenPos);
    // Same as 2D but uses Physics raycast
}
```

### Events

```csharp
// Cell-level events
cell.OnOccupied += (c, entity) => Debug.Log("Cell occupied!");
cell.OnCleared += (c, entity) => Debug.Log("Cell cleared!");
cell.OnTypeChanged += (c, oldType, newType) => UpdateVisuals();

// Grid-level events
grid.OnGridInitialized += (g) => Debug.Log("Grid ready!");
grid.OnCellOccupied += (c) => PlaySound();
grid.OnCellCleared += (c) => RefreshUI();
```

## Extending the System

### Custom Grid Type (e.g., Hexagonal)

```csharp
using ProjectBlast.Grid;

public class GridHex : GridSystem
{
    public override Vector3 GridToWorld(Vector2Int gridPosition)
    {
        // Implement hex coordinate conversion
        float x = gridPosition.x * cellSize * 1.5f;
        float y = gridPosition.y * cellSize * Mathf.Sqrt(3);
        
        if (gridPosition.x % 2 != 0)
            y += cellSize * Mathf.Sqrt(3) * 0.5f;
            
        return transform.position + new Vector3(x, y, 0);
    }
    
    public override Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // Implement inverse conversion
        // ... hex grid math
    }
}
```

### Custom Cell Type

```csharp
public class CustomGridCell : GridCell
{
    public int CustomProperty { get; set; }
    public GameObject VisualPrefab { get; set; }
    
    public CustomGridCell(Vector2Int gridPos, Vector3 worldPos) 
        : base(gridPos, worldPos)
    {
        // Custom initialization
    }
}

// Override CreateCell in your grid
protected override GridCell CreateCell(Vector2Int gridPos, Vector3 worldPos)
{
    return new CustomGridCell(gridPos, worldPos);
}
```

## Utility Functions

```csharp
using ProjectBlast.Grid;

// Distance calculations
int manhattan = GridUtility.GetManhattanDistance(posA, posB);
float euclidean = GridUtility.GetEuclideanDistance(posA, posB);

// Get neighbors
Vector2Int[] neighbors = GridUtility.GetOrthogonalNeighbors(pos);
Vector2Int[] allNeighbors = GridUtility.GetAllNeighbors(pos); // includes diagonals

// Line of sight / targeting
List<Vector2Int> line = GridUtility.GetLinePositions(from, to);

// Area effects
List<Vector2Int> circle = GridUtility.GetCirclePositions(center, radius);
```

## Performance Tips

1. **Cache Grid References**: Store grid reference in Awake/Start
2. **Avoid GetCell in Update**: Cache cells you need frequently
3. **Use Events**: React to changes rather than polling
4. **Batch Operations**: Use area methods instead of individual cell loops
5. **Pre-warm**: Initialize grid in loading screen, not during gameplay

## Porting to Another Project

1. Copy `Assets/Scripts/Grid/Base/` folder to new project
2. Change namespace if needed (optional)
3. That's it! Zero dependencies on other project code

## License

This grid system is designed to be freely reusable across projects.

## Support

For questions or issues, refer to the comprehensive XML documentation in each class.
