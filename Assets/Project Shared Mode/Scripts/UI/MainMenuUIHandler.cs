using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//todo gameObject = canvas trong mainmenu scene
public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerDetailPanel;
    /* public GameObject sessionBrowserlPanel;
    public GameObject createSessionPanel;
    // thong bao dang vao game 
    public GameObject statusPanel;  */

    [Header("Player Settings")]
    [SerializeField] TMP_InputField playerNameInputField;
    //[SerializeField] TMP_InputField sessionNameInputField;

    [Header("Buttons")]
    [SerializeField] Button OnFindGameClick_Button;
    [SerializeField] Button OnQuitGameClick_Button;

    /* [SerializeField] Button OnCreateNewSessionClick_Button;     // active panel tao session
    [SerializeField] Button OnCreateNewSessionClick1_Button;    // input session's name -> vao ready scene */

    private void Awake() {
        OnFindGameClick_Button.onClick.AddListener(OnFindGameClicked);
        OnQuitGameClick_Button.onClick.AddListener(OnQuitGameClicked);

        /* OnCreateNewSessionClick_Button.onClick.AddListener(OnCreateNewGameClicked);
        OnCreateNewSessionClick1_Button.onClick.AddListener(OnStartNewSessionClicked); */
    }

    private void Start() {
        if(PlayerPrefs.HasKey("PlayerNickName_Local")) {
            playerNameInputField.text = PlayerPrefs.GetString("PlayerNickName_Local");
            
        }
    }

    void HidePanels() {
        playerDetailPanel.SetActive(false);
        /* sessionBrowserlPanel.SetActive(false);
        statusPanel.SetActive(false);
        createSessionPanel.SetActive(false); */
    }

    public void OnQuitGameClicked() => Application.Quit();

    // sau khi nhap ten -> tim session
    public void OnFindGameClicked() {
        PlayerPrefs.SetString("PlayerNickName_Local", playerNameInputField.text);
        PlayerPrefs.Save();
        //GameManager.Instance.playerNickName = playerNameInputField.text;

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        //networkRunnerHandler.OnJoinLobby();
        HidePanels();

        //sessionBrowserlPanel.gameObject.SetActive(true);

        //FindObjectOfType<SessionListUIHandler>(true).OnLookingForGameSessions();

        SceneManager.LoadScene("World1");
    }

    public void OnJoiningServer() {
        HidePanels();
        //statusPanel.gameObject.SetActive(true);
    }
}
