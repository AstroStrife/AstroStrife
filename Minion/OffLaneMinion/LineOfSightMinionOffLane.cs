using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LineOfSightMinionOffLane : NetworkBehaviour
{
    public LayerMask obstacleMask; // Layer of the obstacles
    public List<GameObject> visibleTargetsTop = new List<GameObject>(); // List to keep track of visible targets
    public List<GameObject> visibleTargetsBottom = new List<GameObject>(); // List to keep track of visible targets

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
        targetsWithTag = PoolManager.Instance.poolTeam["Player"].ToArray();
    }

    void FindVisibleTargets()
    {
        visibleTargetsTop.Clear();
        visibleTargetsBottom.Clear();

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
                    if (target.tag == "Top")
                    {
                        visibleTargetsTop.Add(target);
                    }
                    else
                    {
                        visibleTargetsBottom.Add(target);
                    }
                }
            }
        }
        // Determine the new visibility status based on the targets detected
        string newVisibility = DetermineVisibility(visibleTargetsTop.Count, visibleTargetsBottom.Count);

        if (newVisibility != currentVisibility)
        {
            currentVisibility = newVisibility;
            ChangeVisibilityServerRpc(newVisibility);
        }
    }

    private string DetermineVisibility(int countTop, int countBottom)
    {
        if (countTop > 0 && countBottom > 0)
        {
            return "Default";
        }
        else if (countTop > 0)
        {
            return "Top";
        }
        else if (countBottom > 0)
        {
            return "Bottom";
        }
        return "OffLane";
    }

    public void ChangeVisibility(string visibility)
    {
        if (visibility == "Default")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("Default"));
        }
        else if (visibility == "Bottom")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamBottomVisible"));
        }
        else if (visibility == "Top")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamTopVisible"));
        }
        else if (visibility == "OffLane")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("OffLaneVisible"));
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