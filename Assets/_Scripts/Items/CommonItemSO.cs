using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CommonItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Nothing";

    public AudioClip actionSFX { get; private set; }

    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        return true;

    }
}