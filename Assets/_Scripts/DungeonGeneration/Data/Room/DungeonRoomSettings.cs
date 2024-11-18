using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonRoomData", menuName = "DungeonData/DungeonRoomSettings")]
public class DungeonRoomSettings : ScriptableObject , IRandomWithProbability
{
    [Header("Dungeon Room Data")]
    public DungeonRoomType RoomType = DungeonRoomType.None;

    [Min(1)]
    public int MinRoomAmount = 1;
    [Min(1)]
    public int MaxRoomAmount = 1;
    [Range(0, 1)]
    public float Probability = 0.5f;

    public bool IsSpawnTreasure = false;

    public bool IsSpawnEnemies = false;

    public bool IsSpawnTraps = false;

    [HideInInspector]
    public List<EnemySpawnData> EnemySpawnDatas;

    [HideInInspector]
    public List<GameObject> TreasurePrefabs;

    [HideInInspector]
    public List<PropData> TrapSpawnDatas;

    int IRandomWithProbability.Max => MaxRoomAmount;
    int IRandomWithProbability.Min => MinRoomAmount;
    float IRandomWithProbability.Probability => Probability;
}
