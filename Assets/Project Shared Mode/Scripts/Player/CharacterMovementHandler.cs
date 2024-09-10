using UnityEngine;
using Fusion;
public class CharacterMovementHandler : NetworkBehaviour
{
    // other
    NetworkCharacterController networkCharacterController;
    LocalCameraHandler localCameraHandler;

    //input
    private bool _jumpPressed;
    Vector3 aimForwardVector = Vector3.zero;
    Vector2 movementInput = Vector2.zero;

    // request after falling
    [SerializeField] float fallHightToRespawn = -10f;
    bool isRespawnRequested = false;

    //...
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    private void Awake() {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
        
    }


    void Update() {
        if (Input.GetButtonDown("Jump")) _jumpPressed = true;

        aimForwardVector = localCameraHandler.transform.forward;

        //? move input local
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    public override void FixedUpdateNetwork() {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false) return;

        // ko chay doan duoi neu dang fall or respawn
        if(Object.HasStateAuthority) {
            if(isRespawnRequested) {
                Respawn();
                return;
            }
            // ko cap nhat vi tri movement khi player death
            //if(hPHandler.isDead) return; 
        }

        //xoay local player theo aimForwardVector -> dam bao localPlayer nhin thang se la huong aimForwardVector
        transform.forward = aimForwardVector;

        // khong cho xoay player len xuong quanh x
        Quaternion rotation = transform.rotation;
        rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, rotation.eulerAngles.z);
        transform.rotation = rotation;

        //move network
        Vector3 moveDir = transform.forward * movementInput.y + transform.right * movementInput.x;
        moveDir.Normalize();
        networkCharacterController.Move(moveDir);

        //jump network
        if(_jumpPressed) {
            networkCharacterController.Jump();
            _jumpPressed = !_jumpPressed;
        }

        // animator


        
        CheckFallToRespawn();
    }

    private void CheckFallToRespawn() {
        if(transform.position.y < fallHightToRespawn) {
            if(Object.HasStateAuthority) {
                Debug.Log($"{Time.time} respawn due to fall {transform.position}");

                //? thong bao khi fall out
                Debug.Log($"__________________co vao Fall off");
                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName_Network.ToString(), " -> fall off");
                Respawn();
            }
        }
    }

    void CharacterControllerEnable(bool isEnable) {
        networkCharacterController.enabled = isEnable;
    }

    private void Respawn() {
        CharacterControllerEnable(true);

        networkCharacterController.Teleport(Utils.GetRandomSpawnPoint());
        //hPHandler.OnRespawned_ResetHP(); // khoi tao lai gia tri HP isDeath - false
        isRespawnRequested = false;
    }
    
    public void RequestRespawn() {
        isRespawnRequested = true;
    }
}