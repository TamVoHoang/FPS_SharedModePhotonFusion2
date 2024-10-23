using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//gameobject = 
public class ShowPlayerInfo : MonoBehaviour
{
    // show info UI
    [SerializeField] TextMeshProUGUI userName;
    [SerializeField] TextMeshProUGUI currentLevel;
    [SerializeField] TextMeshProUGUI highScore;
    [SerializeField] TextMeshProUGUI coins;

    // buttons
    [SerializeField] Button saveFirebaseButton; // playerdata test
    [SerializeField] Button loadFirebaseButton; // playerdata test

    [SerializeField] Button saveFireStoreSignUpButton;
    [SerializeField] Button saveFireStoreRealtimeButton;
    [SerializeField] Button loadFireStoreButton;



    [SerializeField] Button gotoLobby;
    [SerializeField] Button quickPlay;


    const string MAINMENU = "MainMenu";
    const string WORLD1 = "World1";

    //ohters
    DataSaver _dataSaver;
    DataSaveLoadHander _dataSaveLoadHander;
    private void Awake() {
        _dataSaver = FindObjectOfType<DataSaver>();
        _dataSaveLoadHander = FindObjectOfType<DataSaveLoadHander>();
    }

    private void Start() {
        saveFirebaseButton.onClick.AddListener(SaveManualTest);
        loadFirebaseButton.onClick.AddListener(LoadMaunalTest);

        saveFireStoreSignUpButton.onClick.AddListener(SaveFireStoreManulTest);
        loadFireStoreButton.onClick.AddListener(LoadFireStoreManulTest);

        saveFireStoreRealtimeButton.onClick.AddListener(LoadFireStoreManulTestRealTime);


        gotoLobby.onClick.AddListener(GoToLobby);
        quickPlay.onClick.AddListener(GoToQickBattle);


        StartCoroutine(ShowPlayerDataCo(0.5f));
    }

    IEnumerator ShowPlayerDataCo(float time) {
        yield return new WaitForSeconds(time);
        ShowInfoFireStore();
        StopAllCoroutines();
    }

    void SaveManualTest() {
        //_dataSaver.SaveData();  // save realtime database

        _dataSaveLoadHander.SavePlayerDataFireStore();
    }

    void LoadMaunalTest() {
        // _dataSaver.LoadData();
        // StartCoroutine(ShowPlayerDataCo(0.5f));

        _dataSaveLoadHander.LoadPlayerDataFireStore();
        StartCoroutine(ShowPlayerDataCo(0.5f));
    }


    private void SaveFireStoreManulTest()
    {
        _dataSaveLoadHander.SaveInventoryDataFireStoreToSignUp();
    }
    
    private void LoadFireStoreManulTestRealTime(){
        _dataSaveLoadHander.SaveInventoryDataFireStoreRealtime();
    }

    private void LoadFireStoreManulTest()
    {
        _dataSaveLoadHander.LoadInventoryDataFireStore();

        StartCoroutine(ShowPlayerDataCo(0.5f));
    }
    
    private void GoToLobby()
    {
        StartCoroutine(LoadToMainLobby(0.5f));
    }

    IEnumerator LoadToMainLobby(float time) {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(MAINMENU);
    }

    private void GoToQickBattle()
    {
        StartCoroutine(LoadToQuickBattle(0.5f));
    }

    IEnumerator LoadToQuickBattle(float time) {
        yield return new WaitForSeconds(time);
        SceneManager.LoadSceneAsync(WORLD1);
    }
    
    void ShowInfoFireStore() {
        // Debug.Log($"_____show player info");
        //var data = DataSaver.Instance.dataToSave; // data from realtime database
        var data = DataSaveLoadHander.Instance.playerDataToFireStore;   // data from firestore
        
        userName.text = "User name: " + data.userName;
        currentLevel.text = "Current Level: " + data.currLevel.ToString();
        highScore.text = "High Score: " + data.highScore.ToString();
        coins.text = "Coins: " + data.coins.ToString();
    }
}
