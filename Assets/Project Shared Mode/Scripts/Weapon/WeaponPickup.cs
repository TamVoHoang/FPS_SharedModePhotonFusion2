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
        /* if(Object.HasStateAuthority) {
            // Create rotation around Y axis (up)
            Quaternion rotation = Quaternion.Euler(0, 90 * Runner.DeltaTime, 0);
            transform.rotation *= rotation;
        } */
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
        /* if(NetworkPlayer.Local.is3rdPersonCamera) return;
        if(!Object.HasStateAuthority) return;
        if(other.GetComponent<WeaponSwitcher>().IsTouchedWeaponPickup == true) return;

        if(other.TryGetComponent<WeaponSwitcher>(out var weaponSwitcher)) {
            if(weaponSwitcher.IsTouchedWeaponPickup == true) return;
            if(!weaponSwitcher.GetSlotsLocalHolder[slotIndex].GetComponentInChildren<Gun>()) {
                Runner.Despawn(Object);
            }
        } */

        if(NetworkPlayer.Local.is3rdPersonCamera) return;
        if(!Object.HasStateAuthority) return;
        WeaponSwitcher weaponSwitcher_ = other.GetComponent<WeaponSwitcher>();

        /* if(!weaponSwitcher_.isHasGunInInventory_Network) {
            Debug.Log("_______________co destroy");
            Runner.Despawn(Object);
        } else {
            Debug.Log("_______________KO co destroy");
        } */

        StartCoroutine(DelayDesTroy(weaponSwitcher_));
    }

    IEnumerator DelayDesTroy(WeaponSwitcher weaponSwitcher_) {
        yield return new WaitForSeconds(0.1f);
        if(!weaponSwitcher_.isHasGunInInventory_Network) {
            Debug.Log("_______________co destroy");
            Runner.Despawn(Object);
        } else {
            Debug.Log("_______________KO co destroy");
        }
    }
}