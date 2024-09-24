using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] SessionListUIHandler sessionListUIHandler;
    [SerializeField] MainMenuUIHandler mainMenuUIHandler;

    //public GameObject PlayerPrefabGO;
    [SerializeField] NetworkPlayer networkPlayerPrefab;

    //NetworkRunner networkRunner;
    public string customLobbyName;  // game type
    [SerializeField] string sceneName;
    public string SceneName {set { sceneName = value; } }

    private void Awake() {
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

            //? kiem tra co dang spawn tai ready scene hay khong
            bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";
            Vector3 spawnPosition = Utils.GetRandomSpawnPoint();

            if(isReadyScene) {
                if(player.PlayerId == 1) {
                    spawnPosition = new Vector3(0 , 3, 0);
                    ReadyUIHandler readyUIHandler = FindObjectOfType<ReadyUIHandler>();
                    readyUIHandler.SetOnLeaveButtonActive(false);
                    Debug.Log($"Host was Joint  {player.PlayerId} | {spawnPosition}");
                } else if(player.PlayerId % 2 == 0) {
                    spawnPosition = new Vector3(player.PlayerId * -0.5f, 3, 0);
                    Debug.Log($"Client was Joint  {player.PlayerId} | {spawnPosition}");
                } else if(player.PlayerId % 2 != 0) {
                    spawnPosition = new Vector3(player.PlayerId * 0.5f - 0.5f, 3, 0);
                }
            }

            NetworkPlayer spawnNetworkPlayer = runner.Spawn(networkPlayerPrefab, spawnPosition, Quaternion.identity, player, InitializeNetworkPlayerBeforeSpawn);
            spawnNetworkPlayer.transform.position = spawnPosition;

            // gan networkRunner cho NetworkPalyer
            if(runner.IsSharedModeMasterClient)
                networkPlayerPrefab.GetComponent<NetworkPlayer>().SetNetworkRunnerAndSceneToStart(this.sceneName);
        }
    }

    private void InitializeNetworkPlayerBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        /* if(customLobbyName == "OurLobbyID_Team") {
            Debug.Log($"_____co vao xet bool isEnemy in NetworkPlayer.cs");
            bool randomBool = UnityEngine.Random.value > 0.5f;
            obj.GetComponent<NetworkPlayer>().IsEnemy = randomBool;
        } */
        if(customLobbyName == "OurLobbyID_Team") {
            if(obj.InputAuthority.PlayerId % 2 != 0) obj.GetComponent<NetworkPlayer>().IsEnemy = false;
            else obj.GetComponent<NetworkPlayer>().IsEnemy = true;
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
        Debug.Log($"_____OnSessionListUpdated_____");

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
                Debug.Log($"host -" + sessionInfo.Properties);
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
