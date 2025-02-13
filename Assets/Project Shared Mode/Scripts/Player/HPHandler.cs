using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class HPHandler : NetworkBehaviour
{
    //public byte local_HP;

    [Networked]
    public byte Networked_HP { get; set; } = 5;
    
    [Networked]
    public bool Networked_IsDead {get; set;} = false;
    [Networked]
    public NetworkString<_16> Networked_Killer { get; set; }

    [Networked]
    public int deadCountCurr {get; set;}

    bool isInitialized = false;
    const byte startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    // change material when player hit damage (doi mau SkinnedMeshRenderer)
    List<FlashMeshRender> flashMeshRenders = new List<FlashMeshRender>();

    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject localGun;
    [SerializeField] GameObject deathParticlePf;
    HitboxRoot hitboxRoot;  // ko hit remote player 2 lan
    CharacterMovementHandler characterMovementHandler;

    //? thong bao khi bi killed
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    public bool isSkipSettingStartValues = false; // ko cho chay lai ham start() khi thay doi host migration
    ChangeDetector changeDetector;  //duoc foi khi spawned => col 187

    bool isPublicDeathMessageSent = false;
    string killerName;

    public Action<byte, byte> UpdateSliderHealth;

    private void Awake() {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponent<HitboxRoot>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();

    }
    void Start() {
        if(!isSkipSettingStartValues) {
            /* local_HP = startingHP;
            deadCount = 0; */
        }
        UpdateSliderHealth?.Invoke(startingHP, startingHP);
        ResetMeshRenders();

        isInitialized = true;
    }

    //? ham duoc goi khi Object was spawned
    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(Networked_HP):
                var byteReader = GetPropertyReader<byte>(nameof(Networked_HP));
                var (previousByte, currentByte) = byteReader.Read(previousBuffer, currentBuffer);
                OnHPChanged(previousByte, currentByte);
                    break;
                
                case nameof(Networked_IsDead):
                var boolReader = GetPropertyReader<bool>(nameof(Networked_IsDead));
                var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                OnStateChanged(previousBool, currentBool);
                    break;
            }
        }
    }

    public override void FixedUpdateNetwork() {
        CheckPlayerDeath(Networked_HP);
    }

    //? server call | coll 55 WeaponHandler.cs | khi hitInfo.HitBox tren player
    public void OnTakeDamage(string damageCausedByPlayerNickName, byte damageAmount, WeaponHandler weaponHandler) {
        if(Networked_IsDead) return;

        //gioi han gia tri damageAmount
        if(damageAmount > Networked_HP) damageAmount = Networked_HP;

        Networked_HP -= damageAmount;

        killerName = damageCausedByPlayerNickName;
        RPC_SetNetworkedHP(Networked_HP, damageCausedByPlayerNickName);

        Debug.Log($"{Time.time} {transform.name} took damage {Networked_HP} left");

        if(Networked_HP <= 0) {
            Debug.Log($"{Time.time} {transform.name} is dead by {damageCausedByPlayerNickName}");
            /* RPC_SetNetworkedKiller(damageCausedByPlayerNickName); */ // can use
            isPublicDeathMessageSent = false;
            StartCoroutine(ServerRespawnCountine());
            /* RPC_SetNetworkedIsDead(true); */ // can use

            /* deadCount ++; */ //! KO THE XET O DAY, VI BIEN NETWORK KO XET LOCAL TAI DAY
            
            if(weaponHandler != GetComponent<WeaponHandler>()) {
                if(networkPlayer.IsSoloMode()) {
                    weaponHandler.killCountCurr += 1; // cong diem cho killer
                    //? SAVE to PlayerDataToFirestore -> save to weaponHander.UserID | save cho nguoi ban
                    weaponHandler.SaveKilledCount();
                }
                else {
                    bool isWeaponKillerTeam = weaponHandler.GetComponent<NetworkPlayer>().isEnemy_Network;
                    if(networkPlayer.isEnemy_Network == isWeaponKillerTeam) return;
                    //? killer gui va cong don cho team
                    StartCoroutine(SetKillCountFOrTeam(0.1f, weaponHandler));
                    weaponHandler.killCountCurr += 1; // cong diem cho killer
                    //? SAVE to PlayerDataToFirestore -> save to weaponHander.UserID | save cho nguoi ban
                    weaponHandler.SaveKilledCount();
                }
            }

            //? SAVE deathCount cho this.UserId | seve cho nguoi bi ban
            SaveDeathCount();
        }
    }

    IEnumerator SetKillCountFOrTeam(float time, WeaponHandler weaponHandler) {
        yield return new WaitForSeconds(time);
        weaponHandler.SendKillCountCurrToTeamResult();  // tang 1 cho team cua minh
    }

    void CheckPlayerDeath(byte networkHP) {
        if(networkHP <= 0 && !isPublicDeathMessageSent) {
            isPublicDeathMessageSent = true;
            if(Object.HasStateAuthority) {
                networkInGameMessages.SendInGameRPCMessage(Networked_Killer.ToString(), 
                    $" -> killed <b>{networkPlayer.nickName_Network.ToString()}<b>");
            }
        }
    }

    IEnumerator ServerRespawnCountine() {
        yield return new WaitForSeconds(2f);
        // xet bien isRespawnRequested = true de fixUpdatedNetwork() call Respawn()
        Debug.Log("xet respawn sau 2s");
        characterMovementHandler.RequestRespawn();
    }

    //RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SetNetworkedHP(byte hp, string name) {
        this.Networked_HP = hp;

        if(Networked_HP <= 0) {
            this.Networked_IsDead = true;
            this.deadCountCurr += 1;
            this.Networked_Killer = name;
        } 
        else {
            this.Networked_IsDead = false;
            this.Networked_Killer = null;
        } 
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SetNetworkedIsDead(bool isDead) {
        this.Networked_IsDead = isDead;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SetNetworkedKiller(string killerName) => this.Networked_Killer = killerName;

    void OnHPChanged(byte previous, byte current)  {
        // if HP decreased
        if(current < previous) OnHPReduced();
    }
    void OnHPReduced() {
        if(!isInitialized) return;
        StartCoroutine(OnHitCountine());
    }
    IEnumerator OnHitCountine() {
        // this.Object Run this.cs (do dang bi ban trung) 
        // render for Screen of this.Object - localPlayer + remotePlayer
        UpdateSliderHealth?.Invoke(startingHP, Networked_HP);
        foreach (FlashMeshRender flashMeshRender in flashMeshRenders) {
            flashMeshRender.ChangeColor(Color.red);
        }
        
        // this.Object Run this.cs (do dang bi ban trung) 
        // (Object.HasInputAuthority) => chi render tai man hinh MA THIS.OBJECT NAY DANG HasInputAuthority
        if(Object.HasInputAuthority) uiOnHitImage.color = uiOnHitColor;
        
        yield return new WaitForSeconds(0.2f);
        foreach (FlashMeshRender flashMeshRender in flashMeshRenders) {
            flashMeshRender.RestoreColor();
        }

        // render cho man hinh cua this.Object run this.cs - KO HIEN THI O REMOTE
        if(Object.HasInputAuthority && !Networked_IsDead) {
            uiOnHitImage.color = new Color(0,0,0,0);
        } 
    }

    public void ResetMeshRenders() {
        //clear old
        flashMeshRenders.Clear();
        
        //? change color when getting damage
        MeshRenderer[] meshRenderers = playerModel.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            flashMeshRenders.Add(new FlashMeshRender(meshRenderer, null)); // chi dang tao mang cho meshRender
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
            flashMeshRenders.Add(new FlashMeshRender(null, skinnedMeshRenderer)); // chi dang tao mang cho meshRender
        }
    }

    //? OnChange Render networked variable
    void OnStateChanged(bool previous, bool current)  {
        if(current) {
            OnDeath(); // dang song turn die(current)
        }

        else if(!current && previous) {
            OnRelive(); // dang die turn alive(current)
        }
    }

    void OnDeath() {
        Debug.Log($"{Time.time} onDeath");
        playerModel.gameObject.SetActive(false);
        localGun.gameObject.SetActive(false);   // khi death tat luon local gun
        hitboxRoot.HitboxRootActive = false; // ko de nhan them damage
        characterMovementHandler.CharacterControllerEnable(false);

        Instantiate(deathParticlePf, transform.position + Vector3.up * 1, Quaternion.identity);
    }

    void OnRelive() {
        Debug.Log($"{Time.time} onRelive");
        if(Object.HasInputAuthority) {
            uiOnHitImage.color = new Color(0,0,0,0); // turn white image (still red image when player starting die coll 72)
        }
        playerModel.gameObject.SetActive(true);
        localGun.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.CharacterControllerEnable(true);

        // animate equip animation if player having gun on hand
        GetComponent<WeaponSwitcher>().CheckHolster();
    }

    // sau khi resapwn ben movement -> tra ve gia tri HP va isDead
    public void OnRespawned_ResetHPIsDead() {
        // khoi toa lai gia tri bat dau
        RPC_SetNetworkedHP(startingHP, null);
        UpdateSliderHealth?.Invoke(startingHP, startingHP);
        /* RPC_SetNetworkedIsDead(false); */    // can use
    }


    //? Save deathCount to fireStore
    void SaveDeathCount() {
        if(!DataSaveLoadHander.Instance) return;
        DataSaveLoadHander.Instance.playerDataToFireStore.DeathCount += 1;
        DataSaveLoadHander.Instance.SavePlayerDataFireStore();
    }
}