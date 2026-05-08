using UnityEngine;

// Optional visual indicator that toggles when posture is broken (red exclamation, etc.).
// Wire PostureManager.OnPostureBroken → DeathblowMarker.Show
//      PostureManager.OnStaggerEnd  → DeathblowMarker.Hide  (in the Inspector)
public class DeathblowMarker : MonoBehaviour
{
    [SerializeField] private GameObject indicator;

    private void Awake()
    {
        if (indicator != null) indicator.SetActive(false);
    }

    public void Show() { if (indicator != null) indicator.SetActive(true); }
    public void Hide() { if (indicator != null) indicator.SetActive(false); }
}
