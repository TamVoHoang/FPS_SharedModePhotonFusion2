using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] SessionListUIHandler sessionListUIHandler;
    [SerializeField] MainMenuUIHandler mainMenuUIHandler;

    //public GameObject PlayerPrefabGO;
    [SerializeField] NetworkPlayer networkPlayerPrefab;

    //NetworkRunner networkRunner;
    public string customLobbyName;  // game type
    [SerializeField] string sceneName;
    public string SceneName { get { return sceneName; } set { sceneName = value; } }


    private void Awake() {
        //networkRunner = GetComponent<NetworkRunner>();
        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
        mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>(true);

        customLobbyName = "OurLobbyID";
        sceneName = "World1";
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

            NetworkPlayer spawnNetworkPlayer = runner.Spawn(networkPlayerPrefab, spawnPosition, Quaternion.identity, player, InitializeNetworkPlayerBeforeSpawn);
            spawnNetworkPlayer.transform.position = spawnPosition;
        }
    }

    private void InitializeNetworkPlayerBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        if(customLobbyName == "OurLobbyID_Team") {
            Debug.Log($"_____co vao xet bool isEnemy in NetworkPlayer.cs");
            bool randomBool = UnityEngine.Random.value > 0.5f;
            obj.GetComponent<NetworkPlayer>().IsEnemy = randomBool;
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
        mainMenuUIHandler.SessionList = sessionList;
        //todo testing

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
