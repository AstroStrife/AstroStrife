using Unity.Netcode;
using UnityEngine;

public class PortableWall : NetworkBehaviour
{
    private float lifetime = 10f;
    private float remainingLifetime;

    private int skillLevel = 0;
    public ulong ownerNetworkId;
    public string ownerName;


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
    }

    public void SetupWall(ulong ownerId, string ownerName, int skillLevel)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.skillLevel = skillLevel;
    }

}