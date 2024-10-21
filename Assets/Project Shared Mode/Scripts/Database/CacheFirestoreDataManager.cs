using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;


public class CachedFirestoreDataManager : MonoBehaviour
{
    private FirebaseFirestore _firebaseFirestore;
    private string _cachePath;
    private bool _isOnline;
    private Dictionary<string, string> _dataHashes;
    private Dictionary<string, DateTime> _lastAccessTimes;
    private Dictionary<string, TaskCompletionSource<bool>> _pendingOperations;

    private void Awake()
    {
        _firebaseFirestore = FirebaseFirestore.DefaultInstance;
        _cachePath = Path.Combine(Application.persistentDataPath, "firestore_cache");
        _lastAccessTimes = new Dictionary<string, DateTime>();
        _dataHashes = new Dictionary<string, string>();
        _pendingOperations = new Dictionary<string, TaskCompletionSource<bool>>();
        
        if (!Directory.Exists(_cachePath))
        {
            Directory.CreateDirectory(_cachePath);
        }
        
        CheckConnectivity();
        LoadAccessTimes();
        LoadDataHashes();
    }

    private void OnApplicationQuit()
    {
        SaveAccessTimes();
        SaveDataHashes();
    }

    #region OPERATION LOCK MANAGEMENT

    private async Task<bool> AcquireOperationLock(string key)
    {
        if (_pendingOperations.TryGetValue(key, out var existingOperation))
        {
            await existingOperation.Task;
            return false;
        }

        var tcs = new TaskCompletionSource<bool>();
        _pendingOperations[key] = tcs;
        return true;
    }

    private void ReleaseOperationLock(string key)
    {
        if (_pendingOperations.TryGetValue(key, out var tcs))
        {
            tcs.TrySetResult(true);
            _pendingOperations.Remove(key);
        }
    }

    #endregion

    #region HASH MANAGEMENT

