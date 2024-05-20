using Unity.Netcode;
using UnityEngine;

public class AbilityController : NetworkBehaviour
{
    [Header("UnitData")]
    public AbilityShipData AbilityShipData;
    // For Ultimate from driver
    public Ability DriverData;

    private InputManager _inputManager;
    private PlayerStatusController _playerStatus;
    private PlayerLevelController _playerLevel;
    private Skill_HUD _skillHUD;

    public float lastUsedTimeSkillQ = 0f;
    public float lastUsedTimeSkillShift = 0f;
    public float lastUsedTimeSkillE = 0f;
    public float lastUsedTimeSkillR = 0f;

    public override void OnNetworkSpawn()
    {
        _inputManager = GetComponent<InputManager>();
        _playerStatus = GetComponent<PlayerStatusController>();
        _playerLevel = GetComponent<PlayerLevelController>();
        _skillHUD = GetComponentInChildren<Skill_HUD>(true);
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleSkillUse();
        UpdateCooldownTimers();
    }
    void HandleSkillUse()
    {
        TryActivateSkill(ref lastUsedTimeSkillQ, _inputManager.QSkillUseInput, AbilityShipData.ability1, AbilityQServerRpc, _playerLevel.QskillLevel.Value);
        TryActivateSkill(ref lastUsedTimeSkillShift, _inputManager.ShiftSkillUseInput, AbilityShipData.ability2, AbilityShiftServerRpc, _playerLevel.ShiftSkillLevel.Value);
        TryActivateSkill(ref lastUsedTimeSkillE, _inputManager.ESkillUseInput, AbilityShipData.ability3, AbilityEServerRpc, _playerLevel.EskillLevel.Value);
        TryActivateSkill(ref lastUsedTimeSkillR, _inputManager.RSkillUseInput, DriverData, AbilityRServerRpc, _playerLevel.RskillLevel.Value);

        ResetInputFlags();
    }

    private void TryActivateSkill(ref float cooldownTimer, bool inputFlag, Ability ability, System.Action activationMethod, int skillLevel)
    {
        if (inputFlag && cooldownTimer <= 0 && _playerStatus.EP.Value >= ability.activationCost && skillLevel > 0)
        {
            activationMethod.Invoke();
            cooldownTimer = ability.cooldownTime;
            DeductEnergyServerRpc(ability.activationCost);
        }
    }

    private void ResetInputFlags()
    {
        _inputManager.QSkillUseInput = false;
        _inputManager.ShiftSkillUseInput = false;
        _inputManager.ESkillUseInput = false;
        _inputManager.RSkillUseInput = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeductEnergyServerRpc(float activationCost)
    {
        _playerStatus.EP.Value -= activationCost;
    }

    void UpdateCooldownTimers()
    {
        float hasteFactor = (100 - _playerStatus.CooldownHaste.Value) / 100f;
        hasteFactor = Mathf.Clamp(hasteFactor, 0f, 1f);

        lastUsedTimeSkillQ = Mathf.Max(0, lastUsedTimeSkillQ - Time.deltaTime * hasteFactor);
        lastUsedTimeSkillShift = Mathf.Max(0, lastUsedTimeSkillShift - Time.deltaTime * hasteFactor);
        lastUsedTimeSkillE = Mathf.Max(0, lastUsedTimeSkillE - Time.deltaTime * hasteFactor);
        lastUsedTimeSkillR = Mathf.Max(0, lastUsedTimeSkillR - Time.deltaTime * hasteFactor);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AbilityQServerRpc()
    {
        AbilityShipData.ability1.Activate(gameObject, _playerLevel.QskillLevel.Value);
    }
    [ServerRpc(RequireOwnership = false)]
    public void AbilityShiftServerRpc()
    {
        AbilityShipData.ability2.Activate(gameObject, _playerLevel.ShiftSkillLevel.Value);
    }
    [ServerRpc(RequireOwnership = false)]
    public void AbilityEServerRpc()
    {
        AbilityShipData.ability3.Activate(gameObject, _playerLevel.EskillLevel.Value);
    }
    [ServerRpc(RequireOwnership = false)]
    public void AbilityRServerRpc()
    {
        DriverData.Activate(gameObject, _playerLevel.RskillLevel.Value);
    }

    public void SetDriverData(string name)
    {
        // Get driver for player
        Ability driverAbility = DriverUltStore.Instance.GetDriverAbility(name);
        if (driverAbility != null)
        {
            DriverData = driverAbility;
        }

        if (!IsOwner) return;
        _skillHUD.SetIcon();
    }
}