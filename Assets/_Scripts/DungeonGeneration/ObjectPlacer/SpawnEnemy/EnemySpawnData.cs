using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData" ,menuName = "ObjectData/EnemyData")]
public class EnemySpawnData : ScriptableObject, IRandomWithProbability
{
    [Header("Enemy data:")]
    public GameObject EnemyPrefab;      // Enemy prefab

    [Header("Spawn Properties")]

    [Min(1)]
    public int MinSpawnAmount = 1;      // Minimum number of this enemy type
    [Min(1)]
    public int MaxSpawnAmount = 1;      // Maximum number of this enemy type

    [Range(0, 1)]
    public float SpawnProbability = 0.5f;       // Probability to spawn additional enemies (after minSpawnAmount)

    public Vector2Int Size = Vector2Int.one;        // Size of the enemy (width x height)

    public Vector2 PivotOffset = Vector2.zero;

    //Random with Propbability
    int IRandomWithProbability.Max { get => MaxSpawnAmount; }
    int IRandomWithProbability.Min { get => MinSpawnAmount; }
    float IRandomWithProbability.Probability { get => SpawnProbability;}
}
