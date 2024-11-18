using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    // Static instance of SceneController to allow global access
    public static DungeonManager Instance { get; private set; }

    [SerializeField]
    private List<SceneData> dunLevels = new List<SceneData>();

    [SerializeField]
    private SceneData defaultScene;

    private Scene firstLevelScene;
    private int currentLevelIndex = 0;

    private void Awake()
    {
        //Check first active scene is firstLevelScene
        firstLevelScene = SceneManager.GetActiveScene();
        if (!firstLevelScene.name.Equals(dunLevels[0].name))
        {
            Debug.LogError("first floor dungeon is not correct");
            return;
        }
        //Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Reset()
    {
        currentLevelIndex = 0;
    }
    public string GetCurrentSceneName()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < dunLevels.Count)
        {
            return dunLevels[currentLevelIndex].name;
        }
        Debug.LogWarning("Current scene index is out of range.");
        return string.Empty;
    }

    //Function to load the next scene in the list
    public void LoadNextScene()
    {
        if(currentLevelIndex < dunLevels.Count - 1)
        {
            currentLevelIndex++;
            LoadSceneByIndex(currentLevelIndex);
        }
        else
        {
            //If at the last scene, load the default scene instead
            Debug.Log("Reached the last scene. Loading default scene.");
            //Exit
            ExitLevel();
        }
    }

    //Function to load the previous scene in the list
    public void LoadPreviousScene()
    {
        if (currentLevelIndex > 0)
        {
            //Move to the previous scene in the list
            currentLevelIndex--;
            LoadSceneByIndex(currentLevelIndex);
        }
        else
        {
            Debug.Log("You are at the first scene.");
        }
    }

    public void ExitLevel()
    {
        SceneLoader.Load(defaultScene);
        Destroy(gameObject);
    }

    private void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < dunLevels.Count)
        {
            //Load the scene by its name using SceneManager
            SceneLoader.Load(dunLevels[index]);
        }
        else
        {
            Debug.LogWarning("Scene index is out of range.");
        }
    }
    //Function to reload the current scene
    public void ReloadCurrentScene()
    {
        LoadSceneByIndex(currentLevelIndex);
    }
}
