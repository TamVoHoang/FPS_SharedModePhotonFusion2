using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreDataManager : MonoBehaviour
{
    FirebaseFirestore _firebaseFirestore;

    private void Awake() {
        _firebaseFirestore = FirebaseFirestore.DefaultInstance;
    }
    
#region SAVE METHOD
    // Example method to save a list of strings
    public async Task SaveStringList(string collectionPath, string documentId, string fieldName, List<string> stringList) {
        await SaveListGenericToFirestore(collectionPath, documentId, fieldName, stringList);
    }

    // Example method to save a list of custom objects
    public async Task SaveItemsList(string collectionPath, string documentId, string fieldName, List<Item> items) {
        await SaveListGenericToFirestore(collectionPath, documentId, fieldName, items);
    }

    // Generic method to save a list of any type
    async Task SaveListGenericToFirestore<T>(string collectionPath, string documentId, string fieldName, List<T> dataList)
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

#endregion SAVE METHOD

#region LOAD METHOD

    // Example method to load achievements
    public async Task<List<string>> LoadString() {
        return await LoadStringList("gameData", "playerProgress", "achievements");
    }

    // Example method to load Items list trong inventory json
    public async Task<List<Item>> LoadItemsList(string collectionPath, string documentId, string fieldName) {
        return await LoadCustomObjectList<Item>(collectionPath, documentId, fieldName);
    }

    // Generic method to load a list of strings
    async Task<List<string>> LoadStringList(string collectionPath, string documentId, string fieldName)
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
    async Task<List<T>> LoadCustomObjectList<T>(string collectionPath, string documentId, string fieldName)
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
#endregion LOAD MEHTOD

    // example
    [FirestoreData]
    public class PlayerScore
    {
        [FirestoreProperty]
        public string PlayerName { get; set; }

        [FirestoreProperty]
        public int Score { get; set; }
    }

}