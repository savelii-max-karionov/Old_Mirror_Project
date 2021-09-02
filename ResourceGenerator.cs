using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private float interval = 10f;
    [SerializeField] private int resourcesPerInterval = 10;

    private float timer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        // how to get a reference to a player script
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        health.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        // Time.dealtaTime how much time has passed
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            player.SetResouces(player.GetResources() + resourcesPerInterval);

            timer += interval;
        }
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;
    }
}
