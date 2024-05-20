using System.Collections.Generic;
using UnityEngine;

public class BushScript : MonoBehaviour
{
    private int topPlayersInside = 0;
    private int bottomPlayersInside = 0;
    private List<GameObject> playersInside = new List<GameObject>();

    public void PlayerEntered(GameObject player, string team)
    {
        playersInside.Add(player);
        if (team == "Top")
        {
            topPlayersInside++;
        }
        else if (team == "Bottom")
        {
            bottomPlayersInside++;
        }

        UpdateVisibility();
    }

    public void PlayerExited(GameObject player, string team)
    {
        playersInside.Remove(player);
        if (team == "Top")
        {
            topPlayersInside--;
        }
        else if (team == "Bottom")
        {
            bottomPlayersInside--;
        }
    }

    private void UpdateVisibility()
    {
        if (topPlayersInside > 0 && bottomPlayersInside > 0)
        {
            foreach (var player in playersInside)
            {
                player.GetComponent<PlayerStatusController>().ChangeVisibility("Default");
            }
        }
        else
        {
            foreach (var player in playersInside)
            {
                player.GetComponent<PlayerStatusController>().ChangeVisibility(player.tag);
            }
        }
    }
}