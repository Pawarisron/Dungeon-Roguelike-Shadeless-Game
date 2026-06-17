using UnityEngine;

// Always-on-screen "go home" button. Drawn with IMGUI so it needs no Canvas
// or EventSystem and works in every scene where the player exists. It lives on
// the player (DontDestroyOnLoad), so it follows the player across scenes.
//
// Pressing it warps the player back to the home/hub scene (the Shop), which is
// the same scene the game first loads on Play.
public class HomeButtonHUD : MonoBehaviour
{
    [SerializeField] private SceneLoader.GameScene homeScene = SceneLoader.GameScene.Shop;

    [Header("On-screen placement (pixels from the top-left)")]
    [SerializeField] private float x = 20f;
    [SerializeField] private float y = 20f;
    [SerializeField] private float width = 140f;
    [SerializeField] private float height = 46f;
    [SerializeField] private string label = "Home";

    private void OnGUI()
    {
        if (GUI.Button(new Rect(x, y, width, height), label))
        {
            GoHome();
        }
    }

    // Public so a real uGUI Button's OnClick can call it too, if added later.
    public void GoHome()
    {
        Time.timeScale = 1f; // undo any pause before loading
        SceneLoader.Load(homeScene);
    }
}
