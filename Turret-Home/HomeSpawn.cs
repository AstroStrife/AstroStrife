using Unity.Netcode;
using UnityEngine;

public class HomeSpawn : NetworkBehaviour
{
    // Bottom Home
    public Transform BottomHome;
    public GameObject BottomHomeObject;

    // Top Home
    public Transform TopHome;
    public GameObject TopHomeObject;

    private void Start()
    {
        if (IsServer)
        {
            SpawnHome();
        }
    }

    private void SpawnHome()
    {
        // Spawn Bottom Turrets
        SpawnHome(BottomHome, BottomHomeObject);
        SpawnHome(TopHome, TopHomeObject);
    }

    private void SpawnHome(Transform homeTransform, GameObject homePrefab)
    {
        if (homePrefab != null && homeTransform != null)
        {
            GameObject turretInstance = Instantiate(homePrefab, homeTransform.position, homeTransform.rotation);
            NetworkObject turretNetworkObject = turretInstance.GetComponent<NetworkObject>();
            turretNetworkObject.Spawn();
        }
    }
}
