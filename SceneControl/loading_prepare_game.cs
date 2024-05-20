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

public class loading_prepare_game : NetworkBehaviour
{
    public static loading_prepare_game Instance { get; private set; }

    [SerializeField] private Transform playerSingleLoadingTemplate;

    [SerializeField] private Transform container_top;
    [SerializeField] private Transform container_bottom;

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
        playerSingleLoadingTemplate.gameObject.SetActive(false);
        
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateLobbyCharacterSelection(Lobby lobby) {
        ClearLobby();

        foreach (Player player in lobby.Players) {
            if(player.Data[LobbyManager.KEY_PLAYER_TEAM].Value == "Top"){
                
                Transform playerSingleLoadingTransform = Instantiate(playerSingleLoadingTemplate, container_top);
                playerSingleLoadingTransform.gameObject.SetActive(true);
              
                loading_player_card loadingPlayerCard = playerSingleLoadingTransform.GetComponent<loading_player_card>();
                loadingPlayerCard.UpdatePlayer(player);
            }else{
                
                Transform playerSingleLoadingTransform = Instantiate(playerSingleLoadingTemplate, container_bottom);
                playerSingleLoadingTransform.gameObject.SetActive(true);
               
                loading_player_card loadingPlayerCard = playerSingleLoadingTransform.GetComponent<loading_player_card>();
                loadingPlayerCard.UpdatePlayer(player);
            }
            
        }

    
        Show();
    }

    public void ClearLobby() {
        foreach (Transform child in container_top) {
            if (child == playerSingleLoadingTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (Transform child in container_bottom) {
            if (child == playerSingleLoadingTemplate) continue;
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
