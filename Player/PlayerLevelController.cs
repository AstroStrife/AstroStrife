using Unity.Netcode;

public class PlayerLevelController : NetworkBehaviour
{
    public NetworkVariable<int> Level = new NetworkVariable<int>(1);
    public NetworkVariable<int> Experience = new NetworkVariable<int>(0);
    public NetworkVariable<int> SkillPoint = new NetworkVariable<int>(1);

    public NetworkVariable<int> QskillLevel = new NetworkVariable<int>(0);
    public NetworkVariable<int> ShiftSkillLevel = new NetworkVariable<int>(0);
    public NetworkVariable<int> EskillLevel = new NetworkVariable<int>(0);
    public NetworkVariable<int> RskillLevel = new NetworkVariable<int>(0);

    public NetworkVariable<int> experienceToNextLevel = new NetworkVariable<int>(500);

    private PlayerStatusController _playerStatusController;
    private HealthBar _healthbar;

    public override void OnNetworkSpawn()
    {
        _playerStatusController = GetComponent<PlayerStatusController>();
        _healthbar = GetComponentInChildren<HealthBar>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddExperienceServerRpc(int amount)
    {
        if (IsServer)
        {
            Experience.Value += amount;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckLevelUpServerRpc()
    {
        if (Experience.Value >= experienceToNextLevel.Value && Level.Value < 15)
        {
            Experience.Value -= experienceToNextLevel.Value;
            UpdateLevelClientRpc();
            Level.Value++;
            OnLevelUp();

            // increase experienceToNextLevel based on new level
            experienceToNextLevel.Value += 250;
        }
    }

    private void OnLevelUp()
    {
        // Implement what happens on level up, e.g., increase stats
        _playerStatusController.MaxHP.Value += _playerStatusController.GrowthMaxHealthPoint.Value;
        _playerStatusController.HPRegen.Value += _playerStatusController.GrowthHealthPointRegeneration.Value;
        _playerStatusController.Defense.Value += _playerStatusController.GrowthDefense.Value;
        _playerStatusController.AttackDam.Value += _playerStatusController.GrowthAttackDamage.Value;
        _playerStatusController.MaxEP.Value += _playerStatusController.GrowthEnergyPoint.Value;
        _playerStatusController.EPRegen.Value += _playerStatusController.GrowthEnergyPointRegeneration.Value;

        SkillPoint.Value += 1;
        UpdateMaxSliderClientRpc();
    }

    [ClientRpc]
    private void UpdateMaxSliderClientRpc()
    {
        _healthbar.SetMaxSlider(_playerStatusController.MaxHP.Value);
    }

    [ClientRpc]
    private void UpdateLevelClientRpc()
    {
        _healthbar.UpdateLevel(Level.Value + 1);
    }

    // Upgrade Stat
    [ServerRpc(RequireOwnership = false)]
    public void TrySkillUpServerRpc(string skillName, int maxLevel)
    {
        if (SkillPoint.Value <= 0) return;

        switch (skillName)
        {
            case "QSkill":
                if (QskillLevel.Value < maxLevel)
                {
                    QskillLevel.Value++;
                    SkillPoint.Value--;
                }
                break;
            case "ShiftSkill":
                if (ShiftSkillLevel.Value < maxLevel)
                {
                    ShiftSkillLevel.Value++;
                    SkillPoint.Value--;
                }
                break;
            case "ESkill":
                if (EskillLevel.Value < maxLevel)
                {
                    EskillLevel.Value++;
                    SkillPoint.Value--;
                }
                break;
            case "RSkill":
                if (RskillLevel.Value < maxLevel && (Level.Value >= 6 || Level.Value >= 12))
                {
                    if ((RskillLevel.Value < 1 && Level.Value >= 6) || (RskillLevel.Value < 2 && Level.Value >= 12))
                    {
                        RskillLevel.Value++;
                        SkillPoint.Value--;
                    }
                }
                break;
        }
    }
}