using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Defense/PlasmaShield Ability")]
public class PlasmaShield : Ability
{
    public float DefUp;
    public float duration;

    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerStatusController PlayerStatusController))
        {
            PlayerStatusController.ApplyBuff(0, DefUp + ((float)SkillLevel * 0.5f), 0, 0, 0, duration, "PlasmaShield");
        }
    }
}