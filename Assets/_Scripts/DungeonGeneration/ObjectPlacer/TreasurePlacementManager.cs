using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasurePlacementManager : MonoBehaviour
{
    [SerializeField]
    [Range(1,5)]
    int spacingRadius = 1; //Spaceing for player can walk through

    [SerializeField]
    public GameEvent OnFinishedTreasurePlacement;

    [SerializeField]
    private GameObject treasuresParentSceneObject;

    [SerializeField]
    private bool showGizmo = false;

    private DungeonData dungeonData;

    private HashSet<Vector2Int> debugTreasureSpacingPositions = new HashSet<Vector2Int>(); //For Debug

    public void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }

    public void ProcessTreasurePlacement()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }

        //Debug
        debugTreasureSpacingPositions.Clear();

        //Find Regular Rooms
        var regularRooms = dungeonData.GetRoomsByType(DungeonRoomType.Regular);

        //Spawn each Trasure Rooms
        foreach (var room in regularRooms)
        {
            if (room.IsTreasureSpawnable())
            {
                var treasurePrefabs = room.GetTreasurePrefabs();
                if (treasurePrefabs.Count == 0)
                {
                    Debug.LogError("Treasures Prefabs is null," + this.name);
                    return;
                }

                //TODO: make treasure spawn multiple objects
                var treasureReference = SpawnSingleTreasure(treasurePrefabs[0], Vector2Int.CeilToInt( room.CenterPos ));
                //Spaceing for player can walk through 
                var spaceingPositions = ProceduralGenerationAlgorithms.GetCircleMidPoint(Vector2Int.CeilToInt(room.CenterPos), spacingRadius * 2);
                debugTreasureSpacingPositions.UnionWith(spaceingPositions);
                SaveToDungeonRoomData(treasureReference, spaceingPositions, room);
            }
        }

        //Run Event
        Runevent();
    }

    private GameObject SpawnSingleTreasure(GameObject treasurePrefab, Vector2Int spawnPositions)
    {
        Vector3 worldPosition = (Vector3Int)spawnPositions;
        GameObject treasureObject = Instantiate(treasurePrefab, worldPosition, Quaternion.identity);

        //Instantiating Treasure
        treasureObject.transform.parent = treasuresParentSceneObject.transform;
        treasureObject.name = treasurePrefab.name + "" + spawnPositions;
        return treasureObject;
    }
    private void SaveToDungeonRoomData(GameObject treasureReferences, HashSet<Vector2Int> treasureAreaUsage, DungeonRoomData roomData)
    {
        roomData.TreasuresReferences.Add(treasureReferences);
        roomData.TreasurePositions.UnionWith(treasureAreaUsage);
    }

    private void Runevent()
    {
        OnFinishedTreasurePlacement.Raise(this, null);
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmo == false || dungeonData == null)
            return;

        foreach (var position in debugTreasureSpacingPositions)
        {
            Color color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }
    }
}
