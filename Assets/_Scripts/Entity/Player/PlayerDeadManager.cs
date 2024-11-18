using Unity.VisualScripting;
using UnityEngine;

public class PlayerDeadManager : MonoBehaviour
{
    [SerializeField]
    private Canvas deadScene; //TODO: change into prefab instead and create its object in scene

    private HealthManager healthManager;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void Update()
    {

        if(healthManager == null)
        {
            Debug.LogError("Health Manager is null");
            return;
        }

        if (healthManager.isDeath)
        {
            deadScene.gameObject.SetActive(true);
        }
    }
    public void OnPressMainMenu()
    {
        GameManager.Instance.RestartGame();
    }
}
