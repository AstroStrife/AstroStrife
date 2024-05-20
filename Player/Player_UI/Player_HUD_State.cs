using Unity.Netcode;
using UnityEngine;

public class Player_HUD_State : NetworkBehaviour
{
    public GameObject HUD;
    public MiniMapMark MinimapMark;
    public HealthBar healthBar;

    private void Start()
    {
        if (!IsOwner) return;
        Show();
    }

    public void Show()
    {
        HUD.SetActive(true);
        MinimapMark.isSelf = true;
        healthBar.isSelf = true;
    }
}
