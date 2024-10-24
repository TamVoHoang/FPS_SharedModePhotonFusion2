using UnityEngine;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Save to Firestore
/// </summary>


[Serializable]
[FirestoreData]
public class PlayerDataToFireStore {
    [SerializeField] string userName;
    [SerializeField] int currLevel;
    [SerializeField] int highScore;
    [SerializeField] int coins;

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
    [SerializeField] string inventoryName;
    [FirestoreProperty]
    public string InventoryName {get => inventoryName; set => inventoryName = value;}

    [NonSerialized]
    public List<Item> itemsListJson = new List<Item>();
    [FirestoreProperty]
    public List<Item> ItemsListJson {get => itemsListJson; set => itemsListJson = value;}

    // Add this parameterless constructor
    public InventoryDataToFireStore() {
        itemsListJson = new List<Item>();
    }

    public InventoryDataToFireStore(List<Item> itemsListJson) {
        this.itemsListJson = itemsListJson;
    }

    public void ConvertItemTypeToSO() {
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
        var item = new Item {ItemsType = ItemS.itemType, Amount = amount, itemScriptableObject = ItemS};
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

        //? asyn - no cache support
        // await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToSignup);

        //! save generic Object cache support
        await cacheFirestoreDataManager.SaveGenericToFirestore("usersInfo", userId, "playerData", playerDataToSignup);

    }
    
    // Player
    public async void SavePlayerDataFireStore() {
        //? sync _ Not using
        /* _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore); */

        //? asyn - no cache support
        // await _firebaseFirestore.Document($"usersInfo/{userId}").SetAsync(playerDataToFireStore);

        //! save generic Object cache support
        await cacheFirestoreDataManager.SaveGenericToFirestore("usersInfo", userId, "playerData", playerDataToFireStore);

    }

    public async void LoadPlayerDataFireStore() {
        //? sync _ Not using
        /*  _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync().ContinueWith(task => {
            if(task.Result.Exists) {
                playerDataToFireStore = task.Result.ConvertTo<PlayerDataToFireStore>();
            }
        }); */

        //? asyn
        // var snapshot = await _firebaseFirestore.Document($"usersInfo/{userId}").GetSnapshotAsync();
        // if(snapshot.Exists) {
        //     playerDataToFireStore = snapshot.ConvertTo<PlayerDataToFireStore>();
        // }

        //! save generic Object cache support
        playerDataToFireStore = await cacheFirestoreDataManager.LoadGenericObject<PlayerDataToFireStore>("usersInfo", userId, "playerData");

    }
#endregion PLAYER

#region INVENOTRY
    public async void SaveInventoryDataFireStoreToSignUp() {
        //? asyn
        /* InventoryDataToFireStore inventoryDataToFireStore = ReturnInventoryDataToSignUp(); */

        // tao list cho doi tuong InventoryJson
        //ReturnInventoryData();

        //? save to firestore directly
        /* await firestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
                                                FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson); */
        
        //? save to cache and online
        // await cacheFirestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
        //                                         FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);

        // foreach (var item in inventoryDataToFireStore.itemsListJson)
        //     Debug.Log($"_____type" + item.ItemsType + "_____name " + item.itemScriptableObject.name);


        try {
        // Create initial inventory
        ReturnInventoryData();

        // Save using generic method
        await cacheFirestoreDataManager.SaveGenericToFirestore(
            COLLECTIONPATH_INVENTORY,
            userId,
            "inventoryData",
            inventoryDataToFireStore
        );

        Debug.Log("Initial inventory saved successfully");
        }
        catch (Exception e) {
            Debug.LogError($"Error saving initial inventory: {e.Message}");
        }
    }

    public async void SaveInventoryDataFireStoreRealtime() {
        if(inventoryDataToFireStore.itemsListJson.Count <= 0 ) return;

        //? asyn
        /* InventoryDataToFireStore inventoryDataToFireStore = ReturnInventoryDataToSignUp(); */

        //? save to firestore directly
        /* await firestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
                                                FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson); */
        
        //? save to cache and online
        // await cacheFirestoreDataManager.SaveItemsList(COLLECTIONPATH_INVENTORY, userId, 
        //                                         FIELDNAME_ITEMSLIST, inventoryDataToFireStore.itemsListJson);

        // foreach (var item in inventoryDataToFireStore.itemsListJson)
        //     Debug.Log($"_____type" + item.ItemsType + "_____name " + item.itemScriptableObject.name);
        
        
        try {
            await cacheFirestoreDataManager.SaveGenericToFirestore(
                COLLECTIONPATH_INVENTORY, 
                userId, 
                "inventoryData",  // fieldName for the whole inventory object
                inventoryDataToFireStore  // passing the whole inventory object
            );

            Debug.Log("Inventory saved successfully");
            
            // Debug log to verify data
            foreach (var item in inventoryDataToFireStore.itemsListJson) {
                Debug.Log($"Saved item - Type: {item.ItemsType}, Amount: {item.Amount}");
            }
        }
        catch (Exception e) {
            Debug.LogError($"Error saving inventory: {e.Message}");
        }
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

        //? check item.type return item SO -> use it to next process
        //inventoryDataToFireStore.LoadSO();

        foreach (var item in inventoryDataToFireStore.itemsListJson)
            Debug.Log($"_____type" + item.ItemsType + "_____name " + item.itemScriptableObject.name);
    }

    public async Task<bool> LoadInventoryDataFireStore_() {
    try {
        inventoryDataToFireStore = await cacheFirestoreDataManager.LoadGenericObject<InventoryDataToFireStore>(
            COLLECTIONPATH_INVENTORY,
            userId,
            "inventoryData"
        );

        if (inventoryDataToFireStore != null && inventoryDataToFireStore.itemsListJson != null) {
            // Convert item types to ScriptableObjects after loading
            inventoryDataToFireStore.ConvertItemTypeToSO();
            
            // Debug log to verify data
            foreach (var item in inventoryDataToFireStore.itemsListJson) {
                Debug.Log($"Loaded item - Type: {item.ItemsType}, Amount: {item.Amount}");
            }
            return true;
        }
        else {
            Debug.LogWarning("No inventory data found");
            inventoryDataToFireStore = new InventoryDataToFireStore(new List<Item>());
            return false;
        }
    }
    catch (Exception e) {
        Debug.LogError($"Error loading inventory: {e.Message}");
        return false;
    }
}
#endregion INVENOTRY
}