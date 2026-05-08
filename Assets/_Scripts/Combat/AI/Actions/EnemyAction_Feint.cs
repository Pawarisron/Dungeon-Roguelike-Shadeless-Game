using System.Collections;
using UnityEngine;

// Begins a telegraph, holds, then CANCELS without striking. Punishes parry-trigger-happy
// players whose i-frame window expires during the long pause.
[CreateAssetMenu(fileName = "Action_Feint", menuName = "Combat/AI/Action: Feint")]
public class EnemyAction_Feint : EnemyAction
{
    public float maxRange = 1.5f;
    public float telegraphFraction = 0.7f;     // play 70% of the telegraph
    public float recoveryAfterCancel = 0.4f;   // brief stagger when canceling

    public override float ScoreBase(CombatContext ctx)
    {
        if (ctx.distanceToTarget > maxRange) return 0.05f;

        // Higher when player parries a lot — they'll bite on the fake.
        float parryBait = Mathf.Lerp(0f, 0.7f, ctx.targetParryFrequency);
        // Also higher when they have high parry success (skilled players notice the fake too,
        // but a successful feint flips their skill against them).
        float skilledBait = Mathf.Lerp(0f, 0.2f, ctx.targetParrySuccessRate);
        return parryBait + skilledBait;
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        var attack = brain.PickAttackByType(AttackDataSO.AttackType.Normal);
        if (attack == null) yield break;

        float partial = attack.telegraphTime * Mathf.Clamp01(telegraphFraction);
        yield return brain.PlayPartialTelegraph(attack, partial);
        // No OnAttackPressed → animation never fires → no damage.
        yield return new WaitForSeconds(recoveryAfterCancel);
    }
}
