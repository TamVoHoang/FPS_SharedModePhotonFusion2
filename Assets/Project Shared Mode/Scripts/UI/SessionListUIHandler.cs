using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameObject = SessionList_Panel
//todo (cai bang chua cac session update) chua cac thanh sessionListItem(name, count, join)
public class SessionListUIHandler : MonoBehaviour
{
    [SerializeField] GameObject sessionListUpdate;  // panel show all session list update | popup button
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup; // transform noi se spawn cac sessionItemListPF (name, count)
    [SerializeField] GameObject sessionItemListPF; // prefab chua SessionInfoUIListItem.cs | sessionListUpdate -> instantiate
    [SerializeField] GameObject findingSessionPanel; // panel thong bao dang vao game Finding Room ...
    [SerializeField] TextMeshProUGUI sessionListStatusText;    // thong bao co sessionListUpdate sau khi nhan finding game

    List<SessionInfo> sessionList = new List<SessionInfo>();
    public List<SessionInfo> SessionList{set => this.sessionList = value; }

    [Header("       Buttons")]
    [SerializeField] Button OnQuickPlayClick_Button;
    [SerializeField] Button OnCreateSesison_Button; // nut tao new session (tao xong lam host session)
    [SerializeField] Button OnRefresh_Button;   // nut fresh sessions List
    [SerializeField] Button OnPopUpSesisonListClick_Button;

    //others
    
    private void Awake() {
        ClearList();

        OnCreateSesison_Button.interactable = false;

        OnQuickPlayClick_Button.onClick.AddListener(OnQuickPlayClicked);
        OnRefresh_Button.onClick.AddListener(OnRefreshSessionsListClicked);
        OnPopUpSesisonListClick_Button.onClick.AddListener(OnPopupSessionListUpdateClicked);
    }

    //moi lan update se clear va instantiate PF
    public void ClearList() {
        foreach (Transform item in verticalLayoutGroup.transform) {
            Destroy(item.gameObject);
        }

        sessionListStatusText.gameObject.SetActive(false);
    }

    //? add sessionItemListPF vao panel transform - tao thanh room name count join button
    public void AddToList(SessionInfo sessionInfo, string mapName) {

        SessionInfoUIListItem sessionInfoUIListItem = 
            Instantiate(sessionItemListPF, verticalLayoutGroup.transform).GetComponent<SessionInfoUIListItem>();
        
        // sessionListUpdate -> se set ham nay nho vao session Info
        // dung sessionInfo show name, count, active JoinButton
        sessionInfoUIListItem.SetInfomation(sessionInfo, mapName);

        //todo neu sessionInfo lock || having enough active Players => now showing joinButton
        /* if(sessionInfo.IsOpen == false || sessionInfo.PlayerCount >= sessionInfo.MaxPlayers) {
            sessionInfoUIListItem.joinButton.interactable = false;
        }
        else {
            sessionInfoUIListItem.joinButton.interactable = true;
        } */

        // gan dc ham Action<SessionInfo> OnJoinSession coll 19 | Onclick() coll 35 se goi ham nay chay
        sessionInfoUIListItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;
    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        Spawner spawner = FindObjectOfType<Spawner>();
        networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.GameMap);

        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }

    // Finding Button call
    public void OnLookingForGameSessions() {
        ClearList();
        
        sessionListStatusText.text = "Looking sessionInfo";
        sessionListStatusText.gameObject.SetActive(true); // hien thong bao ko tim thay sessionInfo
    }

    //? OnSessionListUpdated() in Spawner called
    public void OnNoSessionFound() {
        ClearList();

        sessionListStatusText.text = "No sessionInfo";
        sessionListStatusText.gameObject.SetActive(true); // hien thong bao ko tim thay sessionInfo

        StartCoroutine(ClearStatusTextCo(2f));  // sessionListUpdate -> "no session found " -> " "
    }

    IEnumerator ClearStatusTextCo(float time) {
        yield return new WaitForSeconds(time);
        sessionListStatusText.text = "";   //! them vao de ko hien lien tuc "No sessionInfo"
    }


    public void ActiveOnCreateSesison_Button() => OnCreateSesison_Button.interactable = true;

    //todo do again OnFindGameClicked() MainMenuUIHandler.cs row 68
    void OnRefreshSessionsListClicked() {
        OnLookingForGameSessions();    // xoa list session - hien chu looking text phia duoi
        findingSessionPanel.gameObject.SetActive(true);

        // no use
        /* NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();
        int sessionsCount = sessionList.Count;
        Debug.Log($"_____SessionsCount = {sessionsCount}"); */

        StartCoroutine(FreshSessionListCo(4));
    }

    IEnumerator FreshSessionListCo(float time) {
        yield return new WaitForSeconds(1); // de co the thay duoc chu looking ben duoi
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(time);

        findingSessionPanel.gameObject.SetActive(false);

        int sessionsCount = sessionList.Count;
        Debug.Log($"_____SessionsCount = {sessionsCount}");
    }

    private void OnQuickPlayClicked()
    {
        //networkRunnerHandler.OnJoinLobby();

        // auto tim sessionInfo trong update sessionList spawner.cs

        /* foreach (var item in sessionList)
        {
            if(item.IsOpen && item.PlayerCount < item.MaxPlayers) {
                sessionInfo = item;
            }
        } */

        // sessionInfo = GetRandomSesisonInfo();
        // if(sessionInfo != null) {
        //     Spawner spawner = FindObjectOfType<Spawner>();
        //     networkRunnerHandler.JoinGame(sessionInfo, spawner.customLobbyName);
        // }

        
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
        /* statusPanel.gameObject.SetActive(true); */
        //statusText.gameObject.SetActive(false);

        OnLookingForGameSessions();
        findingSessionPanel.gameObject.SetActive(true);

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(time);

        var sessionInfo = GetRandomSesisonInfo();
        var spawner = FindObjectOfType<Spawner>();
        if(sessionInfo != null) {
            sessionListStatusText.text = $"Join session {sessionInfo.Name}";
            sessionListStatusText.gameObject.SetActive(true);
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.GameMap);
        }
        else {
            // row 90 da thong bao sessionlistUpdate call OnNoSessionFound() -> set sessionListStatusText
            /* sessionListStatusText.text = "No sessison to join"; */   
            sessionListStatusText.gameObject.SetActive(true);
        }
        
        findingSessionPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        sessionListStatusText.text = "";

        /* statusPanel.gameObject.SetActive(false); */
    }



    // Popup SessionListUpdate
    private void OnPopupSessionListUpdateClicked()
    {
        
        sessionListUpdate.SetActive(!sessionListUpdate.activeSelf);

    }
}