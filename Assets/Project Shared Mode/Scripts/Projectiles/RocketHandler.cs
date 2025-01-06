using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

//todo gameObject = rocket

public class RocketHandler : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField ] GameObject explosionParticleRocketPF;

    [Header("Collision Detection")]
    [SerializeField] LayerMask collisionLayerMask;
    [SerializeField] Transform checkForImpactPoint;
    
    //timing
    TickTimer maxLiveDurationTickTimer = TickTimer.None; // thoi gian ton tai
    //speed Rocket
    [SerializeField] float rocketSpeed = 20f;

    //HitInfo
    List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    //? fired by PLAYER INFO - WHO FIRED
    PlayerRef fireByPlayerRef;
    string fireByPlayerName;
    NetworkObject fireByNetworkObject;

    NetworkObject networkObject;
    WeaponHandler weaponHandler;

    // dectect collison Enter _ not using
    /* private NetworkRigidbody3D networkRigidbody;
    private Collider rocketCollider;
    [SerializeField] float collisionRadius = 0.5f; */

    float detectionRadius = 0.5f;
    private Collider[] hitColliders = new Collider[15];
    public void Fire(PlayerRef fireByPlayerPref, NetworkObject fireByNetworkObject, string fireByPlayerName, WeaponHandler weaponHandler) {
        this.fireByPlayerRef = fireByPlayerPref;
        this.fireByPlayerName = fireByPlayerName;
        this.fireByNetworkObject = fireByNetworkObject;
        this.weaponHandler = weaponHandler;
        networkObject = GetComponent<NetworkObject>();

        // dectect collison Enter
        /* if (!networkRigidbody) networkRigidbody = GetComponent<NetworkRigidbody3D>();
        if (!rocketCollider) rocketCollider = GetComponent<Collider>(); */

        maxLiveDurationTickTimer = TickTimer.CreateFromSeconds(Runner, 10f);
    }

    public override void FixedUpdateNetwork()
    {
        // move rocket ve phia truoc
        transform.position += transform.forward * Runner.DeltaTime * rocketSpeed;

        // neu la host or server
        if(Object.HasStateAuthority) {
            // neu rocker het gio bay va ko cham vat can
            if(maxLiveDurationTickTimer.Expired(Runner)) {
                Runner.Despawn(networkObject);
                return;
            }

            // neu cham vat can trong khi bay
            int hitCount = Physics.OverlapSphereNonAlloc(
                checkForImpactPoint.position, 
                detectionRadius,
                hitColliders,
                collisionLayerMask
            );
            // neu cham anything with physic what happens
            bool isValidHit = false;
            if(hitCount > 0) isValidHit = true;

            // kiem tra da hit cai gi
            for (int i = 0; i < hitCount; i++) {
                NetworkObject hitNetworkObject = hitColliders[i].GetComponentInParent<NetworkObject>();
                if(hitNetworkObject != null && hitNetworkObject == fireByNetworkObject) {
                    isValidHit = false;
                    break;
                }
            }

            //? hit collisionLayer (remotePlayer) || hit physicObject
            if(isValidHit) {
                //todo kiem tra damage
                hitCount = Physics.OverlapSphereNonAlloc(
                    checkForImpactPoint.position,
                    4f, // Bán kính vùng nổ
                    hitColliders,
                    collisionLayerMask
                );

                for (int i = 0; i < hitCount; i++) {
                    HPHandler hPHandler = hitColliders[i].GetComponentInParent<HPHandler>();
                    if(hPHandler != null) {
                        hPHandler.OnTakeDamage(fireByPlayerName, 100, this.weaponHandler);
                    }
                }
                Runner.Despawn(networkObject);
            }
        }
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        Instantiate(explosionParticleRocketPF, meshRenderer.transform.position, Quaternion.identity);
    }

    /* private void OnCollisionEnter(Collision collision)
    {
        if(!Object.HasStateAuthority) return;

        // Ignore collision with the firing player
        NetworkObject collidedWith = collision.gameObject.GetComponent<NetworkObject>();
        if(collidedWith == fireByNetworkObject) return;

        // Check if we hit something in our collision layer
        if(((1 << collision.gameObject.layer) & collisionLayerMask.value) != 0) {
            // Handle explosion damage
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f, collisionLayerMask);
            
            foreach(var hitCollider in hitColliders) {
                HPHandler hPHandler = hitCollider.GetComponentInParent<HPHandler>();
                //HPHandler hPHandler = hitCollider.transform.root.GetComponent<HPHandler>();

                if(hPHandler != null) {
                    hPHandler.OnTakeDamage(fireByPlayerName, 100, weaponHandler);
                }
            }

            // Despawn the rocket
            Runner.Despawn(networkObject);
        }
    } */
    
    
}
