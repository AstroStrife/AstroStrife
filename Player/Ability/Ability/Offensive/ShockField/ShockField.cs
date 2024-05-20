using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAreaDamageAbility", menuName = "Ability System/Ability/Offensive/Shock Field Ability")]
public class ShockField : Ability
{
    public float HealthactivationCost;

    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject ShockField = Instantiate(prefab);
        ShockField.GetComponent<NetworkObject>().Spawn();
        ShockField1 ShockFieldScript = ShockField.GetComponent<ShockField1>();

        if (ShockFieldScript != null)
        {
            if (user.TryGetComponent(out PlayerStatusController _playerStatusController))
            {
                _playerStatusController.HP.Value = Math.Max(1, _playerStatusController.HP.Value - HealthactivationCost);
                ShockFieldScript.SetupField(_playerStatusController.AttackDam.Value, user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, user, SkillLevel);
            }
        }
    }
}
