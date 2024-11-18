using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

//  Summary:
//      A base class contains procedural generation algorithms and generation helper.
public static class ProceduralGenerationAlgorithms
{
    //  Summary:
    //      Generate a HashSet of a path in a random direction.
    //      Offset(X,Y) :   Distances to move in the X and Y directions.
    //      Grid(X,Y)   :   The size of the grid in the X and Y dimensions.
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength, int gridX, int gridY, int offsetX, int offsetY)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        
        //Add start point
        path.UnionWith(GetGrid(startPosition,gridX,gridY));
        var previousPosition = startPosition;

        for (int i = 0; i < walkLength; i++) 
        {
            var direction = Direction2D.GetRandomCardinalDirection();
            Vector2Int newPosition;
            if (direction.Equals(Vector2Int.left) || direction.Equals(Vector2Int.right))
            {
                newPosition = previousPosition + direction * offsetX;
                previousPosition = newPosition;
            }
            else
            {
                newPosition = previousPosition + direction * offsetY;
                previousPosition = newPosition;
            }
            path.UnionWith(GetGrid(previousPosition,gridX,gridY));
        }
        return path;
    }

    //  Summary:
    //      Generate a grid HashSet (X * Y) pivot at bottom left cornor
    public static HashSet<Vector2Int> GetGrid(Vector2Int startPosition, int x, int y)
    {
        HashSet<Vector2Int> gridPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Vector2Int newPosition = new Vector2Int(startPosition.x + i, startPosition.y + j);
                gridPositions.Add(newPosition);
            }
        }
        return gridPositions;
    }
    //  Summary:
    //      Generate a circle with a specified diameter that satisfies dungeon generation,
    //      supporting both even and odd diameters parameter.
    public static HashSet<Vector2Int> GetCircle(Vector2Int centerPosition,int diameter)
    {
        if (diameter == 3)
        {
            //Handle Case :3
            return new HashSet<Vector2Int>
            {
                centerPosition,
                centerPosition+Vector2Int.up,
                centerPosition+Vector2Int.down,
                centerPosition+Vector2Int.left,
                centerPosition+Vector2Int.right,
            };
        }
        else if(diameter % 2 == 0)
        {
            return GetSimpleCircle(centerPosition, diameter);
        }
        else
        {
            return GetCircleMidPoint(centerPosition, diameter);
        }
    }

    //  Summary:
    //      The function calculates and adds points that are within a circle by iterating over a bounding box
    //      and checking if each point falls within the circle using the circle’s radius.
    public static HashSet<Vector2Int> GetSimpleCircle(Vector2Int centerPosition, int diameter)
    {
        //Circle Rasterization or Circle Filling Algorithm
        HashSet<Vector2Int> circlePoints = new HashSet<Vector2Int>();
        float radius = diameter / 2f;   
        Vector2 centerOffset = new Vector2(0.5f, 0.5f);

        // Loop through the bounding box of the circle
        for (int y = -Mathf.FloorToInt(radius); y <= Mathf.CeilToInt(radius); y++)
        {
            for (int x = -Mathf.FloorToInt(radius); x <= Mathf.CeilToInt(radius); x++)
            {
                // Calculate the point relative to the center
                Vector2 point = new Vector2(x, y) + centerOffset;

                //Check if the point is within the circle
                if (point.magnitude <= radius)
                {
                    circlePoints.Add(new Vector2Int(centerPosition.x - x, centerPosition.y - y));
                }
            }
        }
        return circlePoints;
    }


    //  Summary:
    //      Generates a HashSet of Vector2Int points within a filled circle of a given radius
    //      centered at center. It checks each point in the bounding box to see if it lies inside the circle and adds it to the set.
    //          IF USE TO GENERATE The result is like the head of a spiked mace. and can handle case diameter == 3 :3
    public static void FillCircle(HashSet<Vector2Int> circlePositions,Vector2Int centerPosition, int radius)
    {
        //Pythagorean Theorem
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    circlePositions.Add(new Vector2Int(centerPosition.x + x, centerPosition.y + y));
                }
            }
        }
    }

    // Summary:
    //      Generate a circle Outline HashSet pivot at center with specified diameter 
    public static HashSet<Vector2Int> GetCircleOutlineMidPoint(Vector2Int centerPosition, int diameter)
    {
        HashSet<Vector2Int> circlePoints = new HashSet<Vector2Int>();
        int radius = diameter / 2;
        int x = radius;
        int y = 0;
        int p = 1 - radius;

        // Add initial points
        AddSymmetricPoints(circlePoints, centerPosition, x, y);

        // Use Midpoint Circle Algorithm to determine the circle boundary points
        while (x > y)
        {
            y++;
            if (p <= 0)
            {
                p = p + 2 * y + 1;
            }
            else
            {
                x--;
                p = p + 2 * y - 2 * x + 1;
            }
            AddSymmetricPoints(circlePoints, centerPosition, x, y);
        }
        return circlePoints;
    }
    // Summary:
    //      Generate a circle HashSet pivot at center with specified diameter 
    public static HashSet<Vector2Int> GetCircleMidPoint(Vector2Int centerPosition, int diameter)
    {
        int radius = diameter / 2;
        var circlePoints = GetCircleOutlineMidPoint(centerPosition, diameter);
        // Fill the circle
        FillCircle(circlePoints,centerPosition, radius);

        return circlePoints;
    }

    //  Summary:
    //      method adds points to a HashSet based on symmetry around a center point.
    private static void AddSymmetricPoints(HashSet<Vector2Int> points, Vector2Int center, int x, int y)
    {
        points.Add(new Vector2Int(center.x + x, center.y + y));
        points.Add(new Vector2Int(center.x + x, center.y - y));
        points.Add(new Vector2Int(center.x - x, center.y + y));
        points.Add(new Vector2Int(center.x - x, center.y - y));
        points.Add(new Vector2Int(center.x + y, center.y + x));
        points.Add(new Vector2Int(center.x + y, center.y - x));
        points.Add(new Vector2Int(center.x - y, center.y + x));
        points.Add(new Vector2Int(center.x - y, center.y - x));
    }

    //  Summary:
    //      Create a line of positions starting from the specified position in the given direction,
    //      stopping when an obstacle in the provided set is encountered or maximum number of cells is reached.
    public static HashSet<Vector2Int> GetLineInDirection(Vector2Int startPosition,  Vector2Int direction, HashSet<Vector2Int> obstacles, int maxCells) 
    {
        
        HashSet<Vector2Int> linePositions = new HashSet<Vector2Int>();
        var currentPosition = startPosition;
        int currentCells = 0;

        while (!obstacles.Contains(currentPosition) && currentCells < maxCells) 
        {
            linePositions.Add(currentPosition);
            currentPosition += direction;
            currentCells++;
        }
        return linePositions;
    }

    //  Summary:
    //      Generate a List of positions representing a corridor of a specifield length, staring from a given position and moving in a random direction.
    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }      
        return corridor;
    }
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        //Not start at corner
        var xSplit = Random.Range(1, room.size.x);

        BoundsInt leftRoom = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt rightRoom = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(leftRoom);
        roomsQueue.Enqueue(rightRoom);
    }
    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        //Not start at corner
        var ySplit = Random.Range(1, room.size.y);

        BoundsInt upperRoom = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt lowerRoom = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit , room.size.z));

        roomsQueue.Enqueue(upperRoom);
        roomsQueue.Enqueue(lowerRoom);
    }

}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        //clockwise
        Vector2Int.up,    // UP
        Vector2Int.right,    // RIGHT
        Vector2Int.down,   // DOWN
        Vector2Int.left    // LEFT
    };
    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        //clockwise
        new Vector2Int(1, 1),   // UP-RIGHT
        new Vector2Int(1, -1),  // RIGHT-DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT 
        new Vector2Int(-1, 1)   // LEFT-UP
    };
    public static List<Vector2Int> eightDirectionsList = new List<Vector2Int>
    {
        //clockwise
        Vector2Int.up,          // UP
        new Vector2Int(1, 1),   // UP-RIGHT

        Vector2Int.right,       // RIGHT
        new Vector2Int(1, -1),  // RIGHT-DOWN

        Vector2Int.down,        // DOWN
        new Vector2Int(-1,-1),  // DOWN-LEFT 

        Vector2Int.left,        // LEFT
        new Vector2Int(-1, 1)   // LEFT-UP
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[Random.Range(0,cardinalDirectionsList.Count)];
    }
}