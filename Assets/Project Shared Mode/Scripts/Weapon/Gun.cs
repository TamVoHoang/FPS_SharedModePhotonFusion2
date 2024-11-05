using Fusion;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] int slotIndex;
    public WeaponPickup weaponPickup;
    public int SlotIndex { get { return slotIndex; } } // {get => slotIndex;}
    
}