using System;
using System.Runtime.Serialization;
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

    // [FirestoreProperty]
    // public ItemType ItemsType {get=> itemsType; set => itemsType = value;}

    //todo testing
    [FirestoreProperty]
    public ItemType ItemsType 
    { 
        get => itemsType;
        set
        {
            itemsType = value;
            // Automatically load the ScriptableObject when ItemType is set
            itemScriptableObject = GetScriptableObject(value);
        }
    }
    
    [FirestoreProperty]
    public int Amount {get => amount; set => amount = value;}

    [field: NonSerialized]
    public ItemScriptableObject itemScriptableObject;

    //todo Called after deserialization
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        // Restore the ScriptableObject reference after deserialization
        itemScriptableObject = GetScriptableObject(itemsType);
    }
    
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