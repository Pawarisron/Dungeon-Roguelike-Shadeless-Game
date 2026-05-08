using Cinemachine;
using UnityEngine;

// Thin wrapper around CinemachineImpulseSource so UnityEvents can fire camera shake
// without needing direct refs to Cinemachine in every other script.
// Project uses Cinemachine 2.10.1 — namespace is `Cinemachine` (not Unity.Cinemachine).
[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShakeOnHit : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private float lightShake = 0.4f;
    [SerializeField] private float mediumShake = 0.8f;
    [SerializeField] private float heavyShake = 1.6f;

    private void Reset() { impulseSource = GetComponent<CinemachineImpulseSource>(); }
    private void Awake() { if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>(); }

    public void ShakeLight()  { Generate(lightShake); }
    public void ShakeMedium() { Generate(mediumShake); }
    public void ShakeHeavy()  { Generate(heavyShake); }

    private void Generate(float force)
    {
        if (impulseSource != null) impulseSource.GenerateImpulse(force);
    }
}
