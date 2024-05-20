using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GrapplingHook : NetworkBehaviour
{
    private GameObject player;
    private GameObject Target;
    private float Range;
    private float StopRange;
    private float HookSpeed = 150f;

    private LineRenderer line;
    public bool hasCollided;

    public ulong ownerNetworkId;
    public string ownerName;

    HashSet<string> excludedNames = new HashSet<string> { "bullet", "exp", "gold", "inertiazone", "turret", "home", "hook", "bridge" };

    private void Start()
    {
        line = transform.Find("Line").GetComponent<LineRenderer>();
    }
    private void Update()
    {
        if (player != null)
        {
            LinePositionClientRpc(player.GetComponent<NetworkObject>());
            //Check if we have impacted
            if (hasCollided)
            {
                transform.LookAt(player.transform);
                var dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < StopRange)
                {
                    if (IsServer)
                    {
                        gameObject.GetComponent<NetworkObject>().Despawn();
                    }
                }
            }
            else
            {
                var dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist > Range)
                {
                    Collision(null);
                }
            }

            transform.Translate(Vector3.forward * HookSpeed * Time.deltaTime);
            if (Target != null)
            {
                Target.transform.position = transform.position;
            }
        }
        else
        {
            if (IsServer)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check is it in cant hook list
        if (gameObject.tag != other.gameObject.tag && !excludedNames.Any(name => other.name.ToLower().Contains(name)))
        {
            if (other.gameObject.GetComponent<PlayerStatusController>() != null)
            {
                Collision(other.transform);
            }
            // If not player destroy it
            if (IsServer)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    void Collision(Transform col)
    {
        //Stop movement
        hasCollided = true;
        col.gameObject.GetComponent<CharacterController>().enabled = false;
        if (col != null)
        {
            transform.position = col.position;
            Target.transform.position = col.position;
            Target.transform.rotation = col.rotation;
            Target.transform.localScale = col.localScale;
        }
    }

    [ClientRpc]
    public void LinePositionClientRpc(NetworkObjectReference player)
    {
        line = transform.Find("Line").GetComponent<LineRenderer>();
        NetworkObject networkObject = player;
        line.SetPosition(0, networkObject.gameObject.transform.position);
        line.SetPosition(1, transform.position);
    }

    public void SetupHook(ulong ownerId, string ownerName, string tag, GameObject player, float Range, float StopRange)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.player = player;
        this.Range = Range;
        this.StopRange = StopRange;
    }
}