using UnityEngine;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

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
            //item.itemScriptableObject = item.GetScriptableObject();
        }
    }

}

public class DataSaveLoadHander : MonoBehaviour
{
    // const
    private const string COLLECTIONPATH_INVENTORY = "itemsInventory";
    private const string FIELDNAME_ITEMSLIST = "itemsList";

    public static DataSaveLoadHander Instance;
    public string userId;

    public PlayerDataToFireStore playerDataToFireStore;
    public InventoryDataToFireStore inventoryDataToFireStore;
    
    /* [Header ("Item.ScritapleObjects")]
    [SerializeField] ItemScriptableObject IKnife01_SO;
    [SerializeField] ItemScriptableObject IPistol01_SO;
    [SerializeField] ItemScriptableObject IRifle01_SO; */

    //others
    FirebaseFirestore _firebaseFirestore;
    FirestoreDataManager firestoreDataManager;
    CachedFirestoreDataManager cacheFirestoreDataManager;

    public List<Item> AA() {
        return new List<Item>();
    }

    private void Awake() {
        _firebaseFirestore = FirebaseFirestore.DefaultInstance;
        firestoreDataManager = GetComponent<FirestoreDataManager>();
        cacheFirestoreDataManager = GetComponent<CachedFirestoreDataManager>();

        if(Instance != null && this.gameObject != null) {
            Destroy(this.gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Start() {
        DontDestroyOnLoad(this);
    }

    public PlayerDataToFireStore ReturnPlayerData(string username, int currLevel, int hightScore, int coins) {
        return new PlayerDataToFireStore(username, currLevel, hightScore, coins);
    }

    // ham khoi tao va gan list
    private InventoryDataToFireStore ReturnInventoryData() {
        CreateNewItemListJson(ItemAssets.Instance.IKnife01_SO, 1);
        CreateNewItemListJson(ItemAssets.Instance.IPistol01_SO, 1);
        CreateNewItemListJson(ItemAssets.Instance.IRifle01_SO, 1);
        return new InventoryDataToFireStore(inventoryDataToFireStore.itemsListJson);
    }

    private void CreateNewItemListJson(ItemScriptableObject ItemS, int amount) {
        var item = new Item {itemsType = ItemS.itemType, amount = amount, itemScriptableObject = ItemS};
        inventoryDataToFireStore.itemsListJson.Add(item);
    }
#region PLAYER
    public async void SavePlayerDataToSignup(string userName, string userId) {
        // tao moi doi tuong
        /* PlayerDataToFireStore playerDataToSignup = new PlayerDataToFireStore(userName, 1, 0, 0); */

        // ham khoi tao
        PlayerDataToFireStore playerDataToSignup = ReturnPlayerData(userName, 1, 0, 0);

        // conver to string -> gan vao SetAsyn still OK
        /* string dataToFireStore = JsonUtility.ToJson(playerDataToSignup); */

        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToSignup);
    }

    // Player
    public async void SavePlayerDataFireStore() {
        //? asyn
        await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);

        //? sync
        /* _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore); */
    }

    public async void LoadPlayerDataFireStore() {
        //? asyn
        var snapshot = await _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync();
        if(snapshot.Exists) {
            playerDataToFireStore = snapshot.ConvertTo<PlayerDataToFireStore>();
        }

        //? sync
        /*  _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync().ContinueWith(task => {
            if(task.Result.Exists) {
                playerDataToFireStore = task.Result.ConvertTo<PlayerDataToFireStore>();
            }
        }); */

    }
#endregion PLAYER

#region INVENOTRY
    public async void SaveInventoryDataFireStoreToSignUp() {
        //? asyn
        /* InventoryDataToFireStore inventoryDataToFireStore = ReturnInventoryDataToSignUp(); */

        // tao list cho doi tuong InventoryJson
        ReturnInventoryData();

        //? save to firestore directly
        // await firestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
        //                                         FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);
        
        //? save to cache and online
        await cacheFirestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
                                                FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);

        foreach (var item in inventoryDataToFireStore.itemsListJson)
            Debug.Log($"_____type" + item.itemsType + "_____name " + item.itemScriptableObject.name);
    }

    public async void SaveInventoryDataFireStoreRealtime() {
        if(inventoryDataToFireStore.itemsListJson.Count <= 0 ) return;

        //? asyn
        /* InventoryDataToFireStore inventoryDataToFireStore = ReturnInventoryDataToSignUp(); */

        //? save to firestore directly
        // await firestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
        //                                         FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);
        
        //? save to cache and online
        await cacheFirestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
                                                FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);

        foreach (var item in inventoryDataToFireStore.itemsListJson)
            Debug.Log($"_____type" + item.itemsType + "_____name " + item.itemScriptableObject.name);
    }

    public async void LoadInventoryDataFireStore() {
        //? asyn
        /* inventoryDataToFireStore.itemsListJson = await LoadInventory(); */

        //? load from online
        // inventoryDataToFireStore.itemsListJson = 
        //     await firestoreDataManager.LoadItemsList(COLLECTIONPATH_INVENTORY, userId, FIELDNAME_ITEMSLIST);

        //? load from online or cache - SyncCacheWithFirestore()
        inventoryDataToFireStore.itemsListJson = 
            await cacheFirestoreDataManager.LoadItemsList(COLLECTIONPATH_INVENTORY, userId, FIELDNAME_ITEMSLIST);

        //? check item.type return item SO -> use it to nexprocess
        inventoryDataToFireStore.LoadSO();

        foreach (var item in inventoryDataToFireStore.itemsListJson)
            Debug.Log($"_____type" + item.itemsType + "_____name " + item.itemScriptableObject.name);
    }
#endregion INVENOTRY
}