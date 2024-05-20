using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownSelection : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    public Advance_Graph advance_Graph;

    private void Start() {
        advance_Graph = GameObject.Find("Player_UI").GetComponentInChildren<Advance_Graph>(true);
        dropdown.onValueChanged.AddListener(delegate { ChangePlayerDataGraph(); });
    }

    public void AddOption(string option){
        dropdown.options.Add(new TMP_Dropdown.OptionData(option));

        dropdown.RefreshShownValue();
    }

    public void ShowplayerName(string playerName){
        int playerIndex = dropdown.options.FindIndex(option => option.text.Equals(playerName));
        
        if (playerIndex != -1) {
            dropdown.value = -1;
            dropdown.value = playerIndex;

            dropdown.RefreshShownValue();
        } else {
            Debug.LogWarning("Player name not found in dropdown options.");
        }
    }

    public void ChangePlayerDataGraph(){
        Debug.Log("ChangePlayerDataGraph called.");
        int currectEntryIndex = dropdown.value;
        string currectPlayerSelection = dropdown.options[currectEntryIndex].text;
        Debug.Log($"Current selection: {currectPlayerSelection}");
        advance_Graph.SetUsername(currectPlayerSelection, false);
    }

}
