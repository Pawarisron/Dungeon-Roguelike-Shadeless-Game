using Unity.VisualScripting;
using UnityEngine;

public class PlayerDeadManager : MonoBehaviour
{
    [SerializeField]
    private Canvas deadScene; //TODO: change into prefab instead and create its object in scene

    private HealthManager healthManager;
    private bool handled = false;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void Update()
    {
        if (handled) return;

        if (healthManager == null)
        {
            Debug.LogError("Health Manager is null");
            return;
        }

        if (healthManager.isDeath)
        {
            handled = true;
            ShowDeadScreen();
        }
    }

    private void ShowDeadScreen()
    {
        if (deadScene != null)
        {
            deadScene.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("PlayerDeadManager: deadScene is not assigned in the inspector.");
        }

        // Freeze the world behind the Game Over screen so enemies stop acting.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }

    public void OnPressMainMenu()
    {
        GameManager.Instance.RestartGame();
    }
}
