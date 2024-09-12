using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameObject = moi mot SessionListItem prefab (sessionName | playerCount | JoinButton)

public class SessionInfoUIListItem : MonoBehaviour
{
    // session list panel
    [SerializeField] TextMeshProUGUI sessionNameText;
    [SerializeField] TextMeshProUGUI playerCountText;
    public Button joinButton;

    //* OnSessionListUpdated() spawner.cs gan sessionInfo -> AddToList(SessionInfo sessionInfo) SessionListUIHandler.cs
    //* SessionListUIHandler gan sessionInfo -> SetInfomation(sessionInfo)
    //* this.sessionInfo co duoc tu sessionInfo in Spaner.cs row 199
    SessionInfo sessionInfo; // chua tat ca thong tin playerCount

    //Events - khi nguoi choi nhan nut Join
    public event Action<SessionInfo> OnJoinSession; // duoc gan khi AddToList row 38 SeesionUIHandler.cs

    // hien thi thong tin cu the cua tung SessionListItem gamobject

    private void Awake() {
        joinButton.onClick.AddListener(OnJoinSessionClicked);
    }

    public void SetInfomation(SessionInfo sessionInfo) {
        this.sessionInfo = sessionInfo;
        sessionNameText.text = sessionInfo.Name;
        playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

        bool isJoinButtonActice = true; // enable JoinButton

        //todo neu sessionInfo lock || having enough active Players => now showing joinButton
        if(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers || sessionInfo.IsOpen == false)
            isJoinButtonActice = false;

        joinButton.gameObject.SetActive(isJoinButtonActice);
    }

    //? nut Join sau khi nhap ten player show list sessionInfo
    //? run method 
    private void OnJoinSessionClicked()
    {
        // tao 1 su kien khi nhan chuot
        OnJoinSession?.Invoke(sessionInfo);
    }
    public void OnClick() {
        // tao 1 su kien khi nhan chuot
        OnJoinSession?.Invoke(sessionInfo);
    }
}