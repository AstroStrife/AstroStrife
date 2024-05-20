using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurretSpawn : NetworkBehaviour
{
    // Bottom turret
    public Transform BottomFirstTurret;
    public Transform BottomSecondTurret;
    public Transform BottomThirdTurret;
    public Transform BottomLastLeftTurret;
    public Transform BottomLastRightTurret;

    public GameObject BottomFirstTurretObject;
    public GameObject BottomSecondTurretObject;
    public GameObject BottomThirdTurretObject;
    public GameObject BottomLastLeftTurretObject;
    public GameObject BottomLastRightTurretObject;

    // Top turret
    public Transform TopFirstTurret;
    public Transform TopSecondTurret;
    public Transform TopThirdTurret;
    public Transform TopLastLeftTurret;
    public Transform TopLastRightTurret;

    public GameObject TopFirstTurretObject;
    public GameObject TopSecondTurretObject;
    public GameObject TopThirdTurretObject;
    public GameObject TopLastLeftTurretObject;
    public GameObject TopLastRightTurretObject;

    public static List<GameObject> TopTurrets = new List<GameObject>();
    public static List<GameObject> BottomTurrets = new List<GameObject>();

    private void Start()
    {
        if (IsServer)
        {
            SpawnTurrets();
        }
    }

    private void SpawnTurrets()
    {
        // Spawn Bottom Turrets
        SpawnTurretBottom(BottomFirstTurret, BottomFirstTurretObject);
        SpawnTurretBottom(BottomSecondTurret, BottomSecondTurretObject);
        SpawnTurretBottom(BottomThirdTurret, BottomThirdTurretObject);
        SpawnTurretBottom(BottomLastLeftTurret, BottomLastLeftTurretObject);
        SpawnTurretBottom(BottomLastRightTurret, BottomLastRightTurretObject);

        // Spawn Top Turrets
        SpawnTurretTop(TopFirstTurret, TopFirstTurretObject);
        SpawnTurretTop(TopSecondTurret, TopSecondTurretObject);
        SpawnTurretTop(TopThirdTurret, TopThirdTurretObject);
        SpawnTurretTop(TopLastLeftTurret, TopLastLeftTurretObject);
        SpawnTurretTop(TopLastRightTurret, TopLastRightTurretObject);
    }

    private void SpawnTurretBottom(Transform turretTransform, GameObject turretPrefab)
    {
        if (turretPrefab != null && turretTransform != null)
        {
            GameObject turretInstance = Instantiate(turretPrefab, turretTransform.position, turretTransform.rotation);
            NetworkObject turretNetworkObject = turretInstance.GetComponent<NetworkObject>();
            turretNetworkObject.Spawn();
            BottomTurrets.Add(turretInstance);
        }
    }

    private void SpawnTurretTop(Transform turretTransform, GameObject turretPrefab)
    {
        if (turretPrefab != null && turretTransform != null)
        {
            GameObject turretInstance = Instantiate(turretPrefab, turretTransform.position, turretTransform.rotation);
            NetworkObject turretNetworkObject = turretInstance.GetComponent<NetworkObject>();
            turretNetworkObject.Spawn();
            TopTurrets.Add(turretInstance);
        }
    }

    public static bool AreAllTurretsDown(string tag)
    {
        List<GameObject> turretList = tag == "Top" ? TopTurrets : BottomTurrets;
        return turretList.All(turret => turret != null && turret.activeSelf == false);
    }
}