using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//todo gameObject = canvas trong mainmenu scene
public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerDetailPanel;
    public GameObject sessionBrowserlPanel;
    public GameObject createSessionPanel;
    // thong bao dang vao game 
    public GameObject statusPanel; 

    [Header("Player Settings")]
    [SerializeField] TMP_InputField playerNameInputField;
    [SerializeField] TMP_InputField sessionNameInputField;

    /* [SerializeField] TMP_InputField lobbyNameInputField; */


    [Header("Buttons")]
    [SerializeField] Button OnFindGameClick_Button;
    [SerializeField] Button OnQuitGameClick_Button;

    [SerializeField] Button OnCreateNewSessionClick_Button;     // active panel chuan bi tao session Name
    [SerializeField] Button OnCreateNewSessionClick1_Button;    // input session's name -> vao ready scene sau khi tao session

    /* [SerializeField] Button OnFindSessionClick_Button; */


    private void Awake() {
        OnFindGameClick_Button.onClick.AddListener(OnFindGameClicked);
        OnQuitGameClick_Button.onClick.AddListener(OnQuitGameClicked);

        OnCreateNewSessionClick_Button.onClick.AddListener(OnCreateNewGameClicked);
        OnCreateNewSessionClick1_Button.onClick.AddListener(OnStartNewSessionClicked);


        /* OnFindSessionClick_Button.onClick.AddListener(OnFindLobbyClicked); */
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

        // vao thang Game Random character
        /* networkRunnerHandler.CreateGame(sessionNameInputField.text, "World1"); */ 

        networkRunnerHandler.CreateGame(sessionNameInputField.text, "World1");

        HidePanels();
        statusPanel.gameObject.SetActive(true);
    }

    public void OnJoiningServer() {
        HidePanels();
        statusPanel.gameObject.SetActive(true);
    }



    //! testing
    /* private void OnFindLobbyClicked()
    {
        PlayerPrefs.SetString("PlayerNickName_Local", lobbyNameInputField.text);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Lobby");
    } */

    //! testing
}
