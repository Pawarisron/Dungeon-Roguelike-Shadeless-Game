using UnityEngine;

public interface IParriable
{
    // Returns true if the incoming attack is parried.
    // attacker is the source Agent (used for posture retaliation).
    bool TryParry(MonoBehaviour attacker, int incomingDamage);
}
