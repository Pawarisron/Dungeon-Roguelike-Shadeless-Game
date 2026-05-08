using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Boss-grade decision loop. Snapshots the world into a CombatContext, scores every
// available action, executes the winner, then loops. Replaces the simple ChaseAndAttack
// flow on AIEnemy when present.
//
// One brain per enemy. Actions are ScriptableObject assets shared across enemies.
public class UtilityBrain : MonoBehaviour
{
    [Header("Action Pool")]
    [Tooltip("Available actions. Each scored every tick — highest wins.")]
    [SerializeField] private List<EnemyAction> actions;

    [Header("Pacing")]
    [SerializeField] private float decisionTickInterval = 0.08f;
    [Tooltip("Forced breathing room between actions so the enemy isn't a non-stop blender.")]
    [SerializeField] private float postActionDelay = 0.2f;

    [Header("References (auto-fetched if blank)")]
    [SerializeField] private AIEnemy enemy;
    [SerializeField] private Agent agent;
    [SerializeField] private PostureManager posture;
    [SerializeField] private AttackTelegraph telegraph;
    [SerializeField] private BossPhaseManager bossPhases;

    [Header("Awareness")]
    [SerializeField] private float alliesScanRadius = 6f;
    [SerializeField] private LayerMask allyLayer = ~0;

    private readonly Dictionary<EnemyAction, float> cooldownEnd = new Dictionary<EnemyAction, float>();
    private float lastMyAttackTime = -10f;
    private float lastTookHitTime = -10f;

    public bool HasTarget => enemy != null && enemy.AiData != null && enemy.AiData.currentTarget != null;
    public AIEnemy Enemy => enemy;
    public Agent Agent => agent;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponent<AIEnemy>();
        if (agent == null) agent = GetComponent<Agent>();
        if (posture == null) posture = GetComponent<PostureManager>();
        if (telegraph == null) telegraph = GetComponent<AttackTelegraph>();
        if (bossPhases == null) bossPhases = GetComponent<BossPhaseManager>();
    }

    // Driven by AIEnemy.Update — replaces ChaseAndAttack when this component is present.
    public IEnumerator RunLoop()
    {
        while (HasTarget)
        {
            var ctx = Snapshot();
            var action = PickAction(ctx);
            if (action == null)
            {
                StopMovement();
                yield return new WaitForSeconds(decisionTickInterval);
                continue;
            }
            yield return StartCoroutine(action.Execute(this, ctx));
            cooldownEnd[action] = Time.time + action.cooldown;
            yield return new WaitForSeconds(postActionDelay);
        }
        StopMovement();
    }

    private CombatContext Snapshot()
    {
        var ctx = new CombatContext();
        ctx.self = transform;
        ctx.target = enemy.AiData != null ? enemy.AiData.currentTarget : null;
        if (ctx.target == null) return ctx;

        ctx.distanceToTarget = Vector2.Distance(ctx.self.position, ctx.target.position);

        ctx.ownHpPercent = enemy.MaxHealth > 0 ? (float)enemy.CurrentHealth / enemy.MaxHealth : 1f;
        ctx.ownPosturePercent = posture != null ? posture.Ratio : 0f;
        ctx.ownStaggered = posture != null && posture.IsStaggered;

        var targetHealth = ctx.target.GetComponent<HealthManager>();
        if (targetHealth != null && targetHealth.maxHealth > 0)
            ctx.targetHpPercent = targetHealth.healthAmount / targetHealth.maxHealth;

        var targetPosture = ctx.target.GetComponent<PostureManager>();
        if (targetPosture != null)
        {
            ctx.targetPosturePercent = targetPosture.Ratio;
            ctx.targetStaggered = targetPosture.IsStaggered;
        }

        var tracker = ctx.target.GetComponent<PlayerActionTracker>();
        if (tracker != null)
        {
            ctx.targetAttackedRecently = tracker.AttackedRecently(0.6f);
            ctx.targetWhiffedRecently  = tracker.WhiffedRecently(0.6f);
            ctx.targetRolledRecently   = tracker.RolledRecently(0.4f);
            ctx.targetParriedRecently  = tracker.ParriedRecently(0.4f);
        }

        var profile = ctx.target.GetComponent<PlayerPatternProfile>();
        if (profile != null)
        {
            ctx.targetParryFrequency = profile.ParryFrequency;
            ctx.targetParrySuccessRate = profile.ParrySuccessRate;
            ctx.targetRollFrequency = profile.RollFrequency;
        }

        ctx.alliesNearby = CountAlliesNearby();
        ctx.timeSinceMyLastAttack = Time.time - lastMyAttackTime;
        ctx.timeSinceTookHit = Time.time - lastTookHitTime;
        ctx.currentBossPhase = bossPhases != null ? bossPhases.CurrentPhaseIndex : -1;
        return ctx;
    }

    private EnemyAction PickAction(CombatContext ctx)
    {
        if (actions == null || actions.Count == 0) return null;

        EnemyAction best = null;
        float bestScore = 0f;
        foreach (var act in actions)
        {
            if (act == null) continue;
            if (cooldownEnd.TryGetValue(act, out var end) && Time.time < end) continue;
            float s = act.Score(ctx);
            if (s < act.minScoreToConsider) continue;
            if (s > bestScore) { bestScore = s; best = act; }
        }
        return best;
    }

    private int CountAlliesNearby()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, alliesScanRadius, allyLayer);
        int count = 0;
        foreach (var h in hits)
        {
            if (h == null || h.transform == transform) continue;
            if (h.GetComponent<AIEnemy>() != null) count++;
        }
        return count;
    }

    // ===== Helpers exposed to actions =====

    public Vector2 ComputeChaseDirection() => enemy != null ? enemy.ComputeChaseDirection() : Vector2.zero;
    public void SetMovementInput(Vector2 v) { if (enemy != null) enemy.SetExternalMovement(v); }
    public void StopMovement() { if (enemy != null) enemy.SetExternalMovement(Vector2.zero); }

    public AttackDataSO PickAttackByType(AttackDataSO.AttackType type) => enemy != null ? enemy.PickAttackOfType(type) : null;
    public AttackDataSO PickAttackPerilous() => enemy != null ? enemy.PickAttackPerilous() : null;

    public IEnumerator RunAttack(AttackDataSO attack)
    {
        if (attack == null) yield break;
        lastMyAttackTime = Time.time;
        yield return enemy.RunAttack(attack);
    }

    public IEnumerator PlayPartialTelegraph(AttackDataSO attack, float partial)
    {
        if (attack == null) yield break;
        if (telegraph == null) { yield return new WaitForSeconds(partial); yield break; }

        var co = StartCoroutine(telegraph.Play(attack));
        yield return new WaitForSeconds(partial);
        StopCoroutine(co);
    }

    // Wire from PostureManager.OnPostureBroken / OnBeingAttacked etc.
    public void NotifyTookHit() { lastTookHitTime = Time.time; }

    // BossPhaseManager calls this to swap the action pool mid-fight.
    public void SetActions(List<EnemyAction> newActions)
    {
        actions = newActions;
        cooldownEnd.Clear();
    }

    public void SetPostActionDelay(float delay) => postActionDelay = Mathf.Max(0f, delay);
}
