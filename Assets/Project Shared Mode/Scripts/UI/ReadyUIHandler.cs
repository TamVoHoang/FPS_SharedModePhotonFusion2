using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI readyButtonText;
    [SerializeField] TextMeshProUGUI countDownText;
    //[SerializeField] bool isReady = false;

    [Networked]
    private NetworkBool isReady { get; set; }
    [Networked]
    private float startTimeStamp { get; set; }
    [Networked]
    private NetworkBool isTimerActive { get; set; }

    [Header("Count Down")]
    [SerializeField] private int timeRemainingToBattle = 10;
    TickTimer countDownTickTimer = TickTimer.None; // khi nhan ready thi bat dau dem

    [Networked]
    byte countDown {get; set;}

    [Header("Buttons")]
    [SerializeField] Button OnReadyClick_Button;
    [SerializeField] Button OnLeaveClick_Button;
    [SerializeField] Button OnChangeTeamClick_Button;


    [SerializeField] Button OnHeadChageClick_Button;
    [SerializeField] Button OnArmorChageClick_Button;
    [SerializeField] Button OnSkinsChageClick_Button;


    [Header("Others")]
    Vector3 desiredCameraPosition = new Vector3 (0, 5, 20); // camera position based on ready or not
    ChangeDetector changeDetector;
    
    string sceneToStart;

    private void Awake() {
        OnReadyClick_Button.onClick.AddListener(OnReadyClicked);
        OnLeaveClick_Button.onClick.AddListener(OnLeaveClicked);
        OnChangeTeamClick_Button.onClick.AddListener(OnRequestTeamClicked);
    }


    private void Start() {
        countDownText.text = "";
        countDownTickTimer = TickTimer.None;

        OnHeadChageClick_Button.onClick.AddListener(OnChangeCharacter_Head);
        OnArmorChageClick_Button.onClick.AddListener(OnChangeCharacter_Body);
        OnSkinsChageClick_Button.onClick.AddListener(OnChangeCharacter_Skin);

        // hien thi nut change team neu la team mode
        UpdateToggleChangeTeamButton();

    }

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            isTimerActive = false;
            countDown = 0;
        }
    }

    private void Update() {
        if(NetworkPlayer.Local == null) return;

        // xet camera lerp theo player khi o ready scene
        float lerpSpeed = 0.5f;
        if(NetworkPlayer.Local != null) {
            if(!isReady) {
                desiredCameraPosition = new Vector3(NetworkPlayer.Local.transform.position.x, 1.5f, 3f);    // zoom in
                lerpSpeed = 7f;
            } else {
                desiredCameraPosition = new Vector3(NetworkPlayer.Local.transform.position.x, 2.5f, 5);     // zoom out
                lerpSpeed = .15f;
            }
        }
        Camera.main.transform.position = 
            Vector3.Lerp(Camera.main.transform.position, desiredCameraPosition, Time.deltaTime * lerpSpeed);
        
        // neu thoi gian dem nguoi het -> vao game
        /* if(countDownTickTimer.Expired(Runner)) {
            StartGame();
            countDownTickTimer = TickTimer.None;
        }
        else if(countDownTickTimer.IsRunning) {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        } */
    }

    public override void FixedUpdateNetwork()
    {
        /* if(Object.HasStateAuthority) {
            if(countDownTickTimer.Expired(Runner)) {
                StartGame();
                countDownTickTimer = TickTimer.None;
            }
            else if(countDownTickTimer.IsRunning) {
                countDown = (byte)countDownTickTimer.RemainingTime(Runner);
            }
        } */
        if (!Object.HasStateAuthority) return;

        if (isTimerActive)
        {
            float elapsedTime = Runner.SimulationTime - startTimeStamp;
            float remainingTime = timeRemainingToBattle - elapsedTime;

            if (remainingTime <= 0)
            {
                isTimerActive = false;
                countDown = 0;
                StartGame();
            }
            else
            {
                countDown = (byte)Mathf.Ceil(remainingTime);
            }
        }
        
    }



    //todo nhung thay doi cua bien Network
    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(countDown):
                OnCountDownChanged();
                    break;
            }
        }
    }

    void StartGame() {
        // khoa session de ko ai vao khi da ready va vao tran dau
        Runner.SessionInfo.IsOpen = false;

        //! ko xoa tag Player khi loadScene -> shared mode dang ko chay
        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");
        foreach (var item in gameObjectsToTransfer) {
            // ko xoa khi load scene
            DontDestroyOnLoad(item);
            
            // thong bao rpc isReady - set active readyImage
            if(!item.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection) {
                /* Runner.Despawn(item.GetComponent<NetworkObject>()); */
                Runner.Disconnect(item.GetComponent<NetworkObject>().InputAuthority);
            }
        }

        if(Runner.IsSharedModeMasterClient) {
            this.sceneToStart = NetworkPlayer.Local.SceneToStart;
        }
        
        // test load scene chi dinh bat ki
        /* Runner.LoadScene("World1"); */

        // load scene host da chon ben ngoai UI luc tao session
        if(sceneToStart != null)
            Runner.LoadScene(sceneToStart);
    }

    // change skins - heads
    public void OnChangeCharacter_Head() {
        if(isReady) return;
        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleHead();
    }

    public void OnChangeCharacter_Body() {
        if(isReady) return;
        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleBody();
    }

    public void OnChangeCharacter_Skin() {
        if(isReady) return;
        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnCycleSkin();
    }

    // Ready Button
    private void OnReadyClicked()
    {
        /* if(isReady) isReady = false;
        else isReady = true;

        if(isReady) readyButtonText.text = "NOT READY";
        else readyButtonText.text = "READY";

        //? neu la host nhan Ready Button -> hien thi dong ho
        if(Runner.IsSharedModeMasterClient) {
            if(isReady)
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, timeRemainingToBattle);
            else {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
        }

        // thong bao rpc isReady - set active readyImage
        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady); */
        isReady = !isReady;
        readyButtonText.text = isReady ? "NOT READY" : "READY";

        if (Object.HasStateAuthority)
        {
            if (isReady)
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }
        }

        NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }
    private void StartTimer()
    {
        if (!Object.HasStateAuthority) return;
        
        startTimeStamp = Runner.SimulationTime;
        isTimerActive = true;
    }

    private void StopTimer()
    {
        if (!Object.HasStateAuthority) return;

        isTimerActive = false;
        countDown = 0;
    }
    
    private void OnLeaveClicked()
    {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }

    private void OnRequestTeamClicked() {
        if(!CheckCanChangeTeam()) return;
        NetworkPlayer.Local.RPC_RequestChangeTeamAtReadyScene();
    }

    void OnCountDownChanged() {
        if(countDown == 0) countDownText.text = $"";
        else countDownText.text = $"THE BATTLES STARTS IN {countDown}";
    }

    void UpdateToggleChangeTeamButton() {
        bool isTeam = FindObjectOfType<Spawner>().TypeGame == TypeGame.Team;
        if(isTeam) OnChangeTeamClick_Button.gameObject.SetActive(true);
        else OnChangeTeamClick_Button.gameObject.SetActive(false);
    }

    bool CheckCanChangeTeam() {
        bool isEnemyCurr = NetworkPlayer.Local.isEnemy_Network;
        var activePlayers = FindObjectsOfType<NetworkPlayer>();
        int teamA = 0;
        int teamB = 0;
        foreach (var item in activePlayers)
        {
            if(!item.isEnemy_Network) teamA ++;
            else teamB ++;
        }

        if(teamA == teamB && teamA >= 2 && teamB >= 2) return true;
        if(teamA == teamB + 1 && !isEnemyCurr) return true;
        if(teamB == teamA + 1 && isEnemyCurr) return true;
        else return false;
    }

    // disable Leve butotn if networkObject is host session
    public void SetOnLeaveButtonActive(bool isActice) => OnLeaveClick_Button.interactable = isActice;

    

    public void ReadyUIhandlerRequestStateAuthority() {
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
