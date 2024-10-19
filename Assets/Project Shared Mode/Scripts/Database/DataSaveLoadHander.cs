using UnityEngine;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Threading.Tasks;
using Firebase.Extensions;

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

[FirestoreData]
[Serializable]
public class InventoryDataToFireStore {

    public List<Item> itemsListJson = new List<Item>();
    [FirestoreProperty]
    public List<Item> ItemsListJson {get => itemsListJson; set => itemsListJson = value;}

    public InventoryDataToFireStore(List<Item> itemsListJson) {
        this.itemsListJson = itemsListJson;
    }

    public void LoadSO() {
        foreach (var item in itemsListJson)
        {
            item.itemScriptableObject = item.GetScriptableObject();
        }
    }
}

public class DataSaveLoadHander : MonoBehaviour
{
    public static DataSaveLoadHander Instance;
    public string userId;

    public PlayerDataToFireStore playerDataToFireStore;
    public InventoryDataToFireStore inventoryDataToFireStore;
    
    [Header ("Item.ScritapleObjects")]
    [SerializeField] ItemScriptableObject IKnife01_SO;
    [SerializeField] ItemScriptableObject IPistol01_SO;
    [SerializeField] ItemScriptableObject IRifle01_SO;

    //others
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

    private void CreateNewItemListJson_ToSignUp(ItemScriptableObject ItemS, int amount) {
        var item = new Item {itemsType = ItemS.itemType, amount = amount, itemScriptableObject = ItemS};
        inventoryDataToFireStore.itemsListJson.Add(item);
    }

    // ham khoi tao va gan list
    private InventoryDataToFireStore ReturnInventoryDataToSignUp() {
        CreateNewItemListJson_ToSignUp(IKnife01_SO, 1);
        CreateNewItemListJson_ToSignUp(IPistol01_SO, 1);
        CreateNewItemListJson_ToSignUp(IRifle01_SO, 1);
        return new InventoryDataToFireStore(inventoryDataToFireStore.itemsListJson);
    }

    public PlayerDataToFireStore ReturnPlayerDataToSave(string username, int currLevel, int hightScore, int coins) {
        return new PlayerDataToFireStore(username, currLevel, hightScore, coins);
    }

    public async void SaveToSignup(string userName, string userId) {
        PlayerDataToFireStore playerDataToSignup = ReturnPlayerDataToSave(userName, 1, 0, 0);

        /* PlayerDataToFireStore playerDataToSignup = new PlayerDataToFireStore(userName, 1, 0, 0); */

        // conver to string
        // string dataToFireStore = JsonUtility.ToJson(playerDataToSignup);

        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToSignup);
    }
    #region FIRESTORE

    // Player
    public async void SavePlayerDataFireStore() {
        //? sync
        /* _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);
        _firebaseFirestore.Document($"itemsInventory/{userId}").SetAsync(inventoryDataToFireStore); */
        
        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);
    }

    public async void LoadPlayerDataFireStore() {
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
    }

    // Inventory
    public async void SaveInventoryDataFireStore() {
        //? asyn
        /* InventoryDataToFireStore inventoryDataToFireStore = ReturnInventoryDataToSignUp(); */
        
        /* List<PlayerScore> scores = new List<PlayerScore>
        {
            new PlayerScore { PlayerName = "Player1", Score = 1000 },
            new PlayerScore { PlayerName = "Player2", Score = 850 }
        }; */

        // tao list cho doi tuong InventoryJson
        ReturnInventoryDataToSignUp();

        await SaveInventory(inventoryDataToFireStore.itemsListJson);
        foreach (var item in inventoryDataToFireStore.itemsListJson)
        {
            Debug.Log($"_____type" + item.itemsType + "_____amount" + item.amount);
        }
    }

    public async void LoadInventoryDataFireStore() {
        //? asyn
        inventoryDataToFireStore.itemsListJson = await LoadInventory();
        inventoryDataToFireStore.LoadSO();
        foreach (var item in inventoryDataToFireStore.itemsListJson)
        {
            Debug.Log($"_____type" + item.itemsType + "_____amount" + item.amount);
        }
    }
    #endregion FIRESTORE

    #region SAVE
    //todo Generic method to save a list of any type
    public async Task SaveListToFirestore<T>(string collectionPath, string documentId, string fieldName, List<T> dataList)
    {
        try
        {
            // Create a document reference
            DocumentReference docRef = _firebaseFirestore.Collection(collectionPath).Document(documentId);
            
            // Create dictionary to hold the data
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { fieldName, dataList }
            };
            
            // Save to Firestore
            await docRef.SetAsync(data).ContinueWithOnMainThread(task => {
                if (task.IsCompleted)
                {
                    Debug.Log($"Successfully saved list to {collectionPath}/{documentId}");
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Error saving list: " + task.Exception);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving list: {e.Message}");
        }
    }

    // Example method to save a list of strings
    public async Task SaveStringList(List<string> stringList) {
        await SaveListToFirestore("gameData", "playerProgress", "achievements", stringList);
    }

    // Example method to save a list of custom objects
    public async Task SaveInventory(List<Item> items) {
        await SaveListToFirestore("itemsInventory", userId, "items", items);
    }
    #endregion SAVE

    #region LOAD
        // Example method to load achievements
    public async Task<List<string>> LoadString() {
        return await LoadStringList("gameData", "playerProgress", "achievements");
    }

    // Example method to load player scores
    public async Task<List<Item>> LoadInventory() {
        return await LoadCustomObjectList<Item>("itemsInventory", userId, "items");
    }

    // Generic method to load a list of strings
    public async Task<List<string>> LoadStringList(string collectionPath, string documentId, string fieldName)
    {
        try
        {
            DocumentSnapshot snapshot = await _firebaseFirestore.Collection(collectionPath).Document(documentId)
                .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                List<object> rawList = snapshot.GetValue<List<object>>(fieldName);
                return rawList.ConvertAll(item => item.ToString());
            }
            else
            {
                Debug.Log($"No document found at {collectionPath}/{documentId}");
                return new List<string>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading string list: {e.Message}");
            return new List<string>();
        }
    }

    // Generic method to load a list of custom objects
    public async Task<List<T>> LoadCustomObjectList<T>(string collectionPath, string documentId, string fieldName)
    {
        try
        {
            DocumentSnapshot snapshot = await _firebaseFirestore.Collection(collectionPath).Document(documentId)
                .GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                if (data.ContainsKey(fieldName))
                {
                    return snapshot.GetValue<List<T>>(fieldName);
                }
            }
            Debug.Log($"No data found at {collectionPath}/{documentId}/{fieldName}");
            return new List<T>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading custom object list: {e.Message}");
            return new List<T>();
        }
    }


    #endregion LOAD

    [FirestoreData]
    public class PlayerScore
    {
        [FirestoreProperty]
        public string PlayerName { get; set; }

        [FirestoreProperty]
        public int Score { get; set; }
    }


}