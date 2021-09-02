using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    // gameObjects on your machine and another players machine are different
    // but technically the same as mirror through the network identity
    // tries to synch the states of those two objects

    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTraget(GameObject targetGameObject)
    {
        if(targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget))
        {
            Debug.Log("the target was set");
            target = newTarget;
        }
    }

    [Server]
    public void ClearTarget()
    {
        Debug.Log("the target was cleared");
        target = null;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }

    #endregion

    #region Client



    #endregion
}