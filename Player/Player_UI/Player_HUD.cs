using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_HUD : MonoBehaviour

{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI EnergyText;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI EXPText;

    private GameObject _ownerPlayerPrefab;

    public Button OpenStore;
    public Button OpenStat;
    public Button OpenSkill;

    private bool isStoreOpen = false;
    private bool isStatOpen = false;
    private bool isSkillOpen = false;

    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.gameObject;

        OpenStore.onClick.AddListener(() => ChangeState(ref isStoreOpen, true, false, false));
        OpenStat.onClick.AddListener(() => ChangeState(ref isStatOpen, false, true, false));
        OpenSkill.onClick.AddListener(() => ChangeState(ref isSkillOpen, false, false, true));
    }

    private void ChangeState(ref bool currentState, bool isStore, bool isStat, bool isSkill)
    {
        if (currentState)
        {
            // If the current state is already open, close it
            currentState = false;

            var store = _ownerPlayerPrefab.GetComponentInChildren<Player_Store>(true);
            var stat = _ownerPlayerPrefab.GetComponentInChildren<Player_Stat_HUD>(true);
            var skill = _ownerPlayerPrefab.GetComponentInChildren<Player_Skill_HUD>(true);

            if (isStore)
            {
                store.StateOn_Off(false);
            }
            else if (isStat)
            {
                stat.StateOn_Off(false);
            }
            else if (isSkill)
            {
                skill.StateOn_Off(false);
            }
        }
        else
        {
            // If the current state is closed, open it and close others
            currentState = true;

            var store = _ownerPlayerPrefab.GetComponentInChildren<Player_Store>(true);
            var stat = _ownerPlayerPrefab.GetComponentInChildren<Player_Stat_HUD>(true);
            var skill = _ownerPlayerPrefab.GetComponentInChildren<Player_Skill_HUD>(true);

            store.StateOn_Off(isStore);
            stat.StateOn_Off(isStat);
            skill.StateOn_Off(isSkill);
        }
    }

    private void Update()
    {
        healthText.text = "HP : " + _ownerPlayerPrefab.GetComponent<PlayerStatusController>().HP.Value.ToString("0");
        EnergyText.text = "EP : " + _ownerPlayerPrefab.GetComponent<PlayerStatusController>().EP.Value.ToString("0");
        MoneyText.text = "Money : " + _ownerPlayerPrefab.GetComponent<PlayerStatusController>().Money.Value.ToString();
        LevelText.text = "Level : " + _ownerPlayerPrefab.GetComponent<PlayerLevelController>().Level.Value.ToString();
        EXPText.text = "EXP : " + _ownerPlayerPrefab.GetComponent<PlayerLevelController>().Experience.Value.ToString();
    }
}