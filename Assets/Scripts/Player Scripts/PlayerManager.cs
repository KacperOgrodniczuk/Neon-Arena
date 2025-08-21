using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Scripts")]
    public PlayerLocomotionManager locomotionManager { get; private set; }
    public PlayerAnimationManager animationManager { get; private set; }

    private void Awake()
    {
        locomotionManager = GetComponent<PlayerLocomotionManager>();
        animationManager = GetComponent<PlayerAnimationManager>();
    }

    private void Update()
    {
        //if aiming pass both parameters

        //if not aiming pass one parameter since the player rotates in the direction they move in.
        animationManager.UpdateMovementParameters(0, locomotionManager.moveAmount);
    }
}
