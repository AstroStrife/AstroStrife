using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ShowConfirmCharacterUI : MonoBehaviour
{
    public static ShowConfirmCharacterUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI playerShip;
    [SerializeField] private TextMeshProUGUI playerDriver;
    public bool onSelectionShip = false;
    public bool onSelectionDriver = false;


    private void Awake() {
        Instance = this;
        Hide();
    }

    public void UpdatePlayerShip(string ship) {
        playerShip.text = ship;
        onSelectionShip = true;
    }
    public void UpdatePlayerDriver(string driver) {
        playerDriver.text = driver;
        onSelectionDriver = true;
    }
    // public void UpdatePlayer(Player player) {
    //     this.player = player;
    //     playerShip.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
    //     playerDriver.text = player.Data[LobbyManager.KEY_PLAYER_DRIVER].Value;
    //     //playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
    //     // LobbyManager.PlayerCharacter playerCharacter = 
    //     //     System.Enum.Parse<LobbyManager.PlayerCharacter>(player.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value);
    //     //characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    // }
    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
