using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerStateManager : NetworkBehaviour
{
    public enum PlayerState
    {
        Lobby,
        Alive,
        Dead,
    }

    public PlayerManager playerManager { get; private set; }

    public readonly SyncVar<PlayerState> playerState = new SyncVar<PlayerState>(PlayerState.Lobby);

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();

        playerState.OnChange += OnPlayerStateChange;
    }

    [ServerRpc]
    public void ChangeState(PlayerState newState)
    { 
        playerState.Value = newState;
    }

    void OnPlayerStateChange(PlayerState oldValue, PlayerState newValue, bool asServer)
    {

        switch (newValue)
        {
            case PlayerState.Lobby:
                playerManager.locomotionManager.enabled = false;
                playerManager.animationManager.enabled = false;
                playerManager.shootingManager.enabled = false;
                playerManager.playerInfo.enabled = true;
                playerManager.proceduralAnimationManager.enabled = false; // Disable procedural animations in lobby
                playerManager.healthManager.enabled = false;

                playerManager.playerCollider.enabled = false;
                playerManager.playerSkinnedMeshRenderer.enabled = false;
                break;

            case PlayerState.Alive:
                playerManager.locomotionManager.enabled = true;
                playerManager.animationManager.enabled = true;
                playerManager.shootingManager.enabled = true;
                playerManager.playerInfo.enabled = false;
                playerManager.proceduralAnimationManager.enabled = true;
                playerManager.healthManager.enabled = true;

                playerManager.playerCollider.enabled = true;
                playerManager.playerSkinnedMeshRenderer.enabled = true;
                break;

            case PlayerState.Dead:
                playerManager.locomotionManager.enabled = false;
                playerManager.animationManager.enabled = false;
                playerManager.shootingManager.enabled = false;
                playerManager.playerInfo.enabled = false;
                playerManager.proceduralAnimationManager.enabled = false;
                
                playerManager.playerCollider.enabled = false;
                playerManager.playerSkinnedMeshRenderer.enabled = false;
                break;

        }
    }
}