using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance { get; private set; }

    public NetworkList<PlayerData> playerDataNetworkList;
    private int numOfSpawnPointTop;
    private int numOfSpawnPointBottom;

    //[SerializeField] private Transform playerPrefab_1;
    [SerializeField] private Transform UtilityShipPrefab_red;
    [SerializeField] private Transform UtilityShipPrefab_blue;
    [SerializeField] private Transform OffensiveShipPrefab_red;
    [SerializeField] private Transform OffensiveShipPrefab_blue;
    [SerializeField] private Transform DefensiveShipPrefab_red;
    [SerializeField] private Transform DefensiveShipPrefab_blue;
    public List<Transform> spawnPoint;

    public PlayerData _playerdata;

    public NetworkVariable<bool> GameEnd = new NetworkVariable<bool>(false);
    public NetworkVariable<FixedString64Bytes> GameWinStatus = new NetworkVariable<FixedString64Bytes>("");
    public GameObject TopBase;
    public GameObject BottomBase;

    [SerializeField] private GameObject runePrefab;

    private PoolManager poolManager; // Reference to the PoolManager
    public TimeManager timeManager;

    public Transform MinionTopLeft; // Spawn point for Minion left top team
    public Transform MinionTopRight; // Spawn point for Minion right top team
    public Transform MinionBottomLeft; // Spawn point for Minion left bottom team
    public Transform MinionBottomRight; // Spawn point for Minion right bottom team

    public Transform BottomRightRune;
    public Transform BottomLeftRune;
    public Transform TopRightRune;
    public Transform TopLeftRune;

    public List<ulong> PlayerTop;
    public List<ulong> PlayerBottom;

    public GameObject DropdownSelectionGraph;

    public bool hasBeenCalled = false;
    public bool doneAddOption = false;
    public bool setStateEnd = false;

    // Minion level
    public NetworkVariable<int> MinionLevel = new NetworkVariable<int>(1);

    private void Awake()
    {

        Instance = this;


        playerDataNetworkList = new NetworkList<PlayerData>();
        ;
        numOfSpawnPointTop = 5;
        numOfSpawnPointBottom = 0;
    }

    private void Start()
    {
        poolManager = PoolManager.Instance;
    }

    private void Update()
    {
        
        if (IsServer && GameEnd.Value == false)
        {
            if (TopBase.activeSelf == false || BottomBase.activeSelf == false || Input.GetKeyDown(KeyCode.P))
            {
                if (TopBase.activeSelf == false)
                {
                    GameWinStatus.Value = "Bottom";
                }
                if (BottomBase.activeSelf == false)
                {
                    GameWinStatus.Value = "Top";
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    GameWinStatus.Value = "Draw";
                }
                GameEnd.Value = true;
            }

            int CheckSpawnTime = Mathf.FloorToInt(timeManager.currentTime.Value % 30f);
            
            if (CheckSpawnTime == 0 && !hasBeenCalled && timeManager.currentTime.Value > 1)
            {
                // Every 2 minute minion get level up
                if (Mathf.FloorToInt(timeManager.currentTime.Value % 120f) == 0 && MinionLevel.Value < 15)
                {
                    MinionLevel.Value += 1;
                }
                StartCoroutine(SpawnTopMinion());
                StartCoroutine(SpawnBottomMinion());
                SpawnRandomRuneServerRpc();
                if (IsOwner)
                {
                    // Spawn minion log
                    GameLogger.Instance.LogActionServerRpc("Game", " Spawn ", "Minion wave and rune");
                }
                hasBeenCalled = true;
            }
            else if (CheckSpawnTime != 0)
            {
                hasBeenCalled = false;
            }
        }
        if (GameEnd.Value == true)
        {
            if(doneAddOption == false){
                foreach(PlayerData playerData in playerDataNetworkList){
                    Debug.Log("Add player to dropdown : " + playerData.playerName.ToString());
                    DropdownSelectionGraph.GetComponent<DropdownSelection>().AddOption(playerData.playerName.ToString());
                }
                doneAddOption = true;
            }
            if(setStateEnd == false){
                this.gameObject.GetComponent<GameStateManager>().SetGameEnding();
                setStateEnd = true;
            }
            
            
            
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn");
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("NetworkManager.Singleton.IsServer : " + NetworkManager.Singleton.IsServer);
            //NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            //NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "Main-Game-Map") return;
        Debug.Log("On load Complete : " + sceneName);
        numOfSpawnPointTop = 5;
        numOfSpawnPointBottom = 0;

        Transform x = GameObject.Find("BottomSpawnPoint1").transform;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            int numOfSpawnPoint;

            if (LobbyManager.Instance.GetJoinedLobby().Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_TEAM].Value == LobbyManager.PlayerTeam.Top.ToString())
            {
                numOfSpawnPoint = numOfSpawnPointTop;
                numOfSpawnPointTop++;
            }
            else
            {
                numOfSpawnPoint = numOfSpawnPointBottom;
                numOfSpawnPointBottom++;
            }

            playerDataNetworkList.Add(new PlayerData
            {
                clientId = clientId,
                noOfSpawnPoint = numOfSpawnPoint,
                playerTeam = LobbyManager.Instance.joinedLobby.Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_TEAM].Value,
                playerDriver = LobbyManager.Instance.joinedLobby.Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_DRIVER].Value,
                playerShip = LobbyManager.Instance.joinedLobby.Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_SHIP].Value,
                playerName = LobbyManager.Instance.joinedLobby.Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_NAME].Value,
                playerEmail = LobbyManager.Instance.joinedLobby.Players[(int)clientId].Data[LobbyManager.KEY_PLAYER_EMAIL].Value,
            });

            for (int i = 0; i < playerDataNetworkList.Count; i++)
            {
                PlayerData playerData = playerDataNetworkList[i];
                if (playerData.clientId == clientId)
                {
                    _playerdata = playerData;
                    Debug.Log("playerData.playerShip : " + playerData.playerShip);
                    Transform playerTransform;
                    if (playerData.playerShip == LobbyManager.PlayerShip.Offensive.ToString())
                    {
                        if (playerData.playerTeam == LobbyManager.PlayerTeam.Top.ToString())
                        {
                            playerTransform = Instantiate(OffensiveShipPrefab_red, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                        else
                        {
                            playerTransform = Instantiate(OffensiveShipPrefab_blue, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                    }
                    else if (playerData.playerShip == LobbyManager.PlayerShip.Defensive.ToString())
                    {
                        if (playerData.playerTeam == LobbyManager.PlayerTeam.Top.ToString())
                        {
                            playerTransform = Instantiate(DefensiveShipPrefab_red, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                        else
                        {
                            playerTransform = Instantiate(DefensiveShipPrefab_blue, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                        //}else if(playerData.playerShip == LobbyManager.PlayerShip.Utility.ToString()){
                    }
                    else
                    {
                        if (playerData.playerTeam == LobbyManager.PlayerTeam.Top.ToString())
                        {
                            playerTransform = Instantiate(UtilityShipPrefab_red, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                        else
                        {
                            playerTransform = Instantiate(UtilityShipPrefab_blue, spawnPoint[playerData.noOfSpawnPoint]);
                        }
                    }
                    Debug.Log("Instantiated position: " + playerTransform.position);
                    playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);


                    if (playerData.playerTeam == "Top")
                    {

                        playerTransform.GetComponent<PlayerStatusController>().SetTeam(true);
                    }

                    playerTransform.GetComponent<PlayerStatusController>().SetPlayerData(playerData);

                    Debug.Log("After spawn position: " + playerTransform.position);
                }
            }
        }
    }


    [ServerRpc]
    public void SpawnRandomRuneServerRpc()
    {
        List<Transform> runeSpots = new List<Transform> { BottomRightRune, BottomLeftRune, TopRightRune, TopLeftRune };

        foreach (Transform spot in runeSpots)
        {
            if (spot.GetComponentInChildren<Rune>() == null)
            {
                GameObject runeObject = Instantiate(runePrefab, spot.position, Quaternion.identity);

                Rune newRune = runeObject.GetComponent<Rune>();

                newRune.runeType = (RuneType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(RuneType)).Length);

                newRune.GetComponent<NetworkObject>().Spawn();

                runeObject.transform.SetParent(spot.transform, true);
            }
        }
    }

    public IEnumerator SpawnTopMinion()
    {
        SpawnObjectServerRpc("Top", "MeleeMinion", MinionTopLeft.position, MinionTopLeft.rotation);
        SpawnObjectServerRpc("Top", "MeleeMinion", MinionTopRight.position, MinionTopRight.rotation);
        yield return new WaitForSeconds(1);

        SpawnObjectServerRpc("Top", "RangeMinion", MinionTopLeft.position, MinionTopLeft.rotation);
        SpawnObjectServerRpc("Top", "RangeMinion", MinionTopRight.position, MinionTopRight.rotation);
        yield return new WaitForSeconds(1);

        SpawnObjectServerRpc("Top", "TankMinion", MinionTopLeft.position, MinionTopLeft.rotation);
        SpawnObjectServerRpc("Top", "TankMinion", MinionTopRight.position, MinionTopRight.rotation);
    }

    public IEnumerator SpawnBottomMinion()
    {
        SpawnObjectServerRpc("Bottom", "MeleeMinion", MinionBottomLeft.position, MinionBottomLeft.rotation);
        SpawnObjectServerRpc("Bottom", "MeleeMinion", MinionBottomRight.position, MinionBottomRight.rotation);
        yield return new WaitForSeconds(1);

        SpawnObjectServerRpc("Bottom", "RangeMinion", MinionBottomLeft.position, MinionBottomLeft.rotation);
        SpawnObjectServerRpc("Bottom", "RangeMinion", MinionBottomRight.position, MinionBottomRight.rotation);
        yield return new WaitForSeconds(1);

        SpawnObjectServerRpc("Bottom", "TankMinion", MinionBottomLeft.position, MinionBottomLeft.rotation);
        SpawnObjectServerRpc("Bottom", "TankMinion", MinionBottomRight.position, MinionBottomRight.rotation);
    }

    [ServerRpc]
    public void SpawnObjectServerRpc(string teamTag, string typeTag, Vector3 position, Quaternion rotation)
    {
        poolManager.RequestObjectFromPoolServerRpc(teamTag, typeTag, position, rotation);
        // Customize the object's properties based on the team
    }

    public List<GameObject> GetNearbyTeammates(Vector3 position, float radius, List<ulong> teamList)
    {
        List<GameObject> nearbyTeammates = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);
        foreach (var hitCollider in hitColliders)
        {
            NetworkObject networkObject = hitCollider.gameObject.GetComponent<NetworkObject>();
            PlayerStatusController PlayerStatus = hitCollider.gameObject.GetComponent<PlayerStatusController>();
            if (networkObject != null && PlayerStatus != null)
            {
                ulong playerID = networkObject.NetworkObjectId;
                if (teamList.Contains(playerID))
                {
                    nearbyTeammates.Add(hitCollider.gameObject);
                }
            }
        }
        return nearbyTeammates;
    }
    public GameObject GetPlayerFromPoolByNetworkObjectId(string tag, ulong killerID)
    {
        if (IsServer)
        {
            foreach (var playerObject in poolManager.poolPlayer[tag])
            {
                var networkObject = playerObject.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.NetworkObjectId == killerID)
                {
                    return playerObject;
                }
            }
            return null;
        }
        return null;
    }

    public GameObject GetObjectFromPoolByNetworkObjectId(string tag, ulong killerID)
    {
        if (IsServer)
        {
            foreach (var GameObject in poolManager.poolTeam[tag])
            {
                var networkObject = GameObject.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.NetworkObjectId == killerID)
                {
                    return GameObject;
                }
            }
            return null;
        }
        return null;
    }

    public void HandleKill(ulong killerID, int MoneyValue, int EXPValue, string type, Transform DeadTransform)
    {
        GameObject shooter = GetPlayerFromPoolByNetworkObjectId("Player", killerID);
        // Reward the player
        if (shooter != null)
        {
            var playerStatusController = shooter.GetComponent<PlayerStatusController>();
            var playerLevelController = shooter.GetComponent<PlayerLevelController>();
            var playerScore = shooter.GetComponent<PlayerScore>();

            if (IsOwner)
            {
                GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " kill: ", DeadTransform.gameObject.name);
                // Get money and exp log
                GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " Get Money: ", MoneyValue.ToString());
                GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " Get EXP: ", EXPValue.ToString());
            }

            playerStatusController?.AwardMoneyServerRpc(MoneyValue);
            playerLevelController?.AddExperienceServerRpc(EXPValue);

            switch (type)
            {
                case "Player": playerScore?.IncreaseKills(); break;
                case "LaneMinion": playerScore?.IncreaselaneMinionKills(); break;
                case "OffMinion": playerScore?.IncreaseNeutralMinionKills(); break;
                case "Boss": playerScore?.IncreaseNeutralBossKills(); break;
            }

            // Determine the team of the killer
            List<ulong> killerTeamList = PlayerTop.Contains(killerID) ? PlayerTop : PlayerBottom;

            List<GameObject> nearbyTeammates = GetNearbyTeammates(DeadTransform.position, 100f, killerTeamList);
            // Award EXP to nearby teammates
            if (nearbyTeammates.Count > 1)
            {
                int divisor = nearbyTeammates.Count - 1;
                if (divisor > 0) // Ensure divisor is greater than 0 to avoid division by zero
                {
                    int sharedEXP = EXPValue / divisor;
                    foreach (GameObject nearbyTeammate in nearbyTeammates)
                    {
                        if (nearbyTeammate != shooter)
                        {
                            nearbyTeammate.GetComponent<PlayerLevelController>()?.AddExperienceServerRpc(sharedEXP);
                            if (IsOwner)
                            {
                                GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " Get Shared EXP: ", sharedEXP.ToString());
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (GetObjectFromPoolByNetworkObjectId("OffLane", killerID) == null)
            {
                var teamList = GetObjectFromPoolByNetworkObjectId("Top", killerID) != null ? PlayerTop : PlayerBottom;
                List<GameObject> nearbyTeammates = GetNearbyTeammates(DeadTransform.position, 100f, teamList);
                if (nearbyTeammates.Count > 0)
                {
                    int sharedEXP = EXPValue / 2 / nearbyTeammates.Count;
                    foreach (GameObject teammate in nearbyTeammates)
                    {
                        teammate.GetComponent<PlayerLevelController>()?.AddExperienceServerRpc(sharedEXP);
                        if (IsOwner)
                        {
                            GameLogger.Instance.LogActionServerRpc(teammate.GetComponent<PlayerStatusController>().PlayerData.Value.playerName.ToString(), " Get Shared EXP: ", sharedEXP.ToString());
                        }
                    }
                }
            }
        }
    }

    public void GiveGoldTeam(string team, int amount)
    {
        if (!IsServer) return;
        List<ulong> selectedTeamList = new List<ulong>();
        if (team == "Top")
        {
            selectedTeamList = PlayerTop;
        }
        else if (team == "Bottom")
        {
            selectedTeamList = PlayerBottom;
        }
        for (int i = 0; i < selectedTeamList.Count; i++)
        {
            GameObject Player = GetPlayerFromPoolByNetworkObjectId("Player", selectedTeamList[i]);
            Player.GetComponent<PlayerStatusController>().AwardMoneyServerRpc(amount);
        }
        if (IsOwner)
        {
            // Capture log
            GameLogger.Instance.LogActionServerRpc("Every player in: " + team + " team", " Get Money: ", "5");
        }
    }

    public void GiveEXPTeam(string team, int amount)
    {
        if (!IsServer) return;
        List<ulong> selectedTeamList = new List<ulong>();
        if (team == "Top")
        {
            selectedTeamList = PlayerTop;
        }
        else if (team == "Bottom")
        {
            selectedTeamList = PlayerBottom;
        }
        foreach (var playerObject in selectedTeamList)
        {
        }
        for (int i = 0; i < selectedTeamList.Count; i++)
        {
            GameObject Player = GetPlayerFromPoolByNetworkObjectId("Player", selectedTeamList[i]);
            Player.GetComponent<PlayerLevelController>().AddExperienceServerRpc(amount);
        }
        if (IsOwner)
        {
            // Capture log
            GameLogger.Instance.LogActionServerRpc("Every player in: " + team + " team", " Get EXP: ", "10");
        }
    }

    public void GiveBossBotBuffTeam(ulong killerID)
    {
        if (!IsServer) return;
        List<ulong> selectedTeamList = new List<ulong>();
        if (PlayerTop.Contains(killerID))
        {
            selectedTeamList = PlayerTop;
            if (IsOwner)
            {
                // Boss buff log
                GameLogger.Instance.LogActionServerRpc("Top team", " Get ", "Boss Buff");
            }
        }
        else if (PlayerBottom.Contains(killerID))
        {
            selectedTeamList = PlayerBottom;
            if (IsOwner)
            {
                // Boss buff log
                GameLogger.Instance.LogActionServerRpc("Bottom team", " Get ", "Boss Buff");
            }
        }

        for (int i = 0; i < selectedTeamList.Count; i++)
        {
            GameObject Player = GetPlayerFromPoolByNetworkObjectId("Player", selectedTeamList[i]);
            PlayerStatusController playerStatusController = Player.GetComponent<PlayerStatusController>();
            playerStatusController.ApplyBuff(0, 0, 0, 0, 2, 80f, "BossBottomBuff");
        }
    }

    public void GiveBossTopBuffTeam(ulong killerID)
    {
        if (!IsServer) return;
        List<ulong> selectedTeamList = new List<ulong>();
        if (PlayerTop.Contains(killerID))
        {
            selectedTeamList = PlayerTop;
            if (IsOwner)
            {
                // Boss buff log
                GameLogger.Instance.LogActionServerRpc("Top team", " Get ", "Boss Buff");
            }
        }
        else if (PlayerBottom.Contains(killerID))
        {
            selectedTeamList = PlayerBottom;
            if (IsOwner)
            {
                // Boss buff log
                GameLogger.Instance.LogActionServerRpc("Bottom team", " Get ", "Boss Buff");
            }
        }

        for (int i = 0; i < selectedTeamList.Count; i++)
        {
            GameObject Player = GetPlayerFromPoolByNetworkObjectId("Player", selectedTeamList[i]);
            PlayerStatusController playerStatusController = Player.GetComponent<PlayerStatusController>();
            playerStatusController.ApplyBuff(3, 0, 0, 0, 0, 80f, "BossTopBuff");
        }
    }
}