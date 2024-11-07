using TMPro;
using UnityEngine;

public class ResultInfoUIListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI killDeathCountText;

    NetworkPlayer networkPlayer;
    

    public void SetInfomation(NetworkPlayer networkPlayer) {
        this.networkPlayer = networkPlayer;
        playerNameText.text = this.networkPlayer.nickName_Network.ToString();
        killDeathCountText.text = $"<b>{this.networkPlayer.GetComponent<WeaponHandler>().killCountCurr} / {this.networkPlayer.GetComponent<HPHandler>().deadCountCurr}<b>";

        /* SaveToFirestoreEndGame(this.networkPlayer.GetComponent<WeaponHandler>().killCountCurr, 
            this.networkPlayer.GetComponent<HPHandler>().deadCountCurr); */
    }

    void SaveToFirestoreEndGame(int killedCountCurr, int deathCountCurr) {
        DataSaveLoadHander.Instance.playerDataToFireStore.KilledCount += killedCountCurr;
        DataSaveLoadHander.Instance.playerDataToFireStore.DeathCount += deathCountCurr;

        DataSaveLoadHander.Instance.SavePlayerDataFireStore();
    }
}
