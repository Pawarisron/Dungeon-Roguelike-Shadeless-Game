using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DungeonRoomAllocation : MonoBehaviour
{
    private DungeonData dungeonData;

    [SerializeField]
    private GameObject shopTrapDoorPrefab;

    [Header("Event")]
    public GameEvent OnFinishedRoomAllocation;

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }
    public void ProcessRoomAllocation()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }

        //Get rooms of type None and shuffle them
        List<DungeonRoomData> shuffledRoom = dungeonData.GetRoomsByType(DungeonRoomType.None) //Get rooms of type None and shuffle them
                        .OrderBy(x => Guid.NewGuid())
                        .ToList();

        //Apply room setting to Player Spawn Room
        var playerSpawnRoom = ApplyPlayerSpawnRoom(shuffledRoom);
        //Add to Dungeon data
        dungeonData.SetPlayerSpawnRoom(playerSpawnRoom);
        //Set Player Spawn point
        PlayerManager.Instance.SetRespawnPosition(playerSpawnRoom.CenterPos);
        //Set Shop trap door
        var shopTrapDoorRef = SpawnObject(shopTrapDoorPrefab,playerSpawnRoom.CenterPos + Direction2D.GetRandomCardinalDirection());
        SetInteracableToObject(shopTrapDoorRef);

        //Apply room setting to Boss Room
        //Boss Room will spawn at farhtest node from Player spawn
        DungeonRoomData farthestNode = dungeonData.RoomGraph.FindFarthestNode(playerSpawnRoom).farthestNode;

        //Add to Dungeon Data
        farthestNode.SetRoomSetting(dungeonData.BossSpawnRoomSetting);
        dungeonData.SetBossSpawnRoom(farthestNode);

        //Apply room setiing to each rooms
        ApplyRoomSettings(shuffledRoom);

        //Debug:::
        var distances = dungeonData.RoomGraph.Dijkstra(playerSpawnRoom);
        var result = new StringBuilder();
        result.Append("Dijkstra:\n");
        foreach (var node in distances)
        {
            result.AppendLine(node.Key + ":" + node.Value);
        }
        Debug.Log(result.ToString());
        result.AppendLine();
        result = new StringBuilder();
        result.Append("DungeonRoomAllocation:\n");
        foreach (var room in dungeonData.Rooms)
        {
            result.Append(room.ToString());
            result.AppendLine();
        }
        result.AppendLine();
        result.Append("NodeBFS:\n");
        foreach (var room in dungeonData.RoomGraph.BreadthFirstSearch(dungeonData.PlayerSpawnRoom))
        {
            result.Append(room.RoomNumber + ",");
        }
        Debug.Log(result.ToString());
        Debug.Log(dungeonData.RoomGraph.PrintGraph());
        //End Debug:::


        //RunEvent
        RunEvent();
    }
    private void SetInteracableToObject(GameObject gameObject)
    {
        if (gameObject.GetComponentInChildren<Interacable>() == null)
        {
            Debug.LogError("can't fine Interacable component");
            return;
        }
        gameObject.GetComponentInChildren<Interacable>().interactAction.AddListener(DungeonManager.Instance.ExitLevel);

    }
    private GameObject SpawnObject(GameObject gameObject, Vector2 spawnPositions)
    {
        Vector3 worldPosition = (Vector3)spawnPositions;
        GameObject gameObjectReference = Instantiate(gameObject, worldPosition, Quaternion.identity);
        return gameObjectReference;
    }
    private DungeonRoomData ApplyPlayerSpawnRoom(List<DungeonRoomData> rooms)
    {
        DungeonRoomData playerSpawnRoom = null;
        foreach (var room in rooms)
        {
            //Find leaf node and apply setting to it
            if (dungeonData.RoomGraph.GetNeighbors(room).Count == 1)
            {
                room.SetRoomSetting(dungeonData.PlayerSpawnRoomSetting);
                playerSpawnRoom = room;
                break;
            }
        }
        return playerSpawnRoom;
    }

    private void ApplyRoomSettings(List<DungeonRoomData> rooms)
    {

        foreach (var setting in dungeonData.RoomSettings)
        {

            int randomRoomAmount = setting.Value;
            //Get rooms of type None and shuffle them
            rooms = dungeonData.GetRoomsByType(DungeonRoomType.None)
                .OrderBy(x => Guid.NewGuid())
                .ToList();
            //Loop over a random selection of rooms
            foreach (var room in rooms.Take(randomRoomAmount))
            {
                room.SetRoomSetting(setting.Key);
            }
        }
    }
    private void RunEvent()
    {
        OnFinishedRoomAllocation.Raise(this, null);
    }
}
