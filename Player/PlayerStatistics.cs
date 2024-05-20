using UnityEngine;

public class PlayerStatistics : MonoBehaviour
{
    // AfterMath ONLY part
    private int totalGold;
    private float totalPlayerDamage;
    private float totalTurretDamage;
    private float totalDamageReceived;

    // InGame & aftermath part
    private int currentGold;
    private int kills;
    private int deaths;
    private int assists;
    private int laneMinionKills;
    private int neutralBossKills;
    private int neutralMinionKills;
    private int totalMinionKills;
}
