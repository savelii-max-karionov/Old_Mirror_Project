using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGameButton = null;
    // when we know the exact size when can specify it in advance
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];

    private static string defaultDisplayName = "Waiting For Player...";

    private void Start()
    {
        // this event are mostly used to turn on and off the
        // pecific pages (panels) presented in the UI within the same scene
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdate;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdate;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void ClientHandleInfoUpdated()
    {
        // a general way to cast you local network manager code to the
        // building network manager feature 
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].GetDisplayName();
        }

        // to go over the once that need to be changed back to  waiting for player...
        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = defaultDisplayName;
        }

        startGameButton.interactable = players.Count >= 2;
    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    private void AuthorityHandlePartyOwnerStateUpdate(bool state)
    {
        startGameButton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CMDStartGame();
    }

    public void LeaveLobby()
    {
        // if both a server and a client
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        } else
        {
            NetworkManager.singleton.StopClient();

            // reloads to the main menu
            // not to turn off and on the UI
            SceneManager.LoadScene(0);
        }
    }
}
