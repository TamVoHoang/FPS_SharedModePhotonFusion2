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
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup; // transform noi se spawn cac sessionItemListPF (name, count)
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameObject sessionItemListPF; // prefab chua SessionInfoUIListItem.cs

    [SerializeField] Button OnCreateSesison_Button; // nut tao button sau khi kiem tra ko thay room | hoac muon tao phong moi
    [SerializeField] Button OnRefresh_Button;   // nut fresh sessions List

    [SerializeField] List<SessionInfo> sessionList = new List<SessionInfo>();

    public List<SessionInfo> SessionList{set => this.sessionList = value; }
    SessionInfo sessionInfo;
    //others
    [SerializeField] Button OnQuickPlayClick_Button;
    // panel thong bao dang vao game Finding Room ...
    public GameObject findingRoomPanel;

    private void Awake() {
        ClearList();

        OnCreateSesison_Button.interactable = false;

        OnRefresh_Button.onClick.AddListener(OnRefreshSessionsListClicked);
        OnQuickPlayClick_Button.onClick.AddListener(OnQuickPlayClicked);

    }

    //moi lan update se clear va instantiate PF
    public void ClearList() {
        foreach (Transform item in verticalLayoutGroup.transform) {
            Destroy(item.gameObject);
        }

        statusText.gameObject.SetActive(false);
    }

    //? add sessionItemListPF vao panel transform - tao thanh room name count join button
    public void AddToList(SessionInfo sessionInfo, string mapName) {

        SessionInfoUIListItem sessionInfoUIListItem = Instantiate(sessionItemListPF, verticalLayoutGroup.transform).GetComponent<SessionInfoUIListItem>();
        
        sessionInfoUIListItem.SetInfomation(sessionInfo, mapName); //=> dung sessionInfo show name, count, active JoinButton

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

    //todo do again OnFindGameClicked() MainMenuUIHandler.cs row 68
    void OnRefreshSessionsListClicked() {
        OnLookingForGameSessions();    // xoa list session - hien chu looking
        /* NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();
        
        int sessionsCount = sessionList.Count;
        Debug.Log($"_____SessionsCount = {sessionsCount}"); */

        StartCoroutine(Delay(2));
    }

    IEnumerator Delay(float time) {
        yield return new WaitForSeconds(time);
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();
        
        int sessionsCount = sessionList.Count;
        Debug.Log($"_____SessionsCount = {sessionsCount}");
    }

    private void OnQuickPlayClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
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

        StartCoroutine(DelayStartRandom(3));
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
        statusText.gameObject.SetActive(false);

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();

        yield return new WaitForSeconds(time);
        var sessionInfo = GetRandomSesisonInfo();
        var spawner = FindObjectOfType<Spawner>();
        if(sessionInfo != null) {
            statusText.text = $"Join session {sessionInfo.Name}";
            statusText.gameObject.SetActive(true);
            networkRunnerHandler.JoinGame(sessionInfo, spawner.CustomLobbyName, spawner.GameMap);
        }
        else {
            statusText.text = "No sessison to join";
            statusText.gameObject.SetActive(true);
        }
        
        findingRoomPanel.gameObject.SetActive(false);
        //statusPanel.gameObject.SetActive(false);
    }
}