using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using static ScoreManager;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class EndGameWindow : MonoBehaviour
{
    public static EndGameWindow Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI winLoseStatus;
    [SerializeField] private Button back_button;
    [SerializeField] private Button advance_button;
    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container_top;
    [SerializeField] private Transform container_bottom;
    [SerializeField] private GameObject Scoreboard;
    [SerializeField] private GameObject Advanceboard;
    public Advance_Graph advance_Graph;
    private List<ScoreEntry> scoreManager;
    public string winLoseStatus_text = "No Data";
    public string ownUsername = "No Data";
    

    private void Awake() {
        Instance = this;
        playerSingleTemplate.gameObject.SetActive(false);
        scoreManager = GameObject.Find("GameManager").GetComponent<ScoreManager>().scoreBoardEntries;
        advance_Graph = GameObject.Find("Player_UI").GetComponentInChildren<Advance_Graph>(true);
        Hide();
        back_button.onClick.AddListener(async () =>
        {
           
            await LobbyManager.Instance.ExitGame();
            //SceneManager.LoadScene(0);
            Application.Quit();
            ///NetworkManager.Singleton.SceneManager.LoadScene("GameManu", LoadSceneMode.Single);
        });
        advance_button.onClick.AddListener(() =>
        {
            if(Scoreboard.activeInHierarchy){
                Scoreboard.SetActive(false);
                Advanceboard.SetActive(true);
            }else{
                Scoreboard.SetActive(true);
                Advanceboard.SetActive(false);
            }
        });
        
        
    }
    
    public void SetWinLoseStatus(string status){
        winLoseStatus_text = status;
    }
    public void SetUsername(string name){
        ownUsername = name;
        advance_Graph.SetUsername(ownUsername, true);
    }

    public void UpdateData(Lobby lobby){
        
            ClearContainer();
            winLoseStatus.text = winLoseStatus_text;
            foreach (Player player in lobby.Players) {
                if(player.Data[LobbyManager.KEY_PLAYER_TEAM].Value == "Top"){
                    Transform playerSingleCardTransform = Instantiate(playerSingleTemplate, container_top);
                    playerSingleCardTransform.gameObject.SetActive(true);
                    EndGameWindow_PlayerSingleCard playerShipDriverSelection = playerSingleCardTransform.GetComponent<EndGameWindow_PlayerSingleCard>();
                    playerShipDriverSelection.UpdatePlayer(player , scoreManager);
                }else{
                    Transform playerSingleCardTransform = Instantiate(playerSingleTemplate, container_bottom);
                    playerSingleCardTransform.gameObject.SetActive(true);
                    EndGameWindow_PlayerSingleCard playerShipDriverSelection = playerSingleCardTransform.GetComponent<EndGameWindow_PlayerSingleCard>();
                    playerShipDriverSelection.UpdatePlayer(player , scoreManager);
                }
            }
            
        
    }

    public void ClearContainer() {
        
        foreach (Transform child in container_top) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in container_bottom) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
       
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
