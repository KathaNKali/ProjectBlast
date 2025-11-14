using UnityEngine;
using System.Collections.Generic;

namespace ProjectBlast.Grid
{
    /// <summary>
    /// Utility class with helper methods for grid operations.
    /// Static methods for maximum portability.
    /// </summary>
    public static class GridUtility
    {
        /// <summary>
        /// Get Manhattan distance between two grid positions
        /// </summary>
        public static int GetManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
        
        /// <summary>
        /// Get Euclidean distance between two grid positions
        /// </summary>
        public static float GetEuclideanDistance(Vector2Int a, Vector2Int b)
        {
            return Vector2Int.Distance(a, b);
        }
        
        /// <summary>
        /// Get Chebyshev distance (diagonal allowed, takes max of x/y difference)
        /// </summary>
        public static int GetChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
        
        /// <summary>
        /// Get all 4-directional neighbors (up, down, left, right)
        /// </summary>
        public static Vector2Int[] GetOrthogonalNeighbors(Vector2Int position)
        {
            return new Vector2Int[]
            {
                position + Vector2Int.up,
                position + Vector2Int.down,
                position + Vector2Int.left,
                position + Vector2Int.right
            };
        }
        
        /// <summary>
        /// Get all 8-directional neighbors (includes diagonals)
        /// </summary>
        public static Vector2Int[] GetAllNeighbors(Vector2Int position)
        {
            return new Vector2Int[]
            {
                position + new Vector2Int(0, 1),   // Up
                position + new Vector2Int(1, 1),   // Up-Right
                position + new Vector2Int(1, 0),   // Right
                position + new Vector2Int(1, -1),  // Down-Right
                position + new Vector2Int(0, -1),  // Down
                position + new Vector2Int(-1, -1), // Down-Left
                position + new Vector2Int(-1, 0),  // Left
                position + new Vector2Int(-1, 1)   // Up-Left
            };
        }
        
        /// <summary>
        /// Check if two grid positions are adjacent (4-directional)
        /// </summary>
        public static bool AreOrthogonalNeighbors(Vector2Int a, Vector2Int b)
        {
            return GetManhattanDistance(a, b) == 1;
        }
        
        /// <summary>
        /// Check if two grid positions are adjacent (including diagonals)
        /// </summary>
        public static bool AreNeighbors(Vector2Int a, Vector2Int b)
        {
            return GetChebyshevDistance(a, b) == 1;
        }
        
        /// <summary>
        /// Get all positions in a line between two points (Bresenham's algorithm)
        /// </summary>
        public static List<Vector2Int> GetLinePositions(Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> line = new List<Vector2Int>();
            
            int x = start.x;
            int y = start.y;
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                line.Add(new Vector2Int(x, y));
                
                if (x == end.x && y == end.y)
                    break;
                    
                int e2 = 2 * err;
                
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
            
            return line;
        }
        
        /// <summary>
        /// Get all positions in a rectangular area
        /// </summary>
        public static List<Vector2Int> GetRectanglePositions(Vector2Int bottomLeft, Vector2Int topRight)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            for (int x = bottomLeft.x; x <= topRight.x; x++)
            {
                for (int y = bottomLeft.y; y <= topRight.y; y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Get all positions in a circle (filled)
        /// </summary>
        public static List<Vector2Int> GetCirclePositions(Vector2Int center, int radius)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        positions.Add(center + new Vector2Int(x, y));
                    }
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// Get direction from one position to another (8-way)
        /// </summary>
        public static Vector2Int GetDirection(Vector2Int from, Vector2Int to)
        {
            Vector2Int diff = to - from;
            return new Vector2Int(
                diff.x > 0 ? 1 : (diff.x < 0 ? -1 : 0),
                diff.y > 0 ? 1 : (diff.y < 0 ? -1 : 0)
            );
        }
        
        /// <summary>
        /// Clamp position to grid bounds
        /// </summary>
        public static Vector2Int ClampToGrid(Vector2Int position, Vector2Int gridDimensions)
        {
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, gridDimensions.x - 1),
                Mathf.Clamp(position.y, 0, gridDimensions.y - 1)
            );
        }
    }
}
