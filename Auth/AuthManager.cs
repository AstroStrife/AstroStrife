using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;



public class AuthManager : MonoBehaviour
{

    public static AuthManager Instance { get; private set; }

    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public AuthResult Users;
    public FirebaseFirestore db;
 
    public User user;
    public User Currentuser;
    
    

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(Task =>
        {
            dependencyStatus = Task.Result;
            if(dependencyStatus == DependencyStatus.Available){
                InitailzeFirebase();
                db = FirebaseFirestore.DefaultInstance;
                
            }else{
                Debug.LogError("Error at Firebase dependencies : " + dependencyStatus);
            }
        });

        DontDestroyOnLoad(gameObject);
   }

    private void InitailzeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }


    public IEnumerator SignIn(string email, string password)
    {
        var SignInTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(predicate: () => SignInTask.IsCompleted);

        if (SignInTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to Sign In because {SignInTask.Exception}");
            FirebaseException firebaseException = SignInTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseException.ErrorCode;
            Debug.Log("Error Code : " + errorCode);

            string errortext = "**error";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    errortext = "**Forget Input Email";
                    break;
                case AuthError.MissingPassword:
                    errortext = "**Forget Input Password";
                    break;
                case AuthError.WrongPassword:
                    errortext = "**Incorrect Password";
                    break;
                case AuthError.UserNotFound:
                    errortext = "**User not found";
                    break;
                case AuthError.InvalidEmail:
                    errortext = "**Invalid Email";
                    break;
                case AuthError.Failure:
                    errortext = "**Incorrect Password or Email";
                    break;
            }
            GameObject.Find("Authenticate").GetComponent<Authenticate>().text_message_error.text = errortext;
        }
        else
        {
            Users = SignInTask.Result;
            Debug.Log("Sign in successfully : " + Users.User.Email);
            GameObject.Find("Authenticate").GetComponent<Authenticate>().text_message_error.text = "";

            GetDataAsync(Users.User.Email);
        }


    }

    private async void GetDataAsync(string user_email){
        DocumentReference docRef = db.Collection("users").Document(user_email);

            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                user = task.Result.ConvertTo<User>();
                Currentuser = task.Result.ConvertTo<User>();
                
                Debug.Log(user.username);

                LobbyManager.Instance.Authenticate(user.username);

                LobbyManager.Instance.SetEmail(user_email);

            });
            
            Authenticate.Instance.Hide();
            MainManu.Instance.Show();
    }

    public async void StoreHistories(string name , List<ScoreManager.ScoreEntry> scoreManager , string time , string winstatus){
        
        DocumentReference docRef;
        docRef = db.Collection("histories").Document(name);
        Dictionary<string, object> game_data = new Dictionary<string, object>
        {   
            {"TeamWinStatus", winstatus},
            {"Time", time}
        };
        await docRef.SetAsync(game_data).ContinueWithOnMainThread(task =>
            {
                Debug.Log("game_data added successfully.");
            });
        foreach(ScoreManager.ScoreEntry score in scoreManager){
            docRef = db.Collection("histories").Document(name).Collection("users").Document(score.email);
             Dictionary<string, object> users = new Dictionary<string, object>
            {
                { "Username", score.username },
                { "Email", score.email },
                { "Team", score.team },
                { "Driver", score.driver },
                { "Ship", score.ship },
                { "TotalGold", score.TotalGold },
                { "TotalPlayerDamage", score.TotalPlayerDamage },
                { "TotalTurretDamage", score.TotalTurretDamage },
                { "TotalDamageReceived", score.TotalDamageReceived },
                { "Kills", score.Kills },
                { "Deaths", score.Deaths },
                { "LaneMinionKills", score.LaneMinionKills },
                { "NeutralBossKills", score.NeutralBossKills },
                { "NeutralMinionKills", score.NeutralMinionKills },
                { "TotalMinionKills", score.TotalMinionKills },
            };
            await docRef.SetAsync(users).ContinueWithOnMainThread(task =>
            {
                Debug.Log("User added successfully.");
            });

            docRef = db.Collection("users").Document(score.email);
            List<string> histories = new List<string>();
            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                user = task.Result.ConvertTo<User>();
                
                foreach(string game_history in user.game_histories){
                    histories.Add(game_history);
                }

            });
            docRef = db.Collection("users").Document(score.email);
            histories.Add(name);
            Dictionary<string, object> update = new Dictionary<string, object>
            {
                { "game_histories", histories}
                                     
            };
            await docRef.UpdateAsync(update).ContinueWithOnMainThread(task => {
            Debug.Log(
                "Updated");
            });

        }
    }
}

