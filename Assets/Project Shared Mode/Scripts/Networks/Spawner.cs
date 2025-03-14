using System;
using Fusion;
using UnityEngine;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GameMap : int {
    World_1,
    World_2,
    World_3
}

public enum TypeGame : int
{
    Survival,
    Team,
}

public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] SessionListUIHandler sessionListUIHandler;
    [SerializeField] MainMenuUIHandler mainMenuUIHandler;

    [Header("       NetworkPlayerPF and GunPF")]
    [SerializeField] NetworkPlayer networkPlayerPrefab;
    [SerializeField] NetworkObject gunPickupPF;
    [SerializeField] NetworkObject gun1PickupPF;
    [SerializeField] NetworkObject gun2PickupPF;
    [SerializeField] private int numbersOfWeapon = 2;
    List<NetworkObject> weaponLists = new List<NetworkObject>();
    bool isWeaponSpawned = false;

    [Header ("      Lobby GameMap (Scene)")]
    [SerializeField] string customLobbyName;
    [SerializeField] GameMap gameMap;
    [SerializeField] TypeGame typeGame;
    public string CustomLobbyName {get => customLobbyName; set => customLobbyName = value;}
    public GameMap GameMap {get => gameMap; set => gameMap = value;}
    public TypeGame TypeGame {get => typeGame; set => typeGame = value;}

    /* [SerializeField] string sceneToStart;
    public string SceneName {set { sceneToStart = value; } get { return sceneToStart; }} */
    

    private void Awake() {
        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
        mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>(true);

        // set defaut customLobblyName and gameMap (scene)
        customLobbyName = "OurLobbyID_Survial";
        gameMap = (GameMap)GameMap.World_1;
        typeGame = (TypeGame)TypeGame.Survival;

    }

    public void OnConnectedToServer(NetworkRunner runner) {
        Debug.Log($"___OnConnectedToServer");
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
                    spawnPosition = new Vector3(0 , 5, 0);

                    ReadyUIHandler readyUIHandler = FindObjectOfType<ReadyUIHandler>();
                    //readyUIHandler.SetOnLeaveButtonActive(false);   // neu la Host session thi ko Leave

                    Debug.Log($"Host was Joint  {player.PlayerId} | {spawnPosition}");
                } else if(player.PlayerId % 2 == 0) {
                    spawnPosition = new Vector3(player.PlayerId * -0.5f, 5, 0);
                    Debug.Log($"Client was Joint  {player.PlayerId} | {spawnPosition}");
                } else if(player.PlayerId % 2 != 0) {
                    spawnPosition = new Vector3(player.PlayerId * 0.5f - 0.5f, 5, 0);
                }
            }

            if(SceneManager.GetActiveScene().name =="MainMenu") {
                spawnPosition = Vector3.zero;
            }

            NetworkPlayer spawnNetworkPlayer = runner.Spawn(networkPlayerPrefab, spawnPosition, Quaternion.identity, player, InitializeNetworkPlayerBeforeSpawn);
            // need to check at row 73 CharacterMovementHandle.cs
            spawnNetworkPlayer.GetComponent<CharacterController>().enabled = false; 
            spawnNetworkPlayer.transform.position = spawnPosition;
        }
    }
    
    private void InitializeNetworkPlayerBeforeSpawn(NetworkRunner runner, NetworkObject obj) {
        if(customLobbyName == "OurLobbyID_Team") {
            if(obj.InputAuthority.PlayerId % 2 != 0) obj.GetComponent<NetworkPlayer>().IsEnemy = false;
            else obj.GetComponent<NetworkPlayer>().IsEnemy = true;
        }
    }

    //? spawn weapons
    void SpawnWeapons() {
        if(isWeaponSpawned) return;

        for (int i = 0; i < numbersOfWeapon; i++) {
            NetworkObject gunPF = Runner.Spawn(gunPickupPF, Utils.GetRandomWeaponSpawnPoint(), Quaternion.identity, null);
            NetworkObject gun1PF = Runner.Spawn(gun1PickupPF, Utils.GetRandomWeaponSpawnPoint(), Quaternion.identity, null);
            NetworkObject gun2PF = Runner.Spawn(gun2PickupPF, Utils.GetRandomWeaponSpawnPoint(), Quaternion.identity, null);

            //weaponLists.Add(gun1PF);
        }
        isWeaponSpawned = true;
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
        Debug.Log($"_____OnSessionListUpdated");

        //todo testing
        sessionListUIHandler.SessionList = sessionList;
        mainMenuUIHandler.SessionList = sessionList;
        //todo testing

        Debug.Log($"_____Session is updated ");
        if(sessionListUIHandler == null) return;
        
        if(sessionList.Count == 0) {
            Debug.Log("Joined Lobby NO session found _ OnSessionListUpdated() Spanwer.cs");
            
            if(!sessionListUIHandler.isActiveAndEnabled) return;    // neu ko co dieu kien nay row 98 sesisonListUIHandler erro Coroutine()
            sessionListUIHandler.OnNoSessionFound();
        }
        else {
            sessionListUIHandler.ClearList();

            foreach (SessionInfo sessionInfo in sessionList)
            {
                string name = null;
                
                if (sessionInfo.Properties.TryGetValue("mapName", out var propertyType) 
                    && propertyType.IsInt) {
                    var mapName = (int)propertyType.PropertyValue;
                    string map = ((GameMap)mapName).ToString();
                    Debug.Log($"_____mapName" + map);
                    name = map;
                }

                string typeTemp = null;
                if (sessionInfo.Properties.TryGetValue("typeName", out var propertyType_) 
                    && propertyType_.IsInt) {
                    var typeName = (int)propertyType_.PropertyValue;
                    string type = ((TypeGame)typeName).ToString();
                    Debug.Log($"_____typeName" + type);
                    typeTemp = type;
                }
                

                sessionListUIHandler.AddToList(sessionInfo, typeTemp, name);
                Debug.Log($"sessionName: {sessionInfo.Name} playerCount: {sessionInfo.PlayerCount}");
                Debug.Log($"host -" + sessionInfo.Properties);
            }
        }

        // sau khi update kiem tra room list -> hien thi nut tao session
        sessionListUIHandler.ActiveOnCreateSesison_Button();
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
        if(SceneManager.GetActiveScene().name != "Ready" && runner.IsSharedModeMasterClient) {
            SpawnWeapons();
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Debug.Log("On shutdown");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnInput(NetworkRunner runner, NetworkInput input) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnSceneLoadStart(NetworkRunner runner) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
}