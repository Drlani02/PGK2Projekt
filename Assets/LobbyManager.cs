using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public TextMeshProUGUI JoinCodeText;
    public TMP_InputField JoinCodeInputField;
    public TextMeshProUGUI timerText;
    public GameObject StartButton;
    public GameObject PlayerPrefab;
    private int playersCount = 0;
    public int currentRound = 1;
    public NetworkVariable<float> timePlayer1 = new NetworkVariable<float>(0f);
    public NetworkVariable<float> timePlayer2 = new NetworkVariable<float>(0f);
    public async Task Authenticate()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task CreateGame()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        JoinCodeText.text = joinCode;
        StartButton.SetActive(true);
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    public async Task JoinGame(string joinCode)
    {
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }

    public async void OnJoinButtonClick() 
    {
        string joinCode = JoinCodeInputField.text;
        await JoinGame(joinCode);
    }

    public async void OnCreateButtonClick() 
    {
        await CreateGame();
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && sceneEvent.SceneName == "Map1")
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
                int index = (int)sceneEvent.ClientId % spawnPoints.Length;
                Transform selectedSpawn = spawnPoints[index].transform;
                GameObject playerInstance = Instantiate(PlayerPrefab, selectedSpawn.position, selectedSpawn.rotation);
                NetworkObject playerNetworkObject = playerInstance.GetComponent<NetworkObject>();
                playerNetworkObject.SpawnWithOwnership(sceneEvent.ClientId);
                playersCount++;
                if (playersCount == 2)
                {
                    int randomHunterIndex = Random.Range(0, 2);
                    GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                    playerObjects[randomHunterIndex].GetComponent<PlayerState>().CurrentRole.Value = PlayerState.PlayerRoleEnum.Hunter;
                }
            }
        }
    }

    public void CatchRunner()
    {
        PlayerState[] players = FindObjectsByType<PlayerState>();
        currentRound++;
        if (currentRound == 2)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            foreach (PlayerState player in players)
            {
                if(player.CurrentRole.Value == PlayerState.PlayerRoleEnum.Hunter)
                {
                    player.CurrentRole.Value = PlayerState.PlayerRoleEnum.Runner;
                    if(spawnIndex < spawnPoints.Length)
                    {
                        if(spawnIndex == 1)
                            player.transform.position = spawnPoints[1].transform.position;
                        else if(spawnIndex == 0)
                            player.transform.position = spawnPoints[0].transform.position;
                    }
                }
                else
                {
                    player.CurrentRole.Value = PlayerState.PlayerRoleEnum.Hunter;
                    if(spawnIndex < spawnPoints.Length)
                    {
                        if (spawnIndex == 1)
                            player.transform.position = spawnPoints[0].transform.position;
                        else if (spawnIndex == 0)
                            player.transform.position = spawnPoints[1].transform.position;
                    }
                }
            }
        }
        else if (currentRound == 3)
        {
            if(timePlayer1.Value > timePlayer2.Value)
            {
                Debug.Log("Player 1 wins!");
            }
            else if (timePlayer2.Value > timePlayer1.Value)
            {
                Debug.Log("Player 2 wins!");
            }
            else
            {
                Debug.Log("It's a tie!");
            }
        }
    }

    public async void Start()
    {
        
        await Authenticate();
    }

    public void Update()
    {
        if(currentRound == 1) timerText.text = $"Round 1: Player 1 Time: {timePlayer1.Value:F2} seconds";
        else if (currentRound == 2) timerText.text = $"Round 2: Player 2 Time: {timePlayer2.Value:F2} seconds";
        if (!IsServer) return;
        if(currentRound == 1)
        {
            timePlayer1.Value += Time.deltaTime;
        }
        if (currentRound == 2)
        {
            timePlayer2.Value += Time.deltaTime;
        }
     }
}