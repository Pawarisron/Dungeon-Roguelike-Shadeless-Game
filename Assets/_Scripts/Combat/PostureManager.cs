using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Sekiro-style posture: stamina-like resource that breaks the entity when full.
// Attach to Player AND every enemy that can be staggered.
public class PostureManager : MonoBehaviour
{
    [Header("Posture Pool")]
    [SerializeField] private float maxPosture = 100f;
    [SerializeField] private float current = 0f;

    [Header("Regen")]
    [SerializeField] private float regenPerSec = 25f;
    [SerializeField] private float regenDelayAfterHit = 1.0f;

    [Header("Stagger")]
    [SerializeField] private float staggerDuration = 2.0f;

    [Header("UI (optional)")]
    [SerializeField] private Image postureBar;

    public UnityEvent OnPostureBroken;
    public UnityEvent OnStaggerEnd;

    public float Current => current;
    public float Max => maxPosture;
    public float Ratio => maxPosture <= 0 ? 0 : current / maxPosture;
    public bool IsStaggered { get; private set; }

    private float lastHitTime = -10f;

    public void TakePostureDamage(float amount)
    {
        if (IsStaggered) return;
        current = Mathf.Min(current + amount, maxPosture);
        lastHitTime = Time.time;
        UpdateBar();

        if (current >= maxPosture)
            BeginStagger();
    }

    private void BeginStagger()
    {
        IsStaggered = true;
        OnPostureBroken?.Invoke();
        StartCoroutine(StaggerCo());
    }

    private IEnumerator StaggerCo()
    {
        yield return new WaitForSeconds(staggerDuration);
        IsStaggered = false;
        current = 0;
        UpdateBar();
        OnStaggerEnd?.Invoke();
    }

    private void Update()
    {
        if (IsStaggered) return;
        if (Time.time - lastHitTime < regenDelayAfterHit) return;
        if (current <= 0) return;

        current = Mathf.Max(0, current - regenPerSec * Time.deltaTime);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (postureBar != null) postureBar.fillAmount = Ratio;
    }
}
