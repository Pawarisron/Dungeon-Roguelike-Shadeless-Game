using Unity.VisualScripting;
using UnityEngine;

public class ShopManager : MonoBehaviour
{

    [SerializeField]
    private SceneData dungeonScene;

    [SerializeField]
    private Vector2 playerSpawnPosition; // position that player spawn in the shop

    private void Start()
    {
        PlayerManager.Instance.SetRespawnPosition(playerSpawnPosition);        
        PlayerManager.Instance.RespawnPlayer();
    }
    public void OnEnterDungeonDoor()
    {
        SceneLoader.Load(dungeonScene);
    }

    //TODO: Store playerRef in PlayerManager
    
}
