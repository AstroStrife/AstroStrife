using Unity.Netcode;
using UnityEngine;

public class HighInertiaZone : NetworkBehaviour
{
    private float lifetime = 5f;
    private float remainingLifetime;
    private int skillLevel = 0;
    public ulong ownerNetworkId;
    public string ownerName;

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
        if (other.gameObject.GetComponent<Bullet>() != null)
        {
            Bullet TempBulletScript = other.gameObject.GetComponent<Bullet>();
            TempBulletScript.bulletSpeed /= 1 + ((float)skillLevel * 0.5f);
            TempBulletScript.Damage /= 1 + ((float)skillLevel * 0.5f);
        }
        else if (other.gameObject.GetComponent<PenetrateBullet1>() != null)
        {
            PenetrateBullet1 TempBulletScript = other.gameObject.GetComponent<PenetrateBullet1>();
            TempBulletScript.bulletSpeed /= 1 + ((float)skillLevel * 0.5f);
            TempBulletScript.Damage /= 1 + ((float)skillLevel * 0.5f);
        }
    }

    public void SetupField(ulong ownerId, string ownerName, int skillLevel)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.skillLevel = skillLevel;
    }
}