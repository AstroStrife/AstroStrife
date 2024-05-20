using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Defense/HighInertiaZone Ability")]
public class DeployHighInertiaZone : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject highInertiaZone = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        highInertiaZone.GetComponent<NetworkObject>().Spawn();
        HighInertiaZone InertiaZoneScript = highInertiaZone.GetComponent<HighInertiaZone>();

        if (InertiaZoneScript != null)
        {
            InertiaZoneScript.SetupField(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, SkillLevel);
        }
    }
}