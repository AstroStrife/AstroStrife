

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby_MiniGame : MonoBehaviour
{
    public static Lobby_MiniGame Instance { get; private set; }

    [SerializeField] private Button BackButton;
    private void Awake() {
        Instance = this;
        Hide();

        BackButton.onClick.AddListener(() => {
          Hide();
          MainManu.Instance.Show();
        });
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
