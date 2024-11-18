using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private bool isGamePaused = false;


    private void Awake()
    {
        // Ensure only one instance of GameManager exists (Singleton pattern)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent GameManager from being destroyed when switching scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any additional instances
        }
    }

    public void StartGame()
    {
        Debug.Log("== Game Started ==");
    }

    public void PauseGame()
    {
        if (!isGamePaused)
        {
            Time.timeScale = 0; // Pause the game by setting time scale to 0
            isGamePaused = true;
            Debug.Log("Game Paused");
        }
    }

    public void ResumeGame()
    {
        if (isGamePaused)
        {
            Time.timeScale = 1; // Resume the game by setting time scale to 1
            isGamePaused = false;
            Debug.Log("Game Resumed");
        }
    }

    public void RestartGame()
    {
        Debug.Log("Game Restarted");
        MainMenu();
        //Clear singletons
        Destroy(PlayerManager.Instance.gameObject);
        Destroy(DungeonManager.Instance.gameObject);
        Destroy(this.gameObject);
        if (Caching.ClearCache())
        {
            Debug.Log("Cache has been cleared successfully!");
        }
        else
        {
            Debug.Log("Failed to clear the cache.");
        }
    }
    public void MainMenu()
    {
        SceneLoader.Load(SceneLoader.GameScene.MainMenu);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
