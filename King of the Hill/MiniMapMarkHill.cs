using UnityEngine;
using UnityEngine.UI;

public class MiniMapMarkHill : MonoBehaviour
{
    private Image imageComponent;
    public static string localPlayerTag;
    public string parentTag;

    public static bool RecheckFinish = false;


    private void Awake()
    {
        imageComponent = GetComponent<Image>();
    }

    public void ChangeTeam(string parentTag)
    {
        if (parentTag == MiniMapMark.localPlayerTag)
        {
            imageComponent.color = new Color(1f, 0.64f, 0f, 1f);
        }
        else
        {
            imageComponent.color = Color.red;
        }
    }
}
