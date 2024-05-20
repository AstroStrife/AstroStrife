using System;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    public class ScoreBoardWrapper
    {
        public List<ScoreEntry> ScoreBoardEntries = new List<ScoreEntry>();
    }

    [System.Serializable]
    public class ScoreEntry : INetworkSerializable
    {
        public string username;
        public string email;
        public string team;
        public string driver;
        public string ship;
        public int TotalGold;
        public float TotalPlayerDamage;
        public float TotalTurretDamage;
        public float TotalDamageReceived;
        public int Kills;
        public int Deaths;
        public int LaneMinionKills;
        public int NeutralBossKills;
        public int NeutralMinionKills;
        public int TotalMinionKills;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref email);
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref driver);
        serializer.SerializeValue(ref ship);
        serializer.SerializeValue(ref TotalGold);
        serializer.SerializeValue(ref TotalPlayerDamage);
        serializer.SerializeValue(ref TotalTurretDamage);
        serializer.SerializeValue(ref TotalDamageReceived);
        serializer.SerializeValue(ref Kills);
        serializer.SerializeValue(ref Deaths);
        serializer.SerializeValue(ref LaneMinionKills);
        serializer.SerializeValue(ref NeutralBossKills);
        serializer.SerializeValue(ref NeutralMinionKills);
        serializer.SerializeValue(ref TotalMinionKills);
    }

    }

    public List<ScoreEntry> scoreBoardEntries = new List<ScoreEntry>();
    private string logFilePath;

    private bool statusCreateLog = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Define the log file path and name
        string fileName = $"ScoreBoard_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public void LogScore(string name , string email ,string team ,string driver ,string ship , int gold, float playerDamage, float turretDamage, float damageReceived, int kills, int deaths, int laneMinions, int bossKills, int neutralMinions, int totalMinions)
    {
        ScoreEntry newEntry = new ScoreEntry
        {
            username = name,
            email = email,
            team = team,
            driver = driver,
            ship = ship,
            TotalGold = gold,
            TotalPlayerDamage = playerDamage,
            TotalTurretDamage = turretDamage,
            TotalDamageReceived = damageReceived,
            Kills = kills,
            Deaths = deaths,
            LaneMinionKills = laneMinions,
            NeutralBossKills = bossKills,
            NeutralMinionKills = neutralMinions,
            TotalMinionKills = totalMinions
        };

        scoreBoardEntries.Add(newEntry);
    
        LogScoreClientRpc(newEntry);
        
    }

    [ClientRpc]
    private void LogScoreClientRpc(ScoreEntry newEntry)
    { 
        if(!IsServer){
            scoreBoardEntries.Add(newEntry);
        }
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void LogScoreServerRpc(string name, string email, string team, string driver, string ship, int gold, float playerDamage, float turretDamage, float damageReceived, int kills, int deaths, int laneMinions, int bossKills, int neutralMinions, int totalMinions)
    {
        if(IsServer){
            LogScore(name, email, team, driver, ship, gold, playerDamage, turretDamage, damageReceived, kills, deaths, laneMinions, bossKills, neutralMinions, totalMinions);
        }
    }


    private void SaveLogToFile()
    {
        if(IsServer){
            ScoreBoardWrapper wrapper = new ScoreBoardWrapper { ScoreBoardEntries = this.scoreBoardEntries };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(logFilePath, json);
        }
        if(IsServer){
            AuthManager.Instance.StoreHistories($"ScoreBoard_{System.DateTime.Now:yyyyMMdd_HHmmss}" , scoreBoardEntries , GameObject.Find("Player_UI").GetComponent<TimeManager>().currentTime.Value.ToString(), GameObject.Find("GameManager").GetComponent<GameManager>().GameWinStatus.Value.ToString()  );
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameEnd.Value == true && statusCreateLog == false)
        {
            SaveLogToFile();
            statusCreateLog = true;
        }
    }
}