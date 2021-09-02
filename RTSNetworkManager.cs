using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

// an importnat thing to conside is which scripts will need to be disabled
// when the game is over (or when the certain state is reached
// it's better to think about it in advance and mark them 

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    private bool isGameInProgress = false; // a bool to indicate if we are playing
    // in order to prevent players from connecting while the game is going

    // a list to display players in the lobby
    // get; makes it a getter only - cannot change it from other scripts
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(!isGameInProgress) { return; }
        // kick the player if the game is in progress
        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        // when a player disconnects remove them from the list
        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        // clears the player list and resets the game
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        // if there are less then two players connected - return
        if(Players.Count < 2) { return; }

        isGameInProgress = true;

        ServerChangeScene("Scene_main");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.SetDisplayName($"Player {Players.Count}");

        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        ));

        // if there is only one player in the lobby set's that player
        // to be the party owner
        player.SetPartyOwner(Players.Count == 1);

        // this code spawns a base when the player joins
        //GameObject unitSpawnerInstance = Instantiate(
        //    unitSpawnerPrefab,
        //    conn.identity.transform.position,
        //    conn.identity.transform.rotation);

        //NetworkServer.Spawn(unitSpawnerInstance, conn);
    }

    // called right after the scene is changed
    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Main"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            // as the gameOverHandleInstance is a script, we want to spawn it as a gameobject
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in Players)
            {
                // GetStartPosition returns avaliable start position next to be used
                GameObject unitBaseInstance = Instantiate(
                    unitBasePrefab,
                    GetStartPosition().position,
                    Quaternion.identity);

                // the owner is this player - connectionToClient used for ownership
                NetworkServer.Spawn(unitBaseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client

    // default fnc that get's callec when client connects
    public override void OnClientConnect(NetworkConnection conn)
    {
        // perform the default functionality
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion 
}