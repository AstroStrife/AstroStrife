using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Utility/DeploySlowField Ability")]

public class DeploySlowField : Ability
{
    public float slowPercentage;

    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject _slowField = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        _slowField.GetComponent<NetworkObject>().Spawn();
        SlowField SlowFieldScript = _slowField.GetComponent<SlowField>();

        if (SlowFieldScript != null)
        {
            SlowFieldScript.SetupField(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, slowPercentage, SkillLevel);
        }
    }
}
