using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class LogEntriesWrapper
{
    public List<LogEntry> logEntries;
}

[System.Serializable]
public class LogEntry : INetworkSerializable
{
    public string subject;
    public string action; // "Move", "Dash", "Attack", etc.
    public Vector3 position; // For movement and dash
    public string target; // For actions with a target
    public string timestamp;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref subject);
        serializer.SerializeValue(ref action);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref target);
        serializer.SerializeValue(ref timestamp);
    }
}


public class GameLogger : NetworkBehaviour
{
    private List<LogEntry> logEntries = new List<LogEntry>();
    private string logFilePath;
    public static GameLogger Instance { get; private set; }
    public TimeManager timeManager;

    private float movementLogInterval = 1.5f;
    private float nextMovementLogTime = 0f;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        // Define the log file path and name
        string fileName = $"GameLog_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    private void Update()
    {
        if (GameManager.Instance.GameEnd.Value == true)
        {
            if (IsServer)
            {
                SaveLogToFile();
            }
        }
    }

    public List<LogEntry> GetLog(){
        return logEntries;
    }

    public void LogAction(string subject, string action, string target)
    {
        LogEntry newEntry = new LogEntry
        {
            subject = subject,
            action = action,
            target = target,
            timestamp = timeManager.timerString
        };

        logEntries.Add(newEntry);
        LogActionClientRpc(newEntry);
    }
    [ClientRpc]
    private void LogActionClientRpc(LogEntry newEntry)
    { 
        if(!IsServer){
            logEntries.Add(newEntry);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LogActionServerRpc(string subject, string action, string target)
    {
        if(IsServer){
            LogAction(subject, action, target);
        }
    }

    public void LogMovement(string subject, Vector3 position)
    {
        if (Time.time >= nextMovementLogTime)
        {
            LogEntry movementEntry = new LogEntry
            {
                subject = subject,
                action = "Move",
                position = position,
                timestamp = timeManager.timerString
            };

            logEntries.Add(movementEntry);
            nextMovementLogTime = Time.time + movementLogInterval;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LogMovementServerRpc(string subject, Vector3 position)
    {
        LogMovement(subject, position);
    }

    public void LogDash(string subject, Vector3 toPosition)
    {
        LogEntry dashEntry = new LogEntry
        {
            subject = subject,
            action = "Dash",
            position = toPosition,
            timestamp = timeManager.timerString
        };

        logEntries.Add(dashEntry);
    }
    [ServerRpc(RequireOwnership = false)]
    public void LogDashServerRpc(string subject, Vector3 toPosition)
    {
        LogDash(subject, toPosition);
    }

    private void SaveLogToFile()
    {
        LogEntriesWrapper wrapper = new LogEntriesWrapper { logEntries = this.logEntries };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(logFilePath, json);
    }
}