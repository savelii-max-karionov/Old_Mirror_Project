using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    // Action<> - in the brackets we speficy what will be passed
    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawn;
    public static event Action<UnitBase> ServerOnBaseDespawn;

    #region Server

    // the equivalent of the Start method but for the server object
    public override void OnStartServer()
    {
        health.ServerOnDie += HandleServerOnDie;

        ServerOnBaseSpawn?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBaseDespawn?.Invoke(this);

        health.ServerOnDie -= HandleServerOnDie;
    }

    [Server]
    private void HandleServerOnDie()
    {
        // connectionId - unique identifier for the connection
        // connectionToClient give the owner of the current script
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client



    #endregion
}
