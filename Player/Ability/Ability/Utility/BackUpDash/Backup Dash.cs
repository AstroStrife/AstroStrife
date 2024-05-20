using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Utility/BackUpDash Ability")]
public class BackupDash : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerController _playerController))
        {
            _playerController.DashClientRpc(0.5f + ((float)SkillLevel * 0.1f));
        }
    }
}
