using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private float lifetime = 1f;
    private float remainingLifetime;
    public float bulletSpeed = 300f;
    public float Damage = 0;
    private Vector3 shootDir;
    private PoolManager poolManager;

    public ulong ownerNetworkId;
    public string ownerName;

    HashSet<string> excludedNames = new HashSet<string> { "bullet", "exp", "gold", "inertiazone", "hook", "bush", "base" };

    private void Start()
    {
        poolManager = PoolManager.Instance;
        remainingLifetime = lifetime;
    }
    private void Update()
    {
        remainingLifetime -= Time.deltaTime;

        // Check if the bullet's lifetime has expired
        if (remainingLifetime <= 0f)
        {
            InitializeBullet();
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
        else
        {
            transform.position += shootDir.normalized * bulletSpeed * Time.deltaTime;
        }
        if (GameManager.Instance.GameEnd.Value == true)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.tag != other.gameObject.tag && !excludedNames.Any(name => other.name.ToLower().Contains(name)))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (IsServer && other.GetComponentInChildren<BlinkEffect>() != null)
                {
                    other.GetComponentInChildren<BlinkEffect>().Blink();
                }
                float effectiveDamage = Mathf.Max(0, Damage - damageable.GetDefense());
                damageable.TakeDamage(effectiveDamage, ownerNetworkId, ownerName);

                // Register to ScoreBoard
                if (other.gameObject.GetComponent<PlayerScore>() != null)
                {
                    GameObject shooter = GameManager.Instance.GetPlayerFromPoolByNetworkObjectId("Player", ownerNetworkId);
                    shooter.GetComponent<PlayerScore>().IncreaseTotalPlayerDamage(effectiveDamage);
                }
                if (other.gameObject.GetComponent<TurretScript>() != null)
                {
                    GameObject shooter = GameManager.Instance.GetPlayerFromPoolByNetworkObjectId("Player", ownerNetworkId);
                    shooter.GetComponent<PlayerScore>().IncreaseTotalTurretDamage(effectiveDamage);
                }
            }
            InitializeBullet();
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
    }

    public void InitializeBullet()
    {
        remainingLifetime = lifetime;
        shootDir = Vector3.zero;
        Damage = 0;
        bulletSpeed = 100f;
        ownerNetworkId = 0;
        ownerName = string.Empty;
    }


    public void SetupBullet(Vector3 direction, float Damage, ulong ownerId, string ownerName)
    {
        this.shootDir = direction;
        this.Damage = Damage;
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PushToPoolServerRpc()
    {
        poolManager.poolDictionary[gameObject.tag + "_" + "Bullet"].Enqueue(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(false);
    }
}