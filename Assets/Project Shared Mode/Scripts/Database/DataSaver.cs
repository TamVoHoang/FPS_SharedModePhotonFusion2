using System.Collections;
using UnityEngine;
using System;
using Firebase.Database;

/// <summary>
/// save to realtime firebase
/// </summary>

[Serializable]
public class DataToSave {
    public string userName;
    public int killedCount;
    public int deathCount;
    public int coins;

    public DataToSave() {}
    public DataToSave(string userName, int killedCount, int deathCount, int coins) {
        this.userName = userName;
        this.killedCount = killedCount;
        this.deathCount = deathCount;
        this.coins = coins;
    }
}

//? realtime database
public class DataSaver : MonoBehaviour
{
    public static DataSaver Instance;

    public string userId;
    public DataToSave dataToSave;
    DatabaseReference dbRef;

    // buttons
    /* [SerializeField] Button saveButton;
    [SerializeField] Button loadButton; */

    private void Awake() {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        if(Instance != null && this.gameObject != null) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Start() {
        /* saveButton.onClick.AddListener(SaveData);
        loadButton.onClick.AddListener(LoadDaTa); */
        DontDestroyOnLoad(this);
    }

    public DataToSave ReturnDataToSave(string username, int killedCount, int deathCount, int coins) {
        return new DataToSave(username, killedCount, deathCount, coins);
    }

    public void SaveToSignup(string userName, string userId) {
        DataToSave saveDataToSignup = ReturnDataToSave(userName, 0, 0, 1000);
        // chuyen dataToSave -> json
        string json = JsonUtility.ToJson(saveDataToSignup);

        // tao folder trong database realtime
        dbRef.Child("Users").Child(userId).SetRawJsonValueAsync(json);
    }
    
    #region  SAVE LOAD FIREBASE
    public void SaveData() {
        // chuyen dataToSave -> json
        string json = JsonUtility.ToJson(dataToSave);

        // tao folder trong database realtime
        dbRef.Child("Users").Child(userId).SetRawJsonValueAsync(json);
    }

    public void LoadData() {
        StartCoroutine(LoadDataCO());
    }

    IEnumerator LoadDataCO() {
        var serverData = dbRef.Child("Users").Child(userId).GetValueAsync();
        yield return new WaitUntil(() => serverData.IsCompleted);

        Debug.Log($"load process complete");

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if(jsonData != null) {
            Debug.Log($"found jsonData");
            dataToSave = JsonUtility.FromJson<DataToSave>(jsonData);
        }
        else {
            Debug.Log("jsonData not found");
        }
    }
    #endregion SAVE LOAD FIREBASE
    
}
