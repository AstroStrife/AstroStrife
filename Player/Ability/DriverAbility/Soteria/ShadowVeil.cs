using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShadowVeil : NetworkBehaviour
{
    private GameObject player;
    private float remainingLifetime;

    public ulong ownerNetworkId;
    public string ownerName;

    private float Duration;
    private float detectionRadius;

    private List<LineOfSight> nearbyPlayersSight = new List<LineOfSight>();

    private void Start()
    {
        if (!IsServer) return;
        remainingLifetime = Duration;
        ChangeVisibilityServerRpc();
    }
    private void Update()
    {
        if (!IsServer) return;
        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            ChangeVisibilityBackServerRpc();
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }

    [ServerRpc]
    private void ChangeVisibilityServerRpc()
    {
        Collider[] colliders = Physics.OverlapSphere(player.transform.position, detectionRadius);

        foreach (Collider collider in colliders)
        {
            PlayerStatusController playerStatusController = collider.GetComponent<PlayerStatusController>();
            LineOfSight lineofSight = collider.GetComponent<LineOfSight>();
            if (playerStatusController != null && lineofSight != null && collider.tag == player.tag)
            {
                playerStatusController.ChangeVisibility(player.tag);
                ChangeVisibilityClientRpc(player.tag, collider.GetComponent<NetworkObject>().NetworkObjectId);

                lineofSight.CanChangeVisibility.Value = false;
                Debug.Log(lineofSight.CanChangeVisibility.Value);
                nearbyPlayersSight.Add(lineofSight);
            }
        }
    }

    [ClientRpc]
    private void ChangeVisibilityClientRpc(string tag, ulong playerID)
    {
        var Gameobject = GameManager.Instance.GetPlayerFromPoolByNetworkObjectId("Player", playerID);
        Gameobject.GetComponent<PlayerStatusController>().ChangeVisibility(tag);
    }

    [ServerRpc]
    private void ChangeVisibilityBackServerRpc()
    {
        foreach (LineOfSight lineOfSight in nearbyPlayersSight)
        {
            lineOfSight.CanChangeVisibility.Value = true;
        }
    }

    public void SetupField(ulong ownerId, string ownerName, string tag, float Duration, float detectionRadius)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.Duration = Duration;
        this.detectionRadius = detectionRadius;

        // Find player GameObject from NetworkObjectId on the server
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ownerId, out NetworkObject playerNetworkObject);
        this.player = playerNetworkObject != null ? playerNetworkObject.gameObject : null;

        SetupFieldClientRpc(ownerId, ownerName, tag, Duration, detectionRadius);
    }

    [ClientRpc]
    public void SetupFieldClientRpc(ulong ownerId, string ownerName, string tag, float Duration, float detectionRadius)
    {
        this.ownerNetworkId = ownerId;
        this.ownerName = ownerName;
        this.gameObject.tag = tag;
        this.Duration = Duration;
        this.detectionRadius = detectionRadius;

        // Find player GameObject from NetworkObjectId on the client
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ownerId, out NetworkObject playerNetworkObject);
        this.player = playerNetworkObject != null ? playerNetworkObject.gameObject : null;
    }
}