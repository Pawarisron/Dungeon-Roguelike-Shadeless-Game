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


    private void Awake()
    {
        interactText.enabled = false;
    }
    void Update()
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
}
