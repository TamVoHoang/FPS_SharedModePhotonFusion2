using System;
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

    private void Awake() {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        // neu tai scene mainmenu co networkRunner -> se dung networkRunner tai day
        if(networkRunnerInScene != null) {
            networkRunner = networkRunnerInScene;
        }
    }

    private void Start() {
        if(networkRunner == null) // neu ko co networkRunner duoc tim thay tai mainmenu scene thi se dung PF
        {
            networkRunner = Instantiate(networkRunnerPF);
            networkRunner.name = "Network runner";

            //? Neu ko dang o scene mainMenu -> vao thang game
            if(SceneManager.GetActiveScene().name != "MainMenu") {
                var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, "Test_Session", NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
            }

            Debug.Log($"___Server NetworkRunner started");
        }
    }
    
    INetworkSceneManager GetSceneManager(NetworkRunner runner) {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        if(sceneManager == null) {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sceneManager;
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized) {
        var sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs {
            GameMode = gameMode,
            Address = address,
            Scene = scene,

            //SessionName = "Test Room", // vao thang ko can sessionInfo
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",

            /* Initialized = initialized, */
            SceneManager = sceneManager,
        });
    }

    public void OnJoinLobby() {
        var clienTask = JoinLobby();
    }

    private async Task JoinLobby() {
        Debug.Log("JoinLobby started");

        string lobbyID = "OurLobbyID";
        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if(!result.Ok) {
            Debug.Log($"Can Not Join Lobby {lobbyID}");
        } else {
            Debug.Log($"Can Join Lobby OK");
        }
    }
    public void CreateGame(string sessionName, string sceneName) {
        Debug.Log($"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");
        //Join game co san
        var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, sessionName, NetAddress.Any(), SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")), null);
    }

    public void JoinGame(SessionInfo sessionInfo) {
        Debug.Log($"Join session {sessionInfo.Name}");
        var clienTask = InitializeNetworkRunner(networkRunner, GameMode.Shared, sessionInfo.Name, NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);

    }
}