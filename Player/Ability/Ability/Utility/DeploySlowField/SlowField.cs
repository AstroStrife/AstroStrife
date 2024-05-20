using Unity.Netcode;
using UnityEngine;

public class SlowField : NetworkBehaviour
{
    private float lifetime = 5f;
    private float remainingLifetime;
    private float slowPercentage;

    public ulong ownerNetworkId;
    public string ownerName;
    private int skillLevel = 0;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != gameObject.tag)
        {
            if (other.gameObject.GetComponent<PlayerStatusController>() != null)
            {
                other.gameObject.GetComponent<PlayerStatusController>().ApplyDebuff(0, 0, slowPercentage, 0, 0, lifetime, "SlowField");
            }
            else if (other.gameObject.GetComponent<MinionScript>() != null)
            {
                other.gameObject.GetComponent<MinionScript>().ApplyDebuff(0, slowPercentage, 0, lifetime, "SlowField");
            }
            else if (other.gameObject.GetComponent<OffMinionScripts>() != null)
            {
                other.gameObject.GetComponent<OffMinionScripts>().ApplyDebuff(0, slowPercentage, 0, lifetime, "SlowField");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerStatusController>() != null && other.tag != gameObject.tag)
        {
            other.gameObject.GetComponent<PlayerStatusController>().RemoveSelectedDebuff("SlowField");
        }
        else if (other.gameObject.GetComponent<MinionScript>() != null && other.tag != gameObject.tag)
        {
            other.gameObject.GetComponent<MinionScript>().RemoveSelectedDebuff("SlowField");
        }
        else if (other.gameObject.GetComponent<OffMinionScripts>() != null)
        {
            other.gameObject.GetComponent<OffMinionScripts>().RemoveSelectedDebuff("SlowField");
        }
    }

    public void SetupField(ulong ownerId, string ownerName, string tag, float slowPercentage, int skillLevel)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.slowPercentage = slowPercentage;
        this.skillLevel = skillLevel;
    }
}