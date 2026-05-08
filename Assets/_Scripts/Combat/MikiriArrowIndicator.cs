using UnityEngine;

// Shows a small arrow / glow above the attacker when an incoming Thrust is registered,
// so the player has a clear "roll into me" cue (Sekiro shows ➜ kanji over the enemy).
// Wire MikiriDetector.OnIncomingThrust → ShowFor(attacker)
//      MikiriDetector.OnMikiriSuccess  → Hide
public class MikiriArrowIndicator : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float autoHideAfter = 0.6f;

    private GameObject spawned;
    private float hideAt;

    public void ShowFor(MonoBehaviour attacker)
    {
        Hide();
        if (arrowPrefab == null || attacker == null) return;
        spawned = Instantiate(arrowPrefab, attacker.transform.position + offset, Quaternion.identity, attacker.transform);
        spawned.transform.localPosition = offset;
        hideAt = Time.time + autoHideAfter;
    }

    public void Hide()
    {
        if (spawned != null) Destroy(spawned);
        spawned = null;
    }

    private void Update()
    {
        if (spawned != null && Time.time >= hideAt) Hide();
    }
}
