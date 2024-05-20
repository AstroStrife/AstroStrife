using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapMark : MonoBehaviour
{
    public Sprite Player_Self;
    public Sprite Player_Friend;
    public Sprite Player_Enemy;

    private Image imageComponent;
    private RectTransform rectTransform;
    public static string localPlayerTag;
    public string parentTag;

    public static bool RecheckFinish = false;
    public bool isSelf = false;
    private bool isPlayer = false;
    private bool isBoss = false;

    HashSet<string> excludedNames = new HashSet<string> { "minion", "boss" };

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        string parentName = transform.parent.parent.gameObject.name.ToLower();
        isPlayer = !excludedNames.Any(name => parentName.Contains(name));
        isBoss = parentName.Contains("boss");
    }

    void Start()
    {
        StartCoroutine(WaitForReCheck());
    }
    public IEnumerator WaitForReCheck()
    {
        while (RecheckFinish == false)
        {
            yield return null;
        }
        parentTag = transform.parent.parent.gameObject.tag;
        if (isSelf)
        {
            imageComponent.sprite = Player_Self;
        }
        else if (isPlayer)
        {
            imageComponent.sprite = parentTag == MiniMapMark.localPlayerTag ? Player_Friend : Player_Enemy;
        }
        else if (!isBoss && !isPlayer)
        {
            imageComponent.color = parentTag == MiniMapMark.localPlayerTag ? Color.green : Color.red;
            rectTransform.sizeDelta = new Vector2(50, 50);
        }
        else
        {
            imageComponent.color = Color.red;
            rectTransform.sizeDelta = new Vector2(50, 50);
        }
    }
}