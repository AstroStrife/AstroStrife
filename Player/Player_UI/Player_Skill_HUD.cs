using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_Skill_HUD : MonoBehaviour
{
    public TextMeshProUGUI NeededEXPText;
    public Button LevelUp;
    private GameObject _ownerPlayerPrefab;
    private PlayerStatusController _playerStatusController;
    private PlayerLevelController _playerLevelController;
    private AbilityController _abilityController;

    public TextMeshProUGUI RemainSkillPoint;

    [Header("Skill level")]
    public TextMeshProUGUI QskillLevel;
    public TextMeshProUGUI ShiftskillLevel;
    public TextMeshProUGUI EskillLevel;
    public TextMeshProUGUI RskillLevel;

    [Header("Skill Desc")]
    public TextMeshProUGUI QskillDesc;
    public TextMeshProUGUI ShiftskillDesc;
    public TextMeshProUGUI EskillDesc;
    public TextMeshProUGUI RskillDesc;

    [Header("Skill level up")]
    public Button QskillUp;
    public Button ShiftskillUp;
    public Button EskillUp;
    public Button RskillUp;

    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.gameObject;
        _playerStatusController = _ownerPlayerPrefab.GetComponent<PlayerStatusController>();
        _playerLevelController = _ownerPlayerPrefab.GetComponent<PlayerLevelController>();
        _abilityController = _ownerPlayerPrefab.GetComponent<AbilityController>();

        LevelUp.onClick.AddListener(TryLevelUp);
        QskillUp.onClick.AddListener(() => TrySkillUp("QSkill", 4));
        ShiftskillUp.onClick.AddListener(() => TrySkillUp("ShiftSkill", 4));
        EskillUp.onClick.AddListener(() => TrySkillUp("ESkill", 4));
        RskillUp.onClick.AddListener(() => TrySkillUp("RSkill", 3));
    }

    private void TryLevelUp()
    {
        if (_playerStatusController.OnBase.Value)
        {
            _playerLevelController.CheckLevelUpServerRpc();
        }
    }

    private void TrySkillUp(string skillName, int maxLevel)
    {
        if (_playerStatusController.OnBase.Value)
        {
            _playerLevelController.TrySkillUpServerRpc(skillName, maxLevel);
        }
    }

    private void Update()
    {
        NeededEXPText.text = "EXP Needed : " + _playerLevelController.experienceToNextLevel.Value.ToString();
        RemainSkillPoint.text = "Remaining skill point : " + _playerLevelController.SkillPoint.Value.ToString();

        QskillLevel.text = "Q Level : " + _playerLevelController.QskillLevel.Value.ToString();
        ShiftskillLevel.text = "Shift Level : " + _playerLevelController.ShiftSkillLevel.Value.ToString();
        EskillLevel.text = "E Level : " + _playerLevelController.EskillLevel.Value.ToString();
        RskillLevel.text = "R Level : " + _playerLevelController.RskillLevel.Value.ToString();


        QskillDesc.text = _abilityController.AbilityShipData.ability1.Description;
        ShiftskillDesc.text = _abilityController.AbilityShipData.ability2.Description;
        EskillDesc.text = _abilityController.AbilityShipData.ability3.Description;
        RskillDesc.text = _abilityController.DriverData.Description;
    }

    public void StateOn_Off(bool state)
    {
        gameObject.SetActive(state);
    }
}
