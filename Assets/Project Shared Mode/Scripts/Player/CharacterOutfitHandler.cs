using System.Collections.Generic;
using Fusion;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterOutfitHandler : NetworkBehaviour
{
    [Header("Character parts current")]
    [SerializeField] GameObject playerHead;
    [SerializeField] GameObject playerRightArm;
    [SerializeField] GameObject playerLeftArm;
    [SerializeField] GameObject playerBody;
    [SerializeField] GameObject playerSkins;
    [SerializeField] Transform attach;  // transform holding attachItems
    [SerializeField] Transform skins;   // transfrom holding skins

    [Header("Ready UI Selection")]
    [SerializeField] Image readyImage;

    [Header("Animation")]
    [SerializeField] Animator animator;

    //danh sach bodies prefabs
    List<GameObject> headPrefabs = new List<GameObject>();
    List<GameObject> bodyPrefabs = new List<GameObject>();
    List<GameObject> leftArmPrefabs = new List<GameObject>();
    List<GameObject> rightArmPrefabs = new List<GameObject>();
    List<GameObject> skinPrefabs = new List<GameObject>();

    //AI
    NetworkPlayer networkPlayer;

    // INetwork Struct
    struct NetworkOutFit : INetworkStruct
    {
        public byte headPrefabID; // 0 - 255
        public byte bodyPrefabID;
        public byte leftArmPrefabID;
        public byte rightArmPrefabID;
        public byte skinPrefabID;
    }

    [Networked]
    NetworkOutFit networkOutFit{get; set;}

    [Networked]
    public NetworkBool isDoneWithCharacterSelection {get; set;}

    public bool IsBusy => throw new System.NotImplementedException();

    public Scene MainRunnerScene => throw new System.NotImplementedException();

    ChangeDetector changeDetector;

    private void Awake() {

        animator = GetComponentInChildren<Animator>();
        //load headPF tu folder trong unity chua this.PF => cho vao list
        headPrefabs = Resources.LoadAll<GameObject>("BodyParts/Heads/").ToList();
        // dam bao list co thu tu
        headPrefabs = headPrefabs.OrderBy(n=>n.name).ToList();

        /* bodyPrefabs = Resources.LoadAll<GameObject>("BodyParts/Bodies/").ToList();
        bodyPrefabs = bodyPrefabs.OrderBy(n=>n.name).ToList();

        leftArmPrefabs = Resources.LoadAll<GameObject>("BodyParts/LeftArms/").ToList();
        leftArmPrefabs = leftArmPrefabs.OrderBy(n=>n.name).ToList();

        rightArmPrefabs = Resources.LoadAll<GameObject>("BodyParts/RightArms/").ToList();
        rightArmPrefabs = rightArmPrefabs.OrderBy(n=>n.name).ToList();

        skinPrefabs = Resources.LoadAll<GameObject>("BodyParts/Skins/").ToList();
        skinPrefabs = skinPrefabs.OrderBy(n=>n.name).ToList(); */

        //? su dung transform de gom doi tuong con ben trong
        foreach (Transform child in attach) {
            bodyPrefabs.Add(child.gameObject);
        }

        foreach (Transform child in skins) {
            skinPrefabs.Add(child.gameObject);
        }

        networkPlayer = GetComponent<NetworkPlayer>();

    }
    
    void Start()
    {
        animator.SetLayerWeight(1, 0.0f);

        // dieu kien de ai thay doi outfit trong world1 scene
        if(SceneManager.GetActiveScene().name != "Ready") return; 
        NetworkOutFit newOutfit = networkOutFit; //? OK co the dung

        //? random gia tri
        newOutfit.headPrefabID = (byte)Random.Range(0, headPrefabs.Count);
        /* newOutfit.leftArmPrefabID = (byte)Random.Range(0, leftArmPrefabs.Count);
        newOutfit.rightArmPrefabID = (byte)Random.Range(0, rightArmPrefabs.Count); */
        newOutfit.bodyPrefabID = (byte)Random.Range(0, bodyPrefabs.Count);
        newOutfit.skinPrefabID = (byte)Random.Range(0, skinPrefabs.Count);

        //? On layer ReadyUp cua aniamtor
        if(SceneManager.GetActiveScene().name == "Ready") {
            animator.SetLayerWeight(1, 1.0f); // layer ReadyUp = 1 => set 1.0f cho weight
        }

        //? request host change outfit neu la hasInputAuthority thi duoc gui tin hieu RPC
        if(Object.HasInputAuthority) {
            RPC_RequestOutfitChanged(newOutfit);
        }

    }

    //todo nhung thay doi cua bien Network
    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(networkOutFit):
                    OnOutfitChanged();
                    break;
                
                case nameof(isDoneWithCharacterSelection):
                    OnIsDoneWithCharacterSelectionChanged();
                    break;
            }
        }
    }

    GameObject ReplaceBodyPart(GameObject currentBodyPart, GameObject prefabNewBodyPart) {
        GameObject newPart = Instantiate(prefabNewBodyPart, currentBodyPart.transform.position, currentBodyPart.transform.rotation);
        newPart.transform.parent = currentBodyPart.transform.parent;

        // set layer de local camera KO render
        Utils.SetRenderLayerInChildren(newPart.transform, currentBodyPart.layer);

        //!testign neu Replace kieu prefab thi destroy | neu replace kieu object nam trong transform co san thi chi on of
        Destroy(currentBodyPart, 0.1f);
        return newPart;
    }

    void ReplaceBodyTransform(GameObject currentBodyPart, GameObject prefabNewBodyPart, List<GameObject> a) {
        foreach (var item in a)
        {
            if(item == prefabNewBodyPart) {
                item.SetActive(true);
            } else {
                item.SetActive(false);
            }
        }

        // set layer de local camera KO render
        Utils.SetRenderLayerInChildren(skinPrefabs[networkOutFit.skinPrefabID].transform, currentBodyPart.layer);
    }
    
    //? replace thong qua network
    void ReplaceBodyPart() {
        //?replace Head
        playerHead = ReplaceBodyPart(playerHead, headPrefabs[networkOutFit.headPrefabID]); //sv[i]

        //?replace left arm
        //playerLeftArm = ReplaceBodyPart(playerLeftArm, leftArmPrefabs[networkOutFit.leftArmPrefabID]);

        //?replace right arm
        //playerRightArm = ReplaceBodyPart(playerRightArm, rightArmPrefabs[networkOutFit.rightArmPrefabID]);

        //?replace Body
        ReplaceBodyTransform(playerBody, bodyPrefabs[networkOutFit.bodyPrefabID], bodyPrefabs);
        //?replace skins
        ReplaceBodyTransform(playerSkins, skinPrefabs[networkOutFit.skinPrefabID], skinPrefabs);

        GetComponent<HPHandler>().ResetMeshRenders();
    }

    //? client thong bao voi RPC ve viec thay doi biet networked networkOutFit
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestOutfitChanged(NetworkOutFit newNetworkOutFit, RpcInfo info = default) {
        Debug.Log($"recived RPC_RequestOutfitChanged for player {transform.name} | HeadID {newNetworkOutFit.headPrefabID} | BodyId {newNetworkOutFit.bodyPrefabID} | Skins {newNetworkOutFit.skinPrefabID}");
        networkOutFit = newNetworkOutFit;
    }

    /* //! gia tri networkOutFit thay doi => coll 92 => coll 96 => coll 72 => thay doi playerParts theo gia tri networkOutfit
    //? bat ki khi nao bien networkOutFit thuoc struct du lien kieu NetworkOutFit : INetworkStruct
    static void OnOutfitChanged(Changed<CharacterOutfitHandler> changed) {
        changed.Behaviour.OnOutfitChanged();
    } */

    private void OnOutfitChanged() {
        // thay doi cho this.Object tai local va remote
        ReplaceBodyPart();
    }

    //? vong lap de chon tat ca cac PlayerHead ben trong HeadPrefabs list
    public void OnCycleHead() {
        NetworkOutFit newOutFit = networkOutFit; // kieu byte vi tri torng list

        // chon headPrefabs[++]
        newOutFit.headPrefabID ++;

        //vong lap de ko bi ra khoi list.cout
        // chi so trong list bat dau tu 0 || count tu 1
        if(newOutFit.headPrefabID > headPrefabs.Count - 1) {
            newOutFit.headPrefabID = 0;
        }

        // yeu cau host change outFit if this.Object have hasInputAuthority
        if(Object.HasInputAuthority) {
            RPC_RequestOutfitChanged(newOutFit);
        }
    }

    public void OnCycleBody() {
        NetworkOutFit newOutFit = networkOutFit; // kieu byte vi tri torng list

        newOutFit.bodyPrefabID ++;

        if(newOutFit.bodyPrefabID > bodyPrefabs.Count - 1) {
            newOutFit.bodyPrefabID = 0;
        }

        if(Object.HasInputAuthority) RPC_RequestOutfitChanged(newOutFit);
    }

    public void OnCycleSkin() {
        NetworkOutFit newOutFit = networkOutFit; // kieu byte vi tri torng list

        newOutFit.skinPrefabID ++;

        if(newOutFit.skinPrefabID > skinPrefabs.Count - 1) {
            newOutFit.skinPrefabID = 0;
        }

        if(Object.HasInputAuthority) RPC_RequestOutfitChanged(newOutFit);
    }

    //? check is ready
    public void OnReady(bool isReady) {
        // yeu cau host thay doi outfit, neu nhu this.Object hasInputAuthority
        // if we have input authority over this.Object
        if(Object.HasInputAuthority) RPC_SetReady(isReady);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default) {
        this.isDoneWithCharacterSelection = isReady;
    }

    /* static void OnIsDoneWithCharacterSelectionChanged(Changed<CharacterOutfitHandler> changed) {
        changed.Behaviour.IsDoneWithCharacterSelectionChanged(); // thong bao thay doi cho tat ca clients
    } */

    void OnIsDoneWithCharacterSelectionChanged() {
        if(SceneManager.GetActiveScene().name != "Ready") return; //! Dong nay de clients tat duoc nut icon ready Image

        if(isDoneWithCharacterSelection) {
            animator.SetTrigger("ready");
            readyImage.gameObject.SetActive(true);
        } 
        else readyImage.gameObject.SetActive(false);
    }

    void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(scene.name != "Ready") {
            readyImage.gameObject.SetActive(false);
            animator.SetLayerWeight(1, 0.0f); //! tat layer Ready trong animator - khi vao MainGame ko can layer nay
        }
    }

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        OnOutfitChanged();
    }
}
