using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// attach this to enemy - for make they drop items
public class DropPile : MonoBehaviour
{
    public GameObject droppedItemPrefab; // loot prefab
    public List<LootSO> lootList; // this is for scriptable object

    [Header("Coin drop (added straight to the player's CoinManager on death)")]
    [Range(0, 100)]
    [SerializeField] private int coinDropChance = 80; // % chance to drop any coins at all
    [SerializeField] private int minCoinAmount = 5;   // inclusive
    [SerializeField] private int maxCoinAmount = 12;  // inclusive

    LootSO GetDroppedItem()
    {
        int randomNumber = Random.Range(1, 101); // 1 - 100

        List<LootSO> possiblItems = new List<LootSO>();
        foreach (LootSO item in lootList)
        {
            if (randomNumber <= item.dropChance)
            {
                possiblItems.Add(item);
            }
        }
        if (possiblItems.Count > 0)
        {
            LootSO droppedItem = possiblItems[Random.Range(0, possiblItems.Count)];
            return droppedItem;
        }
        return LootSO.GetEmptyItem();
    }

    // Roll the coin chance and, on success, credit a random amount straight
    // to the player's wallet. No physical pickup is spawned.
    public void DropCoins()
    {
        if (coinDropChance <= 0 || maxCoinAmount <= 0) return;
        if (Random.Range(1, 101) > coinDropChance) return; // failed the roll

        int low = Mathf.Max(0, minCoinAmount);
        int high = Mathf.Max(low, maxCoinAmount);
        int amount = Random.Range(low, high + 1); // inclusive
        if (amount <= 0) return;

        CoinManager wallet = FindFirstObjectByType<CoinManager>();
        if (wallet != null) wallet.AddCoin(amount);
    }

    // create item into the scene
    public void InstantiateLoot(Vector3 spawnPosition)
    {
        LootSO droppedItem = GetDroppedItem();
        if (!droppedItem.IsEmpty)
        {
            GameObject LootGameObject = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
            LootGameObject.GetComponent<Item>().InventoryItem = droppedItem.item;
            LootGameObject.GetComponent<Item>().Quantity = droppedItem.dropQuantity;

            
            // force or animaion in dropped
            float dropForce = 100f;
            Vector2 dropDirection = new Vector2(Random.Range(1f, 1f), Random.Range(1f, 1f));
            LootGameObject.GetComponent<Rigidbody2D>().AddForce(dropDirection * dropForce, ForceMode2D.Impulse);
            
        }
    } 
}

[System.Serializable]
public struct LootSO
{
    public int dropQuantity;
    public int dropChance; // percentage of drop able use 1 - 100 in this case the less is the rare
    public ItemSO item;

    public bool IsEmpty => item == null;

    // this is the resons for use stuct
    public LootSO ChanceQuantity(int newDropChance)
    {
        return new LootSO
        {
            item = this.item,
            dropChance = newDropChance,
        };
    }

    // struct cannot be null
    public static LootSO GetEmptyItem()
        => new LootSO
        {
            item = null,
            dropChance = 0,
        };
}
