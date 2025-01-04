using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetAuthorityPlayerDeSpawn : NetworkBehaviour
{
    [Networked]
    PlayerRef playerRef{get; set;}
    public override void Spawned()
    {
        base.Spawned();
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        if(SceneManager.GetActiveScene().name == "Ready") {
            SetReadyUIhandlerStateAuthority();

            if(runner.IsSharedModeMasterClient) {
                Debug.Log($"_____ master client just left room = " + runner.LocalPlayer);
                PlayerRef playerRef = FindNextMasterClient(runner);
                if(playerRef == PlayerRef.None) return;
                Debug.Log($"_____master client next = " + playerRef);
                runner.SetMasterClient(playerRef);
            }
            
        } else {
            if(runner.IsSharedModeMasterClient) {
                if(Object.HasStateAuthority) {
                    Debug.Log($"_____ master client just left room = " + runner.LocalPlayer);
                    PlayerRef playerRef = FindNextMasterClient(runner);
                    Debug.Log($"_____master client next = " + playerRef);
                    if(playerRef == PlayerRef.None) return;
                    runner.SetMasterClient(playerRef);
                }
                    
            }
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


    // tim new master client != this despawn
    private PlayerRef FindNextMasterClient(NetworkRunner runner)
    {
        if (runner == null) return PlayerRef.None;

        // Get all active players except the current one
        var players = runner.ActivePlayers
            .Where(p => p != runner.LocalPlayer)
            .OrderBy(p => p.RawEncoded)
            .ToList();

        return players.Any() ? players.First() : PlayerRef.None;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_RequestMasterClientChange(PlayerRef playerRef) {
        this.playerRef =  playerRef;
    }


}
