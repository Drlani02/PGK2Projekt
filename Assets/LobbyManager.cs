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
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public TextMeshProUGUI JoinCodeText;
    public TMP_InputField JoinCodeInputField;
    public TMP_Dropdown MapDropdown;
    public GameObject StartButton;
    public GameObject JoinText;
    public GameObject PlayerPrefab;
    private int playersCount = 0;
    public int currentRound = 1;
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
        JoinText.SetActive(true);
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
            string selectedMap = MapDropdown.options[MapDropdown.value].text;
            NetworkManager.Singleton.SceneManager.LoadScene(selectedMap, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete && (sceneEvent.SceneName == "Map1" || sceneEvent.SceneName == "Map2"))
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
                if (spawnPoints.Length == 0)
                {
                    Debug.LogError("No spawn points found in the scene.");
                    return;
                }
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


    public async void Start()
    {
        await Authenticate();
    }

    public void Update()
    {

    }
}