using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] BulletHandler bulletVFXPF; // vien dan chua class BulletHandler (chi co chua hieu ung vxf tai noi raycast hit vao)
    [Header("Effects")]
    [SerializeField] ParticleSystem fireParticleSystemLocal;// hieu ung nong sung localCam thay | nen phai gan tag = ignoreLayerChange
    [SerializeField] ParticleSystem fireParticleSystemRemote; // hieu ung nong sung chi remotePlayerCam thay | unTag

    [Header("Aim")]
    [SerializeField] Transform aimPoint; // VI TRI LOCAL CAMERA 1st and 3rd
    [SerializeField] Transform aimPoint_grandeRocket; // VI TRI TREN NONG SUNG trong 1stPersonCam
    [SerializeField] Transform aimPoint_grandeRocket_3rd; // VI TRI TREN NONG SUNG trong 1stPersonCam

    [Header("Collisons")]
    [SerializeField] LayerMask collisionLayers;

    [Networked] // bien updated through the server on all the clients
    public bool isFiring{get; set;}
    ChangeDetector changeDetector;

    float lastTimeFired = 0f;

    //timing cho fire Grenade
    TickTimer grenadeFireDelay = TickTimer.None;
    TickTimer rocketFireDelay = TickTimer.None;
    TickTimer bulletFireDelay = TickTimer.None;

    //? network object nao tao ra tia raycast
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;
    //[SerializeField] HPHandler hPHandler;

    //! testing
    [SerializeField] LocalCameraHandler localCameraHandler;
    float aiFireRate = 2f;
    Vector3 spawnPointRaycastCam = Vector3.zero;

    [Networked]
    public int killCount{get; set;}

    // others 
    bool isMouse0Pressed = false;
    Spawner spawner;

    private void Awake() {
        //hPHandler = GetComponent<HPHandler>();
        networkPlayer = GetComponent<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();

        localCameraHandler = FindFirstObjectByType<LocalCameraHandler>();

        //weaponSwitcher = GetComponent<WeaponSwitcher>();
        spawner = FindObjectOfType<Spawner>();
    }

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    private void Update() {
        if(SceneManager.GetActiveScene().name == "Ready") return;
        if (HasStateAuthority == false) return;
        /* if(hPHandler.isDead) return; */

        // nhan mouse 0 fire bullet
        if(Input.GetKeyDown(KeyCode.Mouse0)) isMouse0Pressed = true;
    }

    public override void FixedUpdateNetwork()
    {
        if(isMouse0Pressed) {
            // chi tao ra hieu ung laser no o nong sung va bay toi muc tieu va cham
            localCameraHandler.RaycastHitPoint();

            var hitPointVector3 = localCameraHandler.hitPoint_Network;

            if(hitPointVector3 != Vector3.zero) FireBulletVFX(hitPointVector3);
            
            Fire(localCameraHandler.transform.forward, aimPoint);  // neu player thi aimpoint = vi tri 1st cam
            isMouse0Pressed = false;
        }
    }

    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer)) {
            switch (change) {
                case nameof(isFiring):
                var boolReader = GetPropertyReader<bool>(nameof(isFiring));
                var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                OnFireChanged(previousBool, currentBool);
                    break;
            }
        }
    }

    //? fire bullet laser VFX => chi tao ra virtual o nong sung + bullet trails + impact
    void FireBulletVFX(Vector3 hitPoint) {
        Vector3 dir = hitPoint - aimPoint_grandeRocket.position;
        if(bulletFireDelay.ExpiredOrNotRunning(Runner)) {
            Runner.Spawn(bulletVFXPF, aimPoint_grandeRocket.position, Quaternion.LookRotation(dir), Object.InputAuthority,
            (runner, spawnBullet) => {
                spawnBullet.GetComponent<BulletHandler>().FireBullet(Object.InputAuthority, networkObject, networkPlayer.nickName_Network.ToString());
            });
            bulletFireDelay = TickTimer.CreateFromSeconds(Runner, 0.15f); // sau 3 s se exp or notRunning
        }
    }

    //? FIRE raycast BULLET FROM CAMERA
    void Fire(Vector3 aimForwardVector, Transform aimPoint) {
        //? AI fire theo AI fireRate
        //if(networkPlayer.isBot && Time.time - lastTimeFired < aiFireRate) return;

        //? player fire rate theo lasTimeLimit
        if(Time.time - lastTimeFired < 0.15f) return;

        StartCoroutine(FireEffect());

        /* var spawnPointRaycastCam = localCameraHandler.raycastSpawnPointCam_Network; */

        //? neu la AI thi diem ban se la camera anrcho
        /* if(!networkPlayer.isBot) 
            spawnPointRaycastCam = localCameraHandler.raycastSpawnPointCam_Network;
        else spawnPointRaycastCam = aiCameraAnchor.position; */
        
        
        spawnPointRaycastCam = localCameraHandler.raycastSpawnPointCam_Network;

        /* if(Runner.GetPhysicsScene().Raycast(spawnPointRaycastCam, aimForwardVector, out var hitInfo, 100, collisionLayers, QueryTriggerInteraction.Collide)) {
            // neu hitInfo do this.gameObject ban ra thi return
            if(hitInfo.transform.GetComponent<WeaponHandler>() == this) return;

            float hitDis = 100f;
            bool isHitOtherRemotePlayers = false;

            if(hitInfo.distance > 0) hitDis = hitInfo.distance;

            if(hitInfo.transform.TryGetComponent<HPHandler>(out var health)) {
                Debug.Log($"{Time.time} {transform.name} hit HitBox {hitInfo.transform.root.name}");

                if(Object.HasStateAuthority) {
                    //tim xem networkObject nao da tao ra vien dan
                    hitInfo.collider.GetComponent<HPHandler>().OnTakeDamage(networkPlayer.nickName_Network.ToString(), 1, this);
                }

                isHitOtherRemotePlayers = true;
            }
            else if(hitInfo.collider != null){
                Debug.Log($"{Time.time} {transform.name} hit PhysiX Collier {hitInfo.transform.root.name}");
            }

            //? ve ra tia neu ban trung remotePlayers
            if(isHitOtherRemotePlayers)
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDis, Color.red, 1f); // aimForwardVector
            else 
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDis, Color.green, 1f); // aimForwardVector
        } */

        if(Physics.Raycast(spawnPointRaycastCam,aimForwardVector, out var hit, 100, collisionLayers)) {
            // neu hitInfo do this.gameObject ban ra thi return
            if(hit.transform.GetComponent<WeaponHandler>() == this) return;

            float hitDis = 100f;
            bool isHitOtherRemotePlayers = false;

            if(hit.distance > 0) hitDis = hit.distance;

            if(hit.transform.TryGetComponent<HPHandler>(out var health)) {
                Debug.Log($"{Time.time} {transform.name} hit HitBox {hit.transform.root.name}");
                
                // kiem tra co phai dong doi hay khong
                bool isEnemyCheck = hit.transform.GetComponent<NetworkPlayer>().isEnemy_Network;
                if(spawner.CustomLobbyName == "OurLobbyID_Team" && networkPlayer.isEnemy_Network == isEnemyCheck) return;
                // kiem tra co phai dong doi hay khong

                if(Object.HasStateAuthority) {
                    //tim xem networkObject nao da tao ra vien dan
                    /* hit.collider.GetComponent<HPHandler>().OnTakeDamage(networkPlayer.nickName_Network.ToString(), 1, this); */
                    hit.collider.GetComponent<HitboxRoot>().GetComponent<HPHandler>().
                                OnTakeDamage(networkPlayer.nickName_Network.ToString(), 1, this);
                }

                isHitOtherRemotePlayers = true;
            }
            else if(hit.collider != null){
                Debug.Log($"{Time.time} {transform.name} hit PhysiX Collier {hit.transform.root.name}");
            }

            //? ve ra tia neu ban trung remotePlayers
            if(isHitOtherRemotePlayers)
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDis, Color.red, 1f); // aimForwardVector
            else 
                Debug.DrawRay(aimPoint.position, aimForwardVector * hitDis, Color.green, 1f); // aimForwardVector

        }
        
        lastTimeFired = Time.time;

        // lam cho ai ban theo tan suat random khoang time
        aiFireRate = Random.Range(0.1f, 1.5f);
    }

    // fire particle on aimPoint
    IEnumerator FireEffect()    
    {
        isFiring = true;
        /* if(NetworkPlayer.Local.is3rdPersonCamera)
            fireParticleSystemRemote.Play();
        else 
            fireParticleSystemLocal.Play(); */ // show cho localPlayer thay hieu ung ban ra
        
        fireParticleSystemLocal.Play();
        yield return new WaitForSeconds(0.09f);
        isFiring = false;
    }

    void OnFireChanged(bool previous, bool current) {
        //? thong bao cho other clients khac biet this.localPlayer fire
        if(current && !previous) 
            OnFireRemote();
    }

    void OnFireRemote() {
        //? thong bao cho tat ca remotePlayer biet

        //(!Object.HasInputAuthority) => this.Object dang xuat hien o man hinh cua other clients
        // hien thi cho cac man hinh Clients noi this.Object nay dang xuat hien
        if(!Object.HasInputAuthority) fireParticleSystemRemote.Play();
    }
}