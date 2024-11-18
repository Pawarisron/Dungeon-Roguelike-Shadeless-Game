using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


//  Summary:
//      Stores all the data about our dungeon. Useful when creating a save / load system
public class DungeonData : MonoBehaviour
{
    //public GameObject PlayerReference { get; private set; }    //Player GameObject reference
    public GameObject NextLevelDoorReference { get; private set; } //NextLevelDoor GameObject reference
    public GameObject BossReference { get; private set; } //Boss GameObject reference
    public DungeonRoomGraph RoomGraph { get; private set; } = new DungeonRoomGraph();
    public Dictionary<DungeonRoomSettings, int> RoomSettings { get; private set; } = new Dictionary<DungeonRoomSettings, int>();
    public DungeonRoomData BossRoom { get; private set; }
    public DungeonRoomData PlayerSpawnRoom { get; private set; }

    public static List<DungeonRoomType> DungeonMainRoomType = new List<DungeonRoomType>
    {
        DungeonRoomType.PlayerSpawn,
        DungeonRoomType.Boss,
    };
    public DungeonRoomSettings DefaultRoomSetting { get; private set; } //default SO of Room setting
    public DungeonRoomSettings DefaultCorridorRoomSetting { get; private set; } //default SO of Corridor setting
    public DungeonRoomSettings BossSpawnRoomSetting { get; private set; }
    public DungeonRoomSettings PlayerSpawnRoomSetting { get; private set; }

    public List<DungeonRoomData> Rooms => GetAllRooms();
    public List<DungeonRoomData> Nodes => RoomGraph.Nodes;
    public HashSet<Vector2Int> PathBase => GetAllPathBasePositions();  //Minimum PathBase Width that the agent can walk through
    public HashSet<Vector2Int> PathExpanded => GetAllPathExpandedPositions();  //PathBase Expanded to Corridor Size (Include PathBase)
    public HashSet<Vector2Int> RoomTilePositions => GetAllRoomTilePositions();
    public HashSet<Vector2Int> TilePositions => GetAllTilePositions();
    public HashSet<Vector2Int> PropPositions => GetAllPropPositions();
    public HashSet<Vector2Int> AvailablePositions => GetALLAvailablePositions();        // All Avaliable positions for new object placement include path
    public HashSet<Vector2Int> RoomsAvaliablePositions => GetALLRoomsAvailablePositions(); // All Avaliable positions for new object placement in each 
    public int TotalRandomedRooms => GetTotalRandomedRooms();


    public void Reset()
    {
        //Clear Object in room
        BossRoom = null;
        PlayerSpawnRoom = null;
        foreach (DungeonRoomData room in Rooms)
        {
            //Destory Props
            foreach (var objectReferenced in room.PropReferences)
            {
                Destroy(objectReferenced);
            }
            //Destory Enemies
            foreach (var objectReferenced in room.EnemiesReferences)
            {
                Destroy(objectReferenced);
            }
            //Destory Treasures
            foreach (var objectReferenced in room.TreasuresReferences)
            {
                Destroy(objectReferenced);
            }
            //Destory Traps
            foreach (var objectReferenced in room.TrapsReferences)
            {
                Destroy(objectReferenced);
            }
        }
        //Clear Data
        RoomGraph = new();
        RoomSettings = new();
        Destroy(NextLevelDoorReference);
        Destroy(BossReference);
    }
    public void SetBossReference(GameObject bossReference)
    {
        this.BossReference = bossReference;
    }

    public void SetNextDoorReference(GameObject nextLevelDoorReference)
    {
        this.NextLevelDoorReference = nextLevelDoorReference;
        //Hide Next level door
        NextLevelDoorReference.SetActive(false);
    }

    public void SetPlayerSpawnRoom(DungeonRoomData playerSpawnRoom)
    {
        if (Rooms.Contains(playerSpawnRoom))
        {
            this.PlayerSpawnRoom = playerSpawnRoom;
        }
        else
        {
            Debug.LogError("The specified room is not part of the rooms list.");
        }
    }
    public void SetBossSpawnRoom(DungeonRoomData bossRoom)
    {
        if (Rooms.Contains(bossRoom))
        {
            this.BossRoom = bossRoom;
        }
        else
        {
            Debug.LogError("The specified room is not part of the rooms list.");
        }
    }
    public void SetDefaultRoomSettings(DungeonRoomSettings defaultRoom, DungeonRoomSettings defaultCorridor)
    {
        this.DefaultRoomSetting = defaultRoom;
        this.DefaultCorridorRoomSetting = defaultCorridor;
    }

    public void SetMainRoomSettings(DungeonRoomSettings playerSpawnRoom, DungeonRoomSettings bossRoom)
    {
        this.BossSpawnRoomSetting = bossRoom;
        this.PlayerSpawnRoomSetting = playerSpawnRoom;
    }
    // get rooms by type using LINQ
    public List<DungeonRoomData> GetRoomsByType(DungeonRoomType roomType)
    {
        return RoomGraph.Nodes.Where(room => room.RoomType == roomType).ToList();
    }
    private HashSet<Vector2Int> GetALLRoomsAvailablePositions()
    {
        HashSet<Vector2Int> allRoomAvailablePositions = new HashSet<Vector2Int>();
        foreach (var room in Rooms)
        {
            allRoomAvailablePositions.UnionWith(room.AvaliablePositions);
        }
        return allRoomAvailablePositions;
    }

    private HashSet<Vector2Int> GetALLAvailablePositions()
    {
        HashSet<Vector2Int> allAvailablePositions = GetAllTilePositions();
        foreach (var room in Rooms)
        {
            allAvailablePositions.ExceptWith(room.ObjectPositions);
        }
        return allAvailablePositions;
    }

    private HashSet<Vector2Int> GetAllRoomTilePositions()
    {
        HashSet<Vector2Int> allTilePositions = new HashSet<Vector2Int>();
        foreach (var room in Rooms)
        {
            allTilePositions.UnionWith(room.FloorTiles);
        }
        return allTilePositions;
    }
    private HashSet<Vector2Int> GetAllTilePositions()
    {
        return new HashSet<Vector2Int>(GetAllRoomTilePositions().Union(PathExpanded));
    }

    private HashSet<Vector2Int> GetAllPropPositions()
    {
        HashSet<Vector2Int> allPropPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < Rooms.Count; i++)
        {
            DungeonRoomData room = Rooms[i];
            allPropPositions.UnionWith(room.PropPositions);
        }
        return allPropPositions;
    }
    // Returns a list of all rooms in the RoomGraph that are not of type 'Corridor'
    private List<DungeonRoomData> GetAllRooms()
    {
        var roomList = new List<DungeonRoomData>();
        for (int i = 0;i < RoomGraph.Nodes.Count; i++)
        {
            //if (RoomGraph.Nodes[i].RoomType != DungeonRoomType.Corridor && RoomGraph.Nodes[i].RoomType != DungeonRoomType.None)
            if (RoomGraph.Nodes[i].RoomType != DungeonRoomType.Corridor)
                roomList.Add(RoomGraph.Nodes[i]);
            
        }
        return roomList;
    }
    private HashSet<Vector2Int> GetAllPathBasePositions()
    {
        var pathPositions = new HashSet<Vector2Int>();
        foreach (var path in RoomGraph.Paths)
        {
            pathPositions.UnionWith(path.Value.PathBase);
        }
        return pathPositions;
    }
    private HashSet<Vector2Int> GetAllPathExpandedPositions()
    {
        var pathExpanded = new HashSet<Vector2Int>();
        foreach (var path in RoomGraph.Paths)
        {
            pathExpanded.UnionWith(path.Value.PathExpanded);
        }
        return pathExpanded;
    }

    private int GetTotalRandomedRooms()
    {
        //1 : SpawnPlayerRoom
        //2 : BossRoom
        int totalRoomCount = 2;
        foreach (var room in RoomSettings)
        {
            totalRoomCount += room.Value;
        }
        return totalRoomCount;
    }

    
}

