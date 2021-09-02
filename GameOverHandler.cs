using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;

    // string in this case represents the players name
    // the helpful events naming convention is to start with the Client or Server
    // depending on where the events is handled
    public static event Action<string> ClientOnGameOver;

    // this object cannot be just dropped in the scen as it's a network object
    // thus we need to spawn it by the server (in the RTSNetworkManager)
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        // when we are calling directly from the script (like here)
        // we catch the event for all of the objects with this script
        // when we are calling from a particular gameobject
        // then we are only catching events for that one object
        UnitBase.ServerOnBaseSpawn += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawn += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawn -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawn -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) { return; }

        // if there is one base left we are accessing the one one remaining
        // [0] it's index is 0 as it's the first index of the list

        int winnerId = bases[0].connectionToClient.connectionId;

        //$"" is equivalent of f"" in python
        RpcGameOver($"Player {winnerId}");

        // invoke takes the parameter initialized in the event definition
        // in this case we have Action - thus is empty
        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    // RPCs are methods called on the server and executed on the client
    // the opposide of CMDs
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        //in this case we have Action<string> thus we need to pass a string
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
