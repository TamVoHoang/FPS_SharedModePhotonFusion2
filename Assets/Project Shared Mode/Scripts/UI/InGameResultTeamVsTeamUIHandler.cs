
using System;
using TMPro;
using UnityEngine;

//todo game object = LocalUIInGame_Canvas trong moi player, nhan thong tin va thong bao
public class InGameResultTeamVsTeamUIHandler : MonoBehaviour
{
    public static Action<bool, string> Action_OnGameMessageRecieved;
    [SerializeField] TextMeshProUGUI teamAResultText;
    [SerializeField] TextMeshProUGUI teamBResultText;

    private void Start() {
        Action_OnGameMessageRecieved = OnGameMessageRecieved;
    }

    public void OnGameMessageRecieved(bool isEnemyTeam, string result) {
        if(!isEnemyTeam) teamAResultText.text = $"A: {result}";
        else teamBResultText.text = $"B: {result}";;
    }
}
