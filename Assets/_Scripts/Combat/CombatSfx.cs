using UnityEngine;

// SFX dispatcher. Wire each UnityEvent (Agent.OnHitLanded, ParryController.OnParrySuccess,
// PlayerInput.OnMikiriSuccess, etc.) to the matching method here from the Inspector.
// Designer assigns AudioClips; this script just plays them on the AudioSource.
[RequireComponent(typeof(AudioSource))]
public class CombatSfx : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource source;

    [Header("Player-side")]
    [SerializeField] private AudioClip parrySpark;       // perfect parry — high-pitch "chink!"
    [SerializeField] private AudioClip guardThud;        // partial guard — low "thud"
    [SerializeField] private AudioClip dodgeSwoosh;      // i-frame roll
    [SerializeField] private AudioClip mikiriShing;      // counter success

    [Header("Combat outcomes")]
    [SerializeField] private AudioClip cleanHit;
    [SerializeField] private AudioClip deathblowImpact;
    [SerializeField] private AudioClip postureBreak;     // when an entity staggers

    [Header("Telegraphs")]
    [SerializeField] private AudioClip perilousHum;      // building red flash

    [Header("Mix")]
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [SerializeField] private Vector2 pitchJitter = new Vector2(0.95f, 1.05f);

    private void Reset() { source = GetComponent<AudioSource>(); }
    private void Awake() { if (source == null) source = GetComponent<AudioSource>(); }

    public void PlayParry()       { Play(parrySpark); }
    public void PlayGuard()       { Play(guardThud); }
    public void PlayDodge()       { Play(dodgeSwoosh); }
    public void PlayMikiri()      { Play(mikiriShing); }
    public void PlayCleanHit()    { Play(cleanHit); }
    public void PlayDeathblow()   { Play(deathblowImpact); }
    public void PlayPostureBreak(){ Play(postureBreak); }
    public void PlayPerilousHum() { Play(perilousHum); }

    private void Play(AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        source.PlayOneShot(clip, volume);
    }
}
