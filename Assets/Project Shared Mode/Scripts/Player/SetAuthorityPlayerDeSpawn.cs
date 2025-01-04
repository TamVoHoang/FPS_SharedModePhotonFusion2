using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SetAuthorityPlayerDeSpawn : NetworkBehaviour
{
    public override void Spawned()
    {
        base.Spawned();
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        SetWeaponPickupStateAuthority();
        SetGameMangerStateAuthority();
    }

    void SetWeaponPickupStateAuthority() {
        WeaponPickup[] weaponPickups = FindObjectsOfType<WeaponPickup>();

        foreach (var item in weaponPickups)
        {
            item.WeaponPickupRequestStateAuthority();
        }
    }

    void SetGameMangerStateAuthority() {
        GameManagerUIHandler gameManagerUIHandler = FindObjectOfType<GameManagerUIHandler>();
        gameManagerUIHandler.GameManagerRequestStateAuthority();
    }
}
