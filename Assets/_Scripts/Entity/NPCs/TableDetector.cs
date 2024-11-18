using System.Collections.Generic;
using UnityEngine;

public class TableDetector : Detector
{
    [SerializeField]
    private float targetDectectionRange = 5;

    [SerializeField]
    private LayerMask obstaclesLayerMask, targetLayerMask;
    [SerializeField]
    private bool showGizmos = false;

    //gizmo parameters
    private List<Transform> colliders;

    public override void Detect(AI_Data aiData)
    {
        // Find out if player is near
        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(transform.position,targetDectectionRange, targetLayerMask);

        if (targetColliders != null)
        {
            colliders = new List<Transform>();

            foreach (var collider in targetColliders) 
            {
                colliders.Add(collider.transform);
            }
            aiData.targets = colliders;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos == false) return;

        Gizmos.DrawWireSphere(transform.position, targetDectectionRange);

        if (colliders == null) return;

        Gizmos.color = Color.magenta;
        foreach (var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.3f);
        }

    }

}
