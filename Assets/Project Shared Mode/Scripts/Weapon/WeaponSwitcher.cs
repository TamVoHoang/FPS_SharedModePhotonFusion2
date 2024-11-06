using System;
using System.Collections;
using Fusion;
using UnityEngine;

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
    [SerializeField] public Gun local_GunPF;
    [SerializeField] private Gun remote_GunPF;

    [SerializeField] private Transform local_GunHolder;
    [SerializeField] private Transform remote_GunHolder;

    [SerializeField] int indexLocalSlotActive;
    public int GetIndexLocalSlotActive { get { return indexLocalSlotActive;}}
    [SerializeField] Transform[] slots_LocalHolder;
    public Transform[] GetSlotsLocalHolder { get { return slots_LocalHolder;}}
    [SerializeField] Transform[] slots_RemoteHolder;

    [SerializeField] Transform playerModel;

    bool isTouchedWeaponPickup = false;
    bool isWeaponDroped = false;

    //
    [SerializeField] UIWeapon uIWeapon;
    public Action<int, int, bool> updateWeaponUI;

    bool isWeaponSwitched = false;

    public override void Spawned() {
        Debug.Log($"co override spawned");
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

        /* if(SceneManager.GetActiveScene().name == "Ready") {
            uIWeapon.gameObject.SetActive(false);
        } else uIWeapon.gameObject.SetActive(true); */
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
                case nameof(isGunChange):
                StartCoroutine(DelayChange());
                    break;

                case nameof(isGunSwitch):
                StartCoroutine(DelaySwitch());
                    break;

                case nameof(isGunDrop):
                StartCoroutine(DelayDrop());
                break;
            }
        }
    }

    IEnumerator DelayChange() {
        yield return new WaitForSeconds(0.09f);
        OnIsGunChange(local_GunPF, remote_GunPF);
        updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), IsGunInIndexSlotActive());
    }

    IEnumerator DelaySwitch() {
        yield return new WaitForSeconds(0.09f);
        OnIsGunsSwitch();
        updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), IsGunInIndexSlotActive());
    }
    IEnumerator DelayDrop() {
        yield return new WaitForSeconds(0.09f);
        OnIsGunDrop();
        updateWeaponUI?.Invoke(indexLocalSlotActive, GunsNumber(), false);

    }

    

    //? send to RPC then get value (to trigger changed value of variable to use Render method)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestWeaponChanged(NetworkBool isChanged ,RpcInfo info = default) {
        this.isGunChange = isChanged;
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

    void SpawnGunsGeneral(int index, Transform[] transforms, Gun gunPF, Transform slotsIndexTransform) {
        for (int i = 0; i < slots_LocalHolder.Length; i++) {
            if(i == index) {
                transforms[i].gameObject.SetActive(true);
                continue;
            }
            transforms[i].gameObject.SetActive(false);
        }
        Instantiate(gunPF, slotsIndexTransform.position, slotsIndexTransform.rotation, slotsIndexTransform);
    }

    //? detect change with render method
    void OnIsGunChange(Gun local_GunPF, Gun remote_GunPF) {
        //spawn gun_local
        var indexLocal = local_GunPF.SlotIndex;
        indexLocalSlotActive = indexLocal;
        var slotsIndexLocalTransform = slots_LocalHolder[indexLocal];

        /* for (int i = 0; i < 3; i++) {
            if(i == indexLocal) {
                slots_LocalHolder[i].gameObject.SetActive(true);
                continue;
            }
            slots_LocalHolder[i].gameObject.SetActive(false);
        }
        Instantiate(local_GunPF, slotsIndexLocalTransform.position, slotsIndexLocalTransform.rotation, slotsIndexLocalTransform); */
        
        SpawnGunsGeneral(indexLocal,slots_LocalHolder,local_GunPF, slotsIndexLocalTransform);
        
        //spawn gun_remote
        var indexRemote = remote_GunPF.SlotIndex;
        var slotIndexRemoteTransform = slots_RemoteHolder[indexRemote];

        /* for (int i = 0; i < 3; i++) {
            if(i == indexRemote) {
                slots_RemoteHolder[i].gameObject.SetActive(true);
                continue;
            } 
            slots_RemoteHolder[i].gameObject.SetActive(false);
        }
        Instantiate(remote_GunPF, slotIndexRemoteTransform.position, slotIndexRemoteTransform.rotation, slotIndexRemoteTransform); */

        SpawnGunsGeneral(indexRemote,slots_RemoteHolder,remote_GunPF, slotIndexRemoteTransform);

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

            Vector3 spawnPoint = playerPosition + playerForward * 1.2f;
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
    private void OnTriggerEnter(Collider other) {

        if(NetworkPlayer.Local.is3rdPersonCamera) return;   // neu la 3rd camaera thi ko change - dang tat local camera holder
        /* if(indexLocalSlotActive == weaponPickup.SlotIndex) return; */

        WeaponPickup weaponPickup = other.GetComponent<WeaponPickup>();
        if (weaponPickup != null && isTouchedWeaponPickup == false) {
            
            if(slots_LocalHolder[weaponPickup.SlotIndex].GetComponentInChildren<Gun>()) return;
            
            StartCoroutine(Delay());
            SetNew_GunPF(weaponPickup.local_GunPF, weaponPickup.remote_GunPF);

            var isChanged = isGunChange; //true

            if(Object.HasInputAuthority)
                RPC_RequestWeaponChanged(!isChanged);

        }
    }

    IEnumerator Delay() {
        isTouchedWeaponPickup = true;
        yield return new WaitForSeconds(0.2f);
        isTouchedWeaponPickup = false;
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
    public void SetNew_GunPF(Gun local_GunPF, Gun remote_GunPF) {
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


}