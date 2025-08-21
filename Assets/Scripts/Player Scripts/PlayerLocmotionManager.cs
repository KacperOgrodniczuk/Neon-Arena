using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocmotionManager : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Input Values")]
    private float horizontalInput;
    private float verticalInput;
    Vector3 targetRotationDirection;
    Vector3 moveDirection;

    [Header("Speed Values")]
    public float moveSpeed = 6f;
    public float gravity = -10f;
    public float rotationSpeed = 15f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GetInputValues();

        HandleGroundMovement();
        HandleRotation();
    }

    void GetInputValues()
    {
        horizontalInput = PlayerInputManager.Instance.movementInput.x;
        verticalInput = PlayerInputManager.Instance.movementInput.y;
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

        characterController.Move(Time.deltaTime * moveSpeed * moveDirection);
    }

    void HandleRotation()
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
