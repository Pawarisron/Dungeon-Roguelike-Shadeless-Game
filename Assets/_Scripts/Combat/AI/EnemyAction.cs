using System.Collections;
using UnityEngine;

// Base class for every enemy action the UtilityBrain can score and execute.
// Each action is a ScriptableObject so designers can mix/match per enemy in the Inspector.
// Cooldown state is held by the brain (per-instance), not on the SO, since SOs are shared
// across multiple enemies.
public abstract class EnemyAction : ScriptableObject
{
    [TextArea] public string description;

    [Tooltip("Multiplier applied to ScoreBase before the brain compares actions.")]
    [Range(0f, 2f)] public float weightMultiplier = 1f;

    [Tooltip("Seconds this action is unavailable after running.")]
    public float cooldown = 0f;

    [Tooltip("Hard floor — never picked unless score >= this.")]
    [Range(0f, 1f)] public float minScoreToConsider = 0.1f;

    // Override in concrete actions. Returns 0..1+ utility score against the snapshot.
    public abstract float ScoreBase(CombatContext ctx);

    // Override in concrete actions. Yields while the action runs (can take seconds).
    public abstract IEnumerator Execute(UtilityBrain brain, CombatContext ctx);

    public float Score(CombatContext ctx) => ScoreBase(ctx) * weightMultiplier;
}
