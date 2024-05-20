using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LineOfSightMinion : NetworkBehaviour
{
    public LayerMask obstacleMask; // Layer of the obstacles
    public List<GameObject> visibleTargets = new List<GameObject>(); // List to keep track of visible targets

    public string targetTag;

    public float viewRadius;
    [Range(0.1f, 20f)]

    private float timeSinceLastCheck = 0f;
    private float checkInterval = 0.5f;
    private GameObject[] targetsWithTag;

    private string currentVisibility = "StartGame";

    void Start()
    {
        if (!IsServer) return;
        StartCoroutine(WaitForReCheck());
    }

    void Update()
    {
        if (!IsServer) return;

        timeSinceLastCheck += Time.deltaTime;

        if (timeSinceLastCheck >= checkInterval && MiniMapMark.RecheckFinish == true)
        {
            FindVisibleTargets();
            timeSinceLastCheck = 0f;
        }
    }

    public IEnumerator WaitForReCheck()
    {
        while (MiniMapMark.RecheckFinish == false)
        {
            yield return null;
        }
        if (gameObject.tag == "Top")
        {
            targetTag = "Bottom";
        }
        else
        {
            targetTag = "Top";
        }
        List<GameObject> targetsList = new List<GameObject>(PoolManager.Instance.poolTeam["Player"]);

        // Remove all game objects that have "bullet" in their name
        targetsList.RemoveAll(target => target != null && target.name.ToLower().Contains("bullet"));

        targetsWithTag = targetsList.ToArray();
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        foreach (GameObject target in targetsWithTag)
        {
            // Early out if the target is not active
            if (target.activeSelf == false) continue;

            Vector3 dirToTarget = (target.transform.position - transform.position);
            float sqrDistanceToTarget = dirToTarget.sqrMagnitude;

            if (sqrDistanceToTarget < viewRadius * viewRadius)
            {
                RaycastHit hit;
                if (!Physics.Raycast(transform.position, dirToTarget, out hit, Mathf.Sqrt(sqrDistanceToTarget), obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
        string newVisibility = visibleTargets.Count > 0 ? "Default" : gameObject.tag;
        if (newVisibility != currentVisibility)
        {
            currentVisibility = newVisibility;
            ChangeVisibilityServerRpc(newVisibility);
        }
    }

    public void ChangeVisibility(string visibility)
    {
        if (visibility == "Top")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamTopVisible"));
        }
        else if (visibility == "Bottom")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamBottomVisible"));
        }
        else if (visibility == "Default")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("Default"));
        }
    }

    private void ChangeLayerOfObjectAndChildren(GameObject obj, int layer)
    {
        if (obj.layer != layer)
        {
            // Set the new layer
            obj.layer = layer;

            // Recursively change the layer for children
            foreach (Transform child in obj.transform)
            {
                ChangeLayerOfObjectAndChildren(child.gameObject, layer);
            }
        }
    }

    [ServerRpc]
    private void ChangeVisibilityServerRpc(string tag)
    {
        ChangeVisibility(tag);
        ChangeVisibilityClientRpc(tag);
    }

    [ClientRpc]
    private void ChangeVisibilityClientRpc(string tag)
    {
        ChangeVisibility(tag);
    }
}