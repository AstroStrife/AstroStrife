using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class User
{
    [FirestoreProperty]
    public string username { get; set; }

    [FirestoreProperty]
    public double level { get; set; }

    [FirestoreProperty]
    public List<string> ship { get; set; }

    [FirestoreProperty]
    public List<string> driver { get; set; }

    [FirestoreProperty]
    public List<string> game_histories { get; set; }
}
