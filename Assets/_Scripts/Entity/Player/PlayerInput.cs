using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, IDamageAble
{
    [SerializeField]
    private int attackDamage;
    
    [SerializeField]
    private HealthManager healthManager;

    [SerializeField] private int energyAttack;

    [SerializeField]
    private CoinManager coinManager;

    private bool energyDrained = false;
    // [SerializeField]
    //private float attackDelay = 0.5f;

    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent OnRoll, OnBeingAttacked, OnDeath, OnAttack;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, roll;
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
    }


    // attack action
    private void OnEnable()
    {
        attack.action.performed += PerformAttack;
        roll.action.performed += PerformRoll;
    }


    private void OnDisable()
    {
        attack.action.performed -= PerformAttack;
        roll.action.performed -= PerformRoll;
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
