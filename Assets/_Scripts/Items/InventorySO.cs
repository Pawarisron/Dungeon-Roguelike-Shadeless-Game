using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<InventoryItem> inventoryItem;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InventoryItem>> OnInventoryChanged;

        public void Initialize()
        {
            inventoryItem = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItem.Add(InventoryItem.GetEmptyItem());
            }
        }

        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if(item.IsStackable == false)
            {
                for (int i = 0; i < inventoryItem.Count; i++)
                {
                    while (quantity > 0 && IsInventoryFull() == false)
                    {
                        quantity -= AddItemOfFirstFreeSlot(item, 1, itemState);
                    }
                    InformAboutChange();

                    // avoid unreachable warning
                    if (i < inventoryItem.Count + 1)
                        return quantity;
                    
                }
                
            }
            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemOfFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            InventoryItem newItem = new InventoryItem
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState == null? item.DefaultParametersList : itemState)
            };

            for (int i = 0; i < inventoryItem.Count; i++)
            {
                if (inventoryItem[i].IsEmpty)
                {
                    inventoryItem[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
            => inventoryItem.Where(item => item.IsEmpty).Any() == false;

        private int AddStackableItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < inventoryItem.Count; i++)
            {
                if (inventoryItem[i].IsEmpty)
                    continue;
                if (inventoryItem[i].item.ID == item.ID)
                {
                    int amoutPossibleToTake =
                        inventoryItem[i].item.MaxStackSize - inventoryItem[i].quantity;

                    if(quantity > amoutPossibleToTake)
                    {
                        inventoryItem[i] = inventoryItem[i]
                            .ChanceQuantity(inventoryItem[i].item.MaxStackSize);
                        quantity -= amoutPossibleToTake;
                    }
                    else
                    {
                        inventoryItem[i] = inventoryItem[i]
                            .ChanceQuantity(inventoryItem[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }

            while(quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemOfFirstFreeSlot(item, newQuantity);
            }
            return quantity;
        }

        public void AddItem(InventoryItem item)
        {
            AddItem(item.item, item.quantity);
        }

        // retuen not modifliable
        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue =
                new Dictionary<int, InventoryItem>();
            for (int i = 0; i < inventoryItem.Count; i++)
            {
                if (inventoryItem[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItem[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            return inventoryItem[itemIndex];
        }

        public void SwapItems(int itemIndex1, int itemIndex2)
        {
            InventoryItem item1 = inventoryItem[itemIndex1];
            inventoryItem[itemIndex1] = inventoryItem[itemIndex2];
            inventoryItem[itemIndex2] = item1;
            InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryChanged?.Invoke(GetCurrentInventoryState());
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if (inventoryItem.Count > itemIndex)
            {
                if (inventoryItem[itemIndex].IsEmpty)
                    return;
                int reminder = inventoryItem[itemIndex].quantity - amount;
                if(reminder <= 0) 
                    inventoryItem[itemIndex] = InventoryItem.GetEmptyItem();
                else
                    inventoryItem[itemIndex] = inventoryItem[itemIndex]
                        .ChanceQuantity(reminder);

                InformAboutChange();
            }
        }
    }

    // to make inventory secure, contain less bugs (from heep and stack memory management)
    // , easily managable, we use class to call struct insteed
    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;

        public bool IsEmpty => item == null;

        // this is the resons for use stuct
        public InventoryItem ChanceQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };
        }

        // struct cannot be null
        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null,
                quantity = 0,
                itemState = new List<ItemParameter>()
            };
    }

}
