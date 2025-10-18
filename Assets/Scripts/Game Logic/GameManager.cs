using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // TODO: Finish the match when the timer reaches zero and decide what happens. e.g. game results screen, restart match, force quit if not enough players, etc.
    private readonly SyncTimer timer = new SyncTimer();

    [SerializeField]
    private float matchDuration = 300f;

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
        timer.Update();

        UpdateHudTimer(timer.Remaining);
    }

    void UpdateHudTimer(float timeRemainingValue)
    {
        HUDManager.Instance.UpdateTimer(timeRemainingValue);
    }
}