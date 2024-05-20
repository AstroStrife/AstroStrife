using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player_Stat_HUD : MonoBehaviour
{
    private GameObject _ownerPlayerPrefab;
    private PlayerStatusController _playerStatusController;

    public TextMeshProUGUI StatUpgradable;

    public TextMeshProUGUI CurrentMaxHP;
    public TextMeshProUGUI CurrentMaxEP;
    public TextMeshProUGUI CurrentDefense;
    public TextMeshProUGUI CurrentAttackDam;
    public TextMeshProUGUI CurrentMovementSpeed;
    public TextMeshProUGUI CurrentCooldownHaste;

    public Button MaxHP;
    public Button MaxEP;
    public Button Defense;
    public Button AttackDam;
    public Button MovementSpeed;
    public Button CooldownHaste;

    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.gameObject;
        _playerStatusController = _ownerPlayerPrefab.GetComponent<PlayerStatusController>();

        AddClickListener(MaxHP, "MaxHP");
        AddClickListener(MaxEP, "MaxEP");
        AddClickListener(Defense, "Defense");
        AddClickListener(AttackDam, "AttackDam");
        AddClickListener(MovementSpeed, "MovementSpeed");
        AddClickListener(CooldownHaste, "CooldownHaste");
    }

    private void AddClickListener(Button button, string statName)
    {
        button.onClick.AddListener(() =>
        {
            if (_playerStatusController.OnBase.Value == true)
            {
                _playerStatusController.CheckBuyStatServerRpc(statName);
            }
        });
    }

    private void Update()
    {
        CurrentMaxHP.text = "Current Max HP : " + _playerStatusController.MaxHP.Value;
        CurrentMaxEP.text = "Current Max EP : " + _playerStatusController.MaxEP.Value;
        CurrentDefense.text = "Current Defense : " + _playerStatusController.Defense.Value;
        CurrentAttackDam.text = "Current Attack : " + _playerStatusController.AttackDam.Value;
        CurrentMovementSpeed.text = "Current Speed : " + _playerStatusController.MovementSpeed.Value;
        CurrentCooldownHaste.text = "Current Skill haste : " + _playerStatusController.CooldownHaste.Value;
        StatUpgradable.text = _playerStatusController.StatUpgradable.Value + " Upgrade remain";
    }

    public void StateOn_Off(bool state)
    {
        gameObject.SetActive(state);
    }
}
