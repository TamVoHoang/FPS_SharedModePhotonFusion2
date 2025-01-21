using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponHandler : NetworkBehaviour, IGameManager
{
    [Header("Prefabs Grande Rocket")]
    [SerializeField] BulletHandler bulletVFXPF; // vien dan chua class BulletHandler (chi co chua hieu ung vxf tai noi raycast hit vao)
    [SerializeField] RocketHandler rocketPF; //? networkObject RocketHandler (co chua RocketHandler.cs)
    [SerializeField] GrandeHandler grenadePF; //? networkObject GrandeHandler (co chua GrandeHandler.cs)

    
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

    [SerializeField] float lastTimeFired = 0f;

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
    public int killCountCurr{get; set;}

    // others 
    Spawner spawner;

    [SerializeField] bool isFirePressed = false;
    bool isRocketPressed = false;
    bool isGrandePressed = false;
    WeaponSwitcher weaponSwitcher;
    bool isFinished = false;

    private void Awake() {
        
        networkPlayer = GetComponent<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();

        localCameraHandler = FindFirstObjectByType<LocalCameraHandler>();

        weaponSwitcher = GetComponent<WeaponSwitcher>();
        spawner = FindObjectOfType<Spawner>();
    }

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
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

    private void Update() {
        if(isFinished) return;
        if(SceneManager.GetActiveScene().name == "Ready") return;
        if (HasStateAuthority == false) return;
        if(GetComponent<HPHandler>().Networked_IsDead) return;

        // nhan mouse 0 fire bullet + co sung trong slot
        if(Input.GetKeyDown(KeyCode.Mouse0) && weaponSwitcher.IsGunInIndexSlotActive()) {
            isFirePressed = true;
        }
            
        // nhan R -> fire Rocket
        if(Input.GetKeyDown(KeyCode.R) && weaponSwitcher.IsGunInIndexSlotActive())
            isRocketPressed = true;

        if(Input.GetKeyDown(KeyCode.Mouse1) && weaponSwitcher.IsGunInIndexSlotActive())
            isGrandePressed = true;
    }

    public override void FixedUpdateNetwork()
    {   
        // co input + bulletFireDelay sau 0.2s => thuc hien row 109
        // dam bao chi send RPC every 0.2s | se ko goi lien tuc du FireButton lien tuc

        if(isFirePressed) {
            if(bulletFireDelay.ExpiredOrNotRunning(Runner)) {
                // chi tao ra hieu ung laser no o nong sung va bay toi muc tieu va cham
                localCameraHandler.RaycastHitPoint();
                var hitPointVector3 = localCameraHandler.hitPoint_Network;

                if(hitPointVector3 != Vector3.zero) FireBulletVFX(hitPointVector3);
                
                Fire(localCameraHandler.transform.forward, aimPoint);  // neu player thi aimpoint = vi tri 1st cam

                bulletFireDelay = TickTimer.CreateFromSeconds(Runner, 0.2f); // sau 0.15 s se exp or notRunning
            }
            isFirePressed = false;
        }

        if(isRocketPressed) {
            // do delay time chi co trong FireRocket_1 || FireRocket
            // nhung dong lenh khac se duoc thuc hien lient tuc trong do co send Rpc tu local camera

            localCameraHandler.RaycastHitPoint();
            var hitPointVector3 = localCameraHandler.hitPoint_Network;
            var spawnedPoint_OnCam = localCameraHandler.spawnedPointOnCam_Network;
            var spawnedPoint_OnHand = localCameraHandler.spawnedPointOnHand_Network;

            if(hitPointVector3 != Vector3.zero) {
                if(NetworkPlayer.Local.is3rdPersonCamera) FireRocket_1(hitPointVector3, spawnedPoint_OnHand);
                else FireRocket_1(hitPointVector3, spawnedPoint_OnCam);
            } 
            else {
                FireRocket(localCameraHandler.transform.forward, aimPoint);    // aimpoint tren local cam
            }
            isRocketPressed = false;
        }

        if(isGrandePressed) {
            localCameraHandler.RaycastHitPoint();
            var hitPointVector3 = localCameraHandler.hitPoint_Network;
            var spawnedPoint_OnCam = localCameraHandler.spawnedPointOnCam_Network;
            var spawnedPoint_OnHand = localCameraHandler.spawnedPointOnHand_Network;

            if(hitPointVector3 != Vector3.zero)
                if(NetworkPlayer.Local.is3rdPersonCamera) FireGrenade_1(hitPointVector3, spawnedPoint_OnHand);
                else FireGrenade_1(hitPointVector3, spawnedPoint_OnCam);

            else
                FireGrenade(localCameraHandler.transform.forward, aimPoint); // aimpoint tren local cam // khi raycast local cam ko tim thay muc tieu

            isGrandePressed = false;
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
            bulletFireDelay = TickTimer.CreateFromSeconds(Runner, 0.2f); // sau 3 s se exp or notRunning
        }
    }

    //? FIRE raycast BULLET FROM CAMERA
    void Fire(Vector3 aimForwardVector, Transform aimPoint) {
        StartCoroutine(FireEffect());

        spawnPointRaycastCam = localCameraHandler.raycastSpawnPointCam_Network;

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

        //? player fire rate theo lasTimeLimit - neu luon chay lien tuc trong update thi oK
        /* if(Time.time - lastTimeFired < 0.15f) return;
        lastTimeFired = Time.time; */
    }

    #region ROCKET
    //? SPAWN ROCKET FROM CAMERA
    void FireRocket(Vector3 aimForwardVector, Transform aimPoint) {
        if(rocketFireDelay.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority) {
            //? runner spawn ra 1 
            Runner.Spawn(rocketPF, aimPoint.position + aimForwardVector * 1f, Quaternion.LookRotation(aimForwardVector), Object.InputAuthority,
                (runner, spawnRocket) => {
                    spawnRocket.GetComponent<RocketHandler>().Fire(Object.InputAuthority, networkObject, networkPlayer.nickName_Network.ToString(), this);
                });

            //? bat dau dem tickTimer cho lan ban ke tiep
            rocketFireDelay = TickTimer.CreateFromSeconds(Runner, 3.0f); // sau 3 s se exp or notRunning
        }
    }

    //! SPAWN ROCKET FROM NONG SUNG
    void FireRocket_1(Vector3 hitPoint, Vector3 spawnedPoint) {
        
        /* Vector3 dir = Vector3.zero;
        if(Object.HasInputAuthority) dir = hitPoint - localCameraHandler.spawnedPointOnCam_Network;
        else if(!Object.HasInputAuthority) dir = hitPoint - localCameraHandler.spawnedPointOnHand_Network; */
        
        Vector3 dir = hitPoint - spawnedPoint;
        //Vector3 dir = hitPoint - aimPoint_grandeRocket.position; //! OK original
        if(rocketFireDelay.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority) {
            //? runner spawn ra 1 | aimPoint_grandeRocket.position 
            Runner.Spawn(rocketPF, spawnedPoint, Quaternion.LookRotation(dir), Object.InputAuthority,
                (runner, spawnRocket) => {
                    spawnRocket.GetComponent<RocketHandler>().Fire(Object.InputAuthority, networkObject, networkPlayer.nickName_Network.ToString(), this);
                });

            //? bat dau dem tickTimer cho lan ban ke tiep
            rocketFireDelay = TickTimer.CreateFromSeconds(Runner, 3.0f); // sau 3 s se exp or notRunning
        }
    }
#endregion ROCKET

#region GRANDE
    //? SPAWN FIRE GRANDE FORM CAMERA
    void FireGrenade(Vector3 aimForwardVector, Transform aimPoint) {
        // kiem tra dang thuc su ko co ban grenade
        if(grenadeFireDelay.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority) {
            //? runner spawn ra 1 
            Runner.Spawn(grenadePF, aimPoint.position + aimForwardVector * 1f, Quaternion.LookRotation(aimForwardVector), Object.InputAuthority,
                (runner, spawnGrenade) => {
                    spawnGrenade.GetComponent<GrandeHandler>().Throw(aimForwardVector * 15f, Object.InputAuthority, networkPlayer.nickName_Network.ToString(), this);
                });

            //? bat dau dem tickTimer cho lan ban ke tiep
            grenadeFireDelay = TickTimer.CreateFromSeconds(Runner, 1.0f); // sau 1 s se exp or notRunning
        }
    }
    //! SPAWN GRANDE FROM FIRE POINT ON GUN
    void FireGrenade_1(Vector3 hitPoint, Vector3 spawnedPoint) {
        Vector3 dir = hitPoint - spawnedPoint;
        // kiem tra dang thuc su ko co ban grenade
        if(grenadeFireDelay.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority) {
            //? runner spawn ra 1 
            Runner.Spawn(grenadePF, spawnedPoint, Quaternion.LookRotation(dir), Object.InputAuthority,
                (runner, spawnGrenade) => {
                    spawnGrenade.GetComponent<GrandeHandler>().Throw(dir * 1.2f, Object.InputAuthority, networkPlayer.nickName_Network.ToString(), this);
                });

            //? bat dau dem tickTimer cho lan ban ke tiep
            grenadeFireDelay = TickTimer.CreateFromSeconds(Runner, 1.0f); // sau 1 s se exp or notRunning
        }
    }
#endregion GRANDE 

    // fire particle on aimPoint
    IEnumerator FireEffect()    
    {
        isFiring = true;
        if(NetworkPlayer.Local.is3rdPersonCamera)
            fireParticleSystemRemote.Play();
        else 
            fireParticleSystemLocal.Play(); // show cho localPlayer thay hieu ung ban ra
        
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

    // save killedCount to firestore
    public void SaveKilledCount() {
        if(!DataSaveLoadHander.Instance) return;
        DataSaveLoadHander.Instance.playerDataToFireStore.KilledCount += 1;
        DataSaveLoadHander.Instance.SavePlayerDataFireStore();
    }

    public void SendKillCountCurrToTeamResult() {
        if(Object.HasStateAuthority) {
            bool isEnemy = NetworkPlayer.Local.isEnemy_Network;

            GameManagerUIHandler gameManagerUIHandler = FindObjectOfType<GameManagerUIHandler>();
            gameManagerUIHandler.RPC_SetKillCount(isEnemy, 1);

            /* int killCountNetwork = gameManagerUIHandler.GetKillCountTeam(isEnemy);
            GetComponent<NetworkInGameTeamResult>().SendInGameResultTeamRPC(isEnemy, killCountNetwork); */

            StartCoroutine(Delay(0.5f, isEnemy));
        }
    }

    IEnumerator Delay(float time, bool isEnemy) {
        yield return new WaitForSeconds(time);
        int killCountNetwork = GameManagerUIHandler.action_(isEnemy);
        GetComponent<NetworkInGameTeamResult>().SendInGameResultTeamRPC(isEnemy, killCountNetwork);
    }

    public void IsFinished(bool isFinished)
    {
        this.isFinished = isFinished;
    }
}