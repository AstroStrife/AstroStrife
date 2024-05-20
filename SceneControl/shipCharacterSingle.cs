using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class shipCharacterSingle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI shipNameText;
    public string shipname;

    public void SetShip(){
        Debug.Log("Ship : " + shipname);
        LobbyManager.Instance.UpdatePlayerShip(shipname);
        ShowConfirmCharacterUI.Instance.UpdatePlayerShip(shipname);
    }
    public void SetName(string name)
    {
        shipNameText.text = name;
        shipname = name;
    }

}
