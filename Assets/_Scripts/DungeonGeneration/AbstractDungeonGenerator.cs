using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;

    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;

    [SerializeField]
    protected EnemySpawnManager enemySpawner = null;
    
    protected DungeonData dungeonData = null;

    [Header("Events")]
    public GameEvent OnFinishedRoomGeneration;

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
        //if (dungeonData == null)
        //    dungeonData = gameObject.AddComponent<DungeonData>();
    }
    public void GenerateDungeon()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }

        //Clear Data
        tilemapVisualizer.Clear();
        enemySpawner.ClearEnemies();

        //Generate Dungeon
        RunProceduralGeneration();

        //Event
        RunEvent();
    }
    private void RunEvent()
    {
        OnFinishedRoomGeneration.Raise(this, null);
    }
    protected abstract void RunProceduralGeneration();
}
