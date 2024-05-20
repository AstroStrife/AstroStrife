using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplay;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_EMAIL = "Email";
    public const string KEY_PLAYER_STATUS = "ReadyStatus";
    public const string KEY_PLAYER_SHIP = "SHIP";
    public const string KEY_PLAYER_DRIVER = "DRIVER";
    public const string KEY_PLAYER_TEAM = "Bottom";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_START_GAME = "1";
    public const string KEY_START_CHARACTER_SELECTION = "2";
    

    public event EventHandler OnLeftLobby;
    public event EventHandler OnGameStarted;
    public event EventHandler OnGameEnd;
    public event EventHandler OnCharacterSelection;
    public event EventHandler OnBackLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnGameEndLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    private bool IsInLobby;

    public enum GameMode
    {
        BrawlMode,
        Mini1,
        Mini2
    }

    public enum PlayerTeam
    {
        Bottom,
        Top
    }

    public enum PlayerShip
    {
        Offensive,
        Defensive,
        Utility
    }

    public enum PlayerDriver
    {
        Nova,
        Ahriman,
        Zeus,
        Menhit,
        Soteria
    }

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float refreshLobbyListTimer = 5f;

    public List<PlayerData> playerData;

    private bool IsStartGame = false;
    private bool IsNetworkStart = false;


    private float autoAllocateTimer = 9999999f;
    private bool alreadyAutoAllocated;
    private static IServerQueryHandler serverQueryHandler;


    public Lobby joinedLobby;
    private string playerName;
    private string playerEmail = "email@test.com";
    //public bool InternetConnection;

    private string relayCode;


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
        DontDestroyOnLoad(gameObject);

// #if DEDICATED_SERVER
//         InitializeUnityAuthenticationForServer();
// #endif


    }

    // private async void InitializeUnityAuthenticationForServer() {
    //     if (UnityServices.State != ServicesInitializationState.Initialized) {
    //         InitializationOptions initializationOptions = new InitializationOptions();
    //         //initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());

    //         await UnityServices.InitializeAsync(initializationOptions);


    //         Debug.Log("DEDICATED_SERVER LOBBY");

    //         MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
    //         multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
    //         multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
    //         multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
    //         multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
    //         IServerEvents serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

    //         serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(4, "MyServerName", "KitchenChaos", "1.0", "Default");

    //         var serverConfig = MultiplayService.Instance.ServerConfig;
    //         if (serverConfig.AllocationId != "") {
    //             // Already Allocated
    //             MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
    //         }
    //     } else {
    //         // Already Initialized
    //         Debug.Log("DEDICATED_SERVER LOBBY - ALREADY INIT");

    //         var serverConfig = MultiplayService.Instance.ServerConfig;
    //         if (serverConfig.AllocationId != "") {
    //             // Already Allocated
    //             MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
    //         }

    //     }
    // }

    // private void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj) {
    //     Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Allocate");

    //     if (alreadyAutoAllocated) {
    //         Debug.Log("Already auto allocated!");
    //         return;
    //     }

    //     alreadyAutoAllocated = true;

    //     var serverConfig = MultiplayService.Instance.ServerConfig;
    //     Debug.Log($"Server ID[{serverConfig.ServerId}]");
    //     Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
    //     Debug.Log($"Port[{serverConfig.Port}]");
    //     Debug.Log($"QueryPort[{serverConfig.QueryPort}]");
    //     Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

    //     string ipv4Address = "0.0.0.0";
    //     ushort port = serverConfig.Port;
    //     NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, "0.0.0.0");

    //     NetworkManager.Singleton.StartServer();
        
    // }

    // private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj) {
    //     Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Deallocate");
    // }

    // private void MultiplayEventCallbacks_Error(MultiplayError obj) {
    //     Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Error");
    //     Debug.Log(obj.Reason);
    // }

    // private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj) {
    //     Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_SubscriptionStateChanged");
    //     Debug.Log(obj);
    // }

    private void Update()
    {
        //HandleRefreshLobbyList();
        HandleLobbyHeartbeat();
        HandleLobbyPolling();

    }
    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        try
        {
            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () =>
            {
                // do nothing
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerName);
                RefreshLobbyList();
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //InternetConnection = true;
        }
        catch
        {
            //InternetConnection = false;
        }
    }
    public void SetEmail(string email){
        this.playerEmail = email;
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
      
        if (joinedLobby != null && IsStartGame == false)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    joinedLobby = null;
                }

                if (joinedLobby.Data[KEY_START_CHARACTER_SELECTION].Value != "0" && IsNetworkStart == false)
                {   

                    OnCharacterSelection?.Invoke(this, EventArgs.Empty);
                    //OnCharacterSelectionHide();
                    if (!IsLobbyHost())
                    {
                        JoinRelay(joinedLobby.Data[KEY_START_CHARACTER_SELECTION].Value, this.playerName);

                    }
                    IsNetworkStart = true;

                }
                if (joinedLobby.Data[KEY_START_CHARACTER_SELECTION].Value == "5")
                {   

                    OnBackLobby?.Invoke(this, EventArgs.Empty);

                }

                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    Debug.Log("In HandleLobbyPolling");

                    
                    // SceneManager.LoadScene(1);
                    //SceneManager.LoadSceneAsync("Main-Game-Map", LoadSceneMode.Additive);
                    if (IsLobbyHost())
                    {
                        CharacterSelection.Instance.StartGame();
                    }

                    OnGameStarted?.Invoke(this, EventArgs.Empty);

                    IsStartGame = true;
                }

            }
        }
        
    }

    private void HandleRefreshLobbyList() {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn) {
            refreshLobbyListTimer -= Time.deltaTime;
            if (refreshLobbyListTimer < 0f) {
                float refreshLobbyListTimerMax = 5f;
                refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList();
            }
        }
    }

    public async void RefreshLobbyList() {
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S2,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void BrawlLobbyList() {
        try {

            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: "BrawlMode"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S2,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
            Debug.Log(lobbyListQueryResponse);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
    public async void Mini1LobbyList() {
        try {

            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: "Mini1"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S2,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void Mini2LobbyList() {
        try {

            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: "Mini2"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S2,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    public Player GetPlayer(){
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) },
            { KEY_PLAYER_EMAIL, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerEmail) },
            { KEY_PLAYER_STATUS, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0") },
            { KEY_PLAYER_TEAM, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerTeam.Top.ToString()) },
            { KEY_PLAYER_SHIP, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "") },
            { KEY_PLAYER_DRIVER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "") }
            });
    }

    public async void ChangeLobbyHost(string id){
        Debug.Log("Target ID : " + id);
        Debug.Log("Host ID : " + joinedLobby.HostId);

        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                HostId=id
            });
        joinedLobby = lobby;
        Debug.Log("Host ID Now : " + joinedLobby.HostId);
    }

    public async void OnCharacterSelectionShow(){
        Debug.Log(joinedLobby);
        relayCode = await CreateRelay(this.playerName);
        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    IsPrivate = true,
                    Data = new Dictionary<string, DataObject>{
                        {KEY_START_CHARACTER_SELECTION, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                        
                    }
                });
                joinedLobby = lobby;
        
        Debug.Log(joinedLobby);
    }
    
    public async void OnBackLobbyShow(){
        Debug.Log(joinedLobby);
        relayCode = await CreateRelay(this.playerName);
        Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    IsPrivate = true,
                    Data = new Dictionary<string, DataObject>{
                        {KEY_START_CHARACTER_SELECTION, new DataObject(DataObject.VisibilityOptions.Member, "5")}
                    }
                });
                joinedLobby = lobby;
        Debug.Log(joinedLobby);
    }

    public async void UpdatePlayerStatus() {
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_STATUS, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: "1")
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                //Debug.Log("OwnerClientId : " + CharacterSelection.Instance.OwnerClientId);

                //GameMultiplayerManager.Instance.AddPlayerDataNetworkList(CharacterSelection.Instance.OwnerClientId);

                //Debug.Log("End Add");
                
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }
    public async void UpdatePlayerToDefault() {
        if (joinedLobby != null) {
            try {
            foreach(Player player in joinedLobby.Players){
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_SHIP, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: "")
                    },
                    {
                        KEY_PLAYER_DRIVER, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: "")
                    },
                    {
                        KEY_PLAYER_STATUS, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: "0")
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;
            }
                

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerTeam(PlayerTeam team) {
        if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_TEAM, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: team.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerShip(string ship) {
         if (joinedLobby != null) {
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_SHIP, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: ship.ToString())
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerDriver(string driver) {
        if (joinedLobby != null) {
       
            try {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_DRIVER, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: driver.ToString())
                    }
                };
        
                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
                joinedLobby = lobby;

              

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
  
    }
    
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, GameMode gameMode)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject> {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString(), index: DataObject.IndexOptions.S1) },
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0", index: DataObject.IndexOptions.S2)},
                { KEY_START_CHARACTER_SELECTION, new DataObject(DataObject.VisibilityOptions.Member, "0")}
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        Debug.Log("Created Lobby " + lobby.Name + " : " + lobby.LobbyCode);

        IsInLobby = true;

    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        Debug.Log(lobbyCode);

        Player player = GetPlayer();
        Debug.Log(player);
        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
        {
            Player = player
        });

        int count = 0;
        foreach(Player playerinlobby in lobby.Players){
            if(playerinlobby.Data[KEY_PLAYER_TEAM].Value == "Top"){
                count++;
            }
        }
        if(count > 5){
            UpdatePlayerTeam(PlayerTeam.Bottom);
        }

        Debug.Log(lobby);
        joinedLobby = lobby;
        
        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        IsInLobby = true;
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();

        int count = 0;
        foreach(Player playerinlobby in lobby.Players){
            Debug.Log(playerinlobby.Data[KEY_PLAYER_TEAM].Value);
            if(playerinlobby.Data[KEY_PLAYER_TEAM].Value == "Top"){
                count++;
            }
        }
        Debug.Log(count);
        if(count >= 1){
            UpdatePlayerTeam(PlayerTeam.Bottom);
        }

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
        {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        IsInLobby = true;
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        Debug.Log("Lobbies found : " + queryResponse.Results.Count);

        IsInLobby = false;
    }

    public async Task ExitGame() {
        
        if (joinedLobby != null) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                joinedLobby = null;
                OnGameEnd?.Invoke(this, EventArgs.Empty);

                NetworkManager.Singleton.Shutdown();
            }
            catch (RelayServiceException e) {
                Debug.Log(e);
            }
        }

        IsInLobby = false;
        IsNetworkStart = false;
        IsStartGame = false;

    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        IsInLobby = false;
    }

    public async void StartGame()
    {
        Debug.Log("Start");
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("StartGame");

                if (joinedLobby != null && joinedLobby.Players != null)
                {
                    foreach (ulong player in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        // Assuming the player's name is stored in the KEY_PLAYER_NAME attribute
                        // If there are other details you want to log, you can fetch them similarly
                        
                        Debug.Log(player);
                        
                    }
                }

                //string relayCode = await CreateRelay(this.playerName);

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>{
                        {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "2")}

                    }
                });
                

                
                joinedLobby = lobby;
                Debug.Log("KEY_START_GAME : "+ joinedLobby.Data[KEY_START_GAME].Value);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public async Task<string> CreateRelay(string name)
    {
        Debug.Log("CreateRelay");
        Debug.Log(name);
        //SceneManager.LoadScene(1);
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(10);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("This is IP address : " + joinCode);
            // IP.text = "IP : " + joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }


    public async void JoinRelay(string joinCode, string name)
    {
        Debug.Log("JoinRelay");
        Debug.Log(name);

        try
        {
            Debug.Log("Joining Relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            //SceneManager.LoadScene(1);

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

}