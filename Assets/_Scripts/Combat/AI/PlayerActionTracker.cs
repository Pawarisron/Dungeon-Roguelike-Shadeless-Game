using UnityEngine;

// Records timestamps of every player combat action so enemy brains can read player rhythm
// and punish whiffs / time feints. Wire from PlayerInput UnityEvents in the Inspector.
public class PlayerActionTracker : MonoBehaviour
{
    public float lastAttackTime = -10f;
    public float lastWhiffTime = -10f;        // attack that hit nothing
    public float lastRollTime = -10f;
    public float lastParryPressTime = -10f;
    public float lastParrySuccessTime = -10f;
    public float lastBeingAttackedTime = -10f;

    public int totalAttacks;
    public int totalWhiffs;
    public int totalParries;
    public int totalParrySuccesses;
    public int totalRolls;

    public bool AttackedRecently(float window) => Time.time - lastAttackTime < window;
    public bool WhiffedRecently(float window) => Time.time - lastWhiffTime < window;
    public bool RolledRecently(float window) => Time.time - lastRollTime < window;
    public bool ParriedRecently(float window) => Time.time - lastParryPressTime < window;

    // Wire-points (called by UnityEvents)
    public void OnAttack()           { lastAttackTime = Time.time; totalAttacks++; }
    public void OnAttackWhiff()      { lastWhiffTime = Time.time; totalWhiffs++; }
    public void OnRoll()             { lastRollTime = Time.time; totalRolls++; }
    public void OnParryPressed()     { lastParryPressTime = Time.time; totalParries++; }
    public void OnParrySuccess()     { lastParrySuccessTime = Time.time; totalParrySuccesses++; }
    public void OnBeingAttacked()    { lastBeingAttackedTime = Time.time; }
}
