using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;

public class HPHandler : NetworkBehaviour
{
    [Networked]
    public byte HP {get; set;}

    [Networked]
    public byte NetworkedHealth { get; set; } = 100;
    
    [Networked]
    public bool isDead {get; set;}

    [Networked]
    public int deadCount {get; set;}

    bool isInitialized = false;
    const byte startingHP = 10;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    // change material when player hit damage (doi mau SkinnedMeshRenderer)
    List<FlashMeshRender> flashMeshRenders = new List<FlashMeshRender>();

    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject deathParticlePf;
    HitboxRoot hitboxRoot;  // ko hit remote player 2 lan
    CharacterMovementHandler characterMovementHandler;

    //? thong bao khi bi killed
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;
    public bool isSkipSettingStartValues = false; // ko cho chay lai ham start() khi thay doi host migration

    //todo fusion 2.0
    ChangeDetector changeDetector;  //duoc foi khi spawned => col 187

    private void Awake() {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }
    void Start() {
        if(!isSkipSettingStartValues) {
            HP = startingHP;
            isDead = false;
            deadCount = 0;
        }

        ResetMeshRenders();
        /* //? change color when getting damage
        MeshRenderer[] meshRenderers = playerModel.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            flashMeshRenders.Add(new FlashMeshRender(meshRenderer, null)); // chi dang tao mang cho meshRender
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
            flashMeshRenders.Add(new FlashMeshRender(null, skinnedMeshRenderer)); // chi dang tao mang cho meshRender
        } */

        //defaultSKin_Material = skinnedMeshRenderer.material;
        isInitialized = true;
    }

    //todo nhung thay doi cua bien Network
    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                /* case nameof(HP):
                var byteReader = GetPropertyReader<byte>(nameof(HP));
                var (previousByte, currentByte) = byteReader.Read(previousBuffer, currentBuffer);
                OnHPChanged(previousByte, currentByte);
                    break; */
                
                case nameof(isDead):
                var boolReader = GetPropertyReader<bool>(nameof(isDead));
                var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                OnStateChanged(previousBool, currentBool);
                    break;

            }
        }
    }

    //? server call | coll 55 WeaponHandler.cs | khi hitInfo.HitBox tren player
    public void OnTakeDamage(string damageCausedByPlayerNickName, byte damageAmount, WeaponHandler weaponHandler) {
        if(isDead) return;

        //gioi han gia tri damageAmount
        if(damageAmount > HP) damageAmount = HP;

        HP -= damageAmount;

        Debug.Log($"{Time.time} {transform.name} took damage {HP} left");

        /* if(HP <= 0) {
            Debug.Log($"{Time.time} {transform.name} is dead");
            //thong bao ai ban ai
            networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickName, 
                                    $"Killed <b>{networkPlayer.nickName_Network.ToString()}<b>");

            StartCoroutine(ServerRespawnCountine());
            isDead = true;

            deadCount ++;
            weaponHandler.killCount ++;
        } */
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(byte damage)
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        Debug.Log("Received DealDamageRpc on StateAuthority, modifying Networked variable");
        NetworkedHealth -= damage;
    }

    void OnHPChanged(byte previous, byte current)  {
        /* Debug.Log($"{Time.time} OnHPChanged {changed.Behaviour.HP}");
        int newHP = changed.Behaviour.HP;
        changed.LoadOld();
        int oldHP = changed.Behaviour.HP; */

        // if HP decreased
        if(current < previous) OnHPReduced();
    }
    private void OnHPReduced() {
        if(!isInitialized) return;
        StartCoroutine(OnHitCountine());
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

    IEnumerator OnHitCountine() {
        // this.Object Run this.cs (do dang bi ban trung) 
        // render for Screen of this.Object - localPlayer + remotePlayer
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
        if(Object.HasInputAuthority && !isDead) {
            uiOnHitImage.color = new Color(0,0,0,0);  
        } 
    }
    IEnumerator ServerRespawnCountine() {
        yield return new WaitForSeconds(2f);
        // xet bien isRespawnRequested = true de fixUpdatedNetwork() call Respawn()
        Debug.Log("xet respawn sau 2s");
        characterMovementHandler.RequestRespawn(); 
    }

    //? thong bao cho remote clients
    void OnStateChanged(bool previous, bool current)  {
        /* Debug.Log($"{Time.time} OnHPChanged {changed.Behaviour.isDead}");
        bool isDeathCurent = changed.Behaviour.isDead;
        changed.LoadOld();
        bool isDeathOld = changed.Behaviour.isDead; */
        
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
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.CharacterControllerEnable(true);
    }

    public void OnRespawned_ResetHP() {
        // khoi toa lai gia tri bat dau
        HP = startingHP;

        isDead = false;
    }

    // ham duoc goi khi Object was spawned
    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
}

