using Unity.Netcode;
using UnityEngine;

public class KingOfTheHill : NetworkBehaviour
{
    public enum Team { None, Top, Bottom }

    public NetworkVariable<Team> captureTeam = new NetworkVariable<Team>(Team.None);

    public NetworkVariable<int> MaxScore = new NetworkVariable<int>(10);
    public NetworkVariable<float> ScoreUpdateTime = new NetworkVariable<float>(1);
    public NetworkVariable<float> RewardUpdateTime = new NetworkVariable<float>(3);

    public NetworkVariable<int> TopTeamScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> BottomTeamScore = new NetworkVariable<int>(0);

    public NetworkVariable<int> topTeamInZone = new NetworkVariable<int>(0);
    public NetworkVariable<int> bottomTeamInZone = new NetworkVariable<int>(0);

    public NetworkVariable<float> timeSinceLastScoreUpdate = new NetworkVariable<float>(0);
    public NetworkVariable<float> timeSinceLastRewardUpdate = new NetworkVariable<float>(0);

    public NetworkVariable<float> ResetUpdate = new NetworkVariable<float>(0);

    public MiniMapMarkHill MiniMapMarkHill { get; private set; }
    public HillPointBar HillPointBar { get; private set; }

    private Team lastCaptureTeam = Team.None;

    private void Start()
    {
        MiniMapMarkHill = GetComponentInChildren<MiniMapMarkHill>();
        HillPointBar = GetComponentInChildren<HillPointBar>();

        // Increase scale slightly at the start
        Transform objectTransform = transform;
        objectTransform.localScale += new Vector3(0.1f, 0.1f, 0.1f);

        InitializeUI();
    }

    private void InitializeUI()
    {
        HillPointBar.SetMaxSlider("Top", MaxScore.Value);
        HillPointBar.SetMaxSlider("Bottom", MaxScore.Value);
        HillPointBar.UpdateSlider("Top", TopTeamScore.Value);
        HillPointBar.UpdateSlider("Bottom", BottomTeamScore.Value);
    }

    private void Update()
    {
        if (!IsServer) return;

        HandleScoring();
        HandleReward();
        CheckCaptureStatus();
        HandleReset();
    }

    private void HandleScoring()
    {
        if (topTeamInZone.Value > 0 || bottomTeamInZone.Value > 0)
        {
            timeSinceLastScoreUpdate.Value += Time.deltaTime;
            if (timeSinceLastScoreUpdate.Value >= ScoreUpdateTime.Value)
            {
                if (IsOwner)
                {
                    GameLogger.Instance.LogActionServerRpc(gameObject.name, " currently being seized ", $"topTeamScore:{TopTeamScore.Value} bottomTeamScore:{BottomTeamScore.Value}");
                }

                UpdateScores();
                timeSinceLastScoreUpdate.Value = 0f;
            }
        }
        else
        {
            timeSinceLastScoreUpdate.Value = 0f;
        }
    }

    private void HandleReward()
    {
        if (captureTeam.Value != Team.None)
        {
            timeSinceLastRewardUpdate.Value += Time.deltaTime;
            if (timeSinceLastRewardUpdate.Value >= RewardUpdateTime.Value)
            {
                PerformCaptureActions();
                timeSinceLastRewardUpdate.Value = 0f;
            }
        }
    }

    private void HandleReset()
    {
        if (captureTeam.Value != Team.None)
        {
            if (timeSinceLastScoreUpdate.Value >= ScoreUpdateTime.Value)
            {
                topTeamInZone.Value = 0;
                bottomTeamInZone.Value = 0;
            }
        }
    }

    private void UpdateScores()
    {
        if (topTeamInZone.Value > bottomTeamInZone.Value && TopTeamScore.Value < MaxScore.Value)
        {
            BottomTeamScore.Value = Mathf.Max(0, BottomTeamScore.Value - 1);
            TopTeamScore.Value++;
        }
        else if (bottomTeamInZone.Value > topTeamInZone.Value && BottomTeamScore.Value < MaxScore.Value)
        {
            TopTeamScore.Value = Mathf.Max(0, TopTeamScore.Value - 1);
            BottomTeamScore.Value++;
        }

        UpdateSliderClientRpc();
        CheckAndLogTeamChange();
    }

    private void CheckAndLogTeamChange()
    {
        if (captureTeam.Value != lastCaptureTeam)
        {
            lastCaptureTeam = captureTeam.Value;

            if (IsOwner)
            {
                string capturingTeamName = captureTeam.Value.ToString();
                GameLogger.Instance.LogActionServerRpc(gameObject.name, " capture team changed to ", $"{capturingTeamName}, topTeamScore:{TopTeamScore.Value}, bottomTeamScore:{BottomTeamScore.Value}");
            }
        }
    }

    private void CheckCaptureStatus()
    {
        bool hasTopTeamWon = TopTeamScore.Value == MaxScore.Value;
        bool hasBottomTeamWon = BottomTeamScore.Value == MaxScore.Value;

        if (hasTopTeamWon || hasBottomTeamWon)
        {
            string winningTeam = hasTopTeamWon ? "Top" : "Bottom";
            captureTeam.Value = hasTopTeamWon ? Team.Top : Team.Bottom;
            changeTeamClientRpc(winningTeam);
        }
        else
        {
            ResetCaptureStatus();
        }
    }

    private void ResetCaptureStatus()
    {
        changeTeamClientRpc("None");
        captureTeam.Value = Team.None;
    }

    private void PerformCaptureActions()
    {
        string team = captureTeam.Value == Team.Top ? "Top" : "Bottom";
        if (captureTeam.Value != Team.None)
        {
            if (gameObject.name.Contains("Gold"))
            {
                GameManager.Instance.GiveGoldTeam(team, 5);
                GoldPopupClientRpc();
            }
            else
            {
                GameManager.Instance.GiveEXPTeam(team, 10);
                EXPPopupClientRpc();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerStatusController playerStatus = other.GetComponent<PlayerStatusController>();
        if (playerStatus != null)
        {
            if (other.gameObject.CompareTag("Top"))
            {
                topTeamInZone.Value++;
            }
            else if (other.gameObject.CompareTag("Bottom"))
            {
                bottomTeamInZone.Value++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerStatusController playerStatus = other.GetComponent<PlayerStatusController>();
        if (playerStatus != null)
        {
            if (other.gameObject.CompareTag("Top"))
            {
                topTeamInZone.Value = Mathf.Max(0, topTeamInZone.Value - 1);
            }
            else if (other.gameObject.CompareTag("Bottom"))
            {
                bottomTeamInZone.Value = Mathf.Max(0, bottomTeamInZone.Value - 1);
            }
        }
    }

    [ClientRpc]
    public void UpdateSliderClientRpc()
    {
        HillPointBar.UpdateSlider("Top", TopTeamScore.Value);
        HillPointBar.UpdateSlider("Bottom", BottomTeamScore.Value);
    }

    [ClientRpc]
    public void changeTeamClientRpc(string parentTag)
    {
        MiniMapMarkHill.parentTag = parentTag;
        MiniMapMarkHill.ChangeTeam(parentTag);
    }

    [ClientRpc]
    public void GoldPopupClientRpc()
    {
        HillPointBar.ShowGoldPopup(5);
    }

    [ClientRpc]
    public void EXPPopupClientRpc()
    {
        HillPointBar.ShowEXPPopup(10);
    }
}