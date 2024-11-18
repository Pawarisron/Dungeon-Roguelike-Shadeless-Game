using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipableItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Equip";

    [field: SerializeField]
    public AudioClip actionSFX {get; private set;}

    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {        
        AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
        if (weaponSystem != null)
        {
            weaponSystem.SetWeapon(this, itemState == null ?
                DefaultParametersList : itemState);
               return true;
        }
        return false;
        
    }
}


