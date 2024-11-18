using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private SceneData onPlayScene;

    private void Awake()
    {
        Reset();
    }

    public void OnPlayButtonPressed()
    {
        SceneLoader.Load(onPlayScene);
    }

    private void Reset()
    {
        if(PlayerManager.Instance != null)
        {
            Destroy(PlayerManager.Instance);
        }
    }
}
