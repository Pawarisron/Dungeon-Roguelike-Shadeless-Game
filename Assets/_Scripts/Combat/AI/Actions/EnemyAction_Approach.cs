using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Action_Approach", menuName = "Combat/AI/Action: Approach")]
public class EnemyAction_Approach : EnemyAction
{
    public float closeEnoughDistance = 1.0f;
    public float duration = 0.6f;

    public override float ScoreBase(CombatContext ctx)
    {
        if (ctx.distanceToTarget <= closeEnoughDistance) return 0.05f;
        return Mathf.Lerp(0.4f, 0.85f, Mathf.Clamp01(ctx.distanceToTarget / 6f));
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        float end = Time.time + duration;
        while (Time.time < end && brain.HasTarget)
        {
            brain.SetMovementInput(brain.ComputeChaseDirection());
            yield return null;
        }
        brain.StopMovement();
    }
}
