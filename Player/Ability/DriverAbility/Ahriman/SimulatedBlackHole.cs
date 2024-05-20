using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ultimate/Ahriman")]
public class SimulatedBlackHole : Ability
{
    public float duration;

    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject BlackHole = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        BlackHole.GetComponent<NetworkObject>().Spawn();
        BlackHole BlackHoleScript = BlackHole.GetComponent<BlackHole>();

        if (BlackHoleScript != null)
        {
            BlackHoleScript.SetupField(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, user.tag, duration + SkillLevel);
        }
    }
}
