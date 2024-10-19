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
    [SerializeField] Button saveFirebaseButton;
    [SerializeField] Button loadFirebaseButton;

    [SerializeField] Button saveFireStoreButton;
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

        saveFireStoreButton.onClick.AddListener(SaveFireStoreManulTest);
        loadFireStoreButton.onClick.AddListener(LoadFireStoreManulTest);

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
        _dataSaver.SaveData();
    }

    void LoadMaunalTest() {
        _dataSaver.LoadData();
        StartCoroutine(ShowPlayerDataCo(0.5f));
    }


    private void SaveFireStoreManulTest()
    {
        _dataSaveLoadHander.SaveFireStore();
    }

    private void LoadFireStoreManulTest()
    {
        _dataSaveLoadHander.LoadFireStore();
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
        var data = DataSaveLoadHander.Instance.playerDataToFireStore;
        //var data = DataSaver.Instance.dataToSave;
        
        userName.text = "User name: " + data.userName;
        currentLevel.text = "Current Level: " + data.currLevel.ToString();
        highScore.text = "High Score: " + data.highScore.ToString();
        coins.text = "Coins: " + data.coins.ToString();
    }
}
