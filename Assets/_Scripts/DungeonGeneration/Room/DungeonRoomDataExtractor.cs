using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class DungeonRoomDataExtractor : MonoBehaviour
{
    private DungeonData dungeonData;

    [SerializeField]
    private bool showGizmo = false;

    [Header("Event")]
    public GameEvent OnFinishedRoomExtractor;

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }
    public void ProcessRooms(Component sender, object data)
    {
        if (dungeonData == null)
        {
            Debug.LogWarning("DungeonData is null,"+this.name);
            return;
        }
            

        foreach (DungeonRoomData room in dungeonData.Rooms)
        {
            //find corener, near wall and inner tiles
            foreach (Vector2Int tilePosition in room.FloorTiles)
            {
                int neighboursCount = 4;

                if (room.FloorTiles.Contains(tilePosition + Vector2Int.up) == false)
                {
                    room.NearWallTilesUp.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.down) == false)
                {
                    room.NearWallTilesDown.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.right) == false)
                {
                    room.NearWallTilesRight.Add(tilePosition);
                    neighboursCount--;
                }
                if (room.FloorTiles.Contains(tilePosition + Vector2Int.left) == false)
                {
                    room.NearWallTilesLeft.Add(tilePosition);
                    neighboursCount--;
                }

                //find corners
                if (neighboursCount <= 2)
                    room.CornerTiles.Add(tilePosition);

                if (neighboursCount == 4)
                    room.InnerTiles.Add(tilePosition);
            }

            room.NearWallTilesUp.ExceptWith(room.CornerTiles);
            room.NearWallTilesDown.ExceptWith(room.CornerTiles);
            room.NearWallTilesLeft.ExceptWith(room.CornerTiles);
            room.NearWallTilesRight.ExceptWith(room.CornerTiles);
        }

        //Delay 1 Sec
        //Invoke("RunEvent", 1f);
        RunEvent();
    }

    private void RunEvent()
    {
        OnFinishedRoomExtractor.Raise(this, null);
    }

    private void OnDrawGizmosSelected()
    {
        if (dungeonData == null || showGizmo == false)
            return;
        foreach (DungeonRoomData room in dungeonData.Rooms)
        {
            //Draw inner tiles
            Color color = Color.yellow;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.InnerTiles)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
            //Draw near wall tiles UP
            color = Color.blue;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.NearWallTilesUp)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
            //Draw near wall tiles DOWN
            color = Color.green;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.NearWallTilesDown)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
            //Draw near wall tiles RIGHT
            color = Color.white;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.NearWallTilesRight)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
            //Draw near wall tiles LEFT
            color = Color.cyan;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.NearWallTilesLeft)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
            //Draw near wall tiles CORNERS
            color = Color.magenta;
            color.a = 0.6f;
            Gizmos.color = color;
            foreach (Vector2Int floorPosition in room.CornerTiles)
            {
                if (dungeonData.PathBase.Contains(floorPosition))
                    continue;
                Gizmos.DrawCube(floorPosition + Vector2.one * 0.5f, Vector2.one);
            }
        }
    }
}