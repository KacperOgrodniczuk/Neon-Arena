using FishNet.Object;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Header("Player Scripts")]
    public PlayerLocomotionManager locomotionManager { get; private set; }
    public PlayerAnimationManager animationManager { get; private set; }
    public PlayerHealthManager healthManager { get; private set; }
    public PlayerShootingManager shootingManager { get; private set; }

    [Header("Camera Follow Target")]
    public GameObject cameraFollowTarget;

    [Header("Flags")]
    public bool isAiming;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Only set the camera for the local player
        if (IsOwner)
        {
            CameraManager.Instance.followTarget = cameraFollowTarget;
        }
    }

    private void Awake()
    {
        locomotionManager = GetComponent<PlayerLocomotionManager>();
        animationManager = GetComponent<PlayerAnimationManager>();
        healthManager = GetComponent<PlayerHealthManager>();
        shootingManager = GetComponent<PlayerShootingManager>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isAiming)
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
