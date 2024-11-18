using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObsticleDetector : Detector
{
    [SerializeField]
    private float detectionRadious = 2;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private bool showGizmos = true;

    Collider2D[] colliders;

    public override void Detect(AI_Data aiData)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadious,layerMask);
        aiData.obsticles = colliders;
        // not add wall to the collider list yet
    }

    private void OnDrawGizmos()
    {
        if (showGizmos == false)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadious);

        if (Application.isPlaying && colliders != null)
        {
            Gizmos.color = Color.red;
            foreach (Collider2D obsticlaCollider in colliders)
            {
                Gizmos.DrawSphere(obsticlaCollider.transform.position, 0.2f);
            }
        }
        
    }
}
