using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemStoreShow : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI Name;
    private TextMeshProUGUI Price;
    private Image Icon;
    public Item item;

    private GameObject Canvas;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        Name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
        Price = transform.Find("Price").GetComponent<TextMeshProUGUI>();
        Icon = transform.Find("Image").GetComponent<Image>();

        Name.text = item.itemName;
        Price.text = item.Price.ToString();
        Icon.sprite = item.itemIcon;

        Canvas = transform.parent.gameObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && Time.time - lastClickTime < doubleClickThreshold)
        {
            Item itemToPurchase = Canvas.GetComponent<Player_Store>().GetItemFromClickedIcon(eventData);
            if (itemToPurchase != null)
            {
                Canvas.GetComponent<Player_Store>().PurchaseItem(itemToPurchase);
            }
        }
        lastClickTime = Time.time;
    }
}