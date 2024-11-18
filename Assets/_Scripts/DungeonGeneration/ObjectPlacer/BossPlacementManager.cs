using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class BossPlacementManager : MonoBehaviour
{
    private DungeonData dungeonData;

    [SerializeField]
    private Transform bossParentSceneObject;

    [SerializeField]
    private bool showGizmo = false;

    [Header("Next Level Door Reference")]
    [SerializeField] private GameObject nextLevelDoorPrefab;

    [Header("Event")]
    public GameEvent OnFinishedBossPlacing;

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();    
    }
    public void ProcessBossPlacement()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }

        var bossSpawnDataList = dungeonData.BossRoom.GetEnemySpawnDatas();

        // Can't spawn mutiple boss yet
        if (bossSpawnDataList.Count < 1 || bossSpawnDataList.Count > 1)
        {
            Debug.LogError("Boss amount is Reach or Less," + this.name);
            return;
        }
        //Initilize boss
        var bossSpawnData = bossSpawnDataList[0];
        Vector2Int bossSize = bossSpawnData.Size;
        Vector2Int bossPosition = Vector2Int.CeilToInt(dungeonData.BossRoom.CenterPos);
        HashSet <Vector2Int> bossUsedArea = ProceduralGenerationAlgorithms.GetGrid(bossPosition, bossSize.x, bossSize.y);

        //SpawnBoss
        var bossReference = SpawnSingleEnemy(bossSpawnData, Vector2Int.CeilToInt(bossPosition));
        
        //Door
        var nextLevelDoorReference = SpawnObject(nextLevelDoorPrefab, bossPosition);

        //Add Interact with door
        SetInteracableToObject(nextLevelDoorReference);

        //Save data 
        SaveToDungeonRoomData(bossReference, bossUsedArea, dungeonData.BossRoom);
        dungeonData.SetNextDoorReference(nextLevelDoorReference);

        //Event
        RunEvent();

    }
    private void SetInteracableToObject(GameObject gameObject)
    {
        if(gameObject.GetComponentInChildren<Interacable>() == null)
        {
            Debug.Log("can't fine Interacable component");
            return;
        }
        gameObject.GetComponentInChildren<Interacable>().interactAction.AddListener(DungeonManager.Instance.LoadNextScene);

    }

    private GameObject SpawnObject(GameObject gameObject, Vector2 spawnPositions)
    {
        Vector3 worldPosition = (Vector3)spawnPositions;
        GameObject gameObjectReference = Instantiate(gameObject, worldPosition, Quaternion.identity);
        return gameObjectReference;
    }

    private GameObject SpawnSingleEnemy(EnemySpawnData enemyData, Vector2Int spawnPositions)
    {
        Vector3 worldPosition = (Vector3Int)spawnPositions;
        worldPosition += (Vector3)enemyData.PivotOffset;
        GameObject enemyReference = Instantiate(enemyData.EnemyPrefab, worldPosition, Quaternion.identity);
        //Instantiating Enemy
        enemyReference.transform.parent = bossParentSceneObject;
        enemyReference.name = enemyData.EnemyPrefab.name + "" + spawnPositions;
        return enemyReference;
    }
    private void SaveToDungeonRoomData(GameObject enemyReference, HashSet<Vector2Int> enemyAreaUsage, DungeonRoomData roomData)
    {
        dungeonData.SetBossReference(enemyReference);
        roomData.EnemiesReferences.Add(enemyReference);
        roomData.EnemiesPositions.UnionWith(enemyAreaUsage);
    }

    private void RunEvent()
    {
        OnFinishedBossPlacing.Raise(this, null);
    }
    private void OnDrawGizmosSelected()
    {
        if (showGizmo == false || dungeonData == null)
            return;

        UnityEngine.Color color = UnityEngine.Color.red;
        color.a = 0.4f;
        Gizmos.color = color;
        Gizmos.DrawCube(dungeonData.BossRoom.CenterPos + Vector2.one * 0.5f, Vector2.one);
    }
 
}