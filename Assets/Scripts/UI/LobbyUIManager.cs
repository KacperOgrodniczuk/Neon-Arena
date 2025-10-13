using FishNet;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [Header("Player List")]
    public GameObject PlayerListObject; // Parent object to hold player cards
    public GameObject PlayerCardPrefab;

    private List<GameObject> playerCards = new List<GameObject>();

    // Short countdown timer before the game starts. 
    [Header("Timer")]
    public TMP_Text countDownTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayerListUI(List<PlayerLobbyManager> playerList)
    {
        // Iterate the player list and create/update player cards
        for (int i = 0; i < playerList.Count; i++)
        {
            if (i < playerCards.Count)
            {
                // Update existing card
                TMP_Text playerNameText = playerCards[i].GetComponentInChildren<TMP_Text>();
                playerNameText.text = playerList[i].PlayerName;
            }
            else
            {
                // Create new card
                GameObject newCard = Instantiate(PlayerCardPrefab, PlayerListObject.transform);
                TMP_Text playerNameText = newCard.GetComponentInChildren<TMP_Text>();
                playerNameText.text = playerList[i].PlayerName;
                playerCards.Add(newCard);
            }
        }

        // Iterate the player card list and delete unused cards.
        for (int i = 0; i < playerCards.Count; i++)
        {
            if (i >= playerList.Count)
            {
                Destroy(playerCards[i]);
                playerCards.RemoveAt(i);
            }
        }
    }

    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        countDownTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Stops connections when quitting the lobby. Cleanup and transition logic is handled in OnClientConnectionState function subscvribed to an event with the same name.
    /// </summary>
    public void QuitLobby()
    {
        // If server, stop the server connection
        if (InstanceFinder.IsServer)
        {
            InstanceFinder.ServerManager.StopConnection(true);
        }
        // if client, stop the client connection
        else if (InstanceFinder.IsClient)
        {
            InstanceFinder.ClientManager.StopConnection();
        }
    }
}
