using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Watches the boss's HP, swaps the brain's action set + AIEnemy attack pool when HP
// drops below each phase's threshold. Phases must be ordered DESCENDING by hpThreshold
// (e.g. 1.0 → 0.5 → 0.25 → 0.1).
public class BossPhaseManager : MonoBehaviour
{
    [SerializeField] private List<BossPhaseSO> phases;
    [SerializeField] private AIEnemy enemy;
    [SerializeField] private UtilityBrain brain;
    [SerializeField] private AudioSource audioSource;

    public UnityEvent<int> OnPhaseEntered;

    private int currentPhaseIndex = -1;
    public int CurrentPhaseIndex => currentPhaseIndex;
    public BossPhaseSO CurrentPhase => (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Count) ? phases[currentPhaseIndex] : null;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponent<AIEnemy>();
        if (brain == null) brain = GetComponent<UtilityBrain>();
    }

    private void Update()
    {
        if (phases == null || phases.Count == 0 || enemy == null) return;

        float hpPct = enemy.MaxHealth > 0 ? (float)enemy.CurrentHealth / enemy.MaxHealth : 1f;

        // Walk phases in order; first one whose threshold we're at-or-below wins.
        // Skip phases we're already at or past.
        int target = currentPhaseIndex;
        for (int i = 0; i < phases.Count; i++)
        {
            if (phases[i] == null) continue;
            if (hpPct <= phases[i].hpThreshold && i > target) target = i;
        }
        if (target != currentPhaseIndex) ApplyPhase(target);
    }

    private void ApplyPhase(int idx)
    {
        currentPhaseIndex = idx;
        var phase = phases[idx];
        if (phase == null) return;

        if (brain != null && phase.actionsOverride != null && phase.actionsOverride.Count > 0)
            brain.SetActions(phase.actionsOverride);

        if (brain != null && phase.aggressionMultiplier > 0f)
            brain.SetPostActionDelay(0.2f * phase.aggressionMultiplier);

        if (enemy != null && phase.attackPoolOverride != null && phase.attackPoolOverride.Count > 0)
            enemy.SetAttackPool(phase.attackPoolOverride);

        if (phase.phaseEnterVfx != null)
            Instantiate(phase.phaseEnterVfx, transform.position, Quaternion.identity);

        if (audioSource != null && phase.phaseEnterSfx != null)
            audioSource.PlayOneShot(phase.phaseEnterSfx);

        OnPhaseEntered?.Invoke(idx);
    }
}
