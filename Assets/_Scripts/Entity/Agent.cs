using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Agent : MonoBehaviour
{
    
    private AgentAnimations agentAnimations;

    private AgentMover agentMover;
    [SerializeField]
    private int energyOnAttack = 35;
    // get attack point 
    public Transform attackPoint;
    public float attackRadious = 0.5f;
    public LayerMask targetLayer;
    public int attackDamage;
    public bool showAttackGizmos = false;
    public Sprite deathSprite = null;


    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }


    private void Update()
    {
        agentMover.MovementInput = MovementInput;
        AnimateCharacter();

    }

    public void Roll()
    {
        agentMover.Rolling();
    }
    
    
    public void PerformAttack()
    {
        // check if have attackPoint
        if (attackPoint)
        {
            // play attack animation
            agentAnimations.PlayAttackAnimation();
            
        }
    }

    //Callback by animation Event
    public void OnAttackAnimation() 
    {
        if (attackPoint)
        {
            //Consume energy
            EnergyManager energy = GetComponent<EnergyManager>();
            if(energy != null)
            {
                energy.TakeEnergy(energyOnAttack);
            }
            // detect colliders in range of attack
            Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadious, targetLayer);

            // damage them
            foreach (Collider2D hitTarget in hitTargets)
            {
                hitTarget.GetComponent<IDamageAble>().TakeDamage(attackDamage);

            }
        }

    }


    public void BeingAtttacked()
    {
        // play animation
        agentAnimations.PlayHurtAnimaiton();
    }

    public void Died()
    {
        // playanimation
        agentAnimations.PlayDeadAnimation();
        Destroy(this); //for now

        // Disable codes
        // you have to disable code in the controller

        // GetComponent<SpriteRenderer>().sprite = deathSprite;
        // GetComponent<Animator>().enabled = false;
        // GetComponent<Collider2D>().enabled = false;
        // attackPoint = null;

        // Destroy(this.gameObject);
        // this.enabled = false;
    }
    
    private void Awake()
    {
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        agentMover = GetComponent<AgentMover>();
    }
    
    private void AnimateCharacter()
    {
        Vector2 lookDirection = pointerInput - (Vector2)transform.position;
        agentAnimations.RotateToPointer(lookDirection);
        agentAnimations.PlayAnimation(MovementInput);
    }

    public void OnDrawGizmos()
    {
        if (!showAttackGizmos) {  return; }
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRadious);
        }
        return;
    }
}