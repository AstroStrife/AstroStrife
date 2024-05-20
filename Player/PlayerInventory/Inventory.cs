using System.Collections.Generic;
using Unity.Netcode;

public class Inventory : NetworkBehaviour
{
    public List<Item> items = new List<Item>();

    public PlayerStatusController playerStatusController;
    private InputManager _inputManager;

    public override void OnNetworkSpawn()
    {
        if (IsServer || IsClient)
        {
            for (int i = 0; i < 6; i++)
            {
                items.Add(null);
            }
        }
    }

    private void Start()
    {
        playerStatusController = GetComponent<PlayerStatusController>();
        _inputManager = GetComponent<InputManager>();
    }

    public void Update()
    {
        if (!IsOwner) return;
        UseItem();
    }

    private void UseItem()
    {
        int itemIndex = (int)_inputManager.ItemUseInput - 1; // Convert input to item index (0-based)
        if (itemIndex >= 0 && itemIndex < items.Count)
        {
            Item itemToUse = items[itemIndex];
            if (itemToUse != null)
            {
                // Using item log
                GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " use ", items[itemIndex].name);
                itemToUse.Use(playerStatusController);
                items[itemIndex] = null;
                _inputManager.ItemUseInput = 0;
            }
        }
    }

    [ServerRpc]
    public void AddItemServerRpc(int itemId)
    {
        Item item = ItemManager.Instance.GetItemById(itemId);
        if (item != null)
        {
            AddItemClientRpc(itemId);
        }
    }

    [ClientRpc]
    public void AddItemClientRpc(int itemId)
    {
        Item item = ItemManager.Instance.GetItemById(itemId);
        for (int i = 0; i < 6; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                if (IsOwner)
                {
                    // Buy item log, ***Need to be player name
                    GameLogger.Instance.LogActionServerRpc(playerStatusController.PlayerData.Value.playerName.ToString(), " Buy ", item.name);
                }
                return;
            }
        }
    }

    [ServerRpc]
    public void RemoveItemServerRpc(int itemId)
    {
        Item item = ItemManager.Instance.GetItemById(itemId);
        if (item != null)
        {
            RemoveItem(item);
        }
    }

    private void RemoveItem(Item item)
    {
        int index = items.IndexOf(item);
        if (index != -1)
        {
            items[index] = null;
        }
    }


    public bool CheckInventoryFull()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                // If there is a null item, inventory is not full
                return false;
            }
        }

        // If the loop completes, it means there were no null items, so the inventory is full
        return true;
    }
}