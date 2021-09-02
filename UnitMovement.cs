using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    //[SC] only gets called on the server and does not print warnings
    // used for fnc we don't control - unity callbacks
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        // we need to use a fnc here as targeter is an actual script reference
        // not the object reference
        if (target != null)
        {
            // does not do the sqrt and is more effitient Vector3.Distance check with the sqrt
            // (checks the distance between target and current obj position)
            if ((target.transform.position - transform.position).sqrMagnitude >
                chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        if(!agent.hasPath) { return; }
        // will stop agent from clearning the path in the same frame as it's calculated

        if(agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath(); //clears the current path so the agents stops the movement 
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position,
            out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion
}
