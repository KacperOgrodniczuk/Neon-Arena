using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameSateManager : NetworkBehaviour
{
    // In game timer
    private readonly SyncTimer timer = new SyncTimer();

    [SerializeField]
    private float matchDuration = 300f;
    private float leaderBoardShowcaseDuration = 10f;

    // Only the server should set the timer and spawn all the players in. 
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Grab the list of current players
        if (IsServer)
        {
            foreach (PlayerInfo playerInfo in LobbyManager.Instance.playerList)
            {
                // RespawnPlayer takes care of setting the player state, resetting health, sending out and RPC etc...
                playerInfo.playerManager.healthManager.RespawnPlayer();
            }

            timer.StartTimer(matchDuration);
        }
    }

    public override void OnStartClient()
    {
        PlayerInputManager.Instance.LockCursor();
        PlayerInputManager.Instance.DisableUIInput();
        PlayerInputManager.Instance.EnableGameplayInput();
    }

    private void Update()
    {
        if (IsServerInitialized)
        {
            timer.Update();
        }

        UpdateHudTimer(timer.Remaining);

        if (timer.Remaining <= 0)
        {
            FinishGame();
        }
    }

    void FinishGame()
    {
        PlayerInputManager.Instance.EnableUIInput();
        PlayerInputManager.Instance.DisableGameplayInput();

        if (IsServerInitialized)
        {
            StartCoroutine(EndGameSequence());
        }
    }

    private IEnumerator EndGameSequence()
    { 
        // Show leaderboard with all the kills.


        yield return new WaitForSeconds(leaderBoardShowcaseDuration);
        
        LobbyManager.Instance.FadeInOnAllClients();

        yield return new WaitForSeconds(TransitionManager.Instance.fadeDuration + 0.1f);

        foreach (PlayerInfo playerInfo in LobbyManager.Instance.playerList.ToList())
        {
            playerInfo.playerManager.stateManager.playerState.Value = PlayerStateManager.PlayerState.Lobby;
        }

        SceneLoadData sceneLoadData = new SceneLoadData("LobbyScene");
        sceneLoadData.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
    }

    void UpdateHudTimer(float timeRemainingValue)
    {
        HUDManager.Instance.UpdateTimer(timeRemainingValue);
    }
}