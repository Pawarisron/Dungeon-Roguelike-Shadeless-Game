using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class AIEnemy : MonoBehaviour, IDamageAble, IHealth
{
    [SerializeField]
    private AI_Data aiData;


    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private int currentHealth;

    [System.Serializable]
    private class AttackParameter
    {
        public float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;
        public float attackDistance = 0.5f;
    }
    [SerializeField]
    AttackParameter attackParameter;

    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours;

    [SerializeField]
    private List<Detector> detectors;


    [SerializeField]
    private ContextSolver movementDirectionSolver;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    //Inputs sent from the Enemy AI to the Enemy controller
    public UnityEvent OnBeingAttacked, OnDeath, OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    private Vector2 movementInput;

    bool following = false;

    private void Start()
    {
        // set health
        currentHealth = maxHealth;
        // Detection Player and Obsticles around
        InvokeRepeating("PerformDetection", 0, attackParameter.detectionDelay);

    }

    private void PerformDetection()
    {
        if (detectors == null) { return; }
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }

        float[] danger = new float[8];
        float[] interest = new float[8];

        foreach (SteeringBehaviour behaviour in steeringBehaviours)
        {
            (danger, interest) = behaviour.GetSteering(danger, interest, aiData);
        }
    }
    private void Update()
    {
        //Enemy AI movement based on Target availability
        if (aiData.currentTarget != null)
        {
            //Looking at the Target
            OnPointerInput?.Invoke(aiData.currentTarget.position);
            if (following == false)
            {
                following = true;
                StartCoroutine(ChaseAndAttack());
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            //Target acquisition logic
            aiData.currentTarget = aiData.targets[0];
        }
        //Moving the Agent
        OnMovementInput?.Invoke(movementInput);
    }

    public void TakeDamage(int damage)
    {
        // decrease health
        currentHealth -= damage;

        // play hurt animation
        OnBeingAttacked?.Invoke();

        // die
        if(currentHealth <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        OnDeath?.Invoke();

        // disable codes
        // GetComponent<Animator>().enabled = false;
        movementInput = Vector2.zero;
        GetComponent<Agent>().enabled = false;
        GetComponent<AgentMover>().enabled = false;

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static; 
        GetComponent<Collider2D>().enabled = false;

        // drop items
        GetComponent<DropPile>()?.InstantiateLoot(transform.position);
        // distroy children
        Transform[] children = this.GetComponentsInChildren<Transform>();
        if (children.Length > 0) 
        {
            for (int num = 1; num < children.Length; num++) 
            {
                Destroy(children[num].gameObject);
            }
        }
        movementDirectionSolver = null;
        steeringBehaviours = null;
        detectors = null;


        // distroy self
        this.gameObject.layer = 10;   // turn to cropse
      
        this.enabled = false;
    }

    private IEnumerator ChaseAndAttack()
    {
        if (steeringBehaviours == null) { yield break; }
        if (aiData.currentTarget == null)
        {
            // Stopping Logic
            //Debug.Log("Stopping");
            movementInput = Vector2.zero;
            following = false;
            yield break;
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);


            if (distance < attackParameter.attackDistance)
            {
                Debug.Log("AIEnemy.ChaseAttack distance " + distance);
                //Attack logic
                movementInput = Vector2.zero;
                OnAttackPressed?.Invoke();
                yield return new WaitForSeconds(attackParameter.attackDelay);
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                //Chase logic
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(attackParameter.aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }

        }

    }

    public int GetCurrentHealth() => currentHealth;
}
