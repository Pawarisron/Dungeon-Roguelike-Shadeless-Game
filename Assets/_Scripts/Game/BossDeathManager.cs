using System;
using System.Collections;
using UnityEngine;

public class BossDeathManager : MonoBehaviour
{
    private DungeonData dungeonData;
    private IHealth bossHp;
    private bool bossIsDeath = false;
    private GameObject nextLevelDoor;

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null, " + this.name);
            return;
        }
        // stop process
        this.enabled = false;
    }
    // Active when boss is placed
    public void ActiveBossDeathManager()
    {
        Debug.Log("ActiveBossDeathManager");
        this.enabled = true;
        Debug.Log("BossDeathManager is now enabled: " + this.enabled);
        // Find boss Hp
        var bossRef = dungeonData.BossReference;
        nextLevelDoor = dungeonData.NextLevelDoorReference;
        bossHp = bossRef.gameObject.GetComponentInChildren<IHealth>();
        if (bossHp == null || nextLevelDoor == null)
        {
            Debug.LogError("bossReference or nextLevelDoor is null, " + this.name);
            return;
        }
    }
    private void Update()
    {
        //Identify boss status
        if ((bossHp.CurrentHealth <= 0) && !bossIsDeath)
        {
            PostBossDeath();
        }

    }
    // method that call when boss is death
    private void PostBossDeath()
    {
        // ActiveBossDeathManager Next Level Door
        nextLevelDoor.SetActive(true);
        bossIsDeath = true;
        Debug.Log("Spawn Next Level Door");
    }
    
}
