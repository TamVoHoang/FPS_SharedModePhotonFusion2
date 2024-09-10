
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;   //transform parent chua cac transform child can dat ten cu the

    private LocalCameraHandler localCameraHandler;
    public LocalCameraHandler LocalCameraHandler => localCameraHandler;

    private void Awake() {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
    }
    public override void Spawned()
    {
        //? kiem tra co dang spawn tai ready scene hay khong
        bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

        if(this.Object.HasInputAuthority) {
            Debug.Log($"___xet Local Instance for NetworkPlayer");
            Local = this;

            //? kiem tra Ready scene de ON MainCam OF LocalCam
            if(isReadyScene) {

            }
            else {
                Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                //? tat main camera cua local player
                if(Camera.main != null)
                    Camera.main.gameObject.SetActive(false);
                
                //? ON local camera
                localCameraHandler.localCamera.enabled = true;  // ON camera component
                localCameraHandler.gameObject.SetActive(true);  //ON ca gameObject LocalCameraHandler(co camera + gun)

                //? deAttach neu localCamera dang enable ra khoi folder cha
                localCameraHandler.transform.parent = null;

                //? bat local UI | canvas cua ca local player(crossHair, onDamageImage, messages rpc send)
                /* localUI.SetActive(true); // con cua localCamera transform */

                //? disable mouse de play
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else {
            localCameraHandler.localCamera.enabled = false;
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;
        }
    }
    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority) {
            Runner.Despawn(Object);
            Debug.Log($"___NetworkPlayer Left Room");
        }
    }

}