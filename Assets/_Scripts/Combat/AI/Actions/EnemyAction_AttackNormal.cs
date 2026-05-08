using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Action_AttackNormal", menuName = "Combat/AI/Action: Attack Normal")]
public class EnemyAction_AttackNormal : EnemyAction
{
    public float attackRange = 1.0f;

    public override float ScoreBase(CombatContext ctx)
    {
        if (ctx.distanceToTarget > attackRange) return 0.05f;
        float whiffPunish = ctx.targetWhiffedRecently ? 0.40f : 0f;
        float baseline = 0.55f;
        return baseline + whiffPunish;
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        var attack = brain.PickAttackByType(AttackDataSO.AttackType.Normal);
        yield return brain.RunAttack(attack);
    }
}
