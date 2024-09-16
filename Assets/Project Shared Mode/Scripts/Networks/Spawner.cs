using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] SessionListUIHandler sessionListUIHandler;
    //public GameObject PlayerPrefabGO;
    [SerializeField] NetworkPlayer networkPlayerPrefab;

    NetworkRunner networkRunner;

    private void Awake() {
        networkRunner = GetComponent<NetworkRunner>();
        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);

    }

    public void OnConnectedToServer(NetworkRunner runner) {
        Debug.Log($"___OnConnectedToServer");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        Debug.Log($"___OnPlayerJoined");
        
        if (player == Runner.LocalPlayer)
        {
            Debug.Log($"playerRef - {player}");
            Debug.Log($"Runner.LocalPlayer - {Runner.LocalPlayer}");

            Vector3 spawnPosition = Utils.GetRandomSpawnPoint();

            NetworkPlayer spawnNetworkPlayer = runner.Spawn(networkPlayerPrefab, spawnPosition, Quaternion.identity, player);
            spawnNetworkPlayer.transform.position = spawnPosition;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner) {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
        //todo testing
        sessionListUIHandler.SessionList = sessionList;
        //

        Debug.Log($"_____Session is updated ");
        if(sessionListUIHandler == null) return;
        
        if(sessionList.Count == 0) {
            Debug.Log("Joined Lobby NO session found _ OnSessionListUpdated() Spanwer.cs");
            sessionListUIHandler.OnNoSessionFound();
        }
        else {
            sessionListUIHandler.ClearList();

            foreach (SessionInfo sessionInfo in sessionList)
            {
                sessionListUIHandler.AddToList(sessionInfo);
                Debug.Log($"sessionName: {sessionInfo.Name} playerCount: {sessionInfo.PlayerCount}");
            }
        }

        // sau khi update kiem tra room list -> hien thi nut tao session
        sessionListUIHandler.ActiveOnCreateSesison_Button();

    }


    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Debug.Log("On shutdown");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {
        
    }
}
