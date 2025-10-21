using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateManager : NetworkBehaviour
{
    public enum GameState
    {
        InGame,
        PostGame
    }

    // In game timer
    private readonly SyncTimer timer = new SyncTimer();

    [SerializeField]
    private float matchDuration = 300f;
    private float leaderBoardShowcaseDuration = 10f;

    public static GameState gameState;

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

        gameState = GameState.InGame;
    }

    public override void OnStartClient()
    {
        PlayerInputManager.Instance.LockCursor();
        PlayerInputManager.Instance.DisableUIInput();
        PlayerInputManager.Instance.EnableGameplayInput();
    }

    private void Update()
    {
        timer.Update();
        HUDManager.Instance.UpdateTimer(timer.Remaining);

        if (timer.Remaining <= 0)
        {
            FinishGame();
        }
    }

    void FinishGame()
    {
        PlayerInputManager.Instance.EnableUIInput();
        PlayerInputManager.Instance.DisableGameplayInput();

        // Show leaderboard with all the kills.
        HUDManager.Instance.UpdateLeaderBoard();
        HUDManager.Instance.ShowLeaderBoard();

        gameState = GameState.PostGame;

        if (IsServerInitialized)
        {
            StartCoroutine(EndGameSequence());
        }
    }

    private IEnumerator EndGameSequence()
    {
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
}