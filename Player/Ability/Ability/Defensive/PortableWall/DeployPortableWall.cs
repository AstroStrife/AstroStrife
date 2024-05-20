using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ability/Defense/PortableWall Ability")]
public class DeployPortableWall : Ability
{
    public override void Activate(GameObject user, int SkillLevel)
    {
        Vector3 playerPosition = user.transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 spawnPosition = hit.point;

            spawnPosition.y = prefab.transform.position.y;

            GameObject Wall = Instantiate(prefab, spawnPosition, Quaternion.identity);
            Vector3 directionToPlayer = playerPosition - Wall.transform.position;
            Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);

            Wall.transform.rotation = rotationToPlayer * Quaternion.Euler(0, 90, 0);
            Vector3 currentEulerAngles = Wall.transform.eulerAngles;
            Wall.transform.eulerAngles = new Vector3(0, currentEulerAngles.y, 0);

            Wall.GetComponent<NetworkObject>().Spawn();

            PortableWall InertiaZoneScript = Wall.GetComponent<PortableWall>();
            if (InertiaZoneScript != null)
            {
                InertiaZoneScript.SetupWall(user.GetComponent<NetworkObject>().NetworkObjectId, user.name, SkillLevel);
            }
        }

    }
}