using UnityEngine;
using UnityEngine.Events;

// Listens for incoming Thrust telegraphs broadcast by enemies.
// If the player presses Roll while a Thrust is in flight, TryMikiri() succeeds:
// the attacker eats heavy posture damage and the attack is consumed.
public class MikiriDetector : MonoBehaviour
{
    [Tooltip("Posture damage dealt back to the attacker on a successful Mikiri.")]
    [SerializeField] private float mikiriPostureDamage = 60f;

    public UnityEvent OnMikiriSuccess;
    public UnityEvent<MonoBehaviour> OnIncomingThrust;  // wire to MikiriArrowIndicator.ShowFor

    private MonoBehaviour incomingAttacker;
    private AttackDataSO incomingAttack;
    private float windowExpiresAt;

    private MonoBehaviour lastMikiriedAttacker;
    private float lastMikiriTime = -10f;

    public bool HasIncoming => incomingAttack != null && Time.time <= windowExpiresAt;

    // Used by PlayerInput.TryParry to confirm that an incoming Thrust hit was already negated
    // by a recent Mikiri (same attacker, within the lookback window).
    public bool DidMikiri(MonoBehaviour attacker, float lookbackSeconds = 0.6f)
    {
        return attacker != null
               && attacker == lastMikiriedAttacker
               && Time.time - lastMikiriTime <= lookbackSeconds;
    }

    public void RegisterIncoming(MonoBehaviour attacker, AttackDataSO attack)
    {
        if (attack == null || !attack.CanBeMikiri) return;
        incomingAttacker = attacker;
        incomingAttack = attack;
        windowExpiresAt = Time.time + attack.telegraphTime;
        OnIncomingThrust?.Invoke(attacker);
    }

    public bool TryMikiri()
    {
        if (!HasIncoming) return false;
        if (incomingAttacker != null)
        {
            var posture = incomingAttacker.GetComponent<PostureManager>();
            if (posture != null) posture.TakePostureDamage(mikiriPostureDamage);
            lastMikiriedAttacker = incomingAttacker;
            lastMikiriTime = Time.time;
        }
        Clear();
        OnMikiriSuccess?.Invoke();
        return true;
    }

    public void Clear()
    {
        incomingAttacker = null;
        incomingAttack = null;
        windowExpiresAt = -1f;
    }
}
