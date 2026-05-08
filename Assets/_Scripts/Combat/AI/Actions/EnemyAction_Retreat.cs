using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Action_Retreat", menuName = "Combat/AI/Action: Retreat")]
public class EnemyAction_Retreat : EnemyAction
{
    public float duration = 0.8f;
    public float postureDangerThreshold = 0.6f;
    public float maxUsefulDistance = 3.0f;

    public override float ScoreBase(CombatContext ctx)
    {
        if (ctx.distanceToTarget > maxUsefulDistance) return 0.05f;
        float postureUrgency = ctx.ownPosturePercent > postureDangerThreshold ? 0.7f : 0f;
        float damageUrgency = ctx.timeSinceTookHit < 0.5f ? 0.3f : 0f;
        return postureUrgency + damageUrgency;
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        float end = Time.time + duration;
        while (Time.time < end && brain.HasTarget)
        {
            brain.SetMovementInput(-brain.ComputeChaseDirection());
            yield return null;
        }
        brain.StopMovement();
    }
}
