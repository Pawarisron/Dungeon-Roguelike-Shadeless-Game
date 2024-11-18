using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private UIInventoryDescription itemDescriptrion;

        [SerializeField]
        private MouseFollower mouseFollower;

        List<UIInventoryItem> listOfItems = new List<UIInventoryItem>();

        private int currentyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested,
        OnStartDragging, OnItemActionRequested;

        public event Action<int, int> OnSwapItems;

        [SerializeField]
        private ItemActionPanel actionPanel;

        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescriptrion.ResetDescription();
            
        }
        public void InitializeInventoryUI(int inventorysize)
        {
            for (int i = 0; i < inventorysize; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab,
                                                     Vector3.zero,
                                                     Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                listOfItems.Add(uiItem);

                // add event handelers
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBinClick += HandleShowItemActions;
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfItems.Count > itemIndex)
            {
                listOfItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }
        private void HandleItemSelection(UIInventoryItem itemUI)
        {
            int index = listOfItems.IndexOf(itemUI);
            if (index == -1)
            { 
                return;
            }
            OnDescriptionRequested?.Invoke(index);
        }

        private void HandleBeginDrag(UIInventoryItem itemUI)
        {
            int index = listOfItems.IndexOf(itemUI);
            if (index == -1) 
                return;
            currentyDraggedItemIndex = index;
            HandleItemSelection(itemUI);
            OnStartDragging?.Invoke(index);

        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleSwap(UIInventoryItem itemUI)
        {
            int index = listOfItems.IndexOf(itemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentyDraggedItemIndex, index);
            HandleItemSelection(itemUI);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentyDraggedItemIndex = -1;
        }

        private void HandleEndDrag(UIInventoryItem itemUI)
        {
            ResetDraggedItem();
        }

        private void HandleShowItemActions(UIInventoryItem itemUI)
        {
            int index = listOfItems.IndexOf(itemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescriptrion.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actoinName, Action performAction)
        {
            actionPanel.AddButton(actoinName, performAction);
        }

        public void ShowItemAction(int itemIndex)
        {
            actionPanel.Toggle(true);
            actionPanel.transform.position = listOfItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfItems)
            {
                item.Deselect();
            }
            actionPanel.Toggle(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        internal void UpDataDescription(int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescriptrion.SetDescriptrion(itemImage, name, description);
            DeselectAllItems();
            listOfItems[itemIndex].Select();
        }

        internal void ResetAllItems()
        {
            foreach (var item in listOfItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }
    }
}