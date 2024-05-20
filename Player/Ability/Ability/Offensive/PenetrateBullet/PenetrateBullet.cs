using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Offensive/Penetrate Bullet Ability")]
public class PenetrateBullet : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerAttackController attackController))
        {
            GameObject bullet = Instantiate(prefab, attackController.spawnpointbullet.position, attackController.spawnpointbullet.rotation);
            bullet.GetComponent<NetworkObject>().Spawn();
            PenetrateBullet1 bulletScript = bullet.GetComponent<PenetrateBullet1>();

            if (bulletScript != null)
            {
                Vector3 shootDirection = attackController.spawnpointbullet.forward;
                bulletScript.SetupBullet(shootDirection, attackController._playerStatusController.AttackDam.Value, user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, SkillLevel);
            }
        }
    }
}
