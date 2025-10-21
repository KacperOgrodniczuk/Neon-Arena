using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionManager : NetworkBehaviour
{
    private PlayerManager playerManager;
    public CharacterController characterController { get; private set; }

    [Header("Input Values")]
    public float horizontalInput { get; private set; }
    public float verticalInput {get; private set;}
    private bool sprintInput;
    private bool isAiming;
    private bool jumpInput;

    public float moveAmount { get; private set; }
    Vector3 targetRotationDirection;
    Vector3 moveDirection;

    [Header("Ground Movement Values")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float sprintSpeed = 6f;
    public float rotationSpeed = 15f;
    private float currentMoveSpeed;

    [Header("Ground Check")]
    public Transform groundCheckTransform;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded { get; private set; }

    [Header("Aerial Movement Values")]
    public float gravity = -10f;
    public float jumpHeight = 3f;
    public float inAirTimer { get; private set; }

    private Vector3 yVelocity;
    private bool fallingVelocityHasBeenSet = false;
    private bool isJumping = false;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        GetInputValues();
        DetermineSpeed();
        HandleGroundCheck();

        HandleGroundMovement();
        HandleRotation();

        HandleGravity();
        HandleJump();

        // Move the character using a combination of gravity and movement direction and speed.
        characterController.Move(Time.deltaTime * ((currentMoveSpeed * moveDirection) + yVelocity));
    }

    void GetInputValues()
    {
        //Get input values from the PlayerInputManager singleton.
        horizontalInput = PlayerInputManager.Instance.movementInput.x;
        verticalInput = PlayerInputManager.Instance.movementInput.y;
        sprintInput = PlayerInputManager.Instance.sprintInput;
        jumpInput = PlayerInputManager.Instance.jumpInput;

        isAiming = playerManager.shootingManager.isAiming;

        // Move amount mandates character speed and animations played, so we clamp it in a few ways.
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        if (moveAmount > 0f && moveAmount <= 0.5f)
            moveAmount = 0.5f;
        else if (moveAmount > 0.5f && moveAmount <= 1f)
            moveAmount = 1f;

        // The character can only sprint if they press the sprint button, are moving forwards, are grounded and not aiming.
        if (sprintInput && isGrounded && !isAiming)
        {
            moveAmount = 2f;
            
            // Clamped for animations.
            verticalInput = Mathf.Clamp(verticalInput * moveAmount, -1f, 2f);
            
            // Scale horizontally only if not moving backwards.
            if(verticalInput >= 0f)
                horizontalInput *= moveAmount;
        }
    }

    void HandleGroundMovement()
    {
        if (!isGrounded) return;

        Vector3 cameraForward = CameraManager.Instance.transform.forward;
        Vector3 cameraRight = CameraManager.Instance.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = cameraForward * verticalInput;
        moveDirection += cameraRight * horizontalInput;
        moveDirection.Normalize();
    }

    void HandleRotation()
    {
        if (isAiming)
        {
            Vector3 cameraForward = CameraManager.Instance.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Quaternion newRotation = Quaternion.LookRotation(cameraForward);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
        else
        {
            Vector3 cameraForward = CameraManager.Instance.transform.forward;
            Vector3 cameraRight = CameraManager.Instance.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            targetRotationDirection = Vector3.zero;
            targetRotationDirection = cameraForward * verticalInput;
            targetRotationDirection += cameraRight * horizontalInput;
            targetRotationDirection.y = 0f;

            if (targetRotationDirection == Vector3.zero)
            {
                targetRotationDirection = transform.forward;
            }

            Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
    }

    void DetermineSpeed()
    {
        if (moveAmount == 2f)
            currentMoveSpeed = sprintSpeed;
        else if (moveAmount == 1f)
            currentMoveSpeed = runSpeed;
        else if (moveAmount == 0.5f)
            currentMoveSpeed = walkSpeed;
    }

    void HandleGravity()
    {
        // If grounded, reset everything related to jumping/falling.
        if (isGrounded)
        {
            if (yVelocity.y < 0)
            {
                inAirTimer = 0;
                fallingVelocityHasBeenSet = false;
                yVelocity.y = gravity;
                isJumping = false;
            }
        }
        // If not grounded, then we are falling
        else
        {
            // If we are not not jumping, and the falling velocity hasn't been set yet, then set it.
            if (!isJumping && !fallingVelocityHasBeenSet)
            { 
                fallingVelocityHasBeenSet = true;
                yVelocity.y = gravity;
            }

            // As we are falling increase the downward velocity over time.
            inAirTimer += Time.deltaTime;
            yVelocity.y += gravity * Time.deltaTime;
        }
    }

    void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            isJumping = true;
            playerManager.animationManager.PlayTargetAnimation("Jump Start");
        }

        jumpInput = false;
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}
