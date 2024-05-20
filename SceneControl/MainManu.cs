using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManu : MonoBehaviour
{
    public static MainManu Instance { get; private set; }

    [SerializeField] private Button QuickPlayButton;
    [SerializeField] private Button LobbyButton;
    [SerializeField] private Button ProfileButton;
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private TMP_Text UsernameText;
    [SerializeField] private TMP_Text LevelText;

    public User user;

    private void Awake()
    {
        Instance = this;

        if(GameObject.Find("FirebaseManager").GetComponent<AuthManager>().Currentuser != null){
            user = GameObject.Find("FirebaseManager").GetComponent<AuthManager>().Currentuser;

            SetUsername(user.username);
            Setlevel(user.level);

            Show();
        }else{
            Hide();
        }

        // QuickPlayButton.onClick.AddListener(() =>
        // {
        //     Hide();
        //     LobbyList.Instance.Show();
        // });
        LobbyButton.onClick.AddListener(() =>
        {
            //Debug.Log(AuthManager.Instance.user.username + " Access to Lobby page");
            Hide();
            LobbyList.Instance.Show();
        });
        ProfileButton.onClick.AddListener(() =>
        {
            //Debug.Log(AuthManager.Instance.user.username + " Access to Profile page");
            Hide();
            Profile.Instance.Show();
        });
        SettingButton.onClick.AddListener(() =>
        {
            //Debug.Log(AuthManager.Instance.user.username + " Access to Setting page");
            Hide();
            Setting.Instance.Show();
        });
        ExitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

    }

     private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("Load Scene Complete");
    }

    private void Update()
    {
        // QuickPlayButtonEnable();

        if(user == null && GameObject.Find("FirebaseManager").GetComponent<AuthManager>().user != null){
            user = GameObject.Find("FirebaseManager").GetComponent<AuthManager>().user;

            SetUsername(user.username);
            Setlevel(user.level);
        }

    }

    public void SetUsername(string name)
    {
        UsernameText.text = name;
    }
    public void Setlevel(double level)
    {
        LevelText.text = "lv : " + level.ToString();
    }

    // public void QuickPlayButtonEnable()
    // {
    //     if (GameObject.Find("LobbyManager").GetComponent<LobbyManager>().InternetConnection == true)
    //     {
    //         QuickPlayButton.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         QuickPlayButton.gameObject.SetActive(false);
    //     }
    // }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
