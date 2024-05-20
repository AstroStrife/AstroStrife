using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ultimate/Soteria")]
public class DeployShadowVeil : Ability
{
    public float Duration;
    public float detectionRadius;
    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject ShadowVeil = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        ShadowVeil.GetComponent<NetworkObject>().Spawn();
        ShadowVeil ShadowVeilScript = ShadowVeil.GetComponent<ShadowVeil>();

        if (ShadowVeilScript != null)
        {
            ShadowVeilScript.SetupField(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, Duration + SkillLevel, detectionRadius);
        }
    }
}