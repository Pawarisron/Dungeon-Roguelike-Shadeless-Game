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

    [Header("Sekiro Combat (Phase 2)")]
    [Tooltip("Pool of attacks this enemy randomly picks from. Leave empty to use Agent's legacy values.")]
    [SerializeField] private List<AttackDataSO> attackPool;
    [SerializeField] private AttackTelegraph telegraph;

    private Agent cachedAgent;
    private AttackDataSO currentAttack;

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
        cachedAgent = GetComponent<Agent>();
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

    // Phase 2 refactor: while-loop replaces recursive StartCoroutine — single IEnumerator
    // instance per chase, no per-tick allocations.
    private IEnumerator ChaseAndAttack()
    {
        while (steeringBehaviours != null && aiData.currentTarget != null)
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (distance < attackParameter.attackDistance)
            {
                movementInput = Vector2.zero;

                // Phase 2: pick attack, broadcast for Mikiri, telegraph, then strike.
                currentAttack = PickAttack();
                if (cachedAgent != null) cachedAgent.SetPendingAttack(currentAttack);
                BroadcastForMikiri(currentAttack);
                yield return PlayTelegraph(currentAttack);

                OnAttackPressed?.Invoke();
                yield return new WaitForSeconds(attackParameter.attackDelay);
            }
            else
            {
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(attackParameter.aiUpdateDelay);
            }
        }

        movementInput = Vector2.zero;
        following = false;
    }

    private AttackDataSO PickAttack()
    {
        if (attackPool == null || attackPool.Count == 0) return null;
        return attackPool[Random.Range(0, attackPool.Count)];
    }

    private void BroadcastForMikiri(AttackDataSO attack)
    {
        if (attack == null || !attack.CanBeMikiri) return;
        if (aiData.currentTarget == null) return;
        var mikiri = aiData.currentTarget.GetComponent<MikiriDetector>();
        if (mikiri != null) mikiri.RegisterIncoming(this, attack);
    }

    private IEnumerator PlayTelegraph(AttackDataSO attack)
    {
        if (attack == null) yield break;
        if (telegraph != null)
        {
            yield return StartCoroutine(telegraph.Play(attack));
        }
        else if (attack.telegraphTime > 0f)
        {
            yield return new WaitForSeconds(attack.telegraphTime);
        }
    }

    public int GetCurrentHealth() => currentHealth;
}
