using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// game object = canvas in batlle scene game
// using to show local UI + realtime result table

public class LocalUIInGameHandler : MonoBehaviour
{
    [Header("       Panels")]
    [SerializeField] GameObject howToPlay_Panel;
    [SerializeField] GameObject backToMainMenu_Panel;   // quit panel in game
    [SerializeField] GameObject realtTimeResultSolo_Panel;
    [SerializeField] GameObject realtTimeResultTeam_Panel;
    [SerializeField] GameObject inGameTeamResult_Panel;

    [Header("       Buttons")]
    [SerializeField] Button backToMainMenuInQuitPanel_Button;

    [SerializeField] List<NetworkPlayer> networkPlayerList = new List<NetworkPlayer>();
    [SerializeField] ResultListUIHandler resultListUIHandler_Solo;
    [SerializeField] ResultListUIHandler_Team resultListUIHandler_Team;
    
    bool isCursorShowed = false;
    bool isShowingRealtimeResultLocal = false;
    [SerializeField] bool isSoloMode;
    bool cursorLocked;
    [SerializeField] TextMeshProUGUI finalResultTeam;
    [SerializeField] NetworkPlayer networkPlayer;
    public bool isShowed = false;
    

    private void Awake() {
        //resultListUIHandler = GetComponentInChildren<ResultListUIHandler>(true);
        resultListUIHandler_Solo = realtTimeResultSolo_Panel.GetComponent<ResultListUIHandler>();
        resultListUIHandler_Team = realtTimeResultTeam_Panel.GetComponent<ResultListUIHandler_Team>();
        networkPlayer = FindObjectOfType<NetworkPlayer>();

        if(!NetworkPlayer.Local) {
            Debug.Log($"_____ solo mode is true player directly join at battle scene");
            isSoloMode = true;
        } else isSoloMode = NetworkPlayer.Local.IsSoloMode();
        
        if(!isSoloMode) inGameTeamResult_Panel.SetActive(true);
        
        //Always make sure that our cursor is locked when the game starts!
        //Update the cursor's state.
        cursorLocked = true;
        UpdateCursorState();
        
    }
    private void Start() {
        howToPlay_Panel.SetActive(false);
        backToMainMenu_Panel.SetActive(false);
        isShowingRealtimeResultLocal = false;
        realtTimeResultSolo_Panel.gameObject.SetActive(false);
        backToMainMenuInQuitPanel_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
    }

    private void Update() {
        // showing tutorial
        if(Input.GetKey(KeyCode.Tab)) {
            howToPlay_Panel.SetActive(true);
        } else howToPlay_Panel.SetActive(false);

        // showing panel to return mainmenu
        /* if(Input.GetKeyDown(KeyCode.Escape)) {
            backToMainMenu_Panel.SetActive(!backToMainMenu_Panel.activeSelf);
            ToggleCursor();
        } */

        if(Input.GetKeyDown(KeyCode.Escape)) {
            OnLockCursor();
            backToMainMenu_Panel.SetActive(!backToMainMenu_Panel.activeSelf);
        }

        // showing realtime result table
        if(Input.GetKeyDown(KeyCode.V)) {
            OnLockCursor();

            isShowingRealtimeResultLocal = !isShowingRealtimeResultLocal;
            
            if(isShowingRealtimeResultLocal) {
                RealTimeResultLocal();
            } else {
                realtTimeResultSolo_Panel.SetActive(false);
                realtTimeResultTeam_Panel.SetActive(false);
            }
        }

        // hci hien thi win or loss cho che do team + finish battle
        if(!isShowed && networkPlayer.isFinished_Network && !networkPlayer.IsSoloMode()) {
            isShowed = true;
            ShowWinOrLossResult();
        }
    }

    //? toggle new version
    void OnLockCursor() {
        cursorLocked = !cursorLocked;
        UpdateCursorState();
    }

    private void UpdateCursorState() {
        //Update cursor visibility.
        Cursor.visible = !cursorLocked;
        //Update cursor lock state.
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    //? toggle cursor old version
    void ToggleCursor() {
        isCursorShowed = !isCursorShowed;
        if(isCursorShowed) ShowCursor();
        else HideCursor();
    }
        
    void ShowCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void HideCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnLeaveRoomButtonClicked() {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }

    private void RealTimeResultLocal() {
        Debug.Log($"showing result realtime___________");

        FinActivePlayersGeneric(networkPlayerList);

        // realtTimeResultSolo_Panel.gameObject.SetActive(true);
        UpdateResult(networkPlayerList, isSoloMode);
    }
    List<NetworkPlayer> FinActivePlayersGeneric(List<NetworkPlayer> activePlayersList)
    {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        activePlayersList.Clear();  // xoa list cu, neu ko se bi override
        foreach (var item in gameObjectsToTransfer)
            activePlayersList.Add(item.GetComponent<NetworkPlayer>());

        return activePlayersList;
    }
    private void UpdateResult(List<NetworkPlayer> networkPlayerList, bool isSoloMode) {
        if(resultListUIHandler_Solo == null) return;
        if(resultListUIHandler_Team == null) return;


        if(networkPlayerList.Count != 0) {
            resultListUIHandler_Solo.ClearList();
            resultListUIHandler_Team.ClearList();

            // sort list theo thu tu kill giam dan
            var newList = networkPlayerList.OrderByDescending(s => s.GetComponent<WeaponHandler>().killCountCurr).ToList();

            if(isSoloMode) {
                realtTimeResultSolo_Panel.gameObject.SetActive(true);
                foreach (NetworkPlayer item in newList) {
                    resultListUIHandler_Solo.AddToList(item);
                }
            } else {
                realtTimeResultTeam_Panel.SetActive(true);
                foreach (NetworkPlayer item in newList) {
                    resultListUIHandler_Team.AddToList(item);
                }
            }
            
        }
        
        Debug.Log($"networkPlayerList.Count = {networkPlayerList.Count}");
    }

    public void ShowWinOrLossResult() {
        if(networkPlayer.isWin_Network) finalResultTeam.text = "WIN";
        else finalResultTeam.text = "LOSS";
    }

    public void SetNetworkPlayer(NetworkPlayer networkPlayer) {
        this.networkPlayer = networkPlayer;
    }
}
