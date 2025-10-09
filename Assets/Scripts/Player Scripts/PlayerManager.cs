using FishNet.Object;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Header("Player Scripts")]
    public PlayerLocomotionManager locomotionManager { get; private set; }
    public PlayerAnimationManager animationManager { get; private set; }
    public PlayerProceduralAnimationManager proceduralAnimationManager { get; private set; }
    public PlayerHealthManager healthManager { get; private set; }
    public PlayerShootingManager shootingManager { get; private set; }
    public PlayerLobbyManager lobbyManager { get; private set; }
    public PlayerStateManager stateManager { get; private set; }

    NetworkObject playerNetworkObject;

    [Header("Camera Follow Target")]
    public GameObject cameraFollowTarget;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Only set the camera for the local player
        // TODO: Needs to be moved to be called in the game scene, not the lobby scene.
        if (IsOwner)
        {
            //CameraManager.Instance.followTarget = cameraFollowTarget;
        }
    }

    private void Awake()
    {
        locomotionManager = GetComponent<PlayerLocomotionManager>();
        animationManager = GetComponent<PlayerAnimationManager>();
        proceduralAnimationManager = GetComponent<PlayerProceduralAnimationManager>();
        healthManager = GetComponent<PlayerHealthManager>();
        shootingManager = GetComponent<PlayerShootingManager>();
        lobbyManager = GetComponent<PlayerLobbyManager>();
        stateManager = GetComponent<PlayerStateManager>();

        playerNetworkObject = GetComponent<NetworkObject>();

        playerNetworkObject.SetIsGlobal(true); // Make the player object persist across scene loads
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (shootingManager.isAiming)
        {
            //if aiming pass both parameters
            animationManager.UpdateMovementParameters(locomotionManager.horizontalInput, locomotionManager.verticalInput);
        }
        else
        {
            //if not aiming pass one parameter since the player rotates in the direction they move in.
            animationManager.UpdateMovementParameters(0, locomotionManager.moveAmount);
        }

        // Used to determine wether the player should play the falling animation or not.
        animationManager.animator.SetFloat("InAirTimer", locomotionManager.inAirTimer);
        animationManager.animator.SetBool("IsGrounded", locomotionManager.isGrounded);
    }
}
