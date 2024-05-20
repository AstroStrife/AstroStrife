using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ultimate/Zeus")]
public class ShootLaser : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        if (user.TryGetComponent(out PlayerAttackController attackController))
        {
            GameObject Laser = Instantiate(prefab, attackController.spawnpointbullet.position, attackController.spawnpointbullet.rotation);
            Laser.GetComponent<NetworkObject>().Spawn();

            Laser.transform.rotation = attackController.spawnpointbullet.rotation * Quaternion.Euler(0f, -90f, 90f);


            Vector3 offset = Laser.transform.up * 400;
            Laser.transform.position = attackController.spawnpointbullet.position - offset;

            LaserScript LaserScriptScript = Laser.GetComponent<LaserScript>();
            if (LaserScriptScript != null)
            {
                LaserScriptScript.SetupLaser(attackController._playerStatusController.AttackDam.Value, user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, SkillLevel);
            }
        }
    }
}