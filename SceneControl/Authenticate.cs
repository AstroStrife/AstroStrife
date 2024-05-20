using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Authenticate : MonoBehaviour

{

    public static Authenticate Instance { get; private set; }

    [SerializeField] private Button AuthenticateButton;

    [Header("Sign In")]
    public TMP_InputField email_inputfield;
    public TMP_InputField password_inputfield;
    public TMP_Text text_message_error;

    private void Awake() {

        Instance = this;
        
        AuthenticateButton.onClick.AddListener(() => {
            LobbyManager.Instance.Authenticate("Player");
            Hide();
            MainManu.Instance.Show();
        });

        if(GameObject.Find("FirebaseManager").GetComponent<AuthManager>().user != null){
            Hide();
            MainManu.Instance.Show();
        }
    }

    public void SigninButton()
    {
        StartCoroutine(AuthManager.Instance.SignIn(email_inputfield.text,password_inputfield.text));
        Debug.Log("Sign In with Email and Password as : " + email_inputfield.text + " and " + password_inputfield.text);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
