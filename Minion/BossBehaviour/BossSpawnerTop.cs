using Unity.Netcode;
using UnityEngine;

public class BossSpawnerTop : NetworkBehaviour
{
    private PoolManager poolManager;

    private float spawnRadius = 50f;
    private float countdownTime = 300f;
    private bool isCountingDown = false;

    private void Start()
    {
        poolManager = PoolManager.Instance;
        if (IsServer)
        {
            SpawnObjectServerRpc("OffLane", "BossTop", this.gameObject.transform.position, this.gameObject.transform.rotation);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (!isCountingDown)
            {
                // Start countdown if no minions are in the area
                if (!IsMinionInArea(transform.position, spawnRadius))
                {
                    isCountingDown = true;
                }
            }
            else
            {
                // Countdown logic
                countdownTime -= Time.deltaTime;
                if (countdownTime <= 0)
                {
                    SpawnObjectServerRpc("OffLane", "BossTop", this.gameObject.transform.position, this.gameObject.transform.rotation);
                    ResetCountdown();
                }
            }
        }
    }

    private void ResetCountdown()
    {
        countdownTime = 300f; // Reset to 5 minutes
        isCountingDown = false;
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
    public void SpawnObjectServerRpc(string teamTag, string typeTag, Vector3 position, Quaternion rotation)
    {
        poolManager.RequestObjectFromPoolServerRpc(teamTag, typeTag, position, rotation);
    }
}
