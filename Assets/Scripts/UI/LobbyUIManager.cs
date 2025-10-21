using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    [Header("Canvas")]
    public Canvas lobbyUICanvas;

    [Header("Buttons")]
    public GameObject startGameButton;

    [Header("Player List")]
    public GameObject PlayerListObject; // Parent object to hold player cards
    public GameObject PlayerCardPrefab;

    private List<GameObject> playerCards = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        PlayerInputManager.Instance.UnlockCursor();
        PlayerInputManager.Instance.DisableGameplayInput();
        PlayerInputManager.Instance.EnableUIInput();
    }

    private void Start()
    {
        lobbyUICanvas.worldCamera = CameraManager.Instance.cameraObject;

        if (InstanceFinder.IsServer)
            startGameButton.SetActive(true);
        else if (InstanceFinder.IsClient)
            startGameButton.SetActive(false);

        StartCoroutine(SubscribeToPlayerListChangeAfterDelay());
    }

    private IEnumerator SubscribeToPlayerListChangeAfterDelay()
    {
        // Arbitrary delay due to losing my sanity.
        yield return new WaitForSeconds(1.5f);

        LobbyManager.Instance.playerList.OnChange += OnPlayerListChange;
        UpdatePlayerListUI(LobbyManager.Instance.playerList.ToList());
    }

    public void UpdatePlayerListUI(List<PlayerInfo> playerList)
    {

        // Iterate the player list and create/update player cards
        for (int i = 0; i < playerList.Count; i++)
        {
            if (i < playerCards.Count)
            {
                if (playerCards[i] == null)
                    return;

                // Update existing card
                TMP_Text playerNameText = playerCards[i].GetComponentInChildren<TMP_Text>();
                playerNameText.text = playerList[i].playerName.Value;
            }
            else
            {
                // Create new card
                GameObject newCard = Instantiate(PlayerCardPrefab, PlayerListObject.transform);
                TMP_Text playerNameText = newCard.GetComponentInChildren<TMP_Text>();
                playerNameText.text = playerList[i].playerName.Value;
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

    public void StartGameButton()
    {
        // Only do this as server
        if (InstanceFinder.IsServer)
        {
            LobbyManager.Instance.FadeInOnAllClients();

            StartCoroutine(StartGameAfterDelay(TransitionManager.Instance.fadeDuration + 0.1f));
        }
    }

    private IEnumerator StartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneLoadData sceneLoadData = new SceneLoadData("GameScene");
        sceneLoadData.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
    }

    public void QuitLobbyButton()
    {
        ConnectionManager.Instance.StopConnection();
    }

    void OnPlayerListChange(SyncListOperation operation, int index, PlayerInfo oldItem, PlayerInfo newItem, bool asServer)
    {
        switch (operation)
        {
            case SyncListOperation.Add:
                newItem.playerName.OnChange += OnPlayerNameChange;
                break;
            case SyncListOperation.RemoveAt:
                newItem.playerName.OnChange -= OnPlayerNameChange;
                break;
        }
    }

    void OnPlayerNameChange(string previousName, string newName, bool asServer)
    {
        // TODO: Instead of updating the whole list find the specific entry and update just the one.
        UpdatePlayerListUI(LobbyManager.Instance.playerList.ToList());
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.playerList.OnChange -= OnPlayerListChange;
    }
}
