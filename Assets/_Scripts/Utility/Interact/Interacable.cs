using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interacable : MonoBehaviour
{
    public bool isInrange;
    public KeyCode interactKey;
    public UnityEvent interactAction;
    public Canvas interactText;

    //TODO: move interact logic to player for performance
    private void Awake()
    {
        interactText.enabled = false;
    }
    private void Update()
    {
        if (isInrange)
        {
            // key
            if (Input.GetKeyDown(interactKey))
            {
                interactAction?.Invoke();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInrange = true;
            if(interactText != null)
                interactText.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isInrange = false;
            if (interactText != null)
                interactText.enabled = false;
        }
    }

    private void OnDisable()
    {
        isInrange = false;

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }
}
