using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Defense/GrapplingHook Ability")]
public class DeployGrapplingHook : Ability
{
    public float Range;
    public float StopRange;
    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerAttackController attackController))
        {
            GameObject Hook = Instantiate(prefab, attackController.spawnpointbullet.position, attackController.spawnpointbullet.rotation);
            Hook.GetComponent<NetworkObject>().Spawn();
            GrapplingHook bulletScript = Hook.GetComponent<GrapplingHook>();

            if (bulletScript != null)
            {
                Vector3 shootDirection = attackController.spawnpointbullet.forward;
                bulletScript.SetupHook(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, user, Range, StopRange);
            }
        }
    }
}