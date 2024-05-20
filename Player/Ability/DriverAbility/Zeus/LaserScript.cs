using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LaserScript : NetworkBehaviour
{
    private float lifetime = 1f;
    private float remainingLifetime;
    public float Damage = 0;

    public ulong ownerNetworkId;
    public string ownerName;
    private int skillLevel = 0;

    HashSet<string> excludedNames = new HashSet<string> { "bullet", "exp", "gold", "inertiazone", "hook", "turret", "home" };

    private void Start()
    {
        remainingLifetime = lifetime;
    }
    private void Update()
    {
        remainingLifetime -= Time.deltaTime;

        // Check if the bullet's lifetime has expired
        if (remainingLifetime <= 0f)
        {
            if (IsServer)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
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
                float effectiveDamage = Mathf.Max(0, Damage * (4 + skillLevel));
                damageable.TakeDamage(effectiveDamage, ownerNetworkId, ownerName);

                // Register to ScoreBoard
                if (other.gameObject.GetComponent<PlayerScore>() != null)
                {
                    GameObject shooter = GameManager.Instance.GetPlayerFromPoolByNetworkObjectId("Player", ownerNetworkId);
                    shooter.GetComponent<PlayerScore>().IncreaseTotalPlayerDamage(effectiveDamage);
                }
            }
        }
    }

    public void SetupLaser(float Damage, ulong ownerId, string ownerName, string tag, int skillLevel)
    {
        this.Damage = Damage;
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.skillLevel = skillLevel;
    }
}