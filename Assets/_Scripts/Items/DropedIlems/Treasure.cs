using UnityEngine;

public class Treasure : MonoBehaviour
{
    [SerializeField]
    private Sprite openedSprite;
    public void OnOpenChest()
    {
        GetComponent<DropPile>()?.InstantiateLoot(transform.position);
        GetComponent<SpriteRenderer>().sprite = openedSprite;
        GetComponent<Interacable>().enabled = false;
    }
}
