using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class PlayerShipDriverSelection : MonoBehaviour {

    public PlayerShipDriverSelection Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerShip;
    [SerializeField] private TextMeshProUGUI playerDriver;

    private Player player;

    private void Awake() {
        Instance = this;
        
    }

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerName.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        playerShip.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
        playerDriver.text = player.Data[LobbyManager.KEY_PLAYER_DRIVER].Value;
        //playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
        // LobbyManager.PlayerCharacter playerCharacter = 
        //     System.Enum.Parse<LobbyManager.PlayerCharacter>(player.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value);
        //characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    }



}