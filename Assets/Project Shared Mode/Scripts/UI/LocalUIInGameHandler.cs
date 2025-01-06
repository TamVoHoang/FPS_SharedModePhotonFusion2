using System.Collections.Generic;
using System.Linq;
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

    [Header("       Buttons")]
    [SerializeField] Button backToMainMenuInQuitPanel_Button;

    [SerializeField] List<NetworkPlayer> networkPlayerList = new List<NetworkPlayer>();
    [SerializeField] ResultListUIHandler resultListUIHandler;
    
    bool isCursorShowed = false;
    bool isShowingRealtimeResultLocal_Solo = false;
    private void Awake() {
        resultListUIHandler = GetComponentInChildren<ResultListUIHandler>(true);
    }
    private void Start() {
        howToPlay_Panel.SetActive(false);
        backToMainMenu_Panel.SetActive(false);
        isShowingRealtimeResultLocal_Solo = false;
        realtTimeResultSolo_Panel.gameObject.SetActive(false);
        backToMainMenuInQuitPanel_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
    }

    private void Update() {
        // showing tutorial
        if(Input.GetKey(KeyCode.Tab)) {
            howToPlay_Panel.SetActive(true);
        } else howToPlay_Panel.SetActive(false);

        // showing panel to return mainmenu
        if(Input.GetKeyDown(KeyCode.Escape)) {
            backToMainMenu_Panel.SetActive(!backToMainMenu_Panel.activeSelf);
            ToggleCursor();
        }

        // showing realtime result table
        if(Input.GetKeyDown(KeyCode.V)) {
            isShowingRealtimeResultLocal_Solo = !isShowingRealtimeResultLocal_Solo;
            if(isShowingRealtimeResultLocal_Solo) {
                realtTimeResultSolo_Panel.SetActive(true);
                RealTimeResultLocal();
            } else {
                realtTimeResultSolo_Panel.SetActive(false);
            }
            
        }
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
        realtTimeResultSolo_Panel.gameObject.SetActive(true);
        UpdateResult(networkPlayerList);
    }
    List<NetworkPlayer> FinActivePlayersGeneric(List<NetworkPlayer> activePlayersList)
    {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        activePlayersList.Clear();  // xoa list cu, neu ko se bi override
        foreach (var item in gameObjectsToTransfer)
            activePlayersList.Add(item.GetComponent<NetworkPlayer>());

        return activePlayersList;
    }
    private void UpdateResult(List<NetworkPlayer> networkPlayerList) {
        if(resultListUIHandler == null) return;

        if(networkPlayerList.Count != 0) {
            resultListUIHandler.ClearList();

            // sort list theo thu tu kill giam dan
            var newList = networkPlayerList.OrderByDescending(s => s.GetComponent<WeaponHandler>().killCountCurr).ToList();

            foreach (NetworkPlayer item in newList) {
                resultListUIHandler.AddToList(item);
            }
        }
        
        Debug.Log($"networkPlayerList.Count = {networkPlayerList.Count}");
    }
}
