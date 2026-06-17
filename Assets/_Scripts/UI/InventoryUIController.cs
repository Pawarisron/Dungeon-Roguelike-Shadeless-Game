using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inventory
{
    public class InventoryUIController : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;

        public List<InventoryItem> initialItems = new List<InventoryItem>();

        // [SerializeField]
        // public AudioSource audioSource;

        // Table move these
        [SerializeField]
        private bool isNearTable;
        private Collision2D table;

        private PlayerInput playerInput;

        private void Start()
        {
            playerInput = GetComponentInParent<PlayerInput>();
            if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();

            PrepareUI();
            PrepareInventoryData();

            // Force a known-closed state so the very first Tab opens it (was
            // taking two presses because the panel could start active).
            inventoryUI.Hide();
        }

        private void OnDestroy()
        {
            // Stop listening so the persistent InventorySO doesn't keep calling
            // into this UI after the scene (and its Images) are destroyed.
            if (inventoryData != null)
                inventoryData.OnInventoryChanged -= ChangeInventoryUI;
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryChanged += ChangeInventoryUI;
            foreach (InventoryItem item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void ChangeInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key,
                    item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpDataDescription(itemIndex, item.ItemImage,
                item.name, description);
        }

        private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {
            inventoryData.SwapItems(itemIndex1, itemIndex2);
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage,
                inventoryItem.quantity);
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt((int)itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));

            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));

                if (isNearTable)
                {
                    inventoryUI.AddAction("Sell", () => SellItem(itemIndex, inventoryItem.quantity));
                }
            }
        }



        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            // audioSource.PlayOneShot(dropClip);
        }

        public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt((int)itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);
                // audioSource.PlayOneShot(itemActoin.actionSFX);
                if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                    inventoryUI.ResetSelection();
            }
        }

        private string PrepareDescription(InventoryItem inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();
            for (int i = 0; i < inventoryItem.itemState.Count; i++)
            {
                sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName} " +
                    $": {inventoryItem.itemState[i].value} / " +
                    $"{inventoryItem.item.DefaultParametersList[i].value}");
            }
            return sb.ToString();
        }

        public void Update()
        {
            // TODO: change the inventory menu input to new input system
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.ItemImage,
                            item.Value.quantity);
                    }
                    // suspend combat input while the bag is open (movement stays live)
                    if (playerInput != null) playerInput.OnInventoryOpened();
                }
                else
                {
                    inventoryUI.Hide();
                    if (playerInput != null) playerInput.OnInventoryClosed();
                }
            }
        }

        //TODO: move this

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == 11
            && !isNearTable)
            {
                isNearTable = true;
                table = collision;
            }
            
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == 11
            && isNearTable)
            {
                isNearTable = false;
                table = null;
            }
            
        }
        private void SellItem(int itemIndex, int quantity)
        {
            // get table 
            table.gameObject.GetComponent<ItemShopDisplay>().Place(inventoryData.GetItemAt(itemIndex), quantity);
            DropItem(itemIndex, quantity);
        }


    }
}