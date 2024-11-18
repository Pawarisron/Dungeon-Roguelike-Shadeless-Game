using Inventory.Model;
using Unity.VisualScripting;
using UnityEngine;

public class ItemShopDisplay : MonoBehaviour
{
    [field: SerializeField]
    public InventorySO shopInventory;

    [SerializeField]
    public int index;

    [SerializeField]
    private GameObject itemDesplay;

    // TODO: It woud be better to make shop manager class to contain tables
    private void Start()
    {
        itemDesplay.GetComponent<SpriteRenderer>().sprite = shopInventory.GetItemAt(index).IsEmpty?
            null : shopInventory.GetItemAt(index).item.ItemImage;
    }

    public InventoryItem GetItem()
    {
        return shopInventory.GetItemAt(index);
    }

    // return index of items
    public void RemoveItem(int quantity)
    {
        shopInventory.RemoveItem(index, quantity);
        ImformChange();
    }
    // can improve by check and return item that is already in table
    public void Place(InventoryItem item, int quantity)
    {
        shopInventory.AddItem(item.item, quantity);
        ImformChange();
    }

    private void ImformChange()
    {
        itemDesplay.GetComponent<SpriteRenderer>().sprite = shopInventory.GetItemAt(index).IsEmpty ?
            null : shopInventory.GetItemAt(index).item.ItemImage;
    }
}
