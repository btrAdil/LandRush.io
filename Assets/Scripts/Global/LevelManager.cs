using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public GameObject playerPrefab;
    private List<Player> players = new List<Player>(); // List of players
    public float PlayerHight = 1f;
    public int PlayersCount = 2;

    private void SetupLevel()
    {
        for (int i = 0; i < PlayersCount; i++)
        {
            Vector3 preDefinedPosition = SurfaceController.Instance.preDefinedPlayerPositions[i];
            preDefinedPosition.y = PlayerHight;

            Player player = Instantiate(playerPrefab, preDefinedPosition, Quaternion.identity).GetComponent<Player>();
            player.Set(i, i != 0);
            players.Add(player);

            SurfaceController.Instance.InitializeSurface(i);

        }

        BasicCamera.Instance.SetCamera(players[0].transform);
    }
    // Start is called before the first frame update
    void Start()
    {
        SetupLevel();
    }
    public void LosePlayer(int playerID){
        Player player = players.Find(p => p.playerID == playerID);
        if(player == null)
            return;
        player.LoseGame();
        players.Remove(player);
    }
    private bool InGame;
    private void RunLevel()
    {

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            player.StartGame();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !InGame)
        {
            RunLevel();
        }
    }
}
