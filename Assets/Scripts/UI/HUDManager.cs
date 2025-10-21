using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Canvas")]
    public Canvas HUDCanvas;

    [Header("Hit Marker")]
    public Image hitMarkerImage;
    public TMP_Text hitText;
    public float hitMarkerDuration = 0.2f;

    private Coroutine hitMarkerCoroutine;
    private float defaultHitMarkerAlpha;
    private float defaultHitTextAlpha;

    [Header("Timer")]
    public TMP_Text timerText;

    [Header("Leaderboard")]
    public GameObject leaderBoardUI;
    public GameObject playerList;
    public GameObject playerLeaderboardCardPrefab;
    private List<GameObject> playerLeaderboardCards = new List<GameObject>();
    private List<PlayerInfo> sortedPlayerInfos;

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

        // Weird workaround to prevent ghost instances from appearing.
        UnityEngine.SceneManagement.Scene currentScene = gameObject.scene;

        if (!currentScene.name.Contains("GameScene"))
        {
            Debug.LogWarning($"[PoolManager] Self-destructing. Running in wrong scene: {currentScene.name}");

            // Destroy this phantom instance immediately.
            Destroy(this.gameObject);
            return;
        }

        HUDCanvas.worldCamera = CameraManager.Instance.cameraObject;
        leaderBoardUI.SetActive(false);
    }

    public void TriggerHitMarkerEffect()
    {
        if (hitMarkerCoroutine != null)
        { 
            StopCoroutine(hitMarkerCoroutine);
        }

        hitMarkerCoroutine = StartCoroutine(HitMarkerEffect());
    }

    private IEnumerator HitMarkerEffect()
    {
        Color hitMarkerColour = hitMarkerImage.color;
        defaultHitMarkerAlpha = hitMarkerColour.a;
        hitMarkerColour.a = 1f;
        hitMarkerImage.color = hitMarkerColour;

        Color hitTextColour = hitText.color;
        defaultHitTextAlpha = hitTextColour.a;
        hitTextColour.a = 1f;
        hitText.color = hitTextColour;

        yield return new WaitForSeconds(hitMarkerDuration);

        hitMarkerColour.a = defaultHitMarkerAlpha;
        hitMarkerImage.color = hitMarkerColour;

        hitTextColour.a = defaultHitTextAlpha;
        hitText.color = hitTextColour;

        hitMarkerCoroutine = null;
    }

    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateLeaderBoard()
    {
        List<PlayerInfo> playerInfos = LobbyManager.Instance.playerList.ToList();

        sortedPlayerInfos = playerInfos.OrderByDescending(p => p.currentKills).ToList();

        for (int i = 0; i < playerInfos.Count; i++)
        {
            if (i < playerLeaderboardCards.Count)
            { 
                PlayerLeaderboardCard playerCard = playerLeaderboardCards[i].GetComponentInChildren<PlayerLeaderboardCard>();
                playerCard.playerNameText.text = sortedPlayerInfos[i].playerName.Value;
                playerCard.killCountText.text = "Kills: " + sortedPlayerInfos[i].currentKills;
            }
            else
            {
                GameObject newCard = Instantiate(playerLeaderboardCardPrefab, playerList.transform);
                PlayerLeaderboardCard playerCard = newCard.GetComponentInChildren<PlayerLeaderboardCard>();
                playerCard.playerNameText.text = sortedPlayerInfos[i].playerName.Value;
                playerCard.killCountText.text = "Kills: " + sortedPlayerInfos[i].currentKills;
                playerLeaderboardCards.Add(newCard);
            }
        }

        for (int i = 0; i < playerLeaderboardCards.Count; i++)
        {
            if (i >= sortedPlayerInfos.Count)
            {
                Destroy(playerLeaderboardCards[i]);
                playerLeaderboardCards.RemoveAt(i);
            }
        }
    }

    public void ShowLeaderBoard()
    {
        leaderBoardUI.SetActive(true);
    }
}
