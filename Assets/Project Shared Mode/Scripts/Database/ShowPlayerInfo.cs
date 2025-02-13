using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using Firebase.Auth;

//gameobject = 
public class ShowPlayerInfo : MonoBehaviour
{
    // show info UI
    [SerializeField] TextMeshProUGUI userName;
    [SerializeField] TextMeshProUGUI killedCountText;
    [SerializeField] TextMeshProUGUI deathCountText;
    [SerializeField] TextMeshProUGUI coins;

    [SerializeField] GameObject loadingScreen;  // loading animation

    // buttons
    [SerializeField] Button saveFirebasePlayerData_Button; // playerdata test
    [SerializeField] Button loadFirebasePlayerData_Button; // playerdata test

    [SerializeField] Button saveFireStoreInvenSignUp_Button;
    [SerializeField] Button saveFireStoreInvenRealtime_Button;
    [SerializeField] Button loadFireStoreInven_Button;
    [SerializeField] Button backToLoginScene_Button;

    [SerializeField] Button gotoMainMenu;
    [SerializeField] Button quickPlay;
    [SerializeField] Button playerStats_Button;
    [SerializeField] GameObject playerStats_Panel;
    bool isPlayerStatsPopUp = false;

    const string MAINMENU = "MainMenu";
    const string WORLD_1 = "World_1";
    const string LOGIN = "Login";


    //ohters
    DataSaver _dataSaver;
    DataSaveLoadHander _dataSaveLoadHander;
    private void Awake() {
        _dataSaver = FindObjectOfType<DataSaver>();
        _dataSaveLoadHander = FindObjectOfType<DataSaveLoadHander>();
    }

    private void Start() {
        playerStats_Panel.SetActive(false);
        isPlayerStatsPopUp = false;

        saveFirebasePlayerData_Button.onClick.AddListener(SaveFBPlayerData);
        loadFirebasePlayerData_Button.onClick.AddListener(LoadFBPlayerData);

        saveFireStoreInvenSignUp_Button.onClick.AddListener(SaveFSInvenSignUp);
        saveFireStoreInvenRealtime_Button.onClick.AddListener(SaveFSInvenRealtime);
        loadFireStoreInven_Button.onClick.AddListener(LoadFSInvenRealtime);


        gotoMainMenu.onClick.AddListener(GoToMainMenu);
        quickPlay.onClick.AddListener(GoToQickBattle);

        playerStats_Button.onClick.AddListener(PlayerStatsOnClick);
        backToLoginScene_Button.onClick.AddListener(GoToLoginOnClick);
        StartCoroutine(ShowPlayerDataCo(0.5f));
    }

    private void PlayerStatsOnClick()
    {
        isPlayerStatsPopUp = !isPlayerStatsPopUp;
        if (isPlayerStatsPopUp) {
            playerStats_Panel.SetActive(true);
        } else playerStats_Panel.SetActive(false);
    }

    IEnumerator ShowPlayerDataCo(float time) {
        yield return new WaitForSeconds(time);
        ShowInfoFireStore();
        StopAllCoroutines();
    }

    [Button]
    void SaveFBPlayerData() {
        //_dataSaver.SaveData();  // save realtime database

        _dataSaveLoadHander.SavePlayerDataFireStore();
    }

    [Button]
    void LoadFBPlayerData() {
        // _dataSaver.LoadData();
        // StartCoroutine(ShowPlayerDataCo(0.5f));

        _dataSaveLoadHander.LoadPlayerDataFireStore();
        StartCoroutine(ShowPlayerDataCo(0.5f));
    }

    [Button]
    private void SaveFSInvenSignUp()
    {
        _dataSaveLoadHander.SaveInventoryDataFireStoreToSignUp();
    }
    [Button]
    private void SaveFSInvenRealtime(){
        _dataSaveLoadHander.SaveInventoryDataFireStoreRealtime();
    }

    [Button]
    private async void LoadFSInvenRealtime()
    {
        await _dataSaveLoadHander.LoadInventoryDataFireStore_();

        StartCoroutine(ShowPlayerDataCo(0.5f));
    }
    
    private void GoToMainMenu()
    {
        StartCoroutine(LoadToMainLobby(1f));
    }

    IEnumerator LoadToMainLobby(float time) {
        // hien animation Loading Pf
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(MAINMENU);
        loadingScreen.SetActive(false);
        
    }

    private void GoToQickBattle()
    {
        StartCoroutine(LoadToQuickBattle(0.5f));
    }

    IEnumerator LoadToQuickBattle(float time) {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(WORLD_1);
    }

    private void GoToLoginOnClick() {
        SignOut();
        StartCoroutine(GoToLoginCo(0.5f));
    }

    IEnumerator GoToLoginCo(float time) {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(LOGIN);
    }

    public void SignOut()
    {
        DataSaveLoadHander.Instance.ResetDataLogout();
        // Firebase sign-out
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();
    }
    
    void ShowInfoFireStore() {
        // Debug.Log($"_____show player info");
        //var data = DataSaver.Instance.dataToSave; // data from realtime database
        var data = DataSaveLoadHander.Instance.playerDataToFireStore;   // data from firestore
        
        userName.text = "User name: " + data.UserName;
        killedCountText.text = "Killed Count: " + data.KilledCount.ToString();
        deathCountText.text = "Death Count: " + data.DeathCount.ToString();
        coins.text = "Coins: " + data.Coins.ToString();
    }
}
