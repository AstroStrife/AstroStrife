using UnityEngine;
using UnityEngine.UI;

public class Skill_HUD : MonoBehaviour
{
    public GameObject _ownerPlayerPrefab;
    public AbilityController _abilityController;

    private bool IconUpdate = false;

    public Image QskillIcons;
    public Image ShiftSkillIcons;
    public Image EskillIcons;
    public Image RskillIcons;

    public Image QskillCooldownImage;
    public Image ShiftSkillCooldownImage;
    public Image EskillCooldownImage;
    public Image RskillCooldownImage;

    public Image DashCooldownImage;
    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.parent.gameObject;
        _abilityController = _ownerPlayerPrefab.GetComponent<AbilityController>();
        UpdateSkillIcons();
    }

    private void Update()
    {
        UpdateSkillIcons();
    }

    public void UpdateSkillIcons()
    {
        if (IconUpdate)
        {
            QskillCooldownImage.fillAmount = _abilityController.lastUsedTimeSkillQ / _abilityController.AbilityShipData.ability1.cooldownTime;
            ShiftSkillCooldownImage.fillAmount = _abilityController.lastUsedTimeSkillShift / _abilityController.AbilityShipData.ability2.cooldownTime;
            EskillCooldownImage.fillAmount = _abilityController.lastUsedTimeSkillE / _abilityController.AbilityShipData.ability3.cooldownTime;
            RskillCooldownImage.fillAmount = _abilityController.lastUsedTimeSkillR / _abilityController.DriverData.cooldownTime;
        }
    }

    public void SetIcon()
    {
        QskillIcons.sprite = _abilityController.AbilityShipData.ability1.abilityIcon;
        ShiftSkillIcons.sprite = _abilityController.AbilityShipData.ability2.abilityIcon;
        EskillIcons.sprite = _abilityController.AbilityShipData.ability3.abilityIcon;
        RskillIcons.sprite = _abilityController.DriverData.abilityIcon;
        IconUpdate = true;
    }

    public void UpdateDashCooldown(float fillAmount)
    {
        DashCooldownImage.fillAmount = fillAmount;
    }
}