using Fusion;
using UnityEngine;
using Cinemachine;

public class LocalCameraHandler : NetworkBehaviour, IGameManager
{
    public Camera localCamera;
    [SerializeField] Transform cameraAnchorPoint; // localCam se di theo camereAnchorPoint
    [SerializeField] GameObject localGun_onCam;   // GUN local camera se bi tat khi dung 3rdCam
    [SerializeField] Transform spawnedPointGun_OnCam; // nha dan cua sung trong cam
    [SerializeField] Transform spawnedPointGun_OnHand; // nah dan cua sung trong tay player

    //Rotation
    float _cameraRotationX = 0f;
    float _cameraRotationY = 0f;
    Vector2 viewInput;
    Vector2 aimDir;
    NetworkCharacterController networkCharacterController;

    //Raycast from local camera
    public Vector3 spawnedPointOnCam_Network{get; set;} = Vector3.zero;
    public Vector3 spawnedPointOnHand_Network{get; set;} = Vector3.zero;
    
    public Vector3 hitPoint_Network {get; set;} = Vector3.zero;
    public Vector3 raycastSpawnPointCam_Network {get; set;} = Vector3.zero;
    Ray ray;
    RaycastHit hitInfo;

    [Header("Collisons")]
    [SerializeField] LayerMask collisionLayers;

    [SerializeField] InGameMessagesUIHandler inGameMessagesUIHandler;
    public InGameMessagesUIHandler InGameMessagesUIHandler{get {return inGameMessagesUIHandler;}}

    //others
    CinemachineVirtualCamera cinemachineVirtualCamera;
    WeaponSwitcher weaponSwitcher;

    bool isFinished = false;
    CharacterInputHandler characterInputHandler;

    #region Recoil
    public Vector3 currentRotation;
    public Vector3 targetRotation;

    [Header("Recoil")]
    [SerializeField] float recoilX = -2;
    [SerializeField] float recoilY = 2;
    [SerializeField] float recoilZ = 0.4f;
    [SerializeField] float snappiness = 6;
    [SerializeField] float returnSpeed = 2;
    #endregion Recoil

    private void Awake() {
        localCamera = GetComponent<Camera>();
        characterInputHandler = GetComponentInParent<CharacterInputHandler>();
        networkCharacterController = GetComponentInParent<NetworkCharacterController>();
        inGameMessagesUIHandler = GetComponentInChildren<InGameMessagesUIHandler>();

        weaponSwitcher = GetComponentInParent<WeaponSwitcher>();
    }

    private void Update() {
        if(characterInputHandler.IsRealtimeResultPanel) return;
        if(characterInputHandler.IsExitPanel) return;

        if(isFinished) return;

        //? view input local
        aimDir = characterInputHandler.AimDir;

        viewInput.x = aimDir.x;
        viewInput.y = aimDir.y * -1f;
        RecoilUpdate();
    }

    void LateUpdate()
    {
        if(characterInputHandler.IsRealtimeResultPanel) return;
        if(characterInputHandler.IsExitPanel) return;

        //? xet cho local cam
        if(cameraAnchorPoint == null) return;
        if(!localCamera.enabled) return;
        
        //? Tim cinemahcine if ko thay - cinemachine 3rd cam ben ngoai Player
        if(cinemachineVirtualCamera == null) {
            cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }
        else {
            // neu co 3rd VA co su dung 3rd (is3rdPersonCamera = true in NetworkPlayer.cs -> is set in NetWorkInputData.cs (press C))
            if(NetworkPlayer.Local.is3rdPersonCamera) {
                if(!cinemachineVirtualCamera.enabled) {
                    cinemachineVirtualCamera.Follow = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.LookAt = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.enabled = true;

                    // set playersModel.transform - chuyen sang default Layer - de 3rdPersonCam render thay
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("Default"));

                    // Disable local gun Holders | 3rd cam is on
                    /* localGun_onCam.SetActive(false); */
                    
                    //? tim slot transform nao dang active
                    if(weaponSwitcher.GetGunNumber() > 0) {
                        var slotIndexLocalTransform = weaponSwitcher.GetLocalSlotTransformActive();
                        slotIndexLocalTransform.gameObject.SetActive(false);
                        Debug.Log($"co OFF local gun holder transform trong localCam.cs");
                    }
                }
                // dung lai tai day ko chay cho phan ben duoi do dang su dung 3rd person Cam
                cinemachineVirtualCamera.transform.position = cameraAnchorPoint.position; // localCam di theo | ko phai nam ben trong
                _cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterController.viewRotationSpeed;
                _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);
                _cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

                cinemachineVirtualCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);
                return;
            }
            else {
                if(cinemachineVirtualCamera.enabled) {
                    cinemachineVirtualCamera.enabled = false;

                    //? Set Playerodel - LocalPlayerModel -> de 1stPersomCam render thay
                    Utils.SetRenderLayerInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                    //Enable local guns Holder | 1sr cam is on
                    /* localGun_onCam.SetActive(true); */

                    if(weaponSwitcher.GetGunNumber() > 0) {
                        var slotIndexLocalTransform = weaponSwitcher.GetLocalSlotTransformActive();
                        slotIndexLocalTransform.gameObject.SetActive(true);
                    }
                }
            }
        }

        //?di chuyen localCam vao trong cameraAnchorPoint OK
        localCamera.transform.position = cameraAnchorPoint.position; // localCam di theo | ko phai nam ben trong

        //?tinh toan cameraRotationX Y
        _cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterController.viewRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);
        _cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

        //?xoay camera theo mouseX mouseY
        localCamera.transform.rotation = Quaternion.Euler(new Vector3(_cameraRotationX, _cameraRotationY, 0) + currentRotation);
    }

    //? Ban tia ray chi diem muc tieu. tim ra diem ban trung. luu len network
    public void RaycastHitPoint() {
        if(this.Object.HasStateAuthority) {
            ray.origin = this.transform.position;
            ray.direction = this.transform.forward;
            Physics.Raycast(ray, out hitInfo, 100, collisionLayers);
            RPC_SetHitPointRaycast(hitInfo.point, this.transform.position);
            RPC_SetBulletPoint(spawnedPointGun_OnCam.transform.position, spawnedPointGun_OnHand.transform.position);
        }
    }

    // local camera ban ra tia raycast - trung vao hitPoint - gui len rpc vector3 hitpoint - su dung
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetHitPointRaycast(Vector3 hitPointVector, Vector3 raycastSpawnedPoint, RpcInfo info = default) {
        Debug.Log($"[RPC] Set hitPointVector {hitPointVector} for localPlayer");
        this.hitPoint_Network = hitPointVector;
        this.raycastSpawnPointCam_Network = raycastSpawnedPoint;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetBulletPoint(Vector3 spawnedPointVector, Vector3 spawnedPointVector_,RpcInfo info = default) {
        Debug.Log($"[RPC] Set hitPointVector {spawnedPointVector} for localPlayer");
        this.spawnedPointOnCam_Network = spawnedPointVector;
        this.spawnedPointOnHand_Network = spawnedPointVector_;
    }

    public void IsFinished(bool isFinished) {
        this.isFinished = isFinished;
    }

    void RecoilUpdate() {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
    }
}