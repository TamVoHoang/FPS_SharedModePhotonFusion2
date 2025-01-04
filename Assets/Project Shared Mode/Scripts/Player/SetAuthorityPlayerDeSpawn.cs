using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetAuthorityPlayerDeSpawn : NetworkBehaviour
{
    public override void Spawned()
    {
        base.Spawned();
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        if(SceneManager.GetActiveScene().name == "Ready") {
            SetReadyUIhandlerStateAuthority();
        } else {
            // trong scene battle
            SetWeaponPickupStateAuthority();
            SetGameMangerStateAuthority();
        }
        
    }

    void SetWeaponPickupStateAuthority() {
        
        try
        {
            WeaponPickup[] weaponPickups = FindObjectsOfType<WeaponPickup>();
            foreach (var item in weaponPickups)
            {
                item.WeaponPickupRequestStateAuthority();
            }
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    void SetGameMangerStateAuthority() {
        /* GameManagerUIHandler gameManagerUIHandler = FindObjectOfType<GameManagerUIHandler>(); */
        try
        {
            GameManagerUIHandler gameManagerUIHandler = FindObjectOfType<GameManagerUIHandler>();
            gameManagerUIHandler.GameManagerRequestStateAuthority();
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    void SetReadyUIhandlerStateAuthority() {
        /* ReadyUIHandler readyUIHandler = FindObjectOfType<ReadyUIHandler>(); */
        try
        {
            ReadyUIHandler readyUIHandler = FindObjectOfType<ReadyUIHandler>();
            readyUIHandler.ReadyUIhandlerRequestStateAuthority();
        }
        catch (System.Exception)
        {
            throw;
        }
        
    }
}
