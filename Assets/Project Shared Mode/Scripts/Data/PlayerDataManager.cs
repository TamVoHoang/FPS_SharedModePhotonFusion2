using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

//! NOT USING 
// Class to hold player data
public class PlayerData
    {
        public string NickName { get; set; }
        // Add other player-related data as needed
    }


public class PlayerDataManager : MonoBehaviour
{
    // Singleton instance
    public static PlayerDataManager Instance { get; private set; }

    private Dictionary<PlayerRef, PlayerData> playerDataDictionary = new Dictionary<PlayerRef, PlayerData>();

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //? Add or update player data
    public void UpdatePlayerData(PlayerRef playerRef, string nickName)
    {
        if (playerDataDictionary.ContainsKey(playerRef))
        {
            playerDataDictionary[playerRef].NickName = nickName;
        }
        else
        {
            playerDataDictionary.Add(playerRef, new PlayerData { NickName = nickName });
        }
    }

    //? Get player data
    public PlayerData GetPlayerData(PlayerRef playerRef)
    {
        if (playerDataDictionary.TryGetValue(playerRef, out PlayerData playerData))
        {
            return playerData;
        }
        return null;
    }

    //? Remove player data
    public void RemovePlayerData(PlayerRef playerRef)
    {
        if (playerDataDictionary.ContainsKey(playerRef))
        {
            playerDataDictionary.Remove(playerRef);
        }
    }

    public void PrintActivePlayerList() {
        foreach (var item in playerDataDictionary) {
            Debug.Log($"PlayerPref - {item.Key} | NickName - {item.Value.NickName}");
        }
    }
}


