using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Netcode;



public class EndGameWindow_PlayerSingleCard : NetworkBehaviour 
    
 {

    public EndGameWindow_PlayerSingleCard Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI name_text;
    [SerializeField] private TextMeshProUGUI playerShip_text;
    [SerializeField] private TextMeshProUGUI playerDriver_text;
    [SerializeField] private TextMeshProUGUI kills_text;
    [SerializeField] private TextMeshProUGUI deaths_text;
    [SerializeField] private TextMeshProUGUI totalGold_text;
    [SerializeField] private TextMeshProUGUI totalDamage_text;

    private Player player;

    private void Awake() {
        Instance = this;
        
    }

    public void UpdatePlayer(Player player, List<ScoreManager.ScoreEntry> scoreManager) {
        this.player = player;
        name_text.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
        playerShip_text.text = player.Data[LobbyManager.KEY_PLAYER_SHIP].Value;
        playerDriver_text.text = player.Data[LobbyManager.KEY_PLAYER_DRIVER].Value;
        foreach(ScoreManager.ScoreEntry score in scoreManager){
            if(score.email == player.Data[LobbyManager.KEY_PLAYER_EMAIL].Value){
                kills_text.text = score.Kills.ToString();
                deaths_text.text = score.Deaths.ToString();
                totalGold_text.text = score.TotalGold.ToString();
                totalDamage_text.text = score.TotalPlayerDamage.ToString();
            }
        }
    }
}