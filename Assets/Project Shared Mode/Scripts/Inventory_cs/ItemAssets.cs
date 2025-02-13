using UnityEngine;

public class ItemAssets : MonoBehaviour {
    public static ItemAssets Instance {get; private set;}

    private void Awake() {
        Instance = this;
    }

    [Header("Item SO")]
    public ItemScriptableObject IKnife01_SO;
    public ItemScriptableObject IPistol01_SO;
    public ItemScriptableObject IRifle01_SO;

}
