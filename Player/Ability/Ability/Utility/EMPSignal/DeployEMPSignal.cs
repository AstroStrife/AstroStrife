using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Utility/EMPSignal Ability")]
public class DeployEMPSignal : Ability
{
    public float duration;
    public float detectionRadius;
    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject EMPSignal = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        EMPSignal.GetComponent<NetworkObject>().Spawn();

        Collider[] colliders = Physics.OverlapSphere(user.transform.position, detectionRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<PlayerStatusController>() != null && collider.tag != user.tag)
            {
                collider.GetComponent<PlayerStatusController>().GetStunt(duration + ((float)SkillLevel * 0.1f));
            }
        }
    }
}