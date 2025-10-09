using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameStateManager: NetworkBehaviour
{
    public enum GameState
    { 
        Lobby,
        Warmup,
        InGame,
        PostGame
    }

    // TODO:
    // 1. Add game states.
    // 2. Only start the timer if there's at least 2 players connected.
    // 3. Finish the match when the timer reaches zero and decide what happens. e.g. game results screen, restart match, force quit if not enough players, etc.

    private readonly SyncTimer timer = new SyncTimer();

    [SerializeField]
    private float matchDuration = 300f;

    public override void OnStartServer()
    { 
        base.OnStartServer();
        timer.StartTimer(matchDuration);
    }

    private void Update()
    {
        timer.Update();

        UpdateHudTimer(timer.Remaining);
    }

    void UpdateHudTimer(float timeRemainingValue)
    { 
        HUDManager.Instance.UpdateTimer(timeRemainingValue);
    }
}