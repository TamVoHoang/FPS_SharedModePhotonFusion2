using System.Collections;
using Fusion;
using UnityEngine;

public class WeaponPickup : NetworkBehaviour
{
    [Networked]
    public bool isTouched { get; set; }
    [SerializeField] int slotIndex;
    public int SlotIndex { get { return slotIndex; } }

    public Gun local_GunPF;
    public Gun remote_GunPF;
    ChangeDetector changeDetector;


    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(isTouched):
                var boolReader = GetPropertyReader<bool>(nameof(isTouched));
                var (previousBool, currentBool) = boolReader.Read(previousBuffer, currentBuffer);
                OnStateChanged(previousBool, currentBool);
                    break;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority) {
            // Create rotation around Y axis (up)
            Quaternion rotation = Quaternion.Euler(0, 90 * Runner.DeltaTime, 0);
            transform.rotation *= rotation;
        }
    }

    IEnumerator Delay() {
        yield return new WaitForSeconds(0.1f);
        isTouched = false;
    }

    private void OnStateChanged(bool previousBool, bool currentBool)
    {
        if(!currentBool && previousBool) {
            DestroyWeapon();
        }
    }

    void DestroyWeapon() {
        Destroy(this.gameObject);
        Debug.Log($"co goi ham destroy weapon object");
    }

    private void OnTriggerEnter(Collider other) {
        if(NetworkPlayer.Local.is3rdPersonCamera) return;
        
        WeaponSwitcher weaponSwitcher = other.GetComponent<WeaponSwitcher>();
        if(weaponSwitcher != null && weaponSwitcher.IsTouchedWeaponPickup == false) {
            if(weaponSwitcher.GetSlotsLocalHolder[slotIndex].GetComponentInChildren<Gun>()) return;

            isTouched = true;
        }
        
        StartCoroutine(Delay());
    }


}
