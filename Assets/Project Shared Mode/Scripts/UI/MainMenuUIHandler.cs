using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//todo gameObject = canvas trong mainmenu scene
public class MainMenuUIHandler : MonoBehaviour
{
    [Header("       Panels")]
    public GameObject playerDetailPanel;
    public GameObject sessionListBrowserPanel;
    public GameObject createSessionPanel;

    public GameObject findingSessionPanel; // panel Finding Room ... | dang tim session de join
    public GameObject joiningGamePanel;  // panel Joining Game ... | vao phog cho
    [SerializeField] GameObject loadingScreen;  // loading animation
    [SerializeField] TextMeshProUGUI quickPlayResultText;   // show text quickPlay 

    [Header("       Input Field")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_InputField sessionNameInputField;

    [Header("       Buttons")]
    [SerializeField] Button OnQuickPlayClick_Button;    // quick play not active SessionList panel
    [SerializeField] Button OnFindGameClick_Button;     // active sessionlist panel
    [SerializeField] Button OnEquipClick_Button;
    [SerializeField] Button OnBackLoginClick_Button;

    [SerializeField] Button OnQuitGameClick_Button;

    // nam trong sessionList Panel | active creategamepanel UI -> show input field session name
    [SerializeField] Button OnCreateSessionClick_Button;

    // nam trong Create game Panel | input session's name -> vao ready scene
    [SerializeField] Button OnCreateAndJoinSessionClick_Button;

    [Header("       Join Randomly Session")]
    [SerializeField] List<SessionInfo> sessionList = new List<SessionInfo>();
    public List<SessionInfo> SessionList{set => this.sessionList = value; }
    
    const string READY_SCENE = "Ready";
    const string EQUIP_SCENE = "Equip";
    const string LOBBY = "MainLobby";

    private void Awake() {
        OnQuickPlayClick_Button.onClick.AddListener(OnQuickPlayClicked);
        OnFindGameClick_Button.onClick.AddListener(OnFindGameClicked);
        OnEquipClick_Button.onClick.AddListener(OnEquipClicked);
        OnQuitGameClick_Button.onClick.AddListener(OnQuitGameClicked);
        OnBackLoginClick_Button.onClick.AddListener(OnBackLoginClicked);

        OnCreateSessionClick_Button.onClick.AddListener(OnActiveCreateGamePanelClicked);
        OnCreateAndJoinSessionClick_Button.onClick.AddListener(OnCreateJoinSessionClicked);
        loadingScreen.SetActive(false);
    }
    
    private void Start() {
        if(DataSaver.Instance) {
            playerNameInputField.text = DataSaver.Instance.dataToSave.userName;
        }
        else {
            playerNameInputField.text = GameManager.names[Random.Range(0, GameManager.names.Length)];
        }
    }

    void HidePanels() {
        playerDetailPanel.SetActive(false);
        sessionListBrowserPanel.SetActive(false);
        createSessionPanel.SetActive(false);

        joiningGamePanel.SetActive(false);
    }

    // xem va trang bi nhan vat
    public void OnEquipClicked() {
        /* NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        Spawner spawner = FindObjectOfType<Spawner>();

        networkRunnerHandler.CreateGame(sessionNameInputField.text, spawner.GameMap, EQUIP_SCENE, spawner.CustomLobbyName); */
    }

    // sau khi nhap ten -> tim list sessin -> chon va join
    public void OnFindGameClicked() {
        PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        /* NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby(); */

        HidePanels();

        sessionListBrowserPanel.gameObject.SetActive(true);
        findingSessionPanel.SetActive(true);
        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();    // xoa list session - hien chu looking ben duoi
        
        StartCoroutine(Delay(4f));
        /* SceneManager.LoadScene("World1"); */

    }

    IEnumerator Delay(float time) {
        yield return new WaitForSeconds(1f);    // de co the thay duoc chu looking ben duoi
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(time);  // sau khi du lau de sessionListUpdate -> tat bang Finding...

        findingSessionPanel.SetActive(false);
    }

    public void OnQuitGameClicked() => Application.Quit();
    void OnBackLoginClicked() {
        StartCoroutine(BackToLogin(0.5f));
    }

    IEnumerator BackToLogin(float time) {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(LOBBY);
    }
    // sau khi looking no session -> vao ui tao session
    public void OnActiveCreateGamePanelClicked() {
        //? old version not popup
        /* HidePanels();
        createSessionPanel.SetActive(true); */

        //? new version
        createSessionPanel.SetActive(!createSessionPanel.activeSelf);   // switching on off create pravite session 
    }

    // nhap ten session -> xac nhan tao session -> vao ready secen
    public void OnCreateJoinSessionClicked() {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        Spawner spawner = FindObjectOfType<Spawner>();

        // vao thang Game Random character
        /* networkRunnerHandler.CreateGame(sessionNameInputField.text, "World1"); */

        networkRunnerHandler.CreateGame(sessionNameInputField.text, spawner.TypeGame, spawner.GameMap, READY_SCENE, spawner.CustomLobbyName);

        HidePanels();
        joiningGamePanel.gameObject.SetActive(true);
    }

    public void OnJoiningServer() {
        HidePanels();
        joiningGamePanel.gameObject.SetActive(true);
    }



    //! testing

    // nhan nut QuickPlay
    private void OnQuickPlayClicked()
    {
        /* PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        var sessionInfo = GetRandomSesisonInfo();
        var spawner = FindObjectOfType<Spawner>();
        if(sessionInfo != null)
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.GameMap); */

        StartCoroutine(DelayStartRandom(4));
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
        findingSessionPanel.gameObject.SetActive(true);

        PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        GameManager.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby(); // sessionListUpdate callback -> tra ve gia tri sessionList raw 170

        yield return new WaitForSeconds(time);    // neu delay theo time -> co the list chua co thi da check sessionInfo != null

        var sessionInfo = GetRandomSesisonInfo();
        var spawner = FindObjectOfType<Spawner>();
        if(sessionInfo != null) {
            quickPlayResultText.text = $"Join session {sessionInfo.Name}";

            //? Option 1: OK join khong can quan tam GamMap this.spawner.GameMap != GameMap OF Runner.IsSharedModeMasterClient
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.TypeGame, spawner.GameMap);   

            //? Option 1: kiem tra sessionInfo chuan bi join co GameMap la gi (do nguoi tao phong quyet dinh)
            //? truyen dung tham so GameMap de join
            /* string name = null;
            GameMap gameMap = GameMap.World_1;
            if (sessionInfo.Properties.TryGetValue("mapName", out var propertyType) && propertyType.IsInt) {
                var mapName = (int)propertyType.PropertyValue;
                string map = ((GameMap)mapName).ToString();
                Debug.Log($"_____mapName" + map);
                name = map;
                gameMap = (GameMap)mapName;
            }
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, gameMap); */
        }
        else {
            quickPlayResultText.text = "No session to join";
        }
        
        findingSessionPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        quickPlayResultText.text = "";
    }

}
