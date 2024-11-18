using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//  Summary:
//      Create single circular dungeon
public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
    [Header("Dungeon Generator")]
    [SerializeField]
    protected SimpleRandomWalkSO randomWalkParameters;

    //Main Function:
    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPosition = RunRandomWalk(randomWalkParameters,startPosition);

        tilemapVisualizer.Clear();
        //CreateWalls "MUSH!!" be called before PaintFloorTiles if CreateWalls have height.
        WallGenerator.CreateWalls(floorPosition, tilemapVisualizer, randomWalkParameters.wallHeight, randomWalkParameters.wallEdgeWidth);
        tilemapVisualizer.PaintFloorTiles(floorPosition);
    }

    //  Summary:
    //      Run the "SimpleRandomWalk" Procedural Generation Algorithms iteratively.
    //      Via Parameters in SimpleRandomWalkSO.
    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO randomWalkParameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPosition = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkParameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, randomWalkParameters.walkLength, 
                randomWalkParameters.gridX, randomWalkParameters.gridY, randomWalkParameters.offsetX, randomWalkParameters.offsetY);

            floorPosition.UnionWith(path);
            if (randomWalkParameters.startRandomlyEachIteration)
            {
                currentPosition = floorPosition.ElementAt(Random.Range(0,floorPosition.Count));

            }
        }

        return floorPosition;
    }

}
