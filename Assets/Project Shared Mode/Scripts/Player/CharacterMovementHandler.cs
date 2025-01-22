using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
public class CharacterMovementHandler : NetworkBehaviour, IGameManager
{
    Vector3 aimForwardVector;
    Vector2 movementInput;

    [Header("Animation")]
    [SerializeField] Animator animator;  // nam trong doi tuong con cua Model transform
    [SerializeField] float walkSpeed = 0f;

    // request after falling
    [SerializeField] float fallHightToRespawn = -10f;
    [SerializeField] bool isRespawnRequested = false;

    [Networked]
    public bool isRespawnRequested_{get; set;} = false;

    //...
    NetworkCharacterController networkCharacterController;
    LocalCameraHandler localCameraHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    HPHandler hPHandler;
    bool isFinished = false;
    CharacterInputHandler characterInputHandler;

    private void Awake() {
        characterInputHandler = GetComponent<CharacterInputHandler>();
        networkCharacterController = GetComponent<NetworkCharacterController>();
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
        hPHandler = GetComponent<HPHandler>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start() {
        characterInputHandler.OnJump += () => networkCharacterController.Jump();;
    }

    private void OnDisable() {
        characterInputHandler.OnJump -= () => networkCharacterController.Jump();
    }

    void Update() {
        if(isFinished) return;
        //lock input to move and jump if Ready scene
        if(SceneManager.GetActiveScene().name == "Ready") return;

        //? move input local
        movementInput = characterInputHandler.Move;
        aimForwardVector = localCameraHandler.transform.forward;
    }
    
    public override void FixedUpdateNetwork() {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false) return;

        // ko chay doan duoi neu dang fall or respawn
        if(Object.HasStateAuthority) {
            if(isRespawnRequested_) {
                Respawn();
                return;
            }
            // ko cap nhat vi tri movement khi player death
            if(hPHandler.Networked_IsDead) return; 
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

        // do khi spawner.cs run OnpPlayerJoin() -> co set Charactercontroller.enable = false
        // ly do la de nhan vat co the roi xuong + co move den vi tri random position
        if(GetComponent<CharacterController>().enabled == false) return;
        networkCharacterController.Move(moveDir);

        //? animator
        Vector2 walkVector = new Vector2(networkCharacterController.Velocity.x,
                                        networkCharacterController.Velocity.z);
        walkVector.Normalize(); // ko cho lon hon 1

        walkSpeed = Mathf.Lerp(walkSpeed, Mathf.Clamp01(walkVector.magnitude), Runner.DeltaTime * 10f);
        animator.SetFloat("walkSpeed", walkSpeed);  // xet gia tri float "walkSpeed" trong animator
        
        CheckFallToRespawn();
    }

    private void CheckFallToRespawn() {
        if(transform.position.y < fallHightToRespawn) {
            if(Object.HasStateAuthority) {
                Debug.Log($"{Time.time} respawn due to fall {transform.position}");

                //? thong bao khi fall out
                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName_Network.ToString(), " -> fall off");
                Respawn();
            }
        }
    }

    public void CharacterControllerEnable(bool isEnable) {
        networkCharacterController.enabled = isEnable;
    }

    private void Respawn() {
        Debug.Log($"_____Starting Respawn");
        CharacterControllerEnable(true);

        networkCharacterController.Teleport(Utils.GetRandomSpawnPoint());
        
        hPHandler.OnRespawned_ResetHPIsDead(); // khoi tao lai gia tri HP isDeath - false
        /* isRespawnRequested = false; */
        RPC_SetNetworkedIsDead(false);
        Debug.Log($"_____Ending Respawn");
        
    }
    
    public void RequestRespawn() {
        Debug.Log($"_____Requested Respawn");
        /* isRespawnRequested = true; */
        RPC_SetNetworkedIsDead(true);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetNetworkedIsDead(bool isRespawnRequested) {
        this.isRespawnRequested_ = isRespawnRequested;
    }

    public void IsFinished(bool isFinished) {
        this.isFinished = isFinished;
    }
}