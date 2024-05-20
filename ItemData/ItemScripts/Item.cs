using UnityEngine;

public class Item : ScriptableObject
{
    public string itemName;
    public int itemID;
    public Sprite itemIcon;
    public int Price;
    public virtual void Use(PlayerStatusController playerStatusController)
    {
        Debug.Log("Using item: " + itemName);
    }
}