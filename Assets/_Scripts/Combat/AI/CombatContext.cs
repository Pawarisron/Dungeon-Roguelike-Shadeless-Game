using UnityEngine;

// Per-tick snapshot of the world from one enemy's perspective. The brain scores actions
// against this — never mutates it. Cheap to allocate (struct), recreated each decision tick.
public struct CombatContext
{
    public Transform self;
    public Transform target;

    public float distanceToTarget;
    public float ownHpPercent;        // 0..1
    public float ownPosturePercent;   // 0..1
    public bool ownStaggered;

    public float targetHpPercent;
    public float targetPosturePercent;
    public bool targetStaggered;

    public bool targetAttackedRecently;     // 0.6s window
    public bool targetWhiffedRecently;      // 0.6s window — open punish
    public bool targetRolledRecently;       // 0.4s window
    public bool targetParriedRecently;      // 0.4s window

    public float targetParryFrequency;      // 0..1 — high = vulnerable to feints
    public float targetParrySuccessRate;    // 0..1
    public float targetRollFrequency;       // rolls/min — high = vulnerable to thrust/Mikiri

    public int alliesNearby;                // other enemies within ~6 units
    public float timeSinceMyLastAttack;
    public float timeSinceTookHit;

    public int currentBossPhase;            // -1 if not a boss
}
