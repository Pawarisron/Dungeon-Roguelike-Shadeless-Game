using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Action_AttackPerilous", menuName = "Combat/AI/Action: Attack Perilous")]
public class EnemyAction_AttackPerilous : EnemyAction
{
    public float maxRange = 1.5f;

    [Tooltip("Min boss phase that allows this action. -1 = always available.")]
    public int unlockedAtPhase = -1;

    public override float ScoreBase(CombatContext ctx)
    {
        if (ctx.currentBossPhase >= 0 && ctx.currentBossPhase < unlockedAtPhase) return 0f;
        if (ctx.distanceToTarget > maxRange) return 0.1f;

        // Push perilous when player posture is rising (close to break) or rolling a lot
        // (Sweep punishes roll-heavy players, Thrust baits Mikiri-attempts).
        float postureFactor = Mathf.Lerp(0.2f, 0.85f, ctx.targetPosturePercent);
        float rollFactor = ctx.targetRolledRecently ? 0.25f : 0f;
        return postureFactor + rollFactor;
    }

    public override IEnumerator Execute(UtilityBrain brain, CombatContext ctx)
    {
        // Pick any non-Normal attack from pool.
        var attack = brain.PickAttackPerilous();
        if (attack == null) yield break;
        yield return brain.RunAttack(attack);
    }
}
