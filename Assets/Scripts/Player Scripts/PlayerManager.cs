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
    public PlayerInfo playerInfo { get; private set; }
    public PlayerStateManager stateManager { get; private set; }

    [Header("Unity Components")]
    public Collider playerCollider { get; private set; }
    public SkinnedMeshRenderer playerSkinnedMeshRenderer { get; private set; }

    [Header("Camera Follow Target")]
    public GameObject cameraFollowTarget;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            CameraManager.Instance.followTarget = cameraFollowTarget;
        }
    }

    private void Awake()
    {
        // Grab player scripts
        locomotionManager = GetComponent<PlayerLocomotionManager>();
        animationManager = GetComponent<PlayerAnimationManager>();
        proceduralAnimationManager = GetComponent<PlayerProceduralAnimationManager>();
        healthManager = GetComponent<PlayerHealthManager>();
        shootingManager = GetComponent<PlayerShootingManager>();
        playerInfo = GetComponent<PlayerInfo>();
        stateManager = GetComponent<PlayerStateManager>();

        // Grab Unity Components
        playerCollider = GetComponent<Collider>();
        playerSkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
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
