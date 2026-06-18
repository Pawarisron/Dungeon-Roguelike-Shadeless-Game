using UnityEngine;

public class Treasure : MonoBehaviour
{
    [SerializeField]
    private Sprite openedSprite;
    public void OnOpenChest()
    {
        GetComponent<DropPile>()?.InstantiateLoot(transform.position);
        GetComponent<SpriteRenderer>().sprite = openedSprite;

        var interactable = GetComponent<Interacable>();
        if (interactable != null)
        {
            interactable.interactText.enabled = false;
            interactable.enabled = false;
        }
    }
}
