using System.Collections;
using System.Collections.Generic;
using Mirror;
using Steamworks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;

    [SerializeField] private bool userSteam = false;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        if(!userSteam) { return; }

        // we pass the fnc that is going to be executed when the steam tells
        // us that the lobby is created
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        if (userSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            return;
        }

        NetworkManager.singleton.StartHost();
    }

    // get's called when lobby is created
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        // this check if steam failed to create us a lobby
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            landingPagePanel.SetActive(true);
            return;
        }

        NetworkManager.singleton.StartHost();

        // in steammatchmaking we can set some data into lobby
        // and another client joining can read that data
        // here we first pass lobby id which is stored in the callback
        // then param name
        // and then our steam id
        SteamMatchmaking.SetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "HostAddress",
            SteamUser.GetSteamID().ToString());
    }

    // get's called when someone requests to join a lobby
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        // when a friend clicks to join lobby we join the lobby with the give ids
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // get's called when anyone (even you as a server) join the lobby
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if(NetworkServer.active) { return; }

        //  this allows us to grab a key from steamMatchmaking
        string hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            "HostAddress");

        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();

        landingPagePanel.SetActive(false);
    }
}
