using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] 
    private GameObject playerPrefab;

    [SerializeField] 
    private Vector2 respawnPosition;

    private GameObject playerInstance;

    public GameObject Player => playerPrefab;

    private void Awake()
    {
        // SingleTon
        if (Instance == null)
        {
            Debug.Log("new PLayer manager instance");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Spawn the player if it hasn't been instantiated yet
        //if (playerInstance == null)
        //{
        //    RespawnPlayer();
        //}
    }
    //private void Update()
    //{
    //    //Debug.Log("Player is null : "+ playerInstance == null);
    //    //Debug.Log("PlayerManager is null : "+ Instance == null);
    //}
    public void RespawnPlayer()
    {
        // Singleton
        if (playerInstance == null)
        {
            Debug.Log("new player instance");
            // Instantiate a new playerPrefab
            playerInstance = Instantiate(playerPrefab, respawnPosition, Quaternion.identity);
            playerInstance.name = "Player";
            DontDestroyOnLoad(playerInstance); // Make player persistent across scenes
        }
        else
        {
            playerInstance.transform.position = respawnPosition;
        }
    }
    // set respawn position dynamically
    
    public Transform GetPlayerTransform() 
    {
        return playerInstance.transform;
    }
    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
    private void OnDestroy()
    {
        Destroy(playerInstance.gameObject);
        playerInstance= null;
    }
}
