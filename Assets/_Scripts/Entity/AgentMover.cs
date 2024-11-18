using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentMover : MonoBehaviour, IMoveable
{
    private Rigidbody2D rb2d;

    [SerializeField]
    private float maxSpeed = 2, acceleration = 50, deacceleration = 100, rollspeed = 20;

    private float tmpRollSpeed; // use to cache RollSpeed in case of changing while perform rolling

    [SerializeField]
    private float currentSpeed = 0;

    private Vector2 oldMovementInput;

    private bool rolling = false;
    public Vector2 MovementInput { get; set; }

    float IMoveable.MaxSpeed { get => maxSpeed; set => maxSpeed = value; }
    float IMoveable.Acceleration { get => acceleration; set => acceleration = value; }
    float IMoveable.Deacceleration { get => deacceleration ; set => deacceleration = value; }
    float IMoveable.RollSpeed { get => rollspeed ; set => rollspeed = value; }
    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        tmpRollSpeed = rollspeed;
    }

    public void Rolling(bool isRolling = true)
    {
        rolling = isRolling;
    }

    private void Update()
    {
        if (rolling)
        {
            
            float rollSpeedDropMultiplier = 5f;
            rollspeed -= rollspeed * rollSpeedDropMultiplier * Time.deltaTime;

            float rollSpeedMinum = 5f;
            if (rollspeed < rollSpeedMinum)
            {   
                rollspeed = tmpRollSpeed;
                rolling = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!rolling)
        {
            if (MovementInput.magnitude > 0 && currentSpeed >= 0)
            {
                oldMovementInput = MovementInput;
                currentSpeed += acceleration * maxSpeed * Time.deltaTime;
            }
            else
            {
                currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;
            }
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
            rb2d.linearVelocity = oldMovementInput * currentSpeed;
        }
        else
        {
            rb2d.linearVelocity = oldMovementInput * rollspeed;
        }
    }


}