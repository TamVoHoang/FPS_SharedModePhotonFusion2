
using UnityEngine;
using UnityEngine.UI;

public class InGameUIHandler : MonoBehaviour
{
    [SerializeField] Button backToMainMenuInResultPanel_Button;
    
    private void Start() {
        backToMainMenuInResultPanel_Button.onClick.AddListener(OnLeaveRoomButtonClicked);
    }

    private void OnLeaveRoomButtonClicked()
    {
        if(NetworkPlayer.Local)
            NetworkPlayer.Local.ShutdownLeftRoom();
    }
}
