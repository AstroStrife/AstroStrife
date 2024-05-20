using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LineOfSight : NetworkBehaviour
{
    public LayerMask obstacleMask; // Layer of the obstacles
    public List<GameObject> visibleTargets = new List<GameObject>(); // List to keep track of visible targets
    private Dictionary<GameObject, PlayerStatusController> playerStatusControllersCache = new Dictionary<GameObject, PlayerStatusController>();

    public string targetTag;

    public float viewRadius = 120;

    private float timeSinceLastCheck = 0f;
    private float checkInterval = 0.5f;
    public PlayerStatusController playerStatusController;
    public GameObject[] targetsWithTag;

    private string currentVisibility = "StartGame";

    public NetworkVariable<bool> CanChangeVisibility = new NetworkVariable<bool>(true);

    void Start()
    {
        playerStatusController = GetComponent<PlayerStatusController>();

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

        if (GameManager.Instance.GameEnd.Value == true)
        {
            targetsWithTag = new GameObject[0];
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
        List<GameObject> targetsList = new List<GameObject>(PoolManager.Instance.poolTeam[targetTag]);

        // Remove all game objects that have "bullet" in their name
        targetsList.RemoveAll(target => target != null && target.name.ToLower().Contains("bullet"));

        targetsWithTag = targetsList.ToArray();
        CachePlayerStatusControllers();
    }

    void CachePlayerStatusControllers()
    {
        playerStatusControllersCache.Clear();
        foreach (var target in targetsWithTag)
        {
            var statusController = target.GetComponent<PlayerStatusController>();
            if (statusController != null)
            {
                playerStatusControllersCache[target] = statusController;
            }
        }
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
                if (!Physics.Raycast(transform.position, dirToTarget.normalized, out hit, Mathf.Sqrt(sqrDistanceToTarget), obstacleMask))
                {
                    if (playerStatusControllersCache.TryGetValue(target, out var targetPlayerStatus))
                    {
                        bool bothInSameBush = targetPlayerStatus.currentBush == playerStatusController.currentBush;
                        bool targetInBushPlayerNot = targetPlayerStatus.currentBush != null && playerStatusController.currentBush == null;

                        if (bothInSameBush || targetInBushPlayerNot)
                        {
                            visibleTargets.Add(target);
                        }
                    }
                    else
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
        string newVisibility = visibleTargets.Count > 0 ? "Default" : gameObject.tag;
        if (newVisibility != currentVisibility && CanChangeVisibility.Value == true)
        {
            currentVisibility = newVisibility;
            ChangeVisibilityServerRpc(newVisibility);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeVisibilityServerRpc(string tag)
    {
        playerStatusController.ChangeVisibility(tag);
        ChangeVisibilityClientRpc(tag);
    }

    [ClientRpc]
    private void ChangeVisibilityClientRpc(string tag)
    {
        playerStatusController.ChangeVisibility(tag);
    }
}