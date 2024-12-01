using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Utils;

public class QuickLobbyManager : MonoBehaviour
{
    public static QuickLobbyManager Instance { get; private set; }

    public enum EncryptionType
    {
        DTLS, //Datagram transport layer security
        WSS //Web Socket Secure
    }

    public string lobbyName = "Lobby";
    public int maxPlayers = 2;
    public EncryptionType encryptionType = EncryptionType.DTLS;
    
    private string playerID;
    private string playerName;

    //const values that do not change
    const float hbInterval = 20f;
    const float pollInterval = 70f;
    const string keyJoinCode = "RelayJoinCode";
    const string dtlsEncryption = "dtls";
    const string wssEncryption = "wss"; //webgl builds only work with web socket secure network encryption

    string connectionType => (encryptionType == EncryptionType.DTLS) ? dtlsEncryption : wssEncryption;

    CountdownTimer hbTimer = new CountdownTimer(hbInterval);
    CountdownTimer pollTimer = new CountdownTimer(pollInterval);

    Lobby currentLobby;

    // Start is called before the first frame update
    async void Start()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        await Authenticate();

        hbTimer.OnTimerStop += () =>
        {
            HandleHeartBeatAsync();
            hbTimer.Start();
        };

        pollTimer.OnTimerStop += () =>
        {
            HandlePollUpdateAsync();
            pollTimer.Start();
        };
    }

    async Task Authenticate()
    {
        await Authenticate("Player" + Random.Range(0, 5000));
    }

    async Task Authenticate(string userName)
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(userName);

            await UnityServices.InitializeAsync(options);
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In As: " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerID = AuthenticationService.Instance.PlayerId;
            playerName = userName;
        }
    }

    public async Task CreateLobby()
    {
        try
        {
            Allocation alloction = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(alloction);

            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = false
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Debug.Log($"Created Lobby: {currentLobby.Name} with code {currentLobby.LobbyCode}");

            hbTimer.Start();
            pollTimer.Start();

            //add the relayJoinCode to an updated list - currently with the code i'm about to type - this only allows one room

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>{
                {
                    keyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
                }}
            });

            //this object that has the quick lobby manager needs to be on the same object that has the unitytransport component.
            GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(alloction, connectionType));
            NetworkManager.Singleton.StartHost();

        } catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async Task JoinLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            pollTimer.Start();

            string relayJoinCode = currentLobby.Data[keyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, connectionType));
            NetworkManager.Singleton.StartClient();


        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
    

    async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            return default;
        }
    }

    async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            return default;
        }
    }

    async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            return default;
        }
    }
    
    async Task HandleHeartBeatAsync()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            Debug.Log("Sent heartbeat ping to lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    async Task HandlePollUpdateAsync()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            Debug.Log("Polled updates on: " + lobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
