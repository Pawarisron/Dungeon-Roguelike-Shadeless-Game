using UnityEngine;

public interface IParriable
{
    // attack may be null when the source has no AttackDataSO assigned (legacy / Player default).
    // Returns true if the target absorbed/handled the hit (attacker should NOT call TakeDamage).
    bool TryParry(MonoBehaviour attacker, AttackDataSO attack, int incomingDamage);
}
