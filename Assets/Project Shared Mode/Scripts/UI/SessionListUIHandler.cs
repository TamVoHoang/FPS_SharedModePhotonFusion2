using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameObject = SessionList_Panel
//todo (cai bang chua cac session update) chua cac thanh sessionListItem(name, count, join)
public class SessionListUIHandler : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup; // transform noi se spawn cac sessionItemListPF (name, count)
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameObject sessionItemListPF; // prefab chua SessionInfoUIListItem.cs

    [SerializeField] Button OnCreateSesison_Button; // nut tao button sau khi kiem tra ko thay room | hoac muon tao phong moi
    [SerializeField] Button OnRefresh_Button;   // nut fresh sessions List

    [SerializeField] List<SessionInfo> sessionList = new List<SessionInfo>();

    public List<SessionInfo> SessionList{set => this.sessionList = value; }
    private void Awake() {
        ClearList();

        OnCreateSesison_Button.interactable = false;

        OnRefresh_Button.onClick.AddListener(OnRefreshSessionsListClicked);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            int sessionsCount = sessionList.Count();
            Debug.Log($"_____SessionsCount = {sessionsCount}");
        }
    }

    //moi lan update se clear va instantiate PF
    public void ClearList() {
        foreach (Transform item in verticalLayoutGroup.transform) {
            Destroy(item.gameObject);
        }

        statusText.gameObject.SetActive(false);
    }

    //? add sessionItemListPF vao panel transform - tao thanh room name count join button
    public void AddToList(SessionInfo sessionInfo) {

        SessionInfoUIListItem sessionInfoUIListItem = Instantiate(sessionItemListPF, verticalLayoutGroup.transform).GetComponent<SessionInfoUIListItem>();
        
        sessionInfoUIListItem.SetInfomation(sessionInfo); //=> dung sessionInfo show name, count, active JoinButton

        // gan dc ham Action<SessionInfo> OnJoinSession coll 19 | Onclick() coll 35 se goi ham nay chay
        sessionInfoUIListItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;

    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.JoinGame(sessionInfo);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnLookingForGameSessions() {
        ClearList();
        
        statusText.text = "Looking sessionInfo";
        statusText.gameObject.SetActive(true); // hien thong bao ko tim thay sessionInfo
    }

    //? OnSessionListUpdated() in Spawner called
    public void OnNoSessionFound() {
        ClearList();

        statusText.text = "No sessionInfo";
        statusText.gameObject.SetActive(true); // hien thong bao ko tim thay sessionInfo
    }

    public void ActiveOnCreateSesison_Button() => OnCreateSesison_Button.interactable = true;

    void OnRefreshSessionsListClicked() {

    }
}
