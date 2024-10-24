using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPF;   // chua spawner.cs
    NetworkRunner networkRunner;

    // create session with number player chossen
    [SerializeField] int playerCount;
    public int PlayerCount { get { return playerCount; } set { playerCount = value; } }
    /* public string customLobbyName; */

    private void Awake() {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        // neu tai scene mainmenu co networkRunner -> se dung networkRunner tai day
        if(networkRunnerInScene != null) {
            networkRunner = networkRunnerInScene;
        }

        /* customLobbyName = "OurLobbyID"; */
    }

    private void Start() {
        if(networkRunner == null) // neu ko co networkRunner duoc tim thay tai mainmenu scene thi se dung PF
        {
            networkRunner = Instantiate(networkRunnerPF);
            networkRunner.name = "Network Runner";

            //? Neu ko dang o scene mainMenu -> vao thang game
            if(SceneManager.GetActiveScene().name != "MainMenu") {
                playerCount = 2;
                var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, "Test_Session", GameMap.World_1, "Test_Lobby", NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
                /* ConnectToSession(networkRunner, GameMode.Shared, "Room", SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));
                ConnectToLobby("Lobby"); */
            }

            Debug.Log($"_____Server NetworkRunner started");
        }
    }
    
    
    INetworkSceneManager GetSceneManager(NetworkRunner runner) {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if(sceneManager == null) {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName, GameMap gameMap,
            string customLobbyName, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized) {
        var customProps = new Dictionary<string, SessionProperty>();
        customProps["mapName"] = (int)gameMap;

        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs {
            GameMode = gameMode,
            Address = address,
            Scene = scene,

            SessionName = sessionName,
            /* CustomLobbyName = "OurLobbyID", */
            CustomLobbyName = customLobbyName,

            PlayerCount = playerCount,
            SceneManager = sceneManager,
            SessionProperties = customProps,
        });
    }

    public void OnJoinLobby() {
        var clienTask = JoinLobby();
    }

    private async Task JoinLobby() {
        Debug.Log("JoinLobby started");

        //string lobbyID = "OurLobbyID";
        string lobbyID = networkRunner.GetComponent<Spawner>().CustomLobbyName;

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if(!result.Ok) {
            Debug.Log($"Can Not Join Lobby -> {lobbyID}");
        } else {
            Debug.Log($"Can Join Lobby OK -> {lobbyID}");
        }
    }

    public void CreateGame(string sessionName, GameMap gameMap, string sceneName, string customLobbyName) {
        Debug.Log($"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");
        
        //Join game co san
        var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, sessionName, gameMap, customLobbyName, 
            NetAddress.Any(), SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")), null);

    }

    public void JoinGame(SessionInfo sessionInfo, string customLobbyName, GameMap gameMap) {
        Debug.Log($"Join session {sessionInfo.Name}");
        var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, sessionInfo.Name, gameMap, customLobbyName, 
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);

    }
    
}