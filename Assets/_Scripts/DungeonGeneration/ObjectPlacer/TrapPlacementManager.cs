using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TrapPlacementManager : MonoBehaviour
{
    [SerializeField]
    private GameObject parentTrapsSceneObject;

    [Header("Event")]
    public GameEvent OnFinishedTrapPlacing;

    [SerializeField]
    private bool showGizmo = false;

    private DungeonData dungeonData;
    private HashSet<Vector2Int> debugAvailablePositions = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }

    public void ProcessTrapPlacement()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }

        //ACTUALLY this is all Enemy Spawn Manager methods :3
        // I'll wrap it with Interface next time 

        //Temporaly variable to avoid enemies spawn in same positions 
        usedPositions.Clear();
        //For Debug
        debugAvailablePositions.Clear();

        //Get Regular room
        var roomGraph = dungeonData.GetRoomsByType(DungeonRoomType.Regular);

        //Debug
        var debugStringResult = new StringBuilder();
        debugStringResult.AppendLine("::TrapPlacementManager::");

        //Place trap for each room
        var dungeonRoomPath = dungeonData.PathBase;
        foreach (var room in roomGraph)
        {
            if (room.IsTrapsSpawnable())
            {
                var trapSettings = room.GetTrapSpawnDatas();

                HashSet<Vector2Int> roomAvaliablePosition = new HashSet<Vector2Int>(room.AvaliablePositions.Except(dungeonData.PathBase));
                StringBuilder debugString = SpawnTraps(roomAvaliablePosition, trapSettings, room);
                debugAvailablePositions.UnionWith(roomAvaliablePosition);
                debugStringResult.Append(debugString);
                debugStringResult.AppendLine("--------------");
            }
        }
        Debug.Log(debugStringResult.ToString());

        //Event
        RunEvent();

    }
    private StringBuilder SpawnTraps(HashSet<Vector2Int> trapSpawnPosition, List<PropData> trapDatas, DungeonRoomData room)
    {
        //Debug
        var debugResult = new StringBuilder();
        debugResult.AppendLine(room.ToString());

        //Spawn each propReference in trapDatas list
        foreach (var trap in trapDatas)
        {
            int spawnAmount = RandomUtility.GetRandomValueWithProbability(trap);
            for (int i = 0; i < spawnAmount; i++)
            {
                Vector2Int spawnPosition = GetRandomAvailablePosition(trapSpawnPosition, trap.PropSize, room);
                //Check available positions
                if (spawnPosition != Vector2Int.zero)
                {
                    //Spawn
                    var enemyReference = SpawnSingleTrap(trap, spawnPosition);
                    var enemyAreaUsage = MarkedUsedPositions(spawnPosition, trap.PropSize);
                    SaveToDungeonRoomData(enemyReference, enemyAreaUsage, room);
                }
            }
            //Debug
            debugResult.AppendLine($"Spawn: {trap.PropPrefab.name},SpawnAmount: {spawnAmount}");
        }
        return debugResult;
    }
    private void SaveToDungeonRoomData(GameObject TrapReference, HashSet<Vector2Int> trapAreaUsage, DungeonRoomData roomData)
    {
        roomData.TrapsReferences.Add(TrapReference);
        roomData.TrapsPositions.UnionWith(trapAreaUsage);
    }
    private GameObject SpawnSingleTrap(PropData trapData, Vector2Int spawnPositions)
    {
        Vector3 worldPosition = (Vector3Int)spawnPositions;
        GameObject propReference = Instantiate(trapData.PropPrefab, worldPosition, Quaternion.identity);

        //Instantiating 
        propReference.transform.parent = parentTrapsSceneObject.transform;
        propReference.name = trapData.PropPrefab.name + "" + spawnPositions;
        propReference.layer = parentTrapsSceneObject.layer;
        return propReference;
    }
    private Vector2Int GetRandomAvailablePosition(HashSet<Vector2Int> trapSpawnPositions, Vector2Int size, DungeonRoomData room)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        //Find avaliable positions
        foreach (var position in trapSpawnPositions)
        {
            if (IsAreaAvailable(position, size, trapSpawnPositions))
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
        Debug.LogWarning("No avaiable spawn positions left.: " + room.ToString());
        return Vector2Int.zero; // No available positions left

    }
    private bool IsAreaAvailable(Vector2Int startPosition, Vector2Int size, HashSet<Vector2Int> propSpawnPosition)
    {
        var grid = ProceduralGenerationAlgorithms.GetGrid(startPosition, size.x, size.y);
        foreach (var checkPosition in grid)
        {
            if (usedPositions.Contains(checkPosition) || !propSpawnPosition.Contains(checkPosition))
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
        foreach (var position in debugAvailablePositions)
        {
            Color color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }
        foreach (var room in dungeonData.Rooms)
        {
            foreach (var position in room.TrapsPositions)
            {
                Color color = Color.red;
                color.a = 0.4f;
                Gizmos.color = color;
                Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
            }

        }
    }
    public void RunEvent()
    {
        OnFinishedTrapPlacing.Raise(this, null);
    }
}
