using Fusion;
using UnityEngine;

public class LocalCameraHandler : NetworkBehaviour
{
    public Camera localCamera;
    [SerializeField] Transform cameraAnchorPoint; // localCam se di theo camereAnchorPoint

    //Rotation
    float _cameraRotationX = 0f;
    float _cameraRotationY = 0f;
    Vector2 viewInput;
    NetworkCharacterController networkCharacterController;

    private void Awake() {
        localCamera = GetComponent<Camera>();
        networkCharacterController = GetComponentInParent<NetworkCharacterController>();
    }
    private void Update() {
        //? view input local
        viewInput.x = Input.GetAxis("Mouse X");
        viewInput.y = Input.GetAxis("Mouse Y") * -1f;
    }

    void LateUpdate()
    {
        //? xet cho local cam
        if(cameraAnchorPoint == null) return;
        if(!localCamera.enabled) return;
        
        //? Set Playerodel - LocalPlayerModel -> de 1stPersomCam render thay
        Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

        //?di chuyen localCam vao trong cameraAnchorPoint OK
        localCamera.transform.position = cameraAnchorPoint.position; // localCam di theo | ko phai nam ben trong

        //?tinh toan cameraRotationX Y
        _cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterController.viewRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);
        _cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

        //?xoay camera theo mouseX mouseY
        localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);

        

    }
}