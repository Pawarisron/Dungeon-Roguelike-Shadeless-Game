using System.Collections;
using UnityEngine;

// Plays a visual warning before an enemy attack lands.
// Pulses the sprite color and toggles a "danger" indicator (e.g. red 危 symbol)
// so the player can read what kind of attack is incoming.
public class AttackTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject perilousIndicator;
    [Tooltip("Number of color pulses across the telegraph window.")]
    [SerializeField] private int pulses = 3;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (perilousIndicator != null) perilousIndicator.SetActive(false);
    }

    // Returns IEnumerator so AIEnemy can `yield return StartCoroutine(...)`
    public IEnumerator Play(AttackDataSO attack)
    {
        if (attack == null) yield break;

        Color original = spriteRenderer != null ? spriteRenderer.color : Color.white;
        bool showIndicator = attack.IsPerilous && perilousIndicator != null;

        if (showIndicator) perilousIndicator.SetActive(true);

        float halfPulse = attack.telegraphTime / Mathf.Max(1, pulses * 2);
        for (int i = 0; i < pulses; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = attack.telegraphColor;
            yield return new WaitForSeconds(halfPulse);
            if (spriteRenderer != null) spriteRenderer.color = original;
            yield return new WaitForSeconds(halfPulse);
        }

        if (showIndicator) perilousIndicator.SetActive(false);
    }
}