    private void LoadDataHashes()
    {
        string hashesPath = Path.Combine(_cachePath, "data_hashes.json");
        if (File.Exists(hashesPath))
        {
            try
            {
                string json = File.ReadAllText(hashesPath);
                _dataHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) 
                            ?? new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data hashes: {e.Message}");
                _dataHashes = new Dictionary<string, string>();
            }
        }
    }

    private void SaveDataHashes()
    {
        string hashesPath = Path.Combine(_cachePath, "data_hashes.json");
        try
        {
            string json = JsonConvert.SerializeObject(_dataHashes);
            File.WriteAllText(hashesPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data hashes: {e.Message}");
        }
    }

    private string CalculateHash<T>(List<T> data)
    {

        string jsonData = JsonConvert.SerializeObject(data);
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    #endregion

    #region SAVE METHODS
    // Example method to load Items list trong inventory json
    public async Task<List<Item>> LoadItemsList(string collectionPath, string documentId, string fieldName) {
        return await LoadCustomObjectList<Item>(collectionPath, documentId, fieldName);
    }
    // Example method to save a list of custom objects
    public async Task SaveItemsList(string collectionPath, string documentId, string fieldName, List<Item> items) {
        await SaveListGenericToFirestore(collectionPath, documentId, fieldName, items);
    }

    public async Task SaveListGenericToFirestore<T>(string collectionPath, string documentId, string fieldName, List<T> dataList)
    {
        string cacheKey = GetCacheKey(collectionPath, documentId, fieldName);
        
        // Try to acquire operation lock
        bool lockAcquired = await AcquireOperationLock(cacheKey);
        if (!lockAcquired)
        {
            Debug.Log($"Operation already in progress for {cacheKey}");
            return;
        }

        try
        {
            string newHash = CalculateHash(dataList);

            // Check if data has actually changed
            if (_dataHashes.TryGetValue(cacheKey, out string existingHash) && existingHash == newHash)
            {
                Debug.Log($"Data hasn't changed for {cacheKey}. Skipping save.");
                return;
            }

            CheckConnectivity();
            if (_isOnline)
            {
                try
                {
                    DocumentReference docRef = _firebaseFirestore.Collection(collectionPath).Document(documentId);
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { fieldName, dataList }
                    };

                    bool success = false;
                    await docRef.SetAsync(data).ContinueWithOnMainThread(task => {
                        success = task.IsCompleted && !task.IsFaulted;
                        if (success)
                        {
                            Debug.Log($"Successfully saved to Firestore: {cacheKey}");
                        }
                        else if (task.IsFaulted)
                        {
                            Debug.LogError($"Error saving to Firestore: {task.Exception}");
                        }
                    });

                    if (success)
                    {
                        // Only update cache and hash after successful Firestore save
                        await SaveToCache(collectionPath, documentId, fieldName, dataList);
                        _dataHashes[cacheKey] = newHash;
                        SaveDataHashes();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error saving to Firestore: {e.Message}");
                    // Save to cache as fallback
                    await SaveToCache(collectionPath, documentId, fieldName, dataList);
                    _dataHashes[cacheKey] = newHash;
                    SaveDataHashes();
                }
            }
            else
            {
                // Offline mode - save to cache only
                await SaveToCache(collectionPath, documentId, fieldName, dataList);
                _dataHashes[cacheKey] = newHash;
                SaveDataHashes();
            }
        }
        finally
        {
            ReleaseOperationLock(cacheKey);
        }
    }
    
    public void Test(List<Item> dataList) {
        var itemsTypeAmount = dataList.Select(item => new
        {
            item.ItemsType,
            item.amount
        }).ToList();
    }

    private async Task SaveToCache<T>(string collectionPath, string documentId, string fieldName, List<T> dataList)
    {
        string filePath = GetCacheFilePath(collectionPath, documentId, fieldName);
        string tempPath = filePath + ".tmp";

        try
        {
            //? kiem tra type of dataList
            /* bool isListItem = dataList is List<Item>;
            Type type = dataList.GetType();
            if(type == typeof(List<Item>)) { } */
            
            string jsonData = JsonConvert.SerializeObject(dataList);  //!OK dung nhung bi loi khi add SO vao trong cache
            await File.WriteAllTextAsync(tempPath, jsonData);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(tempPath, filePath);

            UpdateAccessTime(filePath);
            Debug.Log($"Successfully saved to cache: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving to cache: {e.Message}");
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch { }
            }
            throw;
        }
    }

    #endregion

    #region LOAD METHODS


    public async Task<List<T>> LoadCustomObjectList<T>(string collectionPath, string documentId, string fieldName)
    {
        string cacheKey = GetCacheKey(collectionPath, documentId, fieldName);

        // Try to acquire operation lock
        bool lockAcquired = await AcquireOperationLock(cacheKey);
        if (!lockAcquired)
        {
            Debug.Log($"Load operation already in progress for {cacheKey}");
            return await _pendingOperations[cacheKey].Task.ContinueWith(_ => 
                LoadFromCache<T>(collectionPath, documentId, fieldName));
        }

        try
        {
            CheckConnectivity();
            if (_isOnline)
            {
                try
                {
                    DocumentSnapshot snapshot = await _firebaseFirestore.Collection(collectionPath).Document(documentId)
                        .GetSnapshotAsync();

                    if (snapshot.Exists)
                    {
                        var data = snapshot.GetValue<List<T>>(fieldName);
                        string newHash = CalculateHash(data);

                        // Only update cache if data is different
                        if (!_dataHashes.TryGetValue(cacheKey, out string existingHash) || existingHash != newHash)
                        {
                            await SaveToCache(collectionPath, documentId, fieldName, data);
                            _dataHashes[cacheKey] = newHash;
                            SaveDataHashes();
                        }

                        return data;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load from Firestore: {e.Message}. Falling back to cache.");
                }
            }

            return LoadFromCache<T>(collectionPath, documentId, fieldName);
        }
        finally
        {
            ReleaseOperationLock(cacheKey);
        }
    }

    private List<T> LoadFromCache<T>(string collectionPath, string documentId, string fieldName)
    {
        try
        {
            string filePath = GetCacheFilePath(collectionPath, documentId, fieldName);
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);

                UpdateAccessTime(filePath);
                return JsonConvert.DeserializeObject<List<T>>(jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading from cache: {e.Message}");
        }
        
        return new List<T>();
    }

    #endregion

#region UTILITY METHODS

    private void CheckConnectivity()
    {
        _isOnline = Application.internetReachability != NetworkReachability.NotReachable;
    }

    private void UpdateAccessTime(string filePath)
    {
        _lastAccessTimes[filePath] = DateTime.Now;
        SaveAccessTimes();
    }

    private void LoadAccessTimes()
    {
        string accessTimesPath = Path.Combine(_cachePath, "access_times.json");
        if (File.Exists(accessTimesPath))
        {
            try
            {
                string json = File.ReadAllText(accessTimesPath);
                _lastAccessTimes = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(json)
                                ?? new Dictionary<string, DateTime>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading access times: {e.Message}");
                _lastAccessTimes = new Dictionary<string, DateTime>();
            }
        }
    }

    private void SaveAccessTimes()
    {
        string accessTimesPath = Path.Combine(_cachePath, "access_times.json");
        try
        {
            string json = JsonConvert.SerializeObject(_lastAccessTimes);
            File.WriteAllText(accessTimesPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving access times: {e.Message}");
        }
    }

    private string GetCacheFilePath(string collectionPath, string documentId, string fieldName)
    {
        return Path.Combine(_cachePath, $"{collectionPath}_{documentId}_{fieldName}.json");
    }

    private string GetCacheKey(string collectionPath, string documentId, string fieldName)
    {
        return $"{collectionPath}_{documentId}_{fieldName}";
    }

#endregion UTILITY METHODS
}