using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System;

public class LobbyCreateUI : MonoBehaviour {


    public static LobbyCreateUI Instance { get; private set; }


    [SerializeField] private Button createButton;
    [SerializeField] private Button lobbyNameButton;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private Button gameModeButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI publicPrivateText;
    [SerializeField] private TextMeshProUGUI gameModeText;


    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;
    
    private LobbyManager.GameMode gameMode;

    private void Awake() {
        Instance = this;

        createButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(
                lobbyName,
                10,
                isPrivate,
                gameMode
            );
            //CreateServer();
            //GetServer();
            Hide();
        });


        publicPrivateButton.onClick.AddListener(() => {
            isPrivate = !isPrivate;
            UpdateText();
        });

        gameModeButton.onClick.AddListener(() => {
            switch (gameMode) {
                default:
                case LobbyManager.GameMode.BrawlMode:
                    gameMode = LobbyManager.GameMode.Mini1;
                    break;
                case LobbyManager.GameMode.Mini1:
                    gameMode = LobbyManager.GameMode.Mini2;
                    break;
                case LobbyManager.GameMode.Mini2:
                    gameMode = LobbyManager.GameMode.BrawlMode;
                    break;
            }
            UpdateText();
        });

        backButton.onClick.AddListener(() => {
            Hide();
            LobbyList.Instance.Show();
        });

        Hide();

    }

//     private void CreateServer(){
//             string keyId = "fbf8dcb0-4a52-4df9-a451-57d1fdfbaac8";
//             string keySecret = "BaeLHC091aN4X2vyMAZj6FZbW7CHcuNO";
//             byte[] keyByteArray = Encoding.UTF8.GetBytes(keyId + ":" + keySecret);
//             string keyBase64 = Convert.ToBase64String(keyByteArray);

//             string projectId = "b5c52581-2c5e-4686-b0c0-3f4ef3762f56";
//             string environmentId = "9fc276d1-c30b-4526-90a0-e1cd8c137c54";
//             string url = $"https://services.api.unity.com/auth/v1/token-exchange?projectId={projectId}&environmentId={environmentId}";

//             string jsonRequestBody = JsonUtility.ToJson(new TokenExchangeRequest {
//                 scopes = new[] { "multiplay.allocations.create", "multiplay.allocations.list" , "multiplay.allocations.get"},
//             });

//             Debug.Log("jsonRequestBody : " + jsonRequestBody);

//             WebRequests.PostJson(url,
//             (UnityWebRequest unityWebRequest) => {
//                 unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBase64);
//             },
//             jsonRequestBody,
//             (string error) => {
//                 Debug.Log("Error: " + error);
//             },
//             (string json) => {
//                 Debug.Log("Success: " + json);
//                 TokenExchangeResponse tokenExchangeResponse = JsonUtility.FromJson<TokenExchangeResponse>(json);

//                 Debug.Log("tokenExchangeResponse : " + tokenExchangeResponse.accessToken);

//                 string fleetId = "97241482-9c6c-4ebc-ac85-978681adc98e";
//                 string url = $"https://multiplay.services.api.unity.com/v1/allocations/projects/{projectId}/environments/{environmentId}/fleets/{fleetId}/allocations";

//                 string jsonRequestBody = JsonUtility.ToJson(new QueueAllocationRequest {
//                         allocationId = "d743d643-7ca5-4692-bc71-7e4b7921d3f8",
//                         buildConfigurationId = 1251869,
//                         regionId = "9106c77a-29ab-43ee-83f9-bbb02fc2d42a",
//                 });

//                 WebRequests.PostJson(url,
//                 (UnityWebRequest unityWebRequest) => {
//                     unityWebRequest.SetRequestHeader("Authorization", "Bearer " + tokenExchangeResponse.accessToken);
//                     Debug.Log("Request URL: " + unityWebRequest.url);
//                     Debug.Log("Request Headers: " + unityWebRequest.GetRequestHeader("Authorization"));
//                 },
//                 jsonRequestBody,
//                 (string error) => {
//                     Debug.Log("Error: " + error);
//                 },
//                 (string json) => {
//                     Debug.Log("Response: " + json);

//                     string allocationId = "d743d643-7ca5-4692-bc71-7e4b7921d3f8";

//                     string url = $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectId}/environments/{environmentId}/fleets/{fleetId}/allocations/{allocationId}";

//                     WebRequests.Get(url,
//                     (UnityWebRequest unityWebRequest) => {
//                         unityWebRequest.SetRequestHeader("Authorization", "Bearer " + tokenExchangeResponse.accessToken);
//                         Debug.Log("Request URL: " + unityWebRequest.url);
//                         Debug.Log("Request Headers: " + unityWebRequest.GetRequestHeader("Authorization"));
//                      },
//                      (string error) => {
//                          Debug.Log("Error: " + error);
//                      },
//                      (string json) => {
//                          Debug.Log("Success: " + json);
                        
//                      }     
//                              );
//                         }
//                         );
//             }
//             );
//     }

//     private void GetServer(){
//         string keyId = "fbf8dcb0-4a52-4df9-a451-57d1fdfbaac8";
//         string keySecret = "BaeLHC091aN4X2vyMAZj6FZbW7CHcuNO";
//         byte[] keyByteArray = Encoding.UTF8.GetBytes(keyId + ":" + keySecret);
//         string keyBase64 = Convert.ToBase64String(keyByteArray);

//         string projectId = "AAAAAAAAAAAAAAA";
//         string environmentId = "AAAAAAAAAAAAAAAAAAAAA";
//         string url = $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectId}/environments/{environmentId}/servers";

//         WebRequests.Get(url,
//         (UnityWebRequest unityWebRequest) => {
//             unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBase64);
//         },
//         (string error) => {
//             Debug.Log("Error: " + error);
//         },
//         (string json) => {
//             Debug.Log("Success: " + json);
            
//         }
//         );
//     }

    
// #if !DEDICATED_SERVER
//     private void start(){
//         GetServer();
//     }
// #endif

    private void UpdateText() {
        lobbyNameText.text = lobbyName;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        gameModeText.text = gameMode.ToString();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);

        lobbyName = "MyLobby";
        isPrivate = false;
        gameMode = LobbyManager.GameMode.BrawlMode;

        UpdateText();
    }

    public class TokenExchangeResponse {
        public string accessToken;
    }


    [Serializable]
    public class TokenExchangeRequest {
        public string[] scopes;
    }

    [Serializable]
    public class QueueAllocationRequest {
        public string allocationId;
        public int buildConfigurationId;
        public string payload;
        public string regionId;
        public bool restart;
    }


    private enum ServerStatus {
        AVAILABLE,
        ONLINE,
        ALLOCATED
    }

    [Serializable]
    public class ListServers {
        public Server[] serverList;
    }

    [Serializable]
    public class Server {
        public int buildConfigurationID;
        public string buildConfigurationName;
        public string buildName;
        public bool deleted;
        public string fleetID;
        public string fleetName;
        public string hardwareType;
        public int id;
        public string ip;
        public int locationID;
        public string locationName;
        public int machineID;
        public int port;
        public string status;
    }

}