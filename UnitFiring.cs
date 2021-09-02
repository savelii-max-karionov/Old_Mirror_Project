using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject gun = null;
    [SerializeField] private Transform gunUnitCenter = null;
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private Transform bulletSpawnPoint = null;

    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    float lastFireTime;
    Transform gunTransform;

    private void Start()
    {
        gunTransform = gun.GetComponent<Transform>();
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if(target == null) { return; }
        // checks if we are in range of our target
        if(!CanFireAtTarget()) { return; }

        // calculating the rotation
        // LookRotation returns the needed rotation point when a vector towards the point is given
        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position - gunUnitCenter.position);
        // targeter.GetTarget().transform.position - transform.position
        //returns a vector pointing towards the target we are aiming for

        gunTransform.rotation =
            Quaternion.RotateTowards(gunTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Time.time retuns the time our game has been running for
        // 1 / fireRate indicated how many times can we fire per second (1 = second)
        // lastFireTime is the lat time we fired
        if(Time.time > (1 / fireRate) + lastFireTime)
        {
            // calculating the vector 
            Quaternion bulletRotation =
                Quaternion.LookRotation(
                    target.GetAimAtPoint().position - bulletSpawnPoint.position);

            // instantiates on the server
            GameObject bulletInstance = Instantiate(
                bulletPrefab, bulletSpawnPoint.position, bulletRotation);

            // instanctiates on all clients
            // connectionToClient in this example indicates the ownership
            // connectionToClient give the ownership to the person who owns this script
            NetworkServer.Spawn(bulletInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
            <= fireRange * fireRange;
    }
}
