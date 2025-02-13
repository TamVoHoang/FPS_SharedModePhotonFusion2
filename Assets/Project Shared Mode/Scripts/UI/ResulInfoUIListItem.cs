using Fusion;
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

        // doi mau o dong ten cua minh
        if(networkPlayer == NetworkPlayer.Local) {
            playerNameText.color = Color.green;
            killDeathCountText.color = Color.green;
        }
        /* SaveToFirestoreEndGame(this.networkPlayer.GetComponent<WeaponHandler>().killCountCurr, 
            this.networkPlayer.GetComponent<HPHandler>().deadCountCurr); */
    }

    //? save 1 lan khi ket thuc tran. kill count va death count
    void SaveToFirestoreEndGame(int killedCountCurr, int deathCountCurr) {
        if(DataSaveLoadHander.Instance == null) return;

        DataSaveLoadHander.Instance.playerDataToFireStore.KilledCount += killedCountCurr;
        DataSaveLoadHander.Instance.playerDataToFireStore.DeathCount += deathCountCurr;

        DataSaveLoadHander.Instance.SavePlayerDataFireStore();
    }
}
