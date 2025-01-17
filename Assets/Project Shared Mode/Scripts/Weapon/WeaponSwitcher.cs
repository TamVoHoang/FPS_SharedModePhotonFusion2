using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

//todo gameobject = networkPlayerPF
//todo chuyen doi gun

public class WeaponSwitcher : NetworkBehaviour
{
    [Networked]
    public NetworkBool isGunChange { get; set; }
    [Networked]
    public NetworkBool isGunSwitch { get; set; }
    [Networked]
    public NetworkBool isGunDrop { get; set; }

    [Networked]
    public Vector3 dropPositon { get; set; }

    [Networked]
    public Quaternion dropRotation { get; set; }

    ChangeDetector changeDetector;
    [SerializeField] Animator animator;
    [SerializeField] private Gun local_GunPF;
    [SerializeField] private Gun remote_GunPF;

    [SerializeField] private Transform local_GunHolder;
    [SerializeField] private Transform remote_GunHolder;

    [SerializeField] int indexLocalSlotActive = 0;
    [SerializeField] Transform[] slots_LocalHolder;

    [SerializeField] Transform[] slots_RemoteHolder;


    [SerializeField] Transform playerModel;

    bool isTouchedWeaponPickup = false;
    [Networked] public NetworkBool isTouched_Network {get; set;} = false;
    [Networked] public NetworkBool isHasGunInInventory_Network {get; set;} = false;

    bool isWeaponDroped = false;

    //
    [SerializeField] UIWeapon uIWeapon;
    public Action<int, int, bool> updateWeaponUI;

    bool isWeaponSwitched = false;

    [Networked] public NetworkObject CurrentObjectTouched_Network {get; set;}
    [Networked] public NetworkObject CurrentPlayerTouched_Network {get; set;}

    
    public override void Spawned() {
        Debug.Log($"co override spawned weapon switcher.cs");
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        
    }


    private void Awake() {
        slots_LocalHolder = new Transform[local_GunHolder.GetComponent<Transform>().childCount -1];
        slots_RemoteHolder = new Transform[remote_GunHolder.GetComponent<Transform>().childCount -1];

        
    }

    private void Start() {
        isTouchedWeaponPickup = false;

        for (int i = 0; i < local_GunHolder.GetComponent<Transform>().childCount -1; i++) {
            slots_LocalHolder[i] = local_GunHolder.GetChild(i+1).transform;
        }

        for (int i = 0; i < remote_GunHolder.GetComponent<Transform>().childCount -1; i++) {
            slots_RemoteHolder[i] = remote_GunHolder.GetChild(i+1).transform;
        }
        
        if(Object.HasInputAuthority) {
            uIWeapon.Set(this);
        }

    }

