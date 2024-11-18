using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//  Summary:
//      Holds all the data about the room
public class DungeonRoomData
{
    private DungeonRoomSettings Setting;  //Stores Room setting for create rooms.
    public DungeonRoomType RoomType {  get; private set; }
    public Vector2 CenterPos { get; private set; }
    public int RoomNumber { get; private set; }
    
    //For all
    public HashSet<Vector2Int> AvaliablePositions => GetAvaliablePositions(); //Avaliable positions for new object placement
    public HashSet<Vector2Int> ObjectPositions => GetAllObjectPositions(); //All position of objects in the room (grid scale)

    //Tiles Positions
    public HashSet<Vector2Int> FloorTiles { get; private set; } = new HashSet<Vector2Int>();    //All positions in room
    public HashSet<Vector2Int> NearWallTilesUp { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesDown { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesLeft { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesRight { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CornerTiles { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> InnerTiles { get; private set; } = new HashSet<Vector2Int>();

    //Props
    public HashSet<Vector2Int> PropPositions { get; private set; } = new HashSet<Vector2Int>();     //PropPositions include size of props
    public List<GameObject> PropReferences { get; private set; } = new List<GameObject>();    //Stores all references of props in scene on this rooms

    //Enemies
    public HashSet<Vector2Int> EnemiesPositions { get; private set; } = new HashSet<Vector2Int>();  //EnemiesPositions include size of enemies
    public List<GameObject> EnemiesReferences { get; private set; } = new List<GameObject>(); //Stores all references of enemies in scene on this rooms

    //Treasures
    public HashSet<Vector2Int> TreasurePositions { get; private set; } = new HashSet<Vector2Int>();     //PropPositions include size of Treasures and spacing Radius
    public List<GameObject> TreasuresReferences { get; private set; } = new List<GameObject>();   //Stores all references of treasures in scene on this rooms

    //Traps
    public HashSet<Vector2Int> TrapsPositions { get; private set; } = new HashSet<Vector2Int>();     //PropPositions include size of Traps and spacing Radius
    public List<GameObject> TrapsReferences { get; private set; } = new List<GameObject>();   //Stores all references of Traps in scene on this rooms

    public List<GameObject> GetTreasurePrefabs()
    {
        return new List<GameObject>(Setting.TreasurePrefabs);
    }
    public List<EnemySpawnData> GetEnemySpawnDatas()
    {
        return new List<EnemySpawnData>(Setting.EnemySpawnDatas);
    }
    public List<PropData> GetTrapSpawnDatas()
    {
        return new List<PropData>(Setting.TrapSpawnDatas);
    }

    public DungeonRoomData(Vector2 roomCenterPos, int roomNumber, DungeonRoomType roomType)
    {
        this.CenterPos = roomCenterPos;
        this.RoomNumber = roomNumber;
        this.RoomType = roomType;
    }
    public void SetFloorTile(HashSet<Vector2Int> floorTiles)
    {
        this.FloorTiles = floorTiles;
    }
    public void SetRoomType(DungeonRoomType roomType)
    {
        this.RoomType = roomType;
    }
    public void SetRoomSetting(DungeonRoomSettings setting)
    {
        this.Setting = setting;
        this.RoomType = setting.RoomType;
    }
    public bool IsTreasureSpawnable()
    {
        return Setting.IsSpawnTreasure;
    }

    public bool IsTrapsSpawnable()
    {
        return Setting.IsSpawnTraps;
    }

    public bool IsEnemySpawnable()
    {
        return Setting.IsSpawnEnemies;
    }

    public override string ToString()
    {
        if (Setting == null)
        {
            return $"node{RoomNumber}: {CenterPos}, {RoomType}, NULL";
        }
        return $"node{RoomNumber}: {CenterPos}, {RoomType}, {Setting.name}";
    }


    private HashSet<Vector2Int>GetAllObjectPositions() 
    {
        HashSet<Vector2Int> allObjectPositions = new HashSet<Vector2Int>();

        //add Object Positions
        allObjectPositions.UnionWith(PropPositions);
        allObjectPositions.UnionWith(EnemiesPositions);
        allObjectPositions.UnionWith(TreasurePositions);
        allObjectPositions.UnionWith(TrapsPositions);

        return allObjectPositions;
    }

    private HashSet<Vector2Int>GetAvaliablePositions()
    {
        //Return AvaliablePositions
        return new HashSet<Vector2Int>(FloorTiles.Except(GetAllObjectPositions()));
    }
}

