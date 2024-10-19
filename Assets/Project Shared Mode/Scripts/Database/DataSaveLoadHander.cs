using UnityEngine;
using Firebase.Firestore;
using System;

/// <summary>
/// Save to Firestore
/// </summary>

[Serializable]
[FirestoreData]
public class PlayerDataToFireStore {
    public string userName;
    public int currLevel;
    public int highScore;
    public int coins;

    [FirestoreProperty]
    public string UserName { get => userName; set=> userName = value; }

    [FirestoreProperty]
    public int CurrentLevel { get => currLevel; set => currLevel = value; }

    [FirestoreProperty]
    public int HighScore { get => highScore; set => highScore = value; }

    [FirestoreProperty]
    public int Coins { get => coins; set => coins = value; }

    public PlayerDataToFireStore() {}
    public PlayerDataToFireStore(string userName, int currLevel, int highScore, int coins) {
        this.userName = userName;
        this.currLevel = currLevel;
        this.highScore = highScore;
        this.coins = coins;
    }
}


[Serializable]
[FirestoreData]
public class InventoryDataToFireStore {
    public string[] weaponNames = new string[3];
    [FirestoreProperty]
    public string[] WeaponNames { get => weaponNames; set => weaponNames = value; }
}

public class DataSaveLoadHander : MonoBehaviour
{
    public static DataSaveLoadHander Instance;
    public string userId;
    public PlayerDataToFireStore playerDataToFireStore;
    public InventoryDataToFireStore inventoryDataToFireStore;
    FirebaseFirestore _firebaseFirestore;

    // Buttons
    /* [SerializeField] Button saveButton;
    [SerializeField] Button loadButton; */

    private void Awake() {
        _firebaseFirestore = FirebaseFirestore.DefaultInstance;

        if(Instance != null && this.gameObject != null) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }

    }

    private void Start() {
        /* saveButton.onClick.AddListener(SaveFireStore);
        loadButton.onClick.AddListener(LoadFireStore); */

        DontDestroyOnLoad(this);
    }


    public PlayerDataToFireStore ReturnPlayerDataToSave(string username, int currLevel, int hightScore, int coins) {
        return new PlayerDataToFireStore(username, currLevel, hightScore, coins);
    }


    public async void SaveToSignup(string userName, string userId) {
        PlayerDataToFireStore playerDataToSignup = ReturnPlayerDataToSave(userName, 1, 0, 0);

        //PlayerDataToFireStore playerDataToSignup = new PlayerDataToFireStore(userName, 1, 0, 0);

        // conver to string
        // string dataToFireStore = JsonUtility.ToJson(playerDataToSignup);

        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToSignup);
        //await _firebaseFirestore.Document($"itemsInventory/{userId}").SetAsync(inventoryDataToFireStore);
    }

    #region FIRESTORE
    public async void SaveFireStore() {

        //? sync
        //_firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);
        //_firebaseFirestore.Document($"itemsInventory/{userId}").SetAsync(inventoryDataToFireStore);
        
        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);
        await _firebaseFirestore.Document($"itemsInventory/{userId}").SetAsync(inventoryDataToFireStore);
    }


    public async void LoadFireStore() {
        //? sync
        /* _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync().ContinueWith(task => {
            if(task.Result.Exists) {
                playerDataToFireStore = task.Result.ConvertTo<PlayerDataToFireStore>();
            }
        });

        _firebaseFirestore.Document($"itemsInventory/{userId}").GetSnapshotAsync().ContinueWith(task => {
            if(task.Result.Exists) {
                inventoryDataToFireStore = task.Result.ConvertTo<InventoryDataToFireStore>();
            }
        }); */

        //? asyn
        var snapshot = await _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync();
        if(snapshot.Exists) {
            playerDataToFireStore = snapshot.ConvertTo<PlayerDataToFireStore>();
        }

        var snapshot_ = await _firebaseFirestore.Document($"itemsInventory/{userId}").GetSnapshotAsync();
        if(snapshot.Exists) {
            inventoryDataToFireStore = snapshot_.ConvertTo<InventoryDataToFireStore>();
        }
    }
    #endregion FIRESTORE

}