using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
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
    [SerializeField] TextMeshProUGUI quickPlayResultText;   // show text quickPlay 

    [Header("       Input Field")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_InputField sessionNameInputField;

    [Header("       Buttons")]
    [SerializeField] Button OnQuickPlayClick_Button;    // quick play not active SessionList panel
    [SerializeField] Button OnFindGameClick_Button;     // active sessionlist panel
    [SerializeField] Button OnEquipClick_Button;
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

    private void Awake() {
        OnQuickPlayClick_Button.onClick.AddListener(OnQuickPlayClicked);
        OnFindGameClick_Button.onClick.AddListener(OnFindGameClicked);
        OnEquipClick_Button.onClick.AddListener(OnEquipClicked);
        OnQuitGameClick_Button.onClick.AddListener(OnQuitGameClicked);

        OnCreateSessionClick_Button.onClick.AddListener(OnActiveCreateGamePanelClicked);
        OnCreateAndJoinSessionClick_Button.onClick.AddListener(OnCreateJoinSessionClicked);

    }

    private void Start() {
        // lay gia tri luu previous in playerPrefs gan vao inputfield
        /* if(PlayerPrefs.HasKey("PlayerNickName_Local")) {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickName_Local");
        } */

        //lay gia tri trong firestore khi dang ky nick name gan vao day
        if(DataSaver.Instance == null) return;
        //playerNameInputField.text = DataSaveLoadHander.Instance.playerDataToFireStore.userName;
        playerNameInputField.text = DataSaver.Instance.dataToSave.userName;


        //! not using
        /* if(PlayerPrefs.HasKey("PlayerNickName_Local")) {
            lobbyNameInputField.text = PlayerPrefs.GetString("PlayerNickName_Local");
        } */
        
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

        // NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        // networkRunnerHandler.OnJoinLobby();

        HidePanels();

        sessionListBrowserPanel.gameObject.SetActive(true);
        findingSessionPanel.SetActive(true);
        FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();    // xoa list session - hien chu looking
        
        StartCoroutine(Delay(2f));
        /* SceneManager.LoadScene("World1"); */
    }

    IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(0.5f);
        findingSessionPanel.SetActive(false);
    }

    public void OnQuitGameClicked() => Application.Quit();

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

        networkRunnerHandler.CreateGame(sessionNameInputField.text, spawner.GameMap, READY_SCENE, spawner.CustomLobbyName);

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

        findingSessionPanel.gameObject.SetActive(true);

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
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.GameMap);
        }
        else {
            quickPlayResultText.text = "No session to join";
        }
        
        findingSessionPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        quickPlayResultText.text = "";

        /* statusPanel.gameObject.SetActive(false); */
    }
}
