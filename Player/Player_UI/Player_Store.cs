using UnityEngine;
using UnityEngine.EventSystems;

public class Player_Store : MonoBehaviour
{
    private GameObject _ownerPlayerPrefab;

    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.gameObject;
    }

    public void StateOn_Off(bool state)
    {
        gameObject.SetActive(state);
    }

    public Item GetItemFromClickedIcon(PointerEventData eventData)
    {
        var clickedObject = eventData.pointerPress;

        var item = clickedObject.GetComponent<ItemStoreShow>();
        if (item != null)
        {
            return item.item;
        }

        return null;
    }

    public void PurchaseItem(Item item)
    {
        var playerStatusController = _ownerPlayerPrefab.GetComponent<PlayerStatusController>();
        var playerInventory = _ownerPlayerPrefab.GetComponent<Inventory>();

        if (playerStatusController.OnBase.Value)
        {
            if (playerStatusController.Money.Value >= item.Price && !playerInventory.CheckInventoryFull())
            {
                playerInventory.AddItemServerRpc(item.itemID);
                playerStatusController.DeductMoneyServerRpc(item.Price);
            }
        }
    }
}