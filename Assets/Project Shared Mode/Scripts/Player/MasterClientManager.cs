using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class MasterClientManager : NetworkBehaviour
{
    private NetworkRunner _runner;
    private bool _isQuitting;
    
    // Add RPC to handle master client change
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_RequestMasterClientChange(PlayerRef newMasterClient)
    {
        if (_runner != null && !_isQuitting)
        {
            _runner.SetMasterClient(newMasterClient);
        }
    }

    public override void Spawned()
    {
        _runner = Runner;
        //Runner.AddCallbacks(new CallbackHandler(this));
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void Update()
    {
        // Check if we're the Master Client and our object is about to be destroyed
        if (_runner != null && _runner.IsSharedModeMasterClient && !Object.IsValid)
        {
            HandleMasterClientTransfer();
        }
    }

    private void HandleMasterClientTransfer()
    {
        if (!_isQuitting)
        {
            PlayerRef newMasterClient = FindNextMasterClient();
            if (newMasterClient != PlayerRef.None)
            {
                RPC_RequestMasterClientChange(newMasterClient);
            }
        }
    }

    private PlayerRef FindNextMasterClient()
    {
        if (_runner == null) return PlayerRef.None;

        // Get all active players except the current one
        var players = _runner.ActivePlayers
            .Where(p => p != _runner.LocalPlayer)
            .OrderBy(p => p.RawEncoded)
            .ToList();

        return players.Any() ? players.First() : PlayerRef.None;
    }

    // Method to check if local player is Master Client
    public bool IsMasterClient()
    {
        return _runner != null && _runner.IsSharedModeMasterClient;
    }

    public void RequestMasterClientChange(PlayerRef newMasterClient)
    {
        if (!IsMasterClient())
        {
            Debug.LogWarning("Only the current Master Client can change the Master Client");
            return;
        }

        RPC_RequestMasterClientChange(newMasterClient);
    }

    private void OnSharedModeMasterClientChanged(PlayerRef previousMasterClient, PlayerRef newMasterClient)
    {
        Debug.Log($"Master Client changed from Player {previousMasterClient} to Player {newMasterClient}");
        
        if (_runner.LocalPlayer == newMasterClient)
        {
            Debug.Log("We are now the Master Client!");
            // Initialize Master Client specific logic
        }
    }

    /* private class CallbackHandler : INetworkRunnerCallbacks
    {
        private MasterClientManager _manager;

        public CallbackHandler(MasterClientManager manager)
        {
            _manager = manager;
        }

        public void OnSharedModeMasterClientChanged(NetworkRunner runner, PlayerRef previousMasterClient, PlayerRef newMasterClient)
        {
            _manager.OnSharedModeMasterClientChanged(previousMasterClient, newMasterClient);
        }

        // Implement other required interface methods...
    } */
}
