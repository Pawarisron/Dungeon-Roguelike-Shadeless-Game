using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class AINPC : MonoBehaviour
{
    [SerializeField]
    AI_Data aiData;

    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours;

    [SerializeField]
    private List<Detector> detectors;

    public float detectionDelay = 0.5f, aiUpdateDelay = 0.06f, watchDelay = 1f;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    [SerializeField]
    private PlayerManager playerManager;

    private GameObject player;

    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    private Vector2 movementInput;

    bool following = false;

    private void Start()
    {
        // Detection Player and Obsticles around
        player = playerManager.Player;
        InvokeRepeating("PerformDetection", 0, detectionDelay);
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
                StartCoroutine(LookAndBuy());
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

    private IEnumerator LookAndBuy()
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

            if (distance < 2)
            {
                //Buy logic
                movementInput = Vector2.zero;
                BuyItem();
                aiData.currentTarget = null;
                yield return new WaitForSeconds(watchDelay);
                StartCoroutine(LookAndBuy());
            }
            else
            {
                //Look logic
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(LookAndBuy());
            }

        }
    }

    private void BuyItem()
    {
        ItemShopDisplay targetDisplay = aiData.currentTarget.GetComponent<ItemShopDisplay>();

        if (targetDisplay.GetItem().IsEmpty == false)
        {

            player.GetComponent<CoinManager>()?.AddCoin(targetDisplay.GetItem().item.price * targetDisplay.GetItem().quantity);

            targetDisplay.RemoveItem(targetDisplay.GetItem().quantity);

        }

        ChangeTable();
    }

    private void ChangeTable()
    {
        int nextTargetIndex = UnityEngine.Random.Range(0,aiData.targets.Count);
        aiData.currentTarget = aiData.targets[nextTargetIndex];
    }
}
