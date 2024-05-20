using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class driverCharacterSingle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI driverNameText;
    public string drivername;
    public void SetDriver(){
        Debug.Log("Driver : " + drivername);
        LobbyManager.Instance.UpdatePlayerDriver(drivername);
        ShowConfirmCharacterUI.Instance.UpdatePlayerDriver(drivername);
    }
    public void SetName(string name)
    {
        driverNameText.text = name;
        drivername = name;
    }
}
