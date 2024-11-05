using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class GrandeHandler : NetworkBehaviour
{
    public GameObject explosionParticleGrandePF; // hieu ung no cua Grande
    public LayerMask collisionLayers;

    //? thrown by PlayerInfo
    PlayerRef thrownByPlayerRef;
    string thrownByPlayerName;

    //? timing delay grande explosion
    TickTimer explodeTickTimer = TickTimer.None; // giong nhu gan = 0

    //? hit Info - no trung ai
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    //? components
    NetworkObject networkObject;
    NetworkRigidbody3D networkRigidbody;
    WeaponHandler weaponHandler;

    private Collider[] hitColliders = new Collider[10];

    public override void Spawned() {
        networkObject = GetComponent<NetworkObject>();
        networkRigidbody = GetComponent<NetworkRigidbody3D>();
    }

    public void Throw(Vector3 throwForce, PlayerRef thrownByPlayerPref, string thrownByPlayerName, WeaponHandler weaponHandler) {
        // khoi tao doi tuong
        networkObject = GetComponent<NetworkObject>();
        networkRigidbody = GetComponent<NetworkRigidbody3D>();
        
        // apply force cho networkObject
        networkRigidbody.Rigidbody.AddForce(throwForce, ForceMode.Impulse); // get force directly

        this.thrownByPlayerRef = thrownByPlayerPref;
        this.thrownByPlayerName = thrownByPlayerName;
        this.weaponHandler = weaponHandler;
        // khoi tao gia tri tickerTime
        explodeTickTimer = TickTimer.CreateFromSeconds(Runner, 2f);
    }

    public override void FixedUpdateNetwork() {
        if(Object.HasStateAuthority) {
            if(explodeTickTimer.Expired(Runner)) //todo neu explodeTickTimer chay den 2s
            {
                int hitCount = Physics.OverlapSphereNonAlloc(
                    transform.position,
                    4f, // Bán kính vùng nổ
                    hitColliders,
                    collisionLayers
                );

                Runner.Despawn(networkObject);  // remove this.networkObject = grenade

                // tru hp remote Player
                for (int i = 0; i < hitCount; i++) {
                    HPHandler hPHandler = hitColliders[i].GetComponentInParent<HPHandler>();
                    if(hPHandler != null) {
                        hPHandler.OnTakeDamage(thrownByPlayerName, 100, this.weaponHandler);
                    }
                }

                // stop explodeTickTimer -> no se chay lai
                explodeTickTimer = TickTimer.None;

            }
        }
    }

    //? khi de despawn this.networkObject -> tao rao visual explosion
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        Instantiate(explosionParticleGrandePF, meshRenderer.transform.position, Quaternion.identity);
    }

}
