using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.EventSystems.EventTrigger;

public class SingleUseTrap : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 20;
    
    [SerializeField]
    private float trapDuration = 3f;    //Second

    [SerializeField]
    private Vector2 attachmentPoint = Vector2.zero;

    [Range(0f, 1f)]
    [SerializeField]
    private float slowMultiplier = 0.5f; 

    private bool hasTriggered = false;
    private Animator animator;

    private Collider2D targetEntity;

    //TODO: Bug when double trap attached
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(animator == null)
        {
            Debug.LogError("Animator is null:"+ this.name);
            return;
        }
        //Debug.Log(other.name);

        if(!hasTriggered) // Trigger every thing that have collider 2D
        {
            hasTriggered = true;    // Set the flag to true to prevent re-triggering
            targetEntity = other;
            animator.SetTrigger("Trigger"); // Trigger the animation for the trap
            DisableTrap();

        }
    }
    // This function will be called by an Animation Event
    public void OnAttackAnimation()
    {
        var trapCollider = GetComponent<Collider2D>();
        if (targetEntity != null && targetEntity.GetComponent<IDamageAble>() != null)
        {
            //check for check player dodge
            if (trapCollider.IsTouching(targetEntity))
            {
                StartCoroutine(ApplyEffect());
            }
        }
    }

    private IEnumerator ApplyEffect()
    {
        //Take damage
        targetEntity.GetComponent<IDamageAble>().TakeDamage(damageAmount);

        //Slow target
        IMoveable entity = targetEntity.GetComponent<IMoveable>();
        if(entity != null)
        {
            //Debuff
            float originalMaxSpeed = entity.MaxSpeed;
            entity.MaxSpeed *= slowMultiplier;


            //Attach Trap to IDamageable;
            transform.parent.SetParent(targetEntity.transform);
            // move to player position
            transform.parent.localPosition = attachmentPoint;

            //Change sorting layer
            GetComponent<Renderer>().sortingOrder = 1;

            //Wait
            yield return new WaitForSeconds(trapDuration);

            Debug.Log("Release Trap");

            entity.MaxSpeed = originalMaxSpeed;

            //Destory trap object
            Destroy(transform.parent.gameObject);
        }
    }
    private void DisableTrap()
    {
        this.enabled = false;
    }


}

