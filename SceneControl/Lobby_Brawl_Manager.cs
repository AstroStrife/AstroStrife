using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby_Brawl_Manager : MonoBehaviour
{

    public static Lobby_Brawl_Manager Instance { get; private set; }
    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container_left;
    [SerializeField] private Transform container_right;
    [SerializeField] private TextMeshProUGUI joincode;
    [SerializeField] private Button SwitchBtn_Left_1;
    [SerializeField] private Button SwitchBtn_Left_2;
    [SerializeField] private Button SwitchBtn_Left_3;
    [SerializeField] private Button SwitchBtn_Left_4;
    [SerializeField] private Button SwitchBtn_Left_5;
    [SerializeField] private Button SwitchBtn_Right_1;
    [SerializeField] private Button SwitchBtn_Right_2;
    [SerializeField] private Button SwitchBtn_Right_3;
    [SerializeField] private Button SwitchBtn_Right_4;
    [SerializeField] private Button SwitchBtn_Right_5;
    [SerializeField] private Button backLobbyButton;
    [SerializeField] private Button matchingButton;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        //Hide();
        playerSingleTemplate.gameObject.SetActive(false);

        SwitchBtn_Left_1.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Top);
        });
        SwitchBtn_Left_2.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Top);
        });
        SwitchBtn_Left_3.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Top);
        });
        SwitchBtn_Left_4.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Top);
        });
        SwitchBtn_Left_5.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Top);
        });

        SwitchBtn_Right_1.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Bottom);
        });
        SwitchBtn_Right_2.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Bottom);
        });
        SwitchBtn_Right_3.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Bottom);
        });
        SwitchBtn_Right_4.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Bottom);
        });
        SwitchBtn_Right_5.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerTeam(LobbyManager.PlayerTeam.Bottom);
        });

        backLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            Hide();
            LobbyList.Instance.Show();
        });
        
        matchingButton.onClick.AddListener(() => {
            LobbyManager.Instance.OnCharacterSelectionShow();
            //CharacterSelection.Instance.UpdateTime();
        });
        DontDestroyOnLoad(gameObject);
    }
    private void Start() {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnGameEnd += LobbyManager_OnGameEnd;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnCharacterSelection += LobbyManager_OnCharacterSelection; 
        LobbyManager.Instance.OnBackLobby += LobbyManager_OnBackLobby; 
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted; 

        Hide();
    }
    private void LobbyManager_OnGameStarted(object sender, System.EventArgs e) {
        //SceneManager.LoadScene(1);
        loading_prepare_game.Instance.GetComponent<Canvas>().sortingOrder = 2;
        Hide();
    }
    private void LobbyManager_OnCharacterSelection(object sender, System.EventArgs e) {
        if(CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder != 2){
            CharacterSelection.Instance.UpdateSelectionContainer();
            CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder = 2;
            CharacterSelection.Instance.onShow = true;
        }
    }
    private void LobbyManager_OnBackLobby(object sender, System.EventArgs e) {
        if(CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder != 0){
            CharacterSelection.Instance.UpdateSelectionContainer();
            CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder = 0;
            CharacterSelection.Instance.onShow = false;
        }
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e) {
        ClearLobby();
        Hide();
        CharacterSelection.Instance.ClearLobby();
        loading_prepare_game.Instance.ClearLobby();
        CharacterSelection.Instance.Hide();
        loading_prepare_game.Instance.Hide();
        LobbyList.Instance.Show();
    }
    private void LobbyManager_OnGameEnd(object sender, System.EventArgs e) {
        ClearLobby();
        Hide();
        //CharacterSelection.Instance.ClearLobby();
        loading_prepare_game.Instance.ClearLobby();
        //CharacterSelection.Instance.Hide();
        loading_prepare_game.Instance.Hide();
        loading_prepare_game.Instance.GetComponent<Canvas>().sortingOrder = 0;
        // if(CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder != 0){
        //     CharacterSelection.Instance.GetComponent<Canvas>().sortingOrder = 0;
        //     CharacterSelection.Instance.onShow = false;
        //     CharacterSelection.Instance.Startgame = false;
        //     CharacterSelection.Instance.onCountdown = true;
        //     CharacterSelection.Instance.onReady = false;
        //     CharacterSelection.Instance.CheckReady = false;
        //     ShowConfirmCharacterUI.Instance.Hide();
        // }
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
        if(CharacterSelection.Instance != null){
            CharacterSelection.Instance.UpdateLobbyCharacterSelection(LobbyManager.Instance.GetJoinedLobby());
        }
        loading_prepare_game.Instance.UpdateLobbyCharacterSelection(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby) {
        ClearLobby();
        int num = 0;
        foreach (Player player in lobby.Players) {
            if(player.Data[LobbyManager.KEY_PLAYER_TEAM].Value == "Top"){
                Transform playerSingleTransform = Instantiate(playerSingleTemplate, container_left);
                playerSingleTransform.gameObject.SetActive(true);
                LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

                lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                    LobbyManager.Instance.IsLobbyHost() &&
                    player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
                );

                lobbyPlayerSingleUI.SetToHostPlayerButtonVisible(
                    LobbyManager.Instance.IsLobbyHost() &&
                    player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
                );
            
                lobbyPlayerSingleUI.UpdatePlayer(player);
            }else{
                Transform playerSingleTransform = Instantiate(playerSingleTemplate, container_right);
                playerSingleTransform.gameObject.SetActive(true);
                LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

                lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                    LobbyManager.Instance.IsLobbyHost() &&
                    player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
                );

                lobbyPlayerSingleUI.SetToHostPlayerButtonVisible(
                    LobbyManager.Instance.IsLobbyHost() &&
                    player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
                );
                lobbyPlayerSingleUI.UpdatePlayer(player);
            }
            
            if(player.Data[LobbyManager.KEY_PLAYER_STATUS].Value == "1"){

                num++;
            }
            
            matchingButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());

        }
   
        joincode.text = lobby.LobbyCode;
        Show();
    }

    private void ClearLobby() {
        foreach (Transform child in container_left) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in container_right) {
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





