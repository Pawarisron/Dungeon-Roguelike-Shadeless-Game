using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameEvent OnDungeonStart;
    private DungeonData dungeonData;

    [SerializeField]
    private Canvas loadingScreen;

    private void Awake()
    {
        loadingScreen.enabled = true;
    }

    private void Start()
    {
        Debug.Log("Dungeon Start");
        dungeonData = FindAnyObjectByType<DungeonData>();
        StartDungeon();
    }
    //Pre
    public void StartDungeon()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null, " + this.name);
            return;
        }
        dungeonData.Reset();
        RunEvent();
    }
    //Post
    public void PostProcessing()
    {
        PlayerManager.Instance.RespawnPlayer();
        Invoke("RunPost", 1.5f);
    }
    private void RunPost()
    {
        loadingScreen.enabled = false;
    }

    private void RunEvent()
    {
        OnDungeonStart.Raise(this, null);   
    }
}
