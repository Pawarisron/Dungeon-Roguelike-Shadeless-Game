using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using System.Text;

public class CorridorDungeonGenerator : SimpleRandomWalkDungeonGenerator
{

    [SerializeField]
    private int corridorLength = 14, corridorSize = 3;

    [SerializeField]
    [Range(0f, 0.9f)]
    private float corridorProbability = 0.1f;

    [SerializeField]
    private bool showGizmo = false,fixDeadEnds = true;

    private int totalCorridorCount;

    private int maxInterval = 100;

    private int totalRoomCount = 0;

    protected override void RunProceduralGeneration()
    {
        CorridorDungeonGeneration();
    }

    private void CorridorDungeonGeneration()
    {
        var roomGraph = dungeonData.RoomGraph;

        //Find Total Room and corridor Count
        totalRoomCount = dungeonData.TotalRandomedRooms;
        totalCorridorCount = Mathf.CeilToInt(totalRoomCount * corridorProbability);

        //Create Corridors
        HashSet<Vector2Int> potentialCorridorPositions = new HashSet<Vector2Int>();
        CreateCorridorNodes(roomGraph);

        //Increase Corridors Size
        IncreaseCorridorSize(roomGraph, corridorSize);

        //initialize Room type set potential node "Corridor" to "None"
        var roomsToCreate = InitializeRoomType(roomGraph);

        //Fix Dead Ende node 
        if (fixDeadEnds)
        {
            EliminateDeadEndNodes(roomGraph);
        }

        //Create Rooms
        CreateRooms(roomGraph, roomsToCreate);

        //Create Wall
        WallGenerator.CreateWalls(dungeonData, tilemapVisualizer, randomWalkParameters.wallHeight, randomWalkParameters.wallEdgeWidth);

        //Paint Tilemap
        tilemapVisualizer.PaintFloorTiles(dungeonData.TilePositions);
    }

    private void IncreaseCorridorSize(DungeonRoomGraph roomGraph, int corridorSize)
    {
        foreach (var path in roomGraph.Paths)
        {
            HashSet<Vector2Int> pathExpandedPositions = new HashSet<Vector2Int>();
            //Increase Corridor size via PathBase
            foreach (var position in path.Value.PathBase)
            {
                pathExpandedPositions.UnionWith(ProceduralGenerationAlgorithms.GetCircle(position, corridorSize));
            }
            path.Value.PathExpanded = pathExpandedPositions;
        }
    }
    private void EliminateDeadEndNodes(DungeonRoomGraph roomGraph)
    {
        List<DungeonRoomData> deadEnds = new List<DungeonRoomData>();
        //Debuging
        StringBuilder debugString = new StringBuilder();
        debugString.AppendLine("::DeadEnd Node::");

        int loopCount = 0;
        do
        {
            if (loopCount > maxInterval)
            {
                Debug.LogError("currentLoop reach maxInterval: EliminateDeadEndNodes");
                break;
            }
            
            // Eliminate dead end nodes
            foreach (var node in deadEnds)
            {
                roomGraph.RemoveNode(node);
            }

            // Clear deadEnds for this iteration
            deadEnds.Clear();

            // Find
            foreach (var node in roomGraph.Nodes)
            {
                //Dead End nodes are of corridor type, where their neighbors have a value of 1
                if (node.RoomType == DungeonRoomType.Corridor && roomGraph.GetNeighbors(node).Count <= 1)
                {
                    deadEnds.Add(node);
                    debugString.AppendLine(node.ToString());
                    
                }
            }
            loopCount++;
        }
        while (deadEnds.Count > 0); // Until all nested dead end nodes are eliminated
        
        Debug.Log(debugString.ToString());

    }
    private List<DungeonRoomData> InitializeRoomType(DungeonRoomGraph roomGraph)
    {
        //by percent
        int totalRooms = roomGraph.Nodes.Count - totalCorridorCount;

        //Random Shuffle and take
        //via generate GUID that ordered randomly and random select positions in protential positions for rooms with number of node equal to roomToCreatCount (.Take())
        List<DungeonRoomData> roomsToCreate = roomGraph.Nodes
            .OrderBy(x => Guid.NewGuid())
            .Take(totalRooms)
            .ToList();

        for (int i = 0; i < roomsToCreate.Count; i++)
        {
            roomsToCreate[i].SetRoomType(DungeonRoomType.None);
        }
        return roomsToCreate;
    }

    private void CreateRooms(DungeonRoomGraph roomGraph, List<DungeonRoomData> roomsToCreate)
    {
        //Generate Room Here
        for (int i = 0; i < roomsToCreate.Count; i++)
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, Vector2Int.RoundToInt(roomsToCreate[i].CenterPos));
            roomsToCreate[i].SetFloorTile(roomFloor);
        }
    }

    private void CreateCorridorNodes(DungeonRoomGraph roomGraph)
    {
        //initialize
        int currentNodeCount = 1;
        int currentLoopCount = 1;

        var currentPosition = startPosition;
        DungeonRoomData firstNode = new DungeonRoomData(currentPosition, 0, DungeonRoomType.Corridor);
        roomGraph.AddNode(firstNode);

        var previousRoom = firstNode;
        //find total node count by given totalRooms and corridor Probability
        int totalNodeCount = totalRoomCount + totalCorridorCount;
        while (currentNodeCount < totalNodeCount)
        {
            //Check Maximum interval
            if(currentLoopCount > maxInterval)
            {
                Debug.LogError("currentLoop reach maxInterval: Create Corridor Nodes");
                break;
            }
            //create corridor that connected to last node of each corridor
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count - 1];

            //check dupilcate node position
            DungeonRoomData duplicatePosition = roomGraph.FindDuplicatePosition(currentPosition);
            if (duplicatePosition == null)
            {
                //Add Node
                DungeonRoomData currentNode = new DungeonRoomData(currentPosition, currentNodeCount, DungeonRoomType.Corridor);
                roomGraph.AddNode(currentNode);
                
                //Add Edge
                PathInfo pathInfo = new PathInfo();
                pathInfo.PathBase.UnionWith(corridor); //Corridor Positions
                roomGraph.AddEdge(previousRoom, currentNode, corridorLength,pathInfo);

                previousRoom = currentNode;
                currentNodeCount++;
                //Debug.Log($"ROOM-EDGE = {previousRoom.RoomNumber}->{currentNode.RoomNumber}|{previousRoom.CenterPos}->{currentNode.CenterPos}");
            }
            else
            {
                previousRoom = duplicatePosition;
            }
            currentLoopCount++;
        }

    }
    private void OnDrawGizmosSelected()
    {
        if (showGizmo == false || dungeonData == null)
            return;

        Color color = Color.green;

        int colorIndex = 0;
        foreach (var position in dungeonData.PathExpanded)
        {
            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
            colorIndex++;
        }
        foreach (var position in dungeonData.PathBase)
        {
            color = Color.white;
            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
            colorIndex++;
        }

        // Draw PathExpanded nodes
        foreach (var node in dungeonData.RoomGraph.Nodes)
        {
            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(node.CenterPos + Vector2.one * 0.5f, Vector2.one);

            // Draw the center node text
            Vector2 centerPosition = node.CenterPos + Vector2.one * 0.5f; // Adjust node as needed
            //Handles.Label(centerPosition, node.ToString()); // Assuming centerPos is accessible
        }

    }
}
