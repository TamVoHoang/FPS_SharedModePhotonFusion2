using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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
        saveFireStoreButton.onClick.AddListener(LoadFireStoreManulTest);

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

    private void LoadFireStoreManulTest()
    {
        _dataSaveLoadHander.SaveFireStore();
    }

    private void SaveFireStoreManulTest()
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
        var dataToFireStore = DataSaveLoadHander.Instance.dataToFireStore;

        userName.text = "User name: " + dataToFireStore.userName;
        currentLevel.text = "Current Level: " + dataToFireStore.currLevel.ToString();
        highScore.text = "High Score: " + dataToFireStore.highScore.ToString();
        coins.text = "Coins: " + dataToFireStore.coins.ToString();
    }
}
