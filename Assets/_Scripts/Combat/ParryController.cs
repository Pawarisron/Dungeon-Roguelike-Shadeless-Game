using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// Holds the parry timing window. Fired by PlayerInput when the parry button is pressed.
// Other scripts read IsInParryWindow.
public class ParryController : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private float parryWindow = 0.18f;
    [SerializeField] private float cooldownAfterFail = 0.35f;

    [Header("Visuals (optional)")]
    [SerializeField] private GameObject parryStanceVfx;
    [SerializeField] private ParticleSystem parrySparkVfx;

    public UnityEvent OnParryTriggered;
    public UnityEvent OnParrySuccess;

    //Fast lane event
    public event Action ParrySucceeded;

    private float windowEndTime = -1f;
    private float cooldownEndTime = -1f;

    public bool IsInParryWindow => Time.time <= windowEndTime;
    public bool IsOnCooldown => Time.time < cooldownEndTime;

    public void TriggerParry()
    {
        Debug.Log("Trigger Parry");
        if (IsOnCooldown) return;
        windowEndTime = Time.time + parryWindow;
        cooldownEndTime = windowEndTime + cooldownAfterFail;
        ParrySucceeded?.Invoke();
        OnParryTriggered?.Invoke();
        if (parryStanceVfx != null) parryStanceVfx.SetActive(true);
        Invoke(nameof(HideStance), parryWindow);
    }

    public void NotifySuccess(Vector3 contactPoint)
    {
        Debug.Log("Parry Success");
        OnParrySuccess?.Invoke();

        if (parrySparkVfx != null) 
            parrySparkVfx.Play();

        windowEndTime = -1f;
        cooldownEndTime = -1f;
    }

    private void HideStance()
    {
        if (parryStanceVfx != null) parryStanceVfx.SetActive(false);
    }
}
