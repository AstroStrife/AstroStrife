using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemShow : MonoBehaviour
{
    private GameObject _ownerPlayerPrefab;
    private Inventory _inventory;
    public List<Image> itemIcons;

    private void Start()
    {
        _ownerPlayerPrefab = transform.parent.parent.gameObject;
        _inventory = _ownerPlayerPrefab.GetComponent<Inventory>();
        UpdateItemIcons();
    }

    private void Update()
    {
        UpdateItemIcons();
    }

    public void UpdateItemIcons()
    {
        for (int i = 0; i < _inventory.items.Count; i++)
        {
            if (_inventory.items[i] != null)
            {
                itemIcons[i].sprite = _inventory.items[i].itemIcon;
            }
            else
            {
                itemIcons[i].sprite = null;
            }
        }
    }
}