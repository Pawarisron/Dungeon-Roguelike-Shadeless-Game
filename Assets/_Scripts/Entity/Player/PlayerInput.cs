using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, IDamageAble, IParriable
{
    [SerializeField]
    private int attackDamage;

    [SerializeField]
    private HealthManager healthManager;

    [SerializeField] private int energyAttack;

    [SerializeField]
    private CoinManager coinManager;

    [Header("Sekiro Combat")]
    [SerializeField] private ParryController parryController;
    [SerializeField] private PostureManager postureManager;
    [Tooltip("Posture damage taken when blocking without a perfect parry.")]
    [SerializeField] private float guardedPostureDamage = 12f;
    [Tooltip("HP damage multiplier when guarded but not parried (0=null, 1=full).")]
    [Range(0f, 1f)] [SerializeField] private float guardedDamageMultiplier = 0.4f;

    private bool energyDrained = false;
    // [SerializeField]
    //private float attackDelay = 0.5f;

    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent OnRoll, OnBeingAttacked, OnDeath, OnAttack, OnParryPressed, OnParrySuccess;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, roll, parry;
    private bool isWaitingForAnimation = false;
    // public int MaxHealth => maxEnegy;
    // public int CurrentHealth => currentHealth;

    private void Update()
    {
        //simplier ?. is a Not NULL
        OnMovementInput?.Invoke(movement.action.ReadValue<Vector2>().normalized);
        OnPointerInput?.Invoke(GetPointerInput());
        if (!isWaitingForAnimation && !energyDrained)
        {
            EnableAll();
        }
        else
        {
            movement.action.Enable();
        }

    }

    private Vector2 GetPointerInput()
    {
        Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    public void EnableAll()
    {
        movement.action.Enable();
        roll.action.Enable();
        attack.action.Enable();
        if (parry != null && parry.action != null) parry.action.Enable();
    }


    // attack action
    private void OnEnable()
    {
        attack.action.performed += PerformAttack;
        roll.action.performed += PerformRoll;
        if (parry != null && parry.action != null)
            parry.action.performed += PerformParry;
    }


    private void OnDisable()
    {
        attack.action.performed -= PerformAttack;
        roll.action.performed -= PerformRoll;
        if (parry != null && parry.action != null)
            parry.action.performed -= PerformParry;
    }

    private void PerformParry(InputAction.CallbackContext context)
    {
        if (parryController != null) parryController.TriggerParry();
        OnParryPressed?.Invoke();
    }

    // IParriable — called by the attacker (Agent.ResolveHit).
    public bool TryParry(MonoBehaviour attacker, int incomingDamage)
    {
        if (parryController != null && parryController.IsInParryWindow)
        {
            parryController.NotifySuccess(transform.position);
            OnParrySuccess?.Invoke();
            return true;  // damage absorbed, attacker takes posture retaliation
        }

        // Not in parry window — partial guard if the parry button was held recently
        // (cooldown counts as "blocking stance"). Otherwise full hit goes through TakeDamage.
        if (parryController != null && parryController.IsOnCooldown)
        {
            int reduced = Mathf.Max(1, Mathf.RoundToInt(incomingDamage * guardedDamageMultiplier));
            healthManager.TakeDamage(reduced);
            if (postureManager != null) postureManager.TakePostureDamage(guardedPostureDamage);
            OnBeingAttacked?.Invoke();
            if (healthManager.isDeath) Die();
            return true;  // we already applied damage; tell attacker not to double-hit
        }

        return false;  // open hit — let TakeDamage handle full damage
    }

    private void PerformAttack(InputAction.CallbackContext context)
    {
        movement.action.Disable();
        roll.action.Disable();
        WaitForAnimaiotn();
        OnAttack?.Invoke();
    }
    private void PerformRoll(InputAction.CallbackContext context)
    {
        // if you have roll animaion enable these
        // movement.action?.Disable();
        // attack.action.Disable();
        // WaitForAnimaiotn();
        OnRoll?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        // decrease health
        healthManager.TakeDamage(damage);

        // play hurt animation
        OnBeingAttacked?.Invoke();
        if (!energyDrained)
        {
            EnableAll();
        }
        // die
        if (healthManager.isDeath)
        {
            Die();
        }

    }

    private void Die()
    {
        OnDeath?.Invoke();
    }

    private void WaitForAnimaiotn()
    {
        isWaitingForAnimation = true;
    }

    public void FinishWaiting()
    {
        isWaitingForAnimation = false;
    }
    public void DraindedEnergyEffect()
    {
        roll.action.Disable();
        attack.action.Disable();
        energyDrained = true;
        
    }
    public void UnDraindedEnergyEffect()
    {
        roll.action.Enable();
        attack.action.Enable();
        energyDrained = false;
    }
    
}
