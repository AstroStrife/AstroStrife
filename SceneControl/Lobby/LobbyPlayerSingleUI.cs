using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyPlayerSingleUI : MonoBehaviour {

    public LobbyPlayerSingleUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI playerNameText;
    //[SerializeField] private Image characterImage;
    [SerializeField] private Button kickPlayerButton;
    [SerializeField] private Button SetToHostPlayerButton;

    private Player player;

    private void Awake() {
        Instance = this;
        
        kickPlayerButton.onClick.AddListener(() => {
          if (player != null) {
            LobbyManager.Instance.KickPlayer(player.Id);
        }
        });

        SetToHostPlayerButton.onClick.AddListener(() => {
            if (player != null) {
                LobbyManager.Instance.ChangeLobbyHost(player.Id);
            }
        });
    }

    public void SetKickPlayerButtonVisible(bool visible) {
        kickPlayerButton.gameObject.SetActive(visible);
    }
    public void SetToHostPlayerButtonVisible(bool visible) {
        SetToHostPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player) {
        this.player = player;
        playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        // LobbyManager.PlayerCharacter playerCharacter = 
        //     System.Enum.Parse<LobbyManager.PlayerCharacter>(player.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value);
        //characterImage.sprite = LobbyAssets.Instance.GetSprite(playerCharacter);
    }



}