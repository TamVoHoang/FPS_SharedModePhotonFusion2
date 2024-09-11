using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    // player name
    [Networked]
    public NetworkString<_16> nickName_Network{get; set;} // state authority set bien nay

    [Networked]
    public bool isPublic {get; set;}

    bool isPublicJoinMessageSent = false;
    NetworkInGameMessages networkInGameMessages;
    [SerializeField] TextMeshProUGUI nickName_TM;
    ChangeDetector changeDetector;

    public static NetworkPlayer Local { get; set; }
    public Transform playerModel;   //transform parent chua cac transform child can dat ten cu the

    // camera
    private LocalCameraHandler localCameraHandler;
    public LocalCameraHandler LocalCameraHandler => localCameraHandler;

    // ui chua crossHair, red image get damage
    [SerializeField] GameObject localUI; // game object = PlayerUICanvas (canvas cua ca player)
    private void Awake() {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority) {
            CheckJoinMessage();
        }
            
    }


    void CheckJoinMessage(){
        if(Input.GetKeyDown(KeyCode.U)) {
            if(!isPublicJoinMessageSent) {
                Debug.Log($"__________________co vao join");
                networkInGameMessages.SendInGameRPCMessage(nickName_Network.ToString(), " -> joined room");
                isPublicJoinMessageSent = true;
            }
        }
        
    }

    //todo nhung thay doi cua bien Network
    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(nickName_Network):
                    OnNickNameChanged();
                    break;
            }
        }
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        OnNickNameChanged();// phai co de show ten khi spawn vao world1 scene
            
        // khi spawn chan FixUpdateNetwork in CharactermovementHandler run - coll 45 -> player spawn
        if(Object.HasStateAuthority) {
            GetComponent<CharacterMovementHandler>().RequestRespawn();
        }

        //? kiem tra co dang spawn tai ready scene hay khong
        bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

        if(this.Object.HasInputAuthority) {
            Local = this;

            //? kiem tra Ready scene de ON MainCam OF LocalCam
            if(isReadyScene) {
                Camera.main.transform.position = new Vector3(transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);

                // OF localCam
                localCameraHandler.gameObject.SetActive(false);

                // OF localPlayer UI
                localUI.SetActive(false);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
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
                localUI.SetActive(true); // con cua localCamera transform

                //? disable mouse de play
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // lay gia tri Gamemanager.playerNickName gan vao
            //RPC_SetNickName(GameManager.Instance.playerNickName);
            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickName_Local"));

            //? ko hien playerName cua Local - ko can thay ten minh
            nickName_TM.gameObject.SetActive(false);
        }
        else {
            localCameraHandler.localCamera.enabled = false;
            localCameraHandler.gameObject.SetActive(false);
            localUI.SetActive(false);
        }

        //? set player as a player object -> khi player left se chi hien dung ten player roi
        Runner.SetPlayerObject(Object.InputAuthority, Object);
        Debug.Log($"_____________ObjectNetwork = {Object}");

        /* var name = GameManager.Instance.playerNickName; */
        transform.name = $"P_{Object.Id} -> {nickName_Network.ToString()}";
    }

    //? gan nickName_Network cho bien texMeshPro GUI local
    private void OnNickNameChanged() {
        Debug.Log($"NickName changed to {nickName_Network} for player {gameObject.name}");
        nickName_TM.text = nickName_Network.ToString();
    }

    //? phuong thuc de local player send data cua rieng no len stateAuthority
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default) {
        Debug.Log($"[RPC] Set nickName {nickName} for localPlayer");
        this.nickName_Network = nickName;

        //! SEND TO ALL CLIENTS
        StartCoroutine(Delay());
    }
    IEnumerator Delay() {
        yield return new WaitForSeconds(0.5f);
        if(!isPublicJoinMessageSent) {
            Debug.Log($"__________________co vao join");
            networkInGameMessages.SendInGameRPCMessage(nickName_Network.ToString(), " -> joined room");
            isPublicJoinMessageSent = true;
        }
    }


    /* void OnDestroy() {
        // neu this.Object DeSpawn coll 130 - this.Object destroy - se destroy luon localCam cua no
        if(localCameraHandler != null) {
            Debug.Log("SU KIEN ONDESTROY LOCAL CAMERA HANDLER IN NETWORKPLAYER.CS");
            Destroy(localCameraHandler.gameObject);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    } */

    /* void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    } */

    /* void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Debug.Log($"{Time.time} OnSceneLoaded: " + scene.name);
        isPublicJoinMessageSent = false;
        if(scene.name != "Ready") {
        Debug.Log($"___________________OnSceneLoaded");

            // thong bao cho host biet can phai Spawned code
            if(Object.HasStateAuthority && Object.HasInputAuthority) {
                Spawned();
            }

            if(Object.HasStateAuthority)
                GetComponent<CharacterMovementHandler>().RequestRespawn();
        }
    } */

    //? nut back main menu
    public async void ShutdownLeftRoom() {
        await FindObjectOfType<NetworkRunner>().Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    //? interface IPlayerLeft implement
    public void PlayerLeft(PlayerRef player) {
        Debug.Log($"___________________PlayerLeft" + player);
        // thong bao khi roi khoi phong message
        if(Object.HasStateAuthority) {
            if(Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject)) {
                if(playerLeftNetworkObject == Object) {
                    Debug.Log($"__________________co vao left");
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>()
                        .nickName_Network.ToString(), " -> Left");
                }
            }
        }

        // Despanw - Destroy khi left
        if(player == Object.InputAuthority) {
            Runner.Despawn(Object);
            Debug.Log($"___NetworkPlayer Left Room");
        }
    }

}