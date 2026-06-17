using System.Collections;
using UnityEngine;

// Global hit-stop: freezes Time.timeScale briefly for impact feel.
// Singleton-lazy: any caller can fire HitStop.Trigger(0.08f) without setup.
public class HitStop : MonoBehaviour
{
    private static HitStop instance;
    private Coroutine running;

    public static void Trigger(float duration, float scaleDuringStop = 0.05f)
    {
        // Don't fight a hard pause (e.g. the Game Over screen).
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) return;

        EnsureInstance();
        if (instance.running != null) instance.StopCoroutine(instance.running);
        instance.running = instance.StartCoroutine(instance.StopCo(duration, scaleDuringStop));
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;
        var go = new GameObject("[HitStop]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<HitStop>();
    }

    private IEnumerator StopCo(float duration, float scale)
    {
        Time.timeScale = scale;
        // Use unscaled time so the freeze actually ends.
        yield return new WaitForSecondsRealtime(duration);
        // If the game got hard-paused mid-freeze (death/menu), stay frozen.
        bool hardPaused = GameManager.Instance != null && GameManager.Instance.IsGamePaused;
        Time.timeScale = hardPaused ? 0f : 1f;
        running = null;
    }
}
