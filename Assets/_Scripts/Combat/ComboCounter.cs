using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Tracks consecutive successful hits for the player. Decays after `decaySeconds` without
// landing a hit OR if the player takes damage. Multiplier scales posture pressure to
// reward aggressive play (Sekiro-style "stay on the offensive" loop).
//
// Wire Agent.OnHitLanded → RegisterHit
//      PlayerInput.OnBeingAttacked → ResetCombo
public class ComboCounter : MonoBehaviour
{
    [Header("Decay")]
    [SerializeField] private float decaySeconds = 2.5f;
    [SerializeField] private int maxCombo = 99;

    [Header("Multiplier curve (combo → posture multiplier)")]
    [Tooltip("Posture damage multiplier per combo step. 0=1x, 5=1.5x, 10=2.0x...")]
    [SerializeField] private float multiplierStep = 0.10f;
    [SerializeField] private float maxMultiplier = 2.5f;

    [Header("UI (optional)")]
    [SerializeField] private Text comboText;
    [SerializeField] private Text multiplierText;

    public UnityEvent<int> OnComboChanged;
    public UnityEvent OnComboBroken;

    private int combo;
    private float lastHitTime = -10f;

    public int Combo => combo;
    public float Multiplier => Mathf.Min(1f + combo * multiplierStep, maxMultiplier);

    public void RegisterHit()
    {
        combo = Mathf.Min(combo + 1, maxCombo);
        lastHitTime = Time.time;
        Refresh();
        OnComboChanged?.Invoke(combo);
    }

    public void ResetCombo()
    {
        if (combo == 0) return;
        combo = 0;
        Refresh();
        OnComboBroken?.Invoke();
    }

    private void Update()
    {
        if (combo > 0 && Time.time - lastHitTime >= decaySeconds) ResetCombo();
    }

    private void Refresh()
    {
        if (comboText != null) comboText.text = combo > 1 ? $"x{combo}" : "";
        if (multiplierText != null) multiplierText.text = combo > 1 ? $"{Multiplier:F2}x" : "";
    }
}
