using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    private bool SendData = false;
    // AfterMath ONLY part
    // public NetworkVariable<string> username = new NetworkVariable<string>("");
    // public NetworkVariable<string> userEmail = new NetworkVariable<string>("");
    public NetworkVariable<FixedString64Bytes> username =  new NetworkVariable<FixedString64Bytes>("name");
    public NetworkVariable<FixedString64Bytes> userEmail =  new NetworkVariable<FixedString64Bytes>("Email");
    public NetworkVariable<FixedString64Bytes> userTeam =  new NetworkVariable<FixedString64Bytes>("Team");
    public NetworkVariable<FixedString64Bytes> userDriver =  new NetworkVariable<FixedString64Bytes>("Driver");
    public NetworkVariable<FixedString64Bytes> userShip =  new NetworkVariable<FixedString64Bytes>("Ship");
    public NetworkVariable<int> totalGold = new NetworkVariable<int>(0);
    public NetworkVariable<float> totalPlayerDamage = new NetworkVariable<float>(0);
    public NetworkVariable<float> totalTurretDamage = new NetworkVariable<float>(0);
    public NetworkVariable<float> totalDamageReceived = new NetworkVariable<float>(0);

    // InGame & aftermath part
    public NetworkVariable<int> kills = new NetworkVariable<int>(0);
    public NetworkVariable<int> deaths = new NetworkVariable<int>(0);

    // Implement if have time
    // public NetworkVariable<int> assists = new NetworkVariable<int>(0);
    public NetworkVariable<int> laneMinionKills = new NetworkVariable<int>(0);
    public NetworkVariable<int> neutralBossKills = new NetworkVariable<int>(0);
    public NetworkVariable<int> neutralMinionKills = new NetworkVariable<int>(0);
    public NetworkVariable<int> totalMinionKills = new NetworkVariable<int>(0);

    public Advance_Graph advance_Graph;
    private bool AlreadyLog = false;

    public void SetUsername(string name)
    {
        if(IsServer){
            username.Value = name;
        }
    }
    public void SetEmail(string email)
    {
        if(IsServer){
            userEmail.Value = email;
        }
    }
    public void SetTeam(string team)
    {
        if(IsServer){
            userTeam.Value = team;
        }
    }
    public void SetUserDriver(string driver)
    {
        if(IsServer){
            userDriver.Value = driver;
        }
    }
    public void SetUserShip(string ship)
    {
        if(IsServer){
            userShip.Value = ship;
        }
    }
  

    public void IncreaseTotalGold(int amount)
    {
        if (IsServer)
        {
            totalGold.Value += amount;
        }
    }
    public void IncreaseTotalPlayerDamage(float amount)
    {
        if (IsServer)
        {
            totalPlayerDamage.Value += amount;
        }
    }
    public void IncreaseTotalTurretDamage(float amount)
    {
        if (IsServer)
        {
            totalTurretDamage.Value += amount;
        }
    }
    public void IncreaseTotalDamageReceived(float amount)
    {
        if (IsServer)
        {
            totalDamageReceived.Value += amount;
        }
    }
    public void IncreaseKills()
    {
        if (IsServer)
        {
            kills.Value += 1;
        }
    }
    public void IncreaseDeaths()
    {
        if (IsServer)
        {
            deaths.Value += 1;
        }
    }

    // Implement if have time
    /*public void IncreaseAssists()
    {
        if (IsServer)
        {
            assists.Value += 1;
        }
    }*/

    public void IncreaselaneMinionKills()
    {
        if (IsServer)
        {
            laneMinionKills.Value += 1;
        }
    }
    public void IncreaseNeutralBossKills()
    {
        if (IsServer)
        {
            neutralBossKills.Value += 1;
        }
    }
    public void IncreaseNeutralMinionKills()
    {
        if (IsServer)
        {
            neutralMinionKills.Value += 1;
        }
    }

    public void SendDataToServer()
    {
       if(IsOwner){
            ScoreManager.Instance.LogScoreServerRpc(username.Value.ToString(), userEmail.Value.ToString(), userTeam.Value.ToString(), userDriver.Value.ToString(), userShip.Value.ToString(), totalGold.Value, totalPlayerDamage.Value, totalTurretDamage.Value, totalDamageReceived.Value, kills.Value, deaths.Value, laneMinionKills.Value, neutralBossKills.Value, neutralMinionKills.Value, totalMinionKills.Value);
       }
    }

    private void Start() {
        advance_Graph = GameObject.Find("Player_UI").GetComponentInChildren<Advance_Graph>(true);
    }

    private void Update()
    {
        if (GameManager.Instance.GameEnd.Value == true && SendData == false)
        {
            
            if(IsOwner){
                SendDataToServer();
                SendData = true;
                Debug.Log("gameObject.tag : " + gameObject.tag);
                Debug.Log("GameWinStatus : " + GameManager.Instance.GameWinStatus.Value.ToString());
                if(gameObject.tag == "Top" && GameManager.Instance.GameWinStatus.Value.ToString() == "Top"){
                    EndGameWindow.Instance.SetWinLoseStatus("Victory");
                    WinLoseWindow.Instance.winLoseStatus.text = "Victory";
                }else if(gameObject.tag == "Top" && GameManager.Instance.GameWinStatus.Value.ToString() == "Draw"){
                    EndGameWindow.Instance.SetWinLoseStatus("Draw");
                    WinLoseWindow.Instance.winLoseStatus.text = "Draw";
                }else if(gameObject.tag == "Top" && GameManager.Instance.GameWinStatus.Value.ToString() == "Bottom"){
                    EndGameWindow.Instance.SetWinLoseStatus("Defect");
                    WinLoseWindow.Instance.winLoseStatus.text = "Defect";
                }else if(gameObject.tag == "Bottom" && GameManager.Instance.GameWinStatus.Value.ToString() == "Top"){
                    EndGameWindow.Instance.SetWinLoseStatus("Defect");
                    WinLoseWindow.Instance.winLoseStatus.text = "Defect";
                }else if(gameObject.tag == "Bottom" && GameManager.Instance.GameWinStatus.Value.ToString() == "Draw"){
                    EndGameWindow.Instance.SetWinLoseStatus("Draw");
                    WinLoseWindow.Instance.winLoseStatus.text = "Draw";
                }else if(gameObject.tag == "Bottom" && GameManager.Instance.GameWinStatus.Value.ToString() == "Bottom"){
                    EndGameWindow.Instance.SetWinLoseStatus("Victory");
                    WinLoseWindow.Instance.winLoseStatus.text = "Victory";
                }
                EndGameWindow.Instance.SetUsername(username.Value.ToString());   
                //advance_Graph.SetUsername(username.Value.ToString() , true);
                //GameObject.Find("Player_UI").GetComponentInChildren<Advance_Graph>().SetUsername(username.Value.ToString());
                //Advance_Graph.Instance.SetUsername(username.Value.ToString());
           
        }
    }
    

    if(Mathf.FloorToInt(TimeManager.Instance.currentTime.Value) % 60 == 0 && TimeManager.Instance.currentTime.Value != 0f && AlreadyLog == false){
        if(IsOwner){
            GameLogger.Instance.LogActionServerRpc(username.Value.ToString(), " Report Gold Per min : ", totalGold.Value.ToString());
            AlreadyLog = true;
        }
    }
    if(Mathf.FloorToInt(TimeManager.Instance.currentTime.Value) % 60 == 1){
        if(IsOwner){
            AlreadyLog = false;
        }
    }
}
}