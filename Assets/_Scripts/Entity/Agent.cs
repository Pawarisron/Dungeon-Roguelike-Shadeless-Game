using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Agent : MonoBehaviour
{
    [Header("Combat events (Phase 3+4 hooks)")]
    public UnityEvent OnHitLanded;        // clean hit — wire to ComboCounter, CameraShakeLight, CombatSfx.PlayCleanHit
    public UnityEvent OnAttackParried;    // we got deflected — wire to CameraShakeMedium, CombatSfx.PlayParry
    public UnityEvent OnDeathblowDealt;   // staggered enemy slain — wire to CameraShakeHeavy, CombatSfx.PlayDeathblow
    public UnityEvent OnAttackWhiffed;    // swing connected with nothing — wire to PlayerActionTracker.OnAttackWhiff

    
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

    [Header("Sekiro Combat — fallback when no AttackDataSO is set")]
    [Tooltip("Posture inflicted on the target on a clean hit.")]
    public float postureDamageOnHit = 18f;
    [Tooltip("Posture this attacker eats when its swing is parried.")]
    public float parryRetaliationPosture = 35f;
    [Tooltip("Damage applied during a Deathblow (use a number larger than maxHealth).")]
    public int deathblowDamage = 9999;
    [Tooltip("Hit-stop seconds on a clean hit.")]
    public float hitStopOnHit = 0.04f;
    [Tooltip("Hit-stop seconds on a parry.")]
    public float hitStopOnParry = 0.10f;
    [Tooltip("Hit-stop seconds on a deathblow.")]
    public float hitStopOnDeathblow = 0.20f;

    // Set by AIEnemy (or any controller) BEFORE OnAttackPressed → PerformAttack.
    // Cleared automatically after OnAttackAnimation runs.
    private AttackDataSO pendingAttack;
    public void SetPendingAttack(AttackDataSO attack) => pendingAttack = attack;


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

            if (hitTargets.Length == 0)
            {
                OnAttackWhiffed?.Invoke();
            }

            // resolve each hit through the Sekiro combat pipeline
            foreach (Collider2D hitTarget in hitTargets)
            {
                ResolveHit(hitTarget);
            }
        }

        // Single-shot: clear the pending attack so the next swing starts fresh.
        pendingAttack = null;
    }

    private void ResolveHit(Collider2D hitTarget)
    {
        // Read attack values from AttackDataSO if set, otherwise fall back to inspector fields.
        int hpDmg = pendingAttack != null ? pendingAttack.hpDamage : attackDamage;
        float postureDmg = pendingAttack != null ? pendingAttack.postureDamage : postureDamageOnHit;
        float retaliation = pendingAttack != null ? pendingAttack.parryRetaliationPosture : parryRetaliationPosture;

        // 1. Parry — does the target catch our blade?
        var parriable = hitTarget.GetComponent<IParriable>();
        if (parriable != null && parriable.TryParry(this, pendingAttack, hpDmg))
        {
            // We just got deflected — eat posture damage ourselves.
            var ourPosture = GetComponent<PostureManager>();
            if (ourPosture != null) ourPosture.TakePostureDamage(retaliation);
            HitStop.Trigger(hitStopOnParry);
            OnAttackParried?.Invoke();
            return;
        }

        //// 2. Deathblow — staggered targets die in one swing.
        //var targetPosture = hitTarget.GetComponent<PostureManager>();
        //if (targetPosture != null && targetPosture.IsStaggered)
        //{
        //    var dmgable = hitTarget.GetComponent<IDamageAble>();
        //    if (dmgable != null) dmgable.TakeDamage(deathblowDamage);
        //    HitStop.Trigger(hitStopOnDeathblow);
        //    OnDeathblowDealt?.Invoke();
        //    return;
        //}

        // 3. Clean hit — HP damage AND posture pressure.
        var damageable = hitTarget.GetComponent<IDamageAble>();
        if (damageable != null) damageable.TakeDamage(hpDmg);
        //if (targetPosture != null) targetPosture.TakePostureDamage(postureDmg);
        HitStop.Trigger(hitStopOnHit);
        OnHitLanded?.Invoke();
    }


    public void BeingAtttacked()
    {
        // play animation
        agentAnimations.PlayHurtAnimaiton();
    }

    public void Parried()
    {
        agentAnimations.PlayGuardAnimation();
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