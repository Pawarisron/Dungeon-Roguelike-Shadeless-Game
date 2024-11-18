using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField]
    public UIInventoryPage tableInventory;

    [SerializeField]
    private InventorySO playerInventory;

    [SerializeField]
    public int index;

    public void OpenTableInventory()
    {
        if (tableInventory.isActiveAndEnabled == false)
        {
            tableInventory.Show();
            foreach (var item in playerInventory.GetCurrentInventoryState())
            {
                tableInventory.UpdateData(item.Key,
                    item.Value.item.ItemImage,
                    item.Value.quantity);
            }

        }
        else
        {
            tableInventory.Hide();
        }
    }
}
