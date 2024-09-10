using UnityEngine;
using Fusion;
public class CharacterMovementHandler : NetworkBehaviour
{
    private bool _jumpPressed;

    //
    NetworkCharacterController networkCharacterController;
    LocalCameraHandler localCameraHandler;
    Vector3 aimForwardVector = Vector3.zero;
    Vector2 movementInput = Vector2.zero;

    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump")) _jumpPressed = true;

        aimForwardVector = localCameraHandler.transform.forward;
        //? move input local
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    public override void FixedUpdateNetwork()
    {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false) return;


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
            _jumpPressed = false;
        }
    }
}