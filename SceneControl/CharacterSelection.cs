using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelection : NetworkBehaviour
{
    public static CharacterSelection Instance { get; private set; }

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container_left;
    [SerializeField] private Transform container_right;
    [SerializeField] private Button shipCharacterSingle;
    [SerializeField] private Button driverCharacterSingle;
    [SerializeField] private Transform container_ship_selection;
    [SerializeField] private Transform container_driver_selection;
    [SerializeField] private List<string> ListOfShip;
    [SerializeField] private List<string> ListOfDriver;
    [SerializeField] public Button ConfirmBtn;
    [SerializeField] private TextMeshProUGUI timerText;
    public NetworkVariable<float> currentTime = new NetworkVariable<float>(60.0f);
    public bool onShow = false;
    public bool onCountdown = true;
    public bool Startgame = false;
    public bool onReady = false;
    public bool CheckReady = false;
    public bool SelectionContainer = false;
    private ulong localClientId;
    

    private void Awake() {
       
        Instance = this;
        

        localClientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log(localClientId);

        Hide();
        playerSingleTemplate.gameObject.SetActive(false);
        shipCharacterSingle.gameObject.SetActive(false);
        driverCharacterSingle.gameObject.SetActive(false);
        ConfirmBtn.gameObject.SetActive(false);
        ConfirmBtn.onClick.AddListener(() => {
            ShowConfirmCharacterUI.Instance.Show();
            LobbyManager.Instance.UpdatePlayerStatus();
            onReady = true;
            
        });
       
    }
    private void Update() {
        UpdateTimer();

        if(ShowConfirmCharacterUI.Instance.onSelectionDriver == true && ShowConfirmCharacterUI.Instance.onSelectionShip == true && onReady == false){
            ConfirmBtn.gameObject.SetActive(true);
        }else{
            ConfirmBtn.gameObject.SetActive(false);
        }
        
        

    }
    

    private void UpdateTimer()
    {
        if (IsServer)
        {
            
            if(onShow){
                if(currentTime.Value > 0.5f){
                    currentTime.Value -= Time.deltaTime;
                }
                if(CheckPlayerReady(LobbyManager.Instance.GetJoinedLobby()) && CheckReady == false){
                    currentTime.Value = 10;
                    onCountdown = false;
                    CheckReady = true;
                }
                if (currentTime.Value <= 0.5f && Startgame == false)
                {   
                    if(onCountdown){
                        onCountdown = false;
                        CheckLastPlayerReady(LobbyManager.Instance.GetJoinedLobby());
                        currentTime.Value = 10.0f;
                    }else{
                        if(LobbyManager.Instance.IsLobbyHost()){
                            
                            LobbyManager.Instance.StartGame();
                            Startgame = true; 
                        }
                    }
            
                }
            }
            UpdateTimerText();
                
        }else{
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        int seconds = Mathf.FloorToInt(currentTime.Value % 60);
        string timerString = string.Format("{00}", seconds);

        // Update the Text UI element.
        timerText.text = timerString;
    }

    private bool CheckPlayerReady(Lobby lobby){
        foreach (Player player in lobby.Players) {
            if(player.Data[LobbyManager.KEY_PLAYER_STATUS].Value == "0"){
                return false;
            }
        }
        return true;
    }

    private void CheckLastPlayerReady(Lobby lobby){
        foreach (Player player in lobby.Players) {
            if(player.Data[LobbyManager.KEY_PLAYER_STATUS].Value == "0"){
                if(player.Data[LobbyManager.KEY_PLAYER_DRIVER].Value != "" && player.Data[LobbyManager.KEY_PLAYER_SHIP].Value != ""){
                    ShowConfirmCharacterUI.Instance.Show();
                    //ConfirmBtn.gameObject.SetActive(false);
                    LobbyManager.Instance.UpdatePlayerStatus();
                }else{
           
                    currentTime.Value = 60f;
                    onShow = false;
                    GetComponent<Canvas>().sortingOrder = 0;
                    foreach (Player _player in lobby.Players) {
                        LobbyManager.Instance.UpdatePlayerToDefault();
                    }
                    LobbyManager.Instance.OnBackLobbyShow();
                }
                
            }
        }
    }

    public void UpdateLobbyCharacterSelection(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            if(player.Data[LobbyManager.KEY_PLAYER_TEAM].Value == "Top"){
                Transform playerSingleTransform = Instantiate(playerSingleTemplate, container_left);
              
                playerSingleTransform.gameObject.SetActive(true);
                PlayerShipDriverSelection playerShipDriverSelection = playerSingleTransform.GetComponent<PlayerShipDriverSelection>();
                playerShipDriverSelection.UpdatePlayer(player);
                
            }else{
                Transform playerSingleTransform = Instantiate(playerSingleTemplate, container_right);
           
                playerSingleTransform.gameObject.SetActive(true);
                PlayerShipDriverSelection playerShipDriverSelection = playerSingleTransform.GetComponent<PlayerShipDriverSelection>();
                playerShipDriverSelection.UpdatePlayer(player);
                
            }
            
        }

    
        Show();
    }

    public void ClearLobby() {
        foreach (Transform child in container_left) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in container_right) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
       
    }

    public void ClearSelectionContainer() {
      
        foreach (Transform child in container_ship_selection) {
            if (child == shipCharacterSingle) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in container_driver_selection) {
            if (child == driverCharacterSingle) continue;
            Destroy(child.gameObject);
        }
       
    }
    

    public void UpdateSelectionContainer(){
        //ClearSelectionContainer();
        if(SelectionContainer == false){
            foreach (string ship in ListOfShip) {
        
                Button shipSingleTransform = Instantiate(shipCharacterSingle, container_ship_selection);
                shipSingleTransform.gameObject.SetActive(true);
                shipSingleTransform.GetComponent<shipCharacterSingle>().SetName(ship);
                //shipSingleTransform.setName(ship);
            }

            foreach (string driver in ListOfDriver) {
            
                    Button driverSingleTransform = Instantiate(driverCharacterSingle, container_driver_selection);
                    driverSingleTransform.gameObject.SetActive(true);
                    driverSingleTransform.GetComponent<driverCharacterSingle>().SetName(driver);
                    //shipSingleTransform.setName(ship);
            }
            SelectionContainer = true;
        }

        
    

    }

    public void StartGame(){
        NetworkManager.Singleton.SceneManager.LoadScene("Main-Game-Map", LoadSceneMode.Single);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
