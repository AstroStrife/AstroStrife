using Unity.Netcode;
using UnityEngine;

public class EMPSignal : NetworkBehaviour
{
    private float lifetime = 1f;
    private float remainingLifetime;

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
}
