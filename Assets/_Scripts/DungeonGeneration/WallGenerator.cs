using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public static class WallGenerator
{
    //  Summary:
    //      Find wall positions and create walls without specific height values.
    //      This means the walls will not have assigned heights.
    public static void CreateWalls(HashSet<Vector2Int> floorPositions,  TilemapVisualizer tilemapVisualizer)
    {
        var wallPositions = FindWallInDirections(floorPositions, Direction2D.eightDirectionsList);
        tilemapVisualizer.PaintWallTiles(wallPositions);
        tilemapVisualizer.PaintColliderTiles(wallPositions);
    }
    
    //  Summary:
    //      find wall positions and create walls with specific height values


    // TODO: Refactor CreateWalls
    public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer, int wallHeight, int wallEdgeWidth)
    {
        int totalWallHeight = wallHeight + wallEdgeWidth;

        //High Wall
        var highWallPositions = FindHighWallPositions(floorPositions, totalWallHeight);
        var newFloorPositions = FindCoverFloorPositions(floorPositions, totalWallHeight);

        //Pass by reference with union
        floorPositions.UnionWith(newFloorPositions);

        var tileHighWallPositions = CreateHighWalls(tilemapVisualizer, highWallPositions, wallHeight);

        //Walls Edge
        var floorEdgePositions = floorPositions.Union(tileHighWallPositions).ToHashSet();
        var wallsEdgePositions = FindWallInDirections(floorEdgePositions, Direction2D.eightDirectionsList);
        CreateWallsEdge(tilemapVisualizer, wallsEdgePositions);
        
        //Collider
        CreateWallsCollider(tilemapVisualizer, tileHighWallPositions.Union(wallsEdgePositions).ToHashSet());

        return newFloorPositions;
    }

    public static void CreateWalls(DungeonData dungeonData, TilemapVisualizer tilemapVisualizer, int wallHeight, int wallEdgeWidth)
    {
        int totalWallHeight = wallHeight + wallEdgeWidth;
        foreach (var room in dungeonData.Rooms)
        {
            //Cover floor
            var newFloorPositions = FindCoverFloorPositions(room.FloorTiles.Union(dungeonData.PathExpanded).ToHashSet(), totalWallHeight);
            room.FloorTiles.UnionWith(newFloorPositions);
        }
        //High Wall
        var highWallPositions = (FindHighWallPositions(dungeonData.TilePositions, totalWallHeight));
        var tileHighWallPositions = CreateHighWalls(tilemapVisualizer, highWallPositions, wallHeight);

        //Walls Edge
        var floorEdgePositions = dungeonData.TilePositions.Union(tileHighWallPositions).ToHashSet();
        var wallsEdgePositions = FindWallInDirections(floorEdgePositions, Direction2D.eightDirectionsList);
        CreateWallsEdge(tilemapVisualizer, wallsEdgePositions);

        //Collider
        CreateWallsCollider(tilemapVisualizer, tileHighWallPositions.Union(wallsEdgePositions).ToHashSet());
    }


    //  Summary:
    //      Creates high walls on the Tilemap by generating and painting wall tiles based on specified positions and height.
    //      Return: A HashSet containing the positions of the wall tiles for the created high walls.
    private static HashSet<Vector2Int> CreateHighWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> highWallPositions, int wallHeight)
    {
        HashSet<Vector2Int> wallTilePositions = new HashSet<Vector2Int>();
        wallTilePositions.UnionWith(highWallPositions);
        foreach (var position in highWallPositions)
        {
            wallTilePositions.UnionWith( ProceduralGenerationAlgorithms.GetGrid(position, 1, wallHeight) );
        }
        tilemapVisualizer.PaintWallTiles(wallTilePositions);

        return wallTilePositions;
    }
    private static void CreateWallsCollider(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> wallPositions)
    {
        tilemapVisualizer.PaintColliderTiles(wallPositions);
    }
    private static void CreateWallsEdge(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> wallsEdgePositions)
    {
        tilemapVisualizer.PaintWallsEdgeTiles(wallsEdgePositions);
    }

    //  Summary:
    //      Find and returns positions where walls can be placed based on a list of directions from given floor position
    public static HashSet<Vector2Int> FindWallInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPosition = new HashSet<Vector2Int>();
        foreach (var position in floorPositions) 
        {
            foreach (var direction in directionList)
            {
                //check neighbour position to find wall for each direction
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                {
                    wallPosition.Add(neighbourPosition);
                }
            }
        }
        return wallPosition;
    }

    //  Summary:
    //      Indentifies positions for placing high walls based on exiting floor position and wall height.
    private static HashSet<Vector2Int> FindHighWallPositions(HashSet<Vector2Int> floorPositions, int totalWallHeight)
    {
        HashSet<Vector2Int> highWallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            //Find highwall position
            var neighourPosition = position + Vector2Int.up;
            if(floorPositions.Contains(neighourPosition) == false)
            {
                //Check whether the wall can be placed
                var highWall = ProceduralGenerationAlgorithms.GetGrid(neighourPosition, 1, totalWallHeight);
                if (highWall.Intersect(floorPositions).Count() == 0)
                {
                    highWallPositions.Add(neighourPosition);
                }
            }
        }
        return highWallPositions;
    }

    //  Summary:
    //      Finds and returns new floor positions that need to be covered based on existing floor positions and wall height.
    private static HashSet<Vector2Int> FindCoverFloorPositions(HashSet<Vector2Int> floorPositions, int totalWallHeight)
    {
        HashSet<Vector2Int> newFloorPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            //Find highwall position
            var neighourPosition = position + Vector2Int.up;
            if (floorPositions.Contains(neighourPosition) == false)
            {
                //Check whether the wall can be placed
                var highWall = ProceduralGenerationAlgorithms.GetGrid(neighourPosition, 1, totalWallHeight);
                if (highWall.Intersect(floorPositions).Count() > 0)
                {
                    //Cover The floor
                    newFloorPositions.UnionWith(ProceduralGenerationAlgorithms.GetLineInDirection(neighourPosition, Vector2Int.up, floorPositions, totalWallHeight));
                }
            }
        }
        return newFloorPositions;
    }
}
