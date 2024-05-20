using Unity.Netcode;
using UnityEngine;

public class BlackHole : NetworkBehaviour
{
    private float remainingLifetime;

    public ulong ownerNetworkId;
    public string ownerName;

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
        if (other.tag != gameObject.tag)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (other.gameObject.GetComponent<PlayerStatusController>() != null)
            {
                other.gameObject.GetComponent<PlayerStatusController>().GetStunt(remainingLifetime);
            }
            else if (other.gameObject.GetComponent<MinionScript>() != null)
            {
                float effectiveDamage = Mathf.Max(0, other.gameObject.GetComponent<MinionScript>().MaxHP.Value);
                damageable.TakeDamage(effectiveDamage, ownerNetworkId, ownerName);
            }
            else if (other.gameObject.GetComponent<OffMinionScripts>() != null)
            {
                float effectiveDamage = Mathf.Max(0, other.gameObject.GetComponent<OffMinionScripts>().MaxHP.Value);
                damageable.TakeDamage(effectiveDamage, ownerNetworkId, ownerName);
            }
        }
    }

    public void SetupField(ulong ownerId, string ownerName, string tag, float duration)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.remainingLifetime = duration;
    }
}