using System.Collections;
using UnityEngine;

// Stand still, recover posture, watch the player. Use when posture is in the red zone.
[CreateAssetMenu(fileName = "Action_HoldGround", menuName = "Combat/AI/Action: Hold Ground")]
public class EnemyAction_HoldGround : EnemyAction
{
    public float duration = 1.2f;
    public float postureRecoverThreshold = 0.7f;

    public override float ScoreBase(CombatContext ctx)
    {
        // Strong urge when posture is dangerously high.
        if (ctx.ownPosturePercent >= postureRecoverThreshold) return 0.65f;
        // Mild defensive value when player is actively pressing — wait for opening.
        if (ctx.targetAttackedRecently) return 0.2f;
        return 0.05f;
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        brain.StopMovement();
        yield return new WaitForSeconds(duration);
    }
}
