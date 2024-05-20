using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ultimate/Nova")]
public class NovaUltimate : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerController _playerController))
        {
            _playerController.IframeClientRpc(5f + SkillLevel);
        }
    }
}