using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Offensive/SetUpFort Ability")]
public class SetUpFort : Ability
{
    public float slowPercentage; // Percentage to reduce speed
    public float duration;       // Duration of the slow effect

    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerStatusController PlayerStatusController))
        {
            PlayerStatusController.ApplyDebuff(0, 0, slowPercentage, 0, 0, duration, "SetUpFort");
            PlayerStatusController.ApplyBuff(1.5f + ((float)SkillLevel * 0.5f), 0, 0, 0, 0, duration, "SetUpFort");
        }
    }
}