using UnityEngine;

[CreateAssetMenu(fileName = "New RepairKit", menuName = "Inventory/RepairKit")]
public class RepairKit : Item
{
    public override void Use(PlayerStatusController playerStatusController)
    {
        playerStatusController.HealServerRpc(500f);
    }
}