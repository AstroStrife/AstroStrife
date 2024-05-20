using UnityEngine;

[CreateAssetMenu(fileName = "New Teleporter", menuName = "Inventory/Teleporter")]
public class TeleporterItem : Item
{
    public override void Use(PlayerStatusController playerStatusController)
    {
        playerStatusController.BackToBase();
    }
}