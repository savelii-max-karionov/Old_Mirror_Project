using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    //game over Display is completely client side thus we can keep it as Mono

    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    //we use an invisible image to block the raycast to our scene 

    // method triggered with the button
    public void LeaveGame()
    {
        // if we are running a server and we are also the client
        // then we stop hosting (kicks the other clients)
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // if we are just a client then we disconnect
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        // accessing the text parameter of the TMP_text and filling it up
        winnerNameText.text = $"{winner} Has Won!";

        gameOverDisplayParent.SetActive(true);
    }
}
