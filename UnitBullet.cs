using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBullet : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private int damageToDeal = 20;

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);  
    }

    [ServerCallback]
    // called when the bullets collider enters something else
    private void OnTriggerEnter(Collider other)
    {
        // if belongs to us we return
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            // whoever owns this unit has the same connectionToClient as the owner of this script
            // thus we assume they are the same and return
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        // if has the health script we deal damage and destroy a bullet
        if(other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);
        }

        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
