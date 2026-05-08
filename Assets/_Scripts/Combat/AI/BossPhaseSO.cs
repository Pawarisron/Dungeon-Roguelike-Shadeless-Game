using System.Collections.Generic;
using UnityEngine;

// Per-phase config for a boss. BossPhaseManager swaps the active set when HP crosses
// `hpThreshold` (descending order recommended in the manager's list).
[CreateAssetMenu(fileName = "BossPhase_", menuName = "Combat/AI/Boss Phase")]
public class BossPhaseSO : ScriptableObject
{
    [Tooltip("Phase activates when HP percent drops to or below this value (1=full, 0=dead).")]
    [Range(0f, 1f)] public float hpThreshold = 1.0f;

    [Tooltip("Override the brain's action set during this phase. Leave empty to keep current.")]
    public List<EnemyAction> actionsOverride;

    [Tooltip("Override the AIEnemy attackPool during this phase. Leave empty to keep current.")]
    public List<AttackDataSO> attackPoolOverride;

    [Tooltip("Multiplier applied to all telegraph times during this phase (lower = faster, harder).")]
    [Range(0.3f, 1.5f)] public float telegraphSpeedMultiplier = 1.0f;

    [Tooltip("Multiplier on the brain's post-action delay (lower = more aggressive).")]
    [Range(0.2f, 2.0f)] public float aggressionMultiplier = 1.0f;

    [Tooltip("Optional VFX prefab spawned at the boss when this phase begins.")]
    public GameObject phaseEnterVfx;

    [Tooltip("Optional SFX clip played when this phase begins.")]
    public AudioClip phaseEnterSfx;
}
