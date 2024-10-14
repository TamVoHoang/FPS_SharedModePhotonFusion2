using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI readyButtonText;
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] bool isReady = false;

    [Header("Count Down")]
    [SerializeField] private int timeRemainingToBattle = 10;
    TickTimer countDownTickTimer = TickTimer.None; // khi nhan ready thi bat dau dem

    [Networked]
    byte countDown {get; set;}

    [Header("Buttons")]
    [SerializeField] Button OnReadyClick_Button;
    [SerializeField] Button OnLeaveClick_Button;

    [Header("Others")]
    Vector3 desiredCameraPosition = new Vector3 (0, 5, 20); // camera position based on ready or not
    ChangeDetector changeDetector;
    
    string sceneToStart;

    private void Awake() {
        OnReadyClick_Button.onClick.AddListener(OnReadyClicked);
        OnLeaveClick_Button.onClick.AddListener(OnLeaveClicked);
    }

    private void Start() {
        countDownText.text = "";
        countDownTickTimer = TickTimer.None;

    }

    
    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
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
        if(countDownTickTimer.Expired(Runner)) {
            StartGame();
            countDownTickTimer = TickTimer.None;
        }
        else if(countDownTickTimer.IsRunning) {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
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
            //DontDestroyOnLoad(item);
            
            // thong bao rpc isReady - set active readyImage
            /* if(!item.GetComponent<CharacterOutfitHandler>().isDoneWithCharacterSelection) {
                //Runner.Despawn(item.GetComponent<NetworkObject>());
                Runner.Disconnect(item.GetComponent<NetworkObject>().InputAuthority);
            } */
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

    private void OnReadyClicked()
    {
        if(isReady) isReady = false;
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
        //NetworkPlayer.Local.GetComponent<CharacterOutfitHandler>().OnReady(isReady);
    }
    
    private void OnLeaveClicked()
    {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }

    void OnCountDownChanged() {
        if(countDown == 0) countDownText.text = $"";
        else countDownText.text = $"THE BATTLES STARTS IN {countDown}";
    }

    public void SetOnLeaveButtonActive(bool isActice) => OnLeaveClick_Button.interactable = isActice;

}
