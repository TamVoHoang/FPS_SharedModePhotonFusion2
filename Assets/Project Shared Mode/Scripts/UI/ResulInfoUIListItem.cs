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
        killDeathCountText.text = $"<b>{this.networkPlayer.GetComponent<WeaponHandler>().killCount} / {this.networkPlayer.GetComponent<HPHandler>().deadCount}<b>";

    }
}
