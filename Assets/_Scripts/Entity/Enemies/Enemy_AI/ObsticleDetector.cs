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
    private Collider2D selfCollider;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private bool showGizmos = true;

    Collider2D[] colliders;

    public override void Detect(AI_Data aiData)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadious, layerMask);

        List<Collider2D> filtered = new List<Collider2D>();

        foreach (var c in colliders)
        {
            if (c == selfCollider)
                continue;

            filtered.Add(c);
        }

        aiData.obsticles = filtered.ToArray();
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
                if (obsticlaCollider == selfCollider)
                {
                    continue;
                }
                Gizmos.DrawSphere(obsticlaCollider.transform.position, 0.2f);
            }
        }
        
    }
}
