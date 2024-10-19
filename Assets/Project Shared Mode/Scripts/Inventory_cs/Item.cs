using System;
using Firebase.Firestore;

[Serializable]
[FirestoreData]
public class Item
{
    public enum ItemType
    {
        None,
        Knife01,
        Pistol01,
        Rifle01,
    }

    public ItemType itemsType {get; set;}
    public int amount {get; set;}

    [FirestoreProperty]
    public ItemType ItemsType {get=> itemsType; set => itemsType = value;}
    
    [FirestoreProperty]
    public int Amount {get => amount; set => amount = value;}

    public ItemScriptableObject itemScriptableObject;

    public ItemScriptableObject GetScriptableObject() {
        return GetScriptableObject(itemsType);
    }

    public ItemScriptableObject GetScriptableObject(ItemType itemType) {
        switch (itemType)
        {
            default:
            case ItemType.Knife01: return ItemAssets.Instance.IKnife01_SO;
            case ItemType.Pistol01: return ItemAssets.Instance.IPistol01_SO;
            case ItemType.Rifle01: return ItemAssets.Instance.IRifle01_SO;

        }
    }
}
