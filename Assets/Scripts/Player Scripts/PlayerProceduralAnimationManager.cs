using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerProceduralAnimationManager : MonoBehaviour
{
    PlayerManager playerManager;

    [Header("Right Arm IK References")]
    public Transform handTarget;
    public Transform shoulderTransform; // to ensure the arm ik is shoulder height.
    public TwoBoneIKConstraint rightArmIK;

    // left arm ik in the future


    [Header("Settings")]
    public float handDistanceFromShoulder = 0.5f;
    float rotationSpeed = 15;
    float targetSmoothTime = 0.2f;
    
    Vector3 targetVelocity;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        rightArmIK.weight = 0;
    }

    private void LateUpdate()
    {
        HandleRightArmAimIK();
    }

    void HandleRightArmAimIK()
    {
        Vector3 targetPoint = CameraManager.Instance.GetAimTargetPoint();
        Vector3 targetDirection = (targetPoint - shoulderTransform.position).normalized;
        Vector3 targetPosition = shoulderTransform.position + (targetDirection * handDistanceFromShoulder);

        handTarget.position = Vector3.SmoothDamp(handTarget.position, targetPosition, ref targetVelocity, targetSmoothTime);

        targetDirection = (targetPoint - handTarget.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        Quaternion offset = Quaternion.Euler(90, -90, 0);
        handTarget.rotation = Quaternion.Slerp(handTarget.rotation, targetRotation * offset, Time.deltaTime * rotationSpeed);

        if (playerManager.isAiming)
        {
            rightArmIK.weight += Time.deltaTime / 0.25f;
        }
        else
        {
            rightArmIK.weight -= Time.deltaTime / 0.25f;
        }
    }
}
