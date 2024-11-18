using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private class LoadingMonoBehaviour : MonoBehaviour { }

    public enum GameScene
    {
        MainMenu,
        Loading,
        Shop,
        DunLevel1,
        DunLevel2,
    }

    private static Action onLoaderCallback;

    //private static Action onSceneCompleteCallback;

    

    public static void Load(string scene)
    {
        // Set the loader callback action to load the target scene
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            var loader = loadingGameObject.AddComponent<LoadingMonoBehaviour>();
            loader.StartCoroutine(LoadSceneAsync(scene, loadingGameObject));
        };

        // Load the Loading Scene
        Debug.Log("Load Scene: " + scene);
        SceneManager.LoadScene(GameScene.Loading.ToString());
    }
    public static void Load(GameScene scene)
    {
        Load(scene.ToString());
    }
    public static void Load(SceneData scene)
    {
        Load(scene.name);
    }

    private static IEnumerator LoadSceneAsync(string scene, GameObject loadingGameObject)
    {
        yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene);

        while (!asyncOperation.isDone)
        {
            Debug.Log("Loading progress: " + asyncOperation.progress);
            yield return null;
        }

        GameObject.Destroy(loadingGameObject); // Clean up the loading GameObject
    }

    public static void LoaderCallback()
    {
        // Triggered after the first update, allowing the screen to refresh
        // Execute the loader callback action to load the target scene
        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }
}
