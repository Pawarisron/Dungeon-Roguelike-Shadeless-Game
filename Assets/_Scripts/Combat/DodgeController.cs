using UnityEngine;

// Tracks i-frames during a roll. PlayerInput.PerformRoll → TriggerDodge.
// IParriable checks IsInDodgeWindow to ignore Sweep/Crash damage entirely.
public class DodgeController : MonoBehaviour
{
    [SerializeField] private float dodgeDuration = 0.30f;
    [SerializeField] private float cooldown = 0.15f;

    private float dodgeEndTime = -1f;
    private float cooldownEndTime = -1f;

    public bool IsInDodgeWindow => Time.time <= dodgeEndTime;
    public bool IsOnCooldown => Time.time < cooldownEndTime;

    public void TriggerDodge()
    {
        if (IsOnCooldown) return;
        dodgeEndTime = Time.time + dodgeDuration;
        cooldownEndTime = dodgeEndTime + cooldown;
    }
}