    IEnumerator DelayCo(float time) {
        yield return new WaitForSeconds(time);
        if(SceneManager.GetActiveScene().name == "Ready") {
            uIWeapon.gameObject.SetActive(false);
        } else uIWeapon.gameObject.SetActive(true);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Q)) {
            isWeaponSwitched = true;
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            isWeaponDroped = true;
        }

        
    }

    public override void FixedUpdateNetwork()
    {
        if(isWeaponSwitched) {
            isWeaponSwitched = false;
            OnTriggerSwitch();
        }

        if(isWeaponDroped) {
            isWeaponDroped = false;
            OnWeaponDropPressed();
        }
    }

    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(isGunSwitch):
                StartCoroutine(DelaySwitch());
                    break;

                case nameof(isGunDrop):
                StartCoroutine(DelayDrop());
                break;
            }
        }
    }

    IEnumerator DelaySwitch() {
        yield return new WaitForSeconds(0.09f);
        OnIsGunsSwitch();
        updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), IsGunInIndexSlotActive());
    }

    IEnumerator DelayDrop() {
        yield return new WaitForSeconds(0.2f);
        OnIsGunDrop();
        updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), false);

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestWeaponSwitch(NetworkBool isSwitched ,RpcInfo info = default) {
        this.isGunSwitch = isSwitched;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestWeaponDrop(NetworkBool isDroped ,RpcInfo info = default) {
        this.isGunDrop = isDroped;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestDropPoint(Vector3 dropPoint, Quaternion dropRotation, RpcInfo info = default) {
        this.dropPositon = dropPoint;
        this.dropRotation = dropRotation;
    }

    void SpawnGunsGeneral(int index, Transform[] transforms, Gun gunPF, bool isLocal) {
        var pos = isLocal? slots_LocalHolder[index].position : slots_RemoteHolder[index].position;
        NetworkObject newGun = Runner.Spawn(gunPF.gameObject);
        if(Object.HasStateAuthority) {
            RPC_RequestParent(newGun, index, isLocal);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestParent(NetworkObject newGun, int index, bool isLocal) {
        indexLocalSlotActive = index;

        Transform[] transforms = isLocal ? slots_LocalHolder : slots_RemoteHolder;
        for (int i = 0; i < slots_LocalHolder.Length; i++) {
            if(i == indexLocalSlotActive) {
                transforms[i].gameObject.SetActive(true);
                continue;
            }
            transforms[i].gameObject.SetActive(false);
        }

        newGun.transform.SetParent(isLocal? slots_LocalHolder[index] : slots_RemoteHolder[index], false);
        newGun.GetComponent<NetworkTransform>().Teleport(isLocal? slots_LocalHolder[index].position : slots_RemoteHolder[index].position);
    }


    //? detect change with render method
    void OnIsGunChange(Gun local_GunPF, Gun remote_GunPF) {
        //spawn gun_local
        var indexLocal = local_GunPF.SlotIndex;
        indexLocalSlotActive = indexLocal;

        SpawnGunsGeneral(indexLocal, slots_LocalHolder, local_GunPF, true);

        //spawn gun_remote
        var indexRemote = remote_GunPF.SlotIndex;

        SpawnGunsGeneral(indexRemote, slots_RemoteHolder, remote_GunPF, false);

        if(!Object.HasInputAuthority) {
            Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer("Default"));
        }
        else {
            Utils.SetRenderLayerInChildren(playerModel.transform, LayerMask.NameToLayer("LocalPlayerModel"));
        }
    }

    //? switching gun with Tab button
    void OnIsGunsSwitch() {
        if(GunsNumber() == 1) {
            if(slots_LocalHolder[indexLocalSlotActive].GetComponentInChildren<Gun>()) return;
            foreach (Transform item in slots_LocalHolder) {
                var gun = item.GetComponentInChildren<Gun>();
                if(gun) {
                    indexLocalSlotActive = gun.SlotIndex;
                }
            }
        }
        else if(GunsNumber() > 1) {
            
            Debug.Log($"co vao switch gun Q");
            slots_LocalHolder[indexLocalSlotActive].gameObject.SetActive(false);
            slots_RemoteHolder[indexLocalSlotActive].gameObject.SetActive(false);

            //? so sanh indexLocalSlotActive and gunsNumber
            var indexSlotActive = indexLocalSlotActive;
            if(indexSlotActive == 0) {
                do
                {
                    indexSlotActive ++;
                    Debug.Log($"co vao khi slot active == 0" + indexLocalSlotActive);
                } while (!slots_LocalHolder[indexSlotActive].GetComponentInChildren<Gun>() && indexSlotActive < 3);
            }   
            else if(indexSlotActive == slots_LocalHolder.Length - 1) {
                indexSlotActive = 0;
                while (!slots_LocalHolder[indexSlotActive].GetComponentInChildren<Gun>())
                {
                    indexSlotActive ++;
                }
            }
            else {
                indexSlotActive ++;
                while (!slots_LocalHolder[indexSlotActive].GetComponentInChildren<Gun>())
                {
                    // neu indexSlotActive == max (den slot cuoi cung cua mang) => lui ve slot dau tien
                    if(indexSlotActive == slots_LocalHolder.Length - 1) indexSlotActive = 0;
                    else indexSlotActive ++;
                }
            }
            indexLocalSlotActive = indexSlotActive;
        }

        slots_LocalHolder[indexLocalSlotActive].gameObject.SetActive(true);
        if(NetworkPlayer.Local.is3rdPersonCamera) {
            slots_LocalHolder[indexLocalSlotActive].gameObject.SetActive(false);
        }

        slots_RemoteHolder[indexLocalSlotActive].gameObject.SetActive(true);

        var gunLocal = slots_LocalHolder[indexLocalSlotActive].GetComponentInChildren<Gun>();
        var gunRemote = slots_RemoteHolder[indexLocalSlotActive].GetComponentInChildren<Gun>();

        SetNew_GunPF(gunLocal, gunRemote);
    }

    // xoa sung tren tay dang cam => spawn ra sung pickup
    void OnIsGunDrop() {
        var indexSlotActive = indexLocalSlotActive;
        // spawn pickup
        var pickupDrop = slots_LocalHolder[indexSlotActive].GetComponentInChildren<Gun>().weaponPickup.gameObject;

        var gunActiveLocalIndex = slots_LocalHolder[indexSlotActive].GetComponentInChildren<Gun>().gameObject;
        Destroy(gunActiveLocalIndex);
        /* slots_LocalHolder[indexSlotActive].gameObject.SetActive(false); */

        var gunActiveRemoteIndex = slots_RemoteHolder[indexSlotActive].GetComponentInChildren<Gun>().gameObject;
        Destroy(gunActiveRemoteIndex);
        /* slots_RemoteHolder[indexLocalSlotActive].gameObject.SetActive(false); */
        
        // directon player forwad
        if(Object.HasInputAuthority) {
            Vector3 playerPosition = this.transform.position + new Vector3(0, 1f, 0);
            Vector3 playerForward = this.transform.forward;

            Vector3 spawnPoint = playerPosition + playerForward * 2f;
            Quaternion spawnRotation = Quaternion.Euler(0, 90, 0);

            RPC_RequestDropPoint(spawnPoint, spawnRotation);
        }

        if(Object.HasStateAuthority) {
            Runner.Spawn(pickupDrop, this.dropPositon, this.dropRotation, null,
                (runner, spawnCurrentPickupWeapon) => {
                    
                }
            );
        }
        SetNew_GunPF(null, null);

        indexLocalSlotActive = indexSlotActive;
    }

    //? pickup weapon add to holder local and remote
    private void OnTriggerStay(Collider other) {
        if(NetworkPlayer.Local.is3rdPersonCamera) return;   // neu la 3rd camaera thi ko change - dang tat local camera holder
        
        var weaponPickup = other.GetComponent<WeaponPickup>();
        if (weaponPickup != null && isTouchedWeaponPickup == false) {
            
            if(slots_LocalHolder[weaponPickup.SlotIndex].GetComponentInChildren<Gun>()) {
                /* if(Object.HasStateAuthority) RPC_RequestIsHasGunInInventory(true); */
                return;
            } else {
                /* if(Object.HasStateAuthority) RPC_RequestIsHasGunInInventory(false); */
            }

            StartCoroutine(PickupObjectCO(0.5f)); //! 0.5f

            NetworkObject newNetworkOb = weaponPickup.GetComponent<NetworkObject>();
            if(Object.HasStateAuthority) RPC_RequestNetworkObjectTouched(newNetworkOb);
            SetNew_GunPF(weaponPickup.local_GunPF, weaponPickup.remote_GunPF);

            if(Object.HasStateAuthority) {
                OnIsGunChange(weaponPickup.local_GunPF, weaponPickup.remote_GunPF);
                updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), IsGunInIndexSlotActive());
            }
        }
    }

    IEnumerator PickupObjectCO(float time) {
        isTouchedWeaponPickup = true;
        /* if(Object.HasStateAuthority) RPC_RequestIsTouchedPickupWeapon(true); */
        yield return new WaitForSeconds(time);
        isTouchedWeaponPickup = false;
        /* if(Object.HasStateAuthority) RPC_RequestIsTouchedPickupWeapon(false); */

    }

    void OnTriggerSwitch() {
        var isSwitch = isGunSwitch; //true
        Debug.Log($"checking Switch = {isSwitch}");

        if(Object.HasInputAuthority) {
            /* if(GunsNumber() <= 1) return; */
            RPC_RequestWeaponSwitch(!isSwitch);
        }
    }

    void OnWeaponDropPressed() {
        if(!slots_LocalHolder[indexLocalSlotActive].GetComponentInChildren<Gun>()) return;

        var isDrop = isGunDrop;
        if(Object.HasInputAuthority) {
            RPC_RequestWeaponDrop(!isDrop);
        }
    }

    //? set current local and remote weapon after pickup or switching
    void SetNew_GunPF(Gun local_GunPF, Gun remote_GunPF) {
        this.local_GunPF = local_GunPF;
        this.remote_GunPF = remote_GunPF;

        // stop holding gun
        if(!local_GunPF && !remote_GunPF)
            animator.SetBool("isEquiped", false);
        else
            animator.SetBool("isEquiped", true);

    }

    public Transform GetLocalSlotTransformActive() {
        return slots_LocalHolder[indexLocalSlotActive];
    }

    int GunsNumber() {
        var gunsNum = 0;
        foreach (Transform item in slots_LocalHolder) {
            if(item.GetComponentInChildren<Gun>() != null)
                gunsNum ++;
        }
        return gunsNum;
    }

    public int GetGunNumber() => GunsNumber();
    public bool IsGunInIndexSlotActive() {
        //return slots_LocalHolder[indexLocalSlotActive].GetComponentInChildren<Gun>();

        if(slots_LocalHolder[indexLocalSlotActive].GetComponentInChildren<Gun>()) return true;
        else return false;
    }

    public void CheckHolster() {
        if(!local_GunPF && !remote_GunPF)
            animator.SetBool("isEquiped", false);
        else
            animator.SetBool("isEquiped", true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestIsTouchedPickupWeapon(NetworkBool networkBool) {
        this.isTouched_Network = networkBool;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestIsHasGunInInventory(NetworkBool networkBool) {
        this.isHasGunInInventory_Network = networkBool;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestNetworkObjectTouched(NetworkObject currentNetworkObjectPickup) {
        this.CurrentObjectTouched_Network = currentNetworkObjectPickup;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestNetworkPlayerTouched(NetworkObject currentNetworkPlayerTouch) {
        this.CurrentPlayerTouched_Network = currentNetworkPlayerTouch;
    }

}