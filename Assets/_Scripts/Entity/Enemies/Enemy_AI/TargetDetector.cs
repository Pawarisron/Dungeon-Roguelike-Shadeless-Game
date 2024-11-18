using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : Detector
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
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, targetDectectionRange, targetLayerMask);

        if (playerCollider != null)
        {
            //Check if you see the player
            Vector2 direction = (playerCollider.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, targetDectectionRange, obstaclesLayerMask);

            //Make sure that the collider we see is on the "Player" layer
            if (hit.collider != null && (targetLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                //Debug.DrawRay(transform.position, direction * targetDectectionRange, Color.magenta); 
                colliders = new List<Transform> () { playerCollider.transform };
            }
            else
            {
                colliders = null;
            }
            aiData.targets = colliders;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos == false) return;

        Gizmos.DrawWireSphere(transform.position,targetDectectionRange);

        if (colliders == null) return;

        Gizmos.color = Color.magenta;
        foreach (var item in colliders)
        {
            Gizmos.DrawSphere(item.position, 0.3f);
        }
        
    }
}
