using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyList : MonoBehaviour {


    public static LobbyList Instance { get; private set; }



    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button brawlButton;
    [SerializeField] private Button mini1Button;
    [SerializeField] private Button mini2Button;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField Code_Inputfield;



    private void Awake() {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        Hide();
        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.onClick.AddListener(RefreshButtonClick);

        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);

        brawlButton.onClick.AddListener(BrawlButtonClick);

        mini1Button.onClick.AddListener(Mini1ButtonClick);

        mini2Button.onClick.AddListener(Mini2ButtonClick);

        backButton.onClick.AddListener(BackButtonClick);

        Code_Inputfield.onSubmit.AddListener((value) => {
            LobbyManager.Instance.JoinLobbyByCode(value);
            Hide();
            Lobby_Brawl_Manager.Instance.Show();
            CharacterSelection.Instance.Show();
            loading_prepare_game.Instance.Show();
        });
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void BrawlButtonClick(){
        LobbyManager.Instance.BrawlLobbyList();
    }

    private void Mini1ButtonClick(){
        LobbyManager.Instance.Mini1LobbyList();
    }

    private void Mini2ButtonClick(){
        LobbyManager.Instance.Mini2LobbyList();
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick() {
        LobbyCreateUI.Instance.Show();
    }
    private void BackButtonClick() {
        MainManu.Instance.Show();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

}