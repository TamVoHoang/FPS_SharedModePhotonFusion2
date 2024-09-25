using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameObject = canvas trong mainmenu scene
public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerDetailPanel;
    public GameObject sessionBrowserlPanel;
    public GameObject createSessionPanel;

    // panel thong bao dang vao game Joining Game ...
    public GameObject statusPanel;

    // panel thong bao dang vao game Finding Room ...
    public GameObject findingRoomPanel;

    // thong bao ket qua quickPlay o PlayerDetail_Panel
    [SerializeField] TextMeshProUGUI quickPlayResultText;

    [Header("Player Settings")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_InputField sessionNameInputField;

    [Header("Buttons")]
    [SerializeField] Button OnFindGameClick_Button;
    [SerializeField] Button OnQuitGameClick_Button;
    [SerializeField] Button OnQuickPlayClick_Button;


    [SerializeField] Button OnCreateNewSessionClick_Button;     // active panel chuan bi tao session Name
    [SerializeField] Button OnCreateNewSessionClick1_Button;    // input session's name -> vao ready scene sau khi tao session

    [Header("Create Session")]
    [SerializeField] string sceneName;
    public string SceneName { get { return sceneName; } set { sceneName = value; } }

    [Header("Random And Join Session")]
    [SerializeField] List<SessionInfo> sessionList = new List<SessionInfo>();
    public List<SessionInfo> SessionList{set => this.sessionList = value; }

    private void Awake() {
        OnFindGameClick_Button.onClick.AddListener(OnFindGameClicked);
        OnQuitGameClick_Button.onClick.AddListener(OnQuitGameClicked);
        OnQuickPlayClick_Button.onClick.AddListener(OnQuickPlayClicked);

        OnCreateNewSessionClick_Button.onClick.AddListener(OnCreateNewGameClicked);
        OnCreateNewSessionClick1_Button.onClick.AddListener(OnStartNewSessionClicked);
    }

    private void Start() {
        if(PlayerPrefs.HasKey("PlayerNickName_Local")) {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickName_Local");
        }

        //!
        /* if(PlayerPrefs.HasKey("PlayerNickName_Local")) {
            lobbyNameInputField.text = PlayerPrefs.GetString("PlayerNickName_Local");
        } */
    }

    void HidePanels() {
        playerDetailPanel.SetActive(false);
        sessionBrowserlPanel.SetActive(false);
        statusPanel.SetActive(false);
        createSessionPanel.SetActive(false);
    }

    // sau khi nhap ten -> tim session
    public void OnFindGameClicked() {
        PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();
        HidePanels();

        sessionBrowserlPanel.gameObject.SetActive(true);

        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();    // xoa list session - hien chu looking
        //SceneManager.LoadScene("World1");
    }

    public void OnQuitGameClicked() => Application.Quit();

    // sau khi looking no session -> vao ui tao session
    public void OnCreateNewGameClicked() {
        HidePanels();
        createSessionPanel.SetActive(true);
    }

    // nhap ten session -> xac nhan tao session -> vao ready secen
    public void OnStartNewSessionClicked() {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        Spawner spawner = FindObjectOfType<Spawner>();

        // vao thang Game Random character
        /* networkRunnerHandler.CreateGame(sessionNameInputField.text, "World1"); */

        networkRunnerHandler.CreateGame(sessionNameInputField.text, spawner.gameMap, "Ready", spawner.customLobbyName);

        HidePanels();
        statusPanel.gameObject.SetActive(true);
    }

    public void OnJoiningServer() {
        HidePanels();
        statusPanel.gameObject.SetActive(true);
    }



    //! testing

    // nhan nut QuickPlay
    private void OnQuickPlayClicked()
    {
        /* PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby(); */

        StartCoroutine(DelayStartRandom(3));

        /* var sessionInfo = GetRandomSesisonInfo();
        if(sessionInfo != null)
            networkRunnerHandler.JoinGame(sessionInfo); */
    }

    SessionInfo GetRandomSesisonInfo() {
        foreach (var item in sessionList)
        {
            if(item.IsOpen && item.PlayerCount < item.MaxPlayers) {
                return item;
            }
        }
        return null;
    }

    IEnumerator DelayStartRandom(float time) {
        //statusPanel.gameObject.SetActive(true);
        findingRoomPanel.gameObject.SetActive(true);

        PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(time);
        
        var sessionInfo = GetRandomSesisonInfo();
        var spawner = FindObjectOfType<Spawner>();
        if(sessionInfo != null) {
            quickPlayResultText.text = $"Join session {sessionInfo.Name}";
            networkRunnerHandler.JoinGame(sessionInfo, spawner.customLobbyName, spawner.gameMap);
        }
        else {
            quickPlayResultText.text = "No session to join";
        }
            
        
        findingRoomPanel.gameObject.SetActive(false);
        //statusPanel.gameObject.SetActive(false);
    }

    //! testing
}
