using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class loading_player_card : MonoBehaviour {

    public loading_player_card Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI playerShip_loading_element;
    [SerializeField] private TextMeshProUGUI playerDriver_loading_element;
    [SerializeField] private TextMeshProUGUI name_loading_element;

    private Player player;

    private void Awake() {
        Instance = this;
        
    }

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerShip_loading_element.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
        playerDriver_loading_element.text = player.Data[LobbyManager.KEY_PLAYER_DRIVER].Value;
        name_loading_element.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        //playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
        // LobbyManager.PlayerCharacter playerCharacter = 
        //     System.Enum.Parse<LobbyManager.PlayerCharacter>(player.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value);
        //characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    }



}