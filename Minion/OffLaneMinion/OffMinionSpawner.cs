using Unity.Netcode;
using UnityEngine;

public class OffMinionSpawner : NetworkBehaviour
{
    private PoolManager poolManager; // Reference to the PoolManager
    public TimeManager timeManager;

    private float spawnRadius = 105f;

    public static int CheckSpawnTime;

    private void Start()
    {
        poolManager = PoolManager.Instance;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (CheckSpawnTime == 0)
            {
                // Check if there are any Minion objects in the spawn area
                bool canSpawn = !IsMinionInArea(transform.position, spawnRadius);

                if (canSpawn)
                {
                    SpawnOffMinionServerRpc();
                }
            }
        }
    }

    private bool IsMinionInArea(Vector3 center, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(center, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("OffLane"))
            {
                return true;
            }
        }

        return false;
    }

    [ServerRpc]
    public void SpawnOffMinionServerRpc()
    {
        poolManager.RequestObjectFromPoolServerRpc("OffLane", "SmallOffLaneMinion", this.gameObject.transform.position + new Vector3(10f, 0f, 0f), this.gameObject.transform.rotation);
        poolManager.RequestObjectFromPoolServerRpc("OffLane", "SmallOffLaneMinion", this.gameObject.transform.position, this.gameObject.transform.rotation);
        poolManager.RequestObjectFromPoolServerRpc("OffLane", "SmallOffLaneMinion", this.gameObject.transform.position + new Vector3(0, 0f, -10f), this.gameObject.transform.rotation);
    }
}