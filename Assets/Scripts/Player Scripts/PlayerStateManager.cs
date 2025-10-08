using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    public enum PlayerState
    {
        Lobby,
        Alive,
        Dead,
    }

    public PlayerManager playerManager { get; private set; }

    public SyncVar<PlayerState> playerState = new SyncVar<PlayerState>(PlayerState.Lobby);

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();

        playerState.OnChange += OnPlayerStateChange;
    }

    void OnPlayerStateChange(PlayerState oldValue, PlayerState newValue, bool asServer)
    {

        switch (newValue)
        {
            case PlayerState.Lobby:
                playerManager.locomotionManager.enabled = false;
                playerManager.animationManager.enabled = false;
                playerManager.shootingManager.enabled = false;
                playerManager.healthManager.enabled = false;
                playerManager.lobbyManager.enabled = true;
                break;
            case PlayerState.Alive:
                playerManager.locomotionManager.enabled = true;
                playerManager.animationManager.enabled = true;
                playerManager.shootingManager.enabled = true;
                playerManager.healthManager.enabled = true;
                playerManager.lobbyManager.enabled = false;
                break;
            case PlayerState.Dead:
                playerManager.locomotionManager.enabled = false;
                playerManager.animationManager.enabled = false;
                playerManager.shootingManager.enabled = false;
                playerManager.healthManager.enabled = true; // Keep health manager enabled to allow respawning
                playerManager.lobbyManager.enabled = false;
                break;
            default:
                break;
        }
    }
}