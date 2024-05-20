using Unity.Netcode;
using UnityEngine;

public class InLaneBullet : NetworkBehaviour
{
    private float lifetime = 1f;
    private float remainingLifetime;
    public Transform target;
    private float speed = 200f;
    private float rotationSpeed = 300f;
    private PoolManager poolManager;

    private void Start()
    {
        poolManager = PoolManager.Instance;
        remainingLifetime = lifetime;
    }

    void Update()
    {
        if (target == null || remainingLifetime <= 0)
        {
            gameObject.SetActive(false);
            PushToPoolServerRpc();
            return;
        }

        if (target.gameObject.activeSelf == false)
        {
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
        if (GameManager.Instance.GameEnd.Value == true)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        Vector3 targetDirection = target.position - transform.position;
        float step = rotationSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);

        remainingLifetime -= Time.deltaTime;
    }
    public void SetupBullet(NetworkObjectReference Target)
    {
        NetworkObject networkObject = Target;
        if (networkObject != null)
        {
            this.target = networkObject.transform;
        }

        remainingLifetime = lifetime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PushToPoolServerRpc()
    {
        poolManager.poolDictionary["InLane" + "_" + "Bullet"].Enqueue(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(false);
    }
}