using UnityEngine;

[CreateAssetMenu(fileName = "AttackData_", menuName = "Combat/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    public enum AttackType
    {
        Normal,           // can be parried
        Perilous_Sweep,   // CANNOT parry — must dodge (roll)
        Perilous_Thrust,  // CANNOT parry — Mikiri counter (roll into it)
        Perilous_Crash    // CANNOT parry — must dodge (no Mikiri)
    }

    [Header("Identity")]
    public string attackName = "Slash";
    public AttackType type = AttackType.Normal;

    [Header("Damage")]
    public int hpDamage = 20;
    public float postureDamage = 18f;
    public float parryRetaliationPosture = 35f;

    [Header("Telegraph")]
    [Tooltip("Seconds the enemy spends warning before the attack lands.")]
    public float telegraphTime = 0.45f;
    [Tooltip("Color the sprite flashes during telegraph.")]
    public Color telegraphColor = new Color(1f, 0.2f, 0.2f, 1f);

    public bool IsParryable => type == AttackType.Normal;
    public bool CanBeMikiri => type == AttackType.Perilous_Thrust;
    public bool MustDodge => type == AttackType.Perilous_Sweep || type == AttackType.Perilous_Crash;
    public bool IsPerilous => type != AttackType.Normal;
}
