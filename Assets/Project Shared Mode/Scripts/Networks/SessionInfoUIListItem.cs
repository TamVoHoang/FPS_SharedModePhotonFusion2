using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;

public class SessionInfoUIListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomName;
    [SerializeField] TextMeshProUGUI playerCount;
    [SerializeField] Button jointButton;


    private void Awake() {
        jointButton.onClick.AddListener(OnJointButtonClicked);

    }
    private void Start() {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    void JoinRoom() {

    }

    
    private void OnJointButtonClicked() => JoinRoom();


}
