using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

//todo game object == player
// gui ket qua NetworkKillCurr cua this.object thong qua RPC
// trong luc gui RPC -> goi doi tuong InGameResultTeamVsTeamUIHandler thong bao dong bo cho tat ca
public class NetworkInGameTeamResult : NetworkBehaviour
{
    public void SendInGameResultTeamRPC(bool isEnemy, int killCountCurr) {
        // bool isEnemyTeam = NetworkPlayer.Local.isEnemy_Network;
        RPC_InResultTeam(isEnemy, killCountCurr);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_InResultTeam(bool isEnemy, int killCountCurr, RpcInfo info = default) {
        
        InGameResultTeamVsTeamUIHandler.Action_OnGameMessageRecieved(isEnemy, killCountCurr.ToString());
    }
}
