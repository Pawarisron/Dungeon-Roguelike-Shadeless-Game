using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SearchUtility
{
    //  Summary: Calulate the minimum distance from a starting position to all reachable positions
    //      within a specified set of valid positions using the Breadth-First Search (BFS) algorithm.
    public static Dictionary<Vector2Int, int> CreateDistancesMapAt(
        Vector2Int startPosition,
        HashSet<Vector2Int> validPositions,
        List<Vector2Int> directions)
    {
    Dictionary<Vector2Int, int> distancesMap = new Dictionary<Vector2Int, int>();
    Queue<Vector2Int> positionsToExplore = new Queue<Vector2Int>();

    positionsToExplore.Enqueue(startPosition);
    distancesMap[startPosition] = 0;

    while (positionsToExplore.Count > 0)
    {
        Vector2Int current = positionsToExplore.Dequeue();
        int currentDistance = distancesMap[current];

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = current + direction;

            if (validPositions.Contains(neighbor) && !distancesMap.ContainsKey(neighbor))
            {
                distancesMap[neighbor] = currentDistance + 1;
                positionsToExplore.Enqueue(neighbor);
            }
        }
    }
    return distancesMap;
    }
    //  Summary:
    //      Clamp distances map with given offset min and max
    public static Dictionary<Vector2Int, int> ClampDistancesWithOffset(Dictionary<Vector2Int, int> allEnemySpawnablePosition, int distanceOffsetMin, int distanceOffsetMax)
    {
        Dictionary<Vector2Int, int> clampedDistances = new Dictionary<Vector2Int, int>();
        int min = distanceOffsetMin;
        int max = allEnemySpawnablePosition.Values.Max() - distanceOffsetMax;
        foreach (var position in allEnemySpawnablePosition)
        {
            if (position.Value >= min && position.Value <= max)
            {
                clampedDistances.Add(position.Key, position.Value);
            }
        }
        return clampedDistances;
    }
    //  Summary:
    //      find max distance in distance Map
    public static int FindMaxDistanceIn(Dictionary<Vector2Int,int> distanceMap)
    {
        int maxDistance = 0;
        foreach (int distance in distanceMap.Values)
        {
            if (distance > maxDistance)
                maxDistance = distance;
        }
        return maxDistance;
    }
}
