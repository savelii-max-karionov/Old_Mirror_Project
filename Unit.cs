using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 100;

    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    // unity events are events that you can specify in the editor
    // useful for small/straight-forward events
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;
    [SerializeField] private Targeter targeter = null;

    // these are C# events, then can be caught from other scripts by doing
    // Script_name.event (Unit.ServerOnUnitSpawned)
    // to subscribe to event we need to use += method_of_handling
    // to unsubscribe use -= methond_of_handling
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    // static outline that the method will be related to the type of obj instead of a single instance

    //the events names start with the word "Server" as it indicates where they are called

    // client side events that allow a user to know which units are it his/her list
    // "Authority" indicated that it's a client side action
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public int GetResourceCost()
    {
        return resourceCost;
    }

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server
    // when a start methond (equivalent to start in NetworkBehaviour) is called for this unit
    // (when it's initially spawned) we invoke an action which get's handled in RTSPlayer script
    
    public override void OnStartServer()
    { 
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie += HandleOnServerDie;
        // this passes a particular Unit that was spawned or despawned
        // this fucntion is equivalent of saying "hey everyone, this happened"
    }

    // same with the OnStopServer that get's called when the unit
    // (or any other object the script belongs to)
    // is deleted (destroyed)
    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie -= HandleOnServerDie;
    }

    [Server]
    private void HandleOnServerDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }

        onSelected?.Invoke(); // "?" - evaluate the first variable (operand) if it's null stop with the null result
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) { return; }

        onDeselected?.Invoke();
    }

    // invoked on behaviours that have authority
    public override void OnStartAuthority()
    {
        // if we are the server (host) or we don't have authority
        // hasAuthority prevents this events from being invoked on the cliet side
        // of the user who doesn't own this particualr unit
        //if (!isClientOnly || !hasAuthority) { return; }

        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        //if (!isClientOnly || !hasAuthority) { return; }
        if (!hasAuthority) { return; }

        AuthorityOnUnitDespawned?.Invoke(this);
    }
    #endregion
}
