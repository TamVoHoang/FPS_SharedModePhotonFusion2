using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//todo gameobject = canvasInGame world1 scene
public class GameManagerUIHandler : NetworkBehaviour
{
    [Networked] byte countDown {get; set;}
    [Networked] public bool isFinished {get; set;}
    [Networked] private NetworkBool isTimerRunning { get; set; }
    [Networked] private float networkTimerStart { get; set; }

    [SerializeField] List<NetworkPlayer> networkPlayerList = new List<NetworkPlayer>();         // list show local
    [SerializeField] List<NetworkPlayer> networkPlayerListRemote = new List<NetworkPlayer>();   // list show remote

    [Header("       Panels")]
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] GameObject resultTableSolo_Panel;
    [SerializeField] GameObject resultTableTeam_Panel;


    [Header("       Buttons")]
    [SerializeField] Button backToMainMenuInResultPanelSolo_Button;
    [SerializeField] Button backToMainMenuInResultPanelTeam_Button;


    [Header("       Timer")]
    [SerializeField] bool isStarted = false;
    [SerializeField] int timeRemainingToFinish = 20;
    TickTimer countDownTickTimer = TickTimer.None; // khi vao game thi bat dau dem
    
    //others
    [SerializeField] ResultListUIHandler resultListUIHandler_Solo;
    [SerializeField] ResultListUIHandler_Team resultListUIHandler_Team;

    ChangeDetector changeDetector;
    bool cursorLocked;
    [SerializeField] bool isSoloMode;

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (Object.HasStateAuthority) {
            isTimerRunning = false;
            countDown = (byte)timeRemainingToFinish;
        }
    }

    private void Awake() {
        countDownText.text = "";
        countDownTickTimer = TickTimer.None;

        //resultListUIHandler_Solo = GetComponentInChildren<ResultListUIHandler>(true);
        resultListUIHandler_Solo = resultTableSolo_Panel.GetComponent<ResultListUIHandler>();
        resultListUIHandler_Team = resultTableTeam_Panel.GetComponent<ResultListUIHandler_Team>();

        if(!NetworkPlayer.Local) {
            Debug.Log($"_____ solo mode is true player directly join at battle scene");
            isSoloMode = true;
        } else isSoloMode = NetworkPlayer.Local.IsSoloMode();

        //Always make sure that our cursor is locked when the game starts!
        //Update the cursor's state.
        /* cursorLocked = true;
        UpdateCursorState(); */
    }

    private void Start() {
        StartCoroutine(DelayToStartGame(1));

        backToMainMenuInResultPanelSolo_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
        backToMainMenuInResultPanelTeam_Button.onClick.AddListener(OnLeaveRoomButtonClicked);

        resultTableSolo_Panel.gameObject.SetActive(false);
        resultTableTeam_Panel.gameObject.SetActive(false);
    }

    private void Update() {
        if(NetworkPlayer.Local == null) return;

        // ESC to active or deActive cursor
        /* OnLockCursor(); */
    }

    public override void FixedUpdateNetwork() {
        if (Object.HasStateAuthority) {
            if (isStarted && !isTimerRunning) StartGameTimer();

            if (isTimerRunning) UpdateTimer();
        }
    }

    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer)) {
            switch (change) {
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

    //? toggle new version
    /* void OnLockCursor() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            cursorLocked = !cursorLocked;
            UpdateCursorState();
            backMainMenu_Panel.SetActive(!backMainMenu_Panel.activeSelf);
        }
    }

    private void UpdateCursorState() {
        //Update cursor visibility.
        Cursor.visible = !cursorLocked;
        //Update cursor lock state.
        Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    } */

    //? toggle cursor old version
    void ShowCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    void StartGameTimer() {
        networkTimerStart = Runner.SimulationTime;
        isTimerRunning = true;
        isStarted = false;
    }

    void UpdateTimer() {
        float elapsed = Runner.SimulationTime - networkTimerStart;
        float remaining = timeRemainingToFinish - elapsed;
        
        if (remaining <= 0) {
            isTimerRunning = false;
            countDown = 0;
            if (!isFinished) {
                FinishedGame();
            }
        }
        else {
            countDown = (byte)Mathf.Ceil(remaining);
        }
    }

    IEnumerator DelayToStartGame(float time) {
        yield return new WaitForSeconds(time);
        isStarted = true;
    }

    void OnCountDownChanged() {
        if (countDown == 0) {
            countDownText.text = "";
        } else {
            countDownText.text = $"TIME: {countDown}";
        }
    }

    private void FinishedGame() {
        Debug.Log($"finish game___________");
        ShowCursor();

        FinActivePlayersGeneric(networkPlayerList);
        
        UpdateResult(networkPlayerList);

        StartCoroutine(ShowResultTableCO(0.09f));
    }
    List<NetworkPlayer> FinActivePlayersGeneric(List<NetworkPlayer> activePlayersList)
    {
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        activePlayersList.Clear();
        foreach (var item in gameObjectsToTransfer)
            activePlayersList.Add(item.GetComponent<NetworkPlayer>());

        return activePlayersList;
    }

    private void UpdateResult(List<NetworkPlayer> networkPlayerList) {
        if(resultListUIHandler_Solo == null) return;
        if(resultListUIHandler_Team == null) return;

        if(networkPlayerList.Count != 0) {
            resultListUIHandler_Solo.ClearList();
            resultListUIHandler_Team.ClearList();

            // sort list theo thu tu kill giam dan
            var newList = networkPlayerList.OrderByDescending(s => s.GetComponent<WeaponHandler>().killCountCurr).ToList();

            if(isSoloMode) {
                resultTableSolo_Panel.gameObject.SetActive(true);
                foreach (NetworkPlayer item in newList) {
                    resultListUIHandler_Solo.AddToList(item);
                }
            } else {
                resultTableTeam_Panel.gameObject.SetActive(true);
                foreach (NetworkPlayer item in newList) {
                    resultListUIHandler_Team.AddToList(item);
                }
            }
            
        }
        
        Debug.Log($"networkPlayerList.Count = {networkPlayerList.Count}");
    }

    IEnumerator ShowResultTableCO(float time) {
        isFinished = true;
        yield return new WaitForSeconds(time);
        isFinished = false;
    }
    
    //? remote result table change after isFinish == true
    void OnTableResultChanged(bool previous, bool current) {
        FinActivePlayersGeneric(networkPlayerListRemote);
        if(current && !previous) {
            
            Debug.Log($"networkPlayerListRemote {networkPlayerListRemote.Count}");
            
            UpdateResult(networkPlayerListRemote);
            Debug.Log($"co update result table client");
        }
    }

    public void OnLeaveRoomButtonClicked() {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }

    public void GameManagerRequestStateAuthority() {
        if (Object == null) return;

        if (!Object.HasStateAuthority)
        {
            try
            {
                Object.RequestStateAuthority();
                Debug.Log($"///Requesting state authority for bot {gameObject.name}.");
            }
            catch (Exception ex)
            {
                Debug.Log($"///Failed to request state authority: {ex.Message}");
            }
        }
        else
        {
            Debug.Log("///Object already has state authority.");
        }
    }
}