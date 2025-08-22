using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionManager : MonoBehaviour
{
    private PlayerManager playerManager;
    public CharacterController characterController { get; private set; }

    [Header("Input Values")]
    public float horizontalInput { get; private set; }
    public float verticalInput {get; private set;}
    private bool sprintInput;
    private bool isAiming;

    public float moveAmount { get; private set; }
    Vector3 targetRotationDirection;
    Vector3 moveDirection;

    [Header("Speed Values")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float sprintSpeed = 6f;
    public float gravity = -10f;
    public float rotationSpeed = 15f;
    private float currentMoveSpeed;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GetInputValues();
        DetermineSpeed();
        HandleGroundMovement();
        HandleRotation();
    }

    void GetInputValues()
    {
        horizontalInput = PlayerInputManager.Instance.movementInput.x;
        verticalInput = PlayerInputManager.Instance.movementInput.y;
        sprintInput = PlayerInputManager.Instance.sprintInput;
        isAiming = playerManager.isAiming;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        if (moveAmount > 0f && moveAmount <= 0.5f)
            moveAmount = 0.5f;
        else if (moveAmount > 0.5f && moveAmount <= 1f)
            moveAmount = 1f;
        
        if (sprintInput && moveAmount > 0.1f)
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
        Vector3 cameraForward = CameraManager.Instance.transform.forward;
        Vector3 cameraRight = CameraManager.Instance.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = cameraForward * verticalInput;
        moveDirection += cameraRight * horizontalInput;
        moveDirection.Normalize();

        characterController.Move(Time.deltaTime * currentMoveSpeed * moveDirection);
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
        if (sprintInput && (!isAiming || verticalInput >= 0f))
            currentMoveSpeed = sprintSpeed;
        else if (moveAmount == 1f)
            currentMoveSpeed = runSpeed;
        else if (moveAmount == 0.5f)
            currentMoveSpeed = walkSpeed;

        playerManager.isSprinting = currentMoveSpeed == sprintSpeed;
    }
}
