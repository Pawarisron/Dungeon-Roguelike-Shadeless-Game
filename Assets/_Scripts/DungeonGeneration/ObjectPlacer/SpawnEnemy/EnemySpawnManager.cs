using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemySpawnManager : MonoBehaviour
{
    //[SerializeField]
    //private EnemySpawnData[] enemyLevels;

    [SerializeField]
    private Transform parentEnemySpawnObject;  //parent object in scene that enemies spawn in it

    [SerializeField]
    private bool showGizmo = false;

    [Header("Event")]
    public GameEvent OnFinishedLevelGeneration;

    private HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();
    private DungeonData dungeonData = null;
    private HashSet<Vector2Int> avaliablePositions = new HashSet<Vector2Int>(); //for show gizmo

    // TODO: create super class ObjectPlacer
    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }
    public void ProcessSpawnEnemies()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }
        //Claer Debug data
        avaliablePositions.Clear();

        //Temporaly variable to avoid enemies spawn in same positions 
        usedPositions.Clear();

        //Get all room that type is "Regular"
        var enemiesSpawnRoom = dungeonData.GetRoomsByType(DungeonRoomType.Regular);

        var startPosition = dungeonData.PlayerSpawnRoom.CenterPos;
        var availableAreaInDungeon = dungeonData.AvailablePositions;

        //Find allEnemySpawnablePosition using BFS. aviod enemies spawn in dead spot
        Dictionary<Vector2Int, int> allEnemySpawnablePosition = SearchUtility.CreateDistancesMapAt(Vector2Int.CeilToInt(startPosition), availableAreaInDungeon, Direction2D.cardinalDirectionsList);


        //Debug
        var debugStringResult = new StringBuilder();
        debugStringResult.AppendLine("::EnemySpawnManager::");
        //Spawn enemies 
        foreach (var room in enemiesSpawnRoom)
        {
            //Spawn enemies if this room is spawnable
            if (room.IsEnemySpawnable())
            {
                //IntersectPosition between all room position with allEnemySpawnablePosition
                HashSet<Vector2Int> intersectPoints = new HashSet<Vector2Int>(room.FloorTiles.Where(point => allEnemySpawnablePosition.ContainsKey(point)));

                avaliablePositions.UnionWith(intersectPoints); //ForDebug
                //Spawn enemyReference in this room
                StringBuilder debugString = SpawnEnemies(intersectPoints, room.GetEnemySpawnDatas(), room);

                debugStringResult.Append(debugString);
                debugStringResult.AppendLine("--------------");
            }

        }
        Debug.Log(debugStringResult.ToString());

        //Run Event
        RunEvent();
    }
    private void RunEvent()
    {
        OnFinishedLevelGeneration.Raise(this, null);
    }

    private StringBuilder SpawnEnemies(HashSet<Vector2Int> enemySpawnPosition, List<EnemySpawnData> enemies, DungeonRoomData room)
    {
        //Debug
        var debugResult = new StringBuilder();
        debugResult.AppendLine(room.ToString());

        //Spawn each enemyReference in enemies list
        foreach (var enemyData in enemies)
        {
            int spawnAmount = RandomUtility.GetRandomValueWithProbability(enemyData);
            for (int i= 0; i < spawnAmount; i++)
            {
                Vector2Int spawnPosition = GetRandomAvailablePosition(enemySpawnPosition, enemyData.Size,room);
                //Check available positions
                if (spawnPosition != Vector2Int.zero)
                {
                    //Spawn
                    var enemyReference = SpawnSingleEnemy(enemyData, spawnPosition);
                    var enemyAreaUsage = MarkedUsedPositions(spawnPosition, enemyData.Size);
                    SaveToDungeonRoomData(enemyReference, enemyAreaUsage, room);
                }
            }
            //Debug
            debugResult.AppendLine($"Spawn: {enemyData.EnemyPrefab.name},SpawnAmount: {spawnAmount}");
        }
        return debugResult;
    }

    //Save Data to DungeonDataRoom that in DungeonData 
    private void SaveToDungeonRoomData(GameObject enemyReference,HashSet<Vector2Int> enemyAreaUsage, DungeonRoomData roomData)
    {
        roomData.EnemiesReferences.Add(enemyReference);
        roomData.EnemiesPositions.UnionWith(enemyAreaUsage);
    }
    public void ClearEnemies()
    {
        if(parentEnemySpawnObject != null)
        {
            foreach (Transform child in parentEnemySpawnObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Parent object is not assigned.");
        }
    }
    private Vector2Int GetRandomAvailablePosition(HashSet<Vector2Int> enemySpawnPosition, Vector2Int size, DungeonRoomData room)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        //Find avaliable positions
        foreach (var position in enemySpawnPosition)
        {
            if(IsAreaAvailable(position, size, enemySpawnPosition))
            {
                availablePositions.Add(position);
            }
        }
        //If Available
        if (availablePositions.Count > 0)
        {
            Vector2Int randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
            return randomPosition;

        }
        Debug.LogWarning("No avaiable spawn positions left.: "+room.ToString());
        return Vector2Int.zero; // No available positions left

    }

    private bool IsAreaAvailable(Vector2Int startPosition, Vector2Int size, HashSet<Vector2Int> enemySpawnPosition)
    {
        //Create enemyGridAreaUsage(width x height) for checking
        var grid = ProceduralGenerationAlgorithms.GetGrid(startPosition, size.x, size.y);
        foreach (var checkPosition in grid)
        {
            //reject if checkposition in used position or not in spawnable positions.
            if(usedPositions.Contains(checkPosition) || !enemySpawnPosition.Contains(checkPosition))
            {
                return false;
            }
        }
        return true;
    }

    private HashSet<Vector2Int> MarkedUsedPositions(Vector2Int starPosition, Vector2Int size)
    {
        var enemyGridAreaUsage = ProceduralGenerationAlgorithms.GetGrid(starPosition, size.x, size.y);
        usedPositions.UnionWith(enemyGridAreaUsage);
        return enemyGridAreaUsage;
    }

    private GameObject SpawnSingleEnemy(EnemySpawnData enemyData,Vector2Int spawnPositions)
    {
        Vector3 worldPosition = (Vector3Int)spawnPositions;
        worldPosition += (Vector3)enemyData.PivotOffset;
        GameObject enemyReference = Instantiate(enemyData.EnemyPrefab, worldPosition, Quaternion.identity);

        //Instantiating Enemy
        enemyReference.transform.parent = parentEnemySpawnObject;
        enemyReference.name = enemyData.EnemyPrefab.name+""+spawnPositions;
        return enemyReference;
    }
    private void OnDrawGizmosSelected()
    {
        if (showGizmo == false || dungeonData == null)
            return;

        //foreach (var position in usedPositions)
        //{
        //    Color color = Color.red;
        //    color.a = 0.4f;
        //    Gizmos.color = color;
        //    Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        //    colorIndex++;
        //}
        foreach (var position in avaliablePositions)
        {
            Color color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }
        foreach (var room in dungeonData.Rooms)
        {
            foreach (var position in room.EnemiesPositions)
            {
                Color color = Color.red;
                color.a = 0.4f;
                Gizmos.color = color;
                Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
            }

        }
    }
}
