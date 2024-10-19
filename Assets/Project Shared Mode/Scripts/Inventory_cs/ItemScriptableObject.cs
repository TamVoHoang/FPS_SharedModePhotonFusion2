using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ItemScriptableObject")]
public class ItemScriptableObject : ScriptableObject {
    public Item.ItemType itemType;
    public string itemName;
    public Sprite itemSprite;   // image show on UI

    [Header("Weapon Interfaces")]
    public float damage;
    public float coolDownTime;
}