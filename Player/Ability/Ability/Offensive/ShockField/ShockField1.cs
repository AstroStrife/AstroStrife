using Unity.Netcode;
using UnityEngine;

public class ShockField1 : NetworkBehaviour
{
    private GameObject player;
    private float lifetime = 5f;
    private float remainingLifetime;
    private float Damage = 0;

    public ulong ownerNetworkId;
    public string ownerName;
    private int skillLevel = 0;

    private float damageTimer = 0f;
    private const float damageInterval = 1f;

    private void Start()
    {
        remainingLifetime = lifetime + skillLevel;
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
        else
        {
            Vector3 newPosition = player.transform.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        damageTimer += Time.deltaTime;

        // Check if 1 second has passed
        if (damageTimer >= damageInterval)
        {
            if (gameObject.tag != other.gameObject.tag && !other.name.ToLower().Contains("bullet") && !other.name.ToLower().Contains("exp") && !other.name.ToLower().Contains("gold"))
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    if (IsServer && other.GetComponentInChildren<BlinkEffect>() != null)
                    {
                        other.GetComponentInChildren<BlinkEffect>().Blink();
                    }
                    float effectiveDamage = Mathf.Max(0, Damage - damageable.GetDefense());
                    damageable.TakeDamage(Damage, ownerNetworkId, ownerName);

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
            }

            // Reset the timer
            damageTimer = 0f;
        }
    }


    public void SetupField(float Damage, ulong ownerId, string ownerName, string tag, GameObject player, int skillLevel)
    {
        this.Damage = Damage;
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.player = player;
        this.skillLevel = skillLevel;
    }
}