using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<int> currentRound = new NetworkVariable<int>(1);
    public NetworkVariable<float> timePlayer1 = new NetworkVariable<float>(0f);
    public NetworkVariable<float> timePlayer2 = new NetworkVariable<float>(0f);

    [SerializeField] private TextMeshProUGUI timerTextP1;
    [SerializeField] private TextMeshProUGUI timerTextP2;
    [SerializeField] private float catchCooldown = 2f;
    private float lastCatchTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void CatchRunner()
    {
        if (!IsServer) return;

        if (Time.time - lastCatchTime < catchCooldown)
        {
            Debug.Log("Catch is on cooldown!");
            return;
        }

        lastCatchTime = Time.time;

        PlayerState[] players = FindObjectsByType<PlayerState>();
        currentRound.Value++;

        PlayerState player1 = null;
        PlayerState player2 = null;

        foreach (PlayerState p in players)
        {
            if (p.OwnerClientId == 0) player1 = p;
            else if (p.OwnerClientId == 1) player2 = p;
        }

        if (player1 == null || player2 == null)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("EndGameScene", LoadSceneMode.Single);
        }

        if (currentRound.Value == 2)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            int spawnIndex = Random.Range(0, spawnPoints.Length);

            foreach (PlayerState player in players)
            {
                if (player.CurrentRole.Value == PlayerState.PlayerRoleEnum.Hunter)
                {
                    player.CurrentRole.Value = PlayerState.PlayerRoleEnum.Runner;
                    player.TeleportClientRpc(spawnPoints[spawnIndex].transform.position);
                }
                else
                {
                    player.CurrentRole.Value = PlayerState.PlayerRoleEnum.Hunter;
                    player.TeleportClientRpc(spawnPoints[(spawnIndex + 1) % spawnPoints.Length].transform.position);
                }
            }
        }
        else if (currentRound.Value == 3)
        {
            string msg = (timePlayer1.Value < timePlayer2.Value) ? "Player 1 Wins!" : "Player 2 Wygrywa!";

            PrepareResultClientRpc(msg);

            NetworkManager.Singleton.SceneManager.LoadScene("EndGameScene", LoadSceneMode.Single);
        }
    }
    [ClientRpc]
    private void PrepareResultClientRpc(string message)
    {
        if (ResultManager.Instance != null)
        {
            ResultManager.Instance.SetWinner(message);
        }
    }
    public void Update()
    {
        if (IsServer)
        {
            if (currentRound.Value == 1)
            {
                timePlayer1.Value += Time.deltaTime;
            }
            if (currentRound.Value == 2)
            {
                timePlayer2.Value += Time.deltaTime;
            }
        }
        timerTextP1.text = "Player 1 Time: " + timePlayer1.Value.ToString("F2") + "s";
        timerTextP2.text = "Player 2 Time: " + timePlayer2.Value.ToString("F2") + "s";
    }
}