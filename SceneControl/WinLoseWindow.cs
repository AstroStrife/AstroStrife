using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseWindow : MonoBehaviour
{
    public static WinLoseWindow Instance { get; private set; }

    
    [SerializeField] public TextMeshProUGUI winLoseStatus;
    
    private void Awake(){
        Instance = this;
        Hide();
        
    }

    public IEnumerator GoEndGameWindow(){
        yield return new WaitForSeconds(3);
        EndGameWindow.Instance.Show();
    }
    public void Show() {
        gameObject.SetActive(true);
        StartCoroutine(GoEndGameWindow());
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
