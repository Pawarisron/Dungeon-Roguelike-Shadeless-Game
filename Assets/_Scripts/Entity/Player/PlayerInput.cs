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
    [SerializeField] private MikiriDetector mikiriDetector;
    [SerializeField] private DodgeController dodgeController;
    [Tooltip("Posture damage taken when blocking without a perfect parry.")]

    //[SerializeField] private float guardedPostureDamage = 12f;
    //[Tooltip("HP damage multiplier when guarded but not parried (0=null, 1=full).")]
    //[Range(0f, 1f)] [SerializeField] private float guardedDamageMultiplier = 0.4f;

    private bool energyDrained = false;
    private bool isDead = false;
    // [SerializeField]
    //private float attackDelay = 0.5f;

    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent OnRoll, OnBeingAttacked, OnDeath, OnAttack, OnParryPressed, OnParrySuccess, OnMikiriSuccess, OnDodgeSuccess;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, roll, parry;
    private bool isWaitingForAnimation = false;
    // public int MaxHealth => maxEnegy;
    // public int CurrentHealth => currentHealth;

    private void Update()
    {
        // Dead — stop reading input and re-enabling actions.
        if (isDead) return;

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

    public void DisableAll()
    {
        movement.action.Disable();
        roll.action.Disable();
        attack.action.Disable();
        if (parry != null && parry.action != null) parry.action.Disable();
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
    // Returns true if we handled the hit (attacker should NOT call TakeDamage afterwards).
    public bool TryParry(MonoBehaviour attacker, AttackDataSO attack, int incomingDamage)
    {
        // 1. Mikiri already happened on a Thrust — the strike is null and void.
        if (attack != null && attack.CanBeMikiri && mikiriDetector != null && mikiriDetector.DidMikiri(attacker))
        {
            return true;
        }

        // 2. Sweep / Crash — must i-frame through with a roll. Parry is impossible.
        if (attack != null && attack.MustDodge)
        {
            if (dodgeController != null && dodgeController.IsInDodgeWindow)
            {
                OnDodgeSuccess?.Invoke();
                return true;  // i-framed, no damage
            }
            return false;  // open hit — full damage via TakeDamage
        }

        // 3. Perilous Thrust without Mikiri — also unparriable; let it land.
        if (attack != null && !attack.IsParryable)
        {
            return false;
        }

        // 4. Normal attack — perfect parry window check.
        if (parryController != null && parryController.IsInParryWindow)
        {
            parryController.NotifySuccess(transform.position);
            OnParrySuccess?.Invoke();
            return true;
        }

        //// 5. Partial guard during parry cooldown.
        //if (parryController != null && parryController.IsOnCooldown)
        //{
        //    int reduced = Mathf.Max(1, Mathf.RoundToInt(incomingDamage * guardedDamageMultiplier));
        //    healthManager.TakeDamage(reduced);
        //    if (postureManager != null) postureManager.TakePostureDamage(guardedPostureDamage);
        //    OnBeingAttacked?.Invoke();
        //    if (healthManager.isDeath) Die();
        //    return true;
        //}

        return false;  // open hit — Agent will call TakeDamage with full damage
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
        // Sekiro Phase 2: roll = Mikiri (vs Thrust) + i-frames (vs Sweep/Crash)
        if (mikiriDetector != null && mikiriDetector.TryMikiri())
        {
            OnMikiriSuccess?.Invoke();
        }
        if (dodgeController != null) dodgeController.TriggerDodge();

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
        if (isDead) return;          // guard against multiple lethal hits in one frame
        isDead = true;

        OnMovementInput?.Invoke(Vector2.zero); // kill residual movement
        DisableAll();                          // no more attacking/rolling/parrying

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
