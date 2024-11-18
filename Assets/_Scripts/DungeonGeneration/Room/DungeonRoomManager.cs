using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoomManager : MonoBehaviour
{
    [Header("Main Room")]
    [SerializeField] private DungeonRoomSettings playerSpawnRoom;
    [SerializeField] private DungeonRoomSettings bossRoom;

    [Header("Add Rooms")]
    [SerializeField] private DungeonRoomSettings[] dungeonRoomSettings;   //Stores Room Type
    
    [Header("Default Rooms")]
    [SerializeField] private DungeonRoomSettings defaultRoom;
    [SerializeField] private DungeonRoomSettings defaultCorridor;

    [Header("Event")]
    public GameEvent OnFinishedRoomManager;

    [Header("Main Camera")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;

    private DungeonData dungeonData;


    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }
    //Random amount of Room type in dungeon
    public void ProcessRoomManagement() //OnGameStarted
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null, " + this.name);
            return;
        }
        //TODO: create debug mode for unorder scene debuging;
        //Attach Main camera to Player
        if(PlayerManager.Instance == null)
        {
            Debug.LogError("The PlayerManager is null because the scene did not run in the correct order, and I haven't set up debug mode yet. T_T");
            return;
        }
        mainCamera.Follow = PlayerManager.Instance.GetPlayerTransform();

        //Add data To DungeonData
        dungeonData.SetDefaultRoomSettings(defaultRoom,defaultCorridor);
        dungeonData.SetMainRoomSettings(playerSpawnRoom, bossRoom);
        
        //Add Room Settings
        var roomSettings = dungeonData.RoomSettings;

        foreach (var room in dungeonRoomSettings)
        {
            //Not contain main room
            if (DungeonData.DungeonMainRoomType.Contains(room.RoomType))
            {
                Debug.LogError($"Error: The room type '{room.RoomType}' is not allowed in random room generation.");
                return;
            }
            int roomAmont = RandomUtility.GetRandomValueWithProbability(room);
            roomSettings.Add(room, roomAmont);
        }

        //DEBUG::
        int total = 0;
        var result = new System.Text.StringBuilder();
        result.AppendLine("::RoomManager::");
        foreach (var room in dungeonData.RoomSettings)
        {
            result.AppendLine(room.Key.name + ": " + room.Value + ": " + room.Key.RoomType);
            total += room.Value;
        }
        result.AppendLine("\nBossRoom and PlayerSpawnRoom = 2 Room");
        result.AppendLine("TotalAddedRoom:" + total);
        result.AppendLine("ActuallyTotal:" + (total + 2));
        Debug.Log(result.ToString());

        //Debug.Log(dungeonData.DefaultRoomSetting.name+":"+dungeonData.DefaultRoomSetting.RoomType);
        //Debug.Log(dungeonData.DefaultCorridorRoomSetting.name+":"+dungeonData.DefaultCorridorRoomSetting.RoomType);

        //RunEvent
        RunEvent();

    }

    private void RunEvent()
    {
        OnFinishedRoomManager.Raise(this, null);
    }

}
