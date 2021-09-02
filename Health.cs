using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    // server can change it, and when it's changed the clients get
    // notified when it happens through the hook
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    // Unity build-in event system 
    public event Action ServerOnDie;

    // passing int for current health and int for max health
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void DealDamage(int damage)
    {
        if(currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        // picks the biggest value
        // in this case if the health went to negative after dealing damage
        // we change the current health to 0

        if(currentHealth != 0) { return; }

        ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        // if the player who died is not us then return 
        if(connectionToClient.connectionId != connectionId) { return; }

        // deals the same amout of damage as the current health 
        DealDamage(currentHealth);
    }

    #endregion

    #region Client

    // this is the hook that get's called when the currentHealth changes
    // it triggers the event the get's activeted
    private void HandleHealthUpdated(int oldHeath, int newHeath)
    {
        // as specified in the action<int, int> we pass two int parameters
        ClientOnHealthUpdated?.Invoke(newHeath, maxHealth);
    }

    #endregion
}