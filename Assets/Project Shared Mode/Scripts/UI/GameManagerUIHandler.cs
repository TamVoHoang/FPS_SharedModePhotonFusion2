using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameobject = canvasInGame world1 scene
public class GameManagerUIHandler : NetworkBehaviour
{
    [Networked]
    byte countDown {get; set;}

    [Networked]
    public bool isFinished {get; set;}
    
    bool isCursorShowed = false;

    [SerializeField] List<NetworkPlayer> networkPlayerList = new List<NetworkPlayer>();
    [SerializeField] List<NetworkPlayer> networkPlayerListRemote = new List<NetworkPlayer>();

    [Header("       Panels")]
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] GameObject resultTable_Panel;
    [SerializeField] GameObject backMainMenu_Panel;

    [Header("       Buttons")]
    [SerializeField] Button backToMainMenuInResultPanel_Button;
    [SerializeField] Button backToMainMenu_Button;

    [Header("       Timer")]
    [SerializeField] bool isStarted = false;
    [SerializeField] int timeRemainingToFinish = 20;
    TickTimer countDownTickTimer = TickTimer.None; // khi vao game thi bat dau dem
    
    //others
    [SerializeField] ResultListUIHandler resultListUIHandler;
    ChangeDetector changeDetector;

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    private void Awake() {
        backToMainMenu_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
        backMainMenu_Panel.SetActive(false);

        countDownText.text = "";
        countDownTickTimer = TickTimer.None;

        resultListUIHandler = FindObjectOfType<ResultListUIHandler>(true);
    }

    private void Start() {
        StartCoroutine(DelayToStartGame(1));

        backToMainMenuInResultPanel_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
        resultTable_Panel.gameObject.SetActive(false);
    }

    private void Update() {
        if(NetworkPlayer.Local == null) return;

        if(Input.GetKeyDown(KeyCode.Escape)) {
            backMainMenu_Panel.SetActive(!backMainMenu_Panel.activeSelf);
            ToggleCursor();
        }
    }

    public override void FixedUpdateNetwork() {
        // vao game timer dem nguoc
        StartGameTimer();

        // checking countdown timer to show finish game
        if(countDownTickTimer.Expired(Runner) && !isFinished) {
            FinishedGame();
            countDownTickTimer = TickTimer.None;
        }
        else if(countDownTickTimer.IsRunning) {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        }
    }

    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer)) {
            switch (change)
            {
                case nameof(countDown):
                OnCountDownChanged();
                    break;

                case nameof(isFinished):
                var boolReader = GetPropertyReader<bool>(nameof(isFinished));
                var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                OnTableResultChanged(previousBool, currentBool);
                    break;
            }
        }
    }

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
    
    void StartGameTimer() {
        if(isStarted) {
            if(Runner.IsSharedModeMasterClient)
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, timeRemainingToFinish);
            else {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
            isStarted = false;
        }
    }

    IEnumerator DelayToStartGame(float time) {
        yield return new WaitForSeconds(time);
        isStarted = true;
    }

    void OnCountDownChanged() {
        if(countDown == 0) countDownText.text = $"";
        else countDownText.text = $"TIME: {countDown}";
    }

    private void FinishedGame() {
        Debug.Log($"finish game___________");
        ShowCursor();

        //FindActivePlayers(networkPlayerList);
        FinActivePlayersGeneric(networkPlayerList);
        resultTable_Panel.gameObject.SetActive(true);
        UpdateResult(networkPlayerList);

        StartCoroutine(ShowResultTableCO(0.09f));
    }

    IEnumerator ShowResultTableCO(float time) {
        isFinished = true;
        yield return new WaitForSeconds(time);
        isFinished = false;
    }

    /* void FindActivePlayers(NetworkLinkedList<NetworkPlayer> networkPlayerList) {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in gameObjectsToTransfer)
            networkPlayerList.Add(item.GetComponent<NetworkPlayer>());

        Debug.Log($"FindActivePlayers = {networkPlayerList.Count}");
    }

    void FindActivePlayersRemote() {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in gameObjectsToTransfer)
            networkPlayerListRemote.Add(item.GetComponent<NetworkPlayer>());

        Debug.Log($"FindActivePlayers = {networkPlayerListRemote.Count}");
    } */

    List<NetworkPlayer> FinActivePlayersGeneric(List<NetworkPlayer> activePlayersList)
    {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in gameObjectsToTransfer)
            activePlayersList.Add(item.GetComponent<NetworkPlayer>());

        return activePlayersList;
    }

    private void UpdateResult(List<NetworkPlayer> networkPlayerList) {
        if(resultListUIHandler == null) return;

        if(networkPlayerList.Count != 0) {
            resultListUIHandler.ClearList();
            foreach (NetworkPlayer item in networkPlayerList) {
                resultListUIHandler.AddToList(item);
            }
        }
        
        Debug.Log($"networkPlayerList.Count = {networkPlayerList.Count}");
    }
    
    // remote result table change after isFinish == true
    void OnTableResultChanged(bool previous, bool current) {
        FinActivePlayersGeneric(networkPlayerListRemote);
        if(current && !previous) {
            
            Debug.Log($"networkPlayerListRemote {networkPlayerListRemote.Count}");
            resultTable_Panel.gameObject.SetActive(true);
            UpdateResult(networkPlayerListRemote);
            Debug.Log($"co update result table client");
        }
    }

    public void OnLeaveRoomButtonClicked() {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }

}