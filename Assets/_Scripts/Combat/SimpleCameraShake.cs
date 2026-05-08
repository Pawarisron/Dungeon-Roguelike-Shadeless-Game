using System.Collections;
using UnityEngine;

// Lightweight camera shake that works with the project's existing CameraController
// (direct Vector3.Lerp follow — no Cinemachine in scene). Disables the follow during
// the shake window so the offset is actually visible, then re-enables it.
//
// Wire UnityEvents in the Inspector (Auto-Wire does this for the Player prefab):
//   PlayerInput.OnParrySuccess  → ShakeMedium
//   PlayerInput.OnMikiriSuccess → ShakeMedium
//   PlayerInput.OnDodgeSuccess  → ShakeLight
//   Agent.OnHitLanded           → ShakeLight
//   Agent.OnDeathblowDealt      → ShakeHeavy
public class SimpleCameraShake : MonoBehaviour
{
    [SerializeField] private float lightForce = 0.10f;
    [SerializeField] private float mediumForce = 0.22f;
    [SerializeField] private float heavyForce = 0.45f;
    [SerializeField] private float lightDuration = 0.12f;
    [SerializeField] private float mediumDuration = 0.18f;
    [SerializeField] private float heavyDuration = 0.30f;

    private Camera cam;
    private CameraController follow;
    private Coroutine running;

    private void Awake()
    {
        cam = Camera.main;
        if (cam != null) follow = cam.GetComponent<CameraController>();
    }

    public void ShakeLight()  { Shake(lightForce, lightDuration); }
    public void ShakeMedium() { Shake(mediumForce, mediumDuration); }
    public void ShakeHeavy()  { Shake(heavyForce, heavyDuration); }

    public void Shake(float force, float duration)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        if (follow == null) follow = cam.GetComponent<CameraController>();

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ShakeCo(force, duration));
    }

    private IEnumerator ShakeCo(float force, float duration)
    {
        bool reEnable = false;
        if (follow != null && follow.enabled) { follow.enabled = false; reEnable = true; }

        var t = cam.transform;
        Vector3 origin = t.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float decay = 1f - (elapsed / duration);
            Vector2 jitter = Random.insideUnitCircle * force * decay;
            t.position = origin + new Vector3(jitter.x, jitter.y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        t.position = origin;
        if (reEnable) follow.enabled = true;
        running = null;
    }
}
