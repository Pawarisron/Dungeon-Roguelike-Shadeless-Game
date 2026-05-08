using UnityEngine;

// Computes derived patterns from PlayerActionTracker counters that the brain can score against.
// e.g. "Player parries 70% of the time" → boost Feint action weight.
public class PlayerPatternProfile : MonoBehaviour
{
    [SerializeField] private PlayerActionTracker tracker;

    private void Awake()
    {
        if (tracker == null) tracker = GetComponent<PlayerActionTracker>();
    }

    // Parry attempts per attack received — how trigger-happy the player is on parry.
    public float ParryFrequency
    {
        get
        {
            if (tracker == null) return 0f;
            int totalIncoming = Mathf.Max(1, tracker.totalParries + (int)Mathf.Max(0, tracker.totalAttacks * 0.3f));
            return Mathf.Clamp01((float)tracker.totalParries / totalIncoming);
        }
    }

    // How often parries actually land (success / press).
    public float ParrySuccessRate
    {
        get
        {
            if (tracker == null || tracker.totalParries == 0) return 0f;
            return Mathf.Clamp01((float)tracker.totalParrySuccesses / tracker.totalParries);
        }
    }

    // Rolls per minute — high value = player loves dodging.
    public float RollFrequency
    {
        get
        {
            if (tracker == null) return 0f;
            return tracker.totalRolls / Mathf.Max(0.5f, Time.timeSinceLevelLoad / 60f);
        }
    }
}
