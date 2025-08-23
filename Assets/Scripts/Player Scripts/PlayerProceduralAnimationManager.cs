using FishNet.Object;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerProceduralAnimationManager : NetworkBehaviour
{
    PlayerManager playerManager;

    [Header("Right Arm IK References")]
    public Transform handTarget;
    public Transform shoulderTransform; // to ensure the arm ik is shoulder height.
    public TwoBoneIKConstraint rightArmIK;

    Vector3 targetDirection;
    Vector3 targetVelocity;
    float targetIkWeight = 0f;

    [Header("Settings")]
    public float handDistanceFromShoulder = 0.5f;
    float rotationSpeed = 15;
    float targetSmoothTime = 0.2f;

    [Header("Network Variables")]
    Vector3 syncedAimDirection = Vector3.forward;
    float syncedIKWeight = 0f;


    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        rightArmIK.weight = 0;
    }

    private void LateUpdate()
    {
        CalculateAimVariables();

        if (IsOwner)
        {
            HandleRightArmAimIK(targetDirection, targetIkWeight);
            SubmitAimServerRpc(targetDirection, targetIkWeight);
        }
        else if (!IsOwner)
        {
            HandleRightArmAimIK(syncedAimDirection, syncedIKWeight);
        }
    }

    void CalculateAimVariables()
    {
        Vector3 targetPoint = CameraManager.Instance.GetAimTargetPoint();
        targetDirection = (targetPoint - shoulderTransform.position).normalized;
        targetIkWeight = playerManager.isAiming ? 1f : 0f;
    }

    void HandleRightArmAimIK(Vector3 aimDirection, float ikWeight)
    {
        Vector3 targetPosition = shoulderTransform.position + (aimDirection * handDistanceFromShoulder);
        handTarget.position = Vector3.SmoothDamp(handTarget.position, targetPosition, ref targetVelocity, targetSmoothTime);

        Quaternion targetRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
        Quaternion offset = Quaternion.Euler(90, -90, 0);
        handTarget.rotation = Quaternion.Slerp(handTarget.rotation, targetRotation * offset, Time.deltaTime * rotationSpeed);

        rightArmIK.weight = Mathf.MoveTowards(rightArmIK.weight, ikWeight, Time.deltaTime * 4f);
    }

    [ServerRpc]
    private void SubmitAimServerRpc(Vector3 aimDir, float ikWeight)
    {
        BroadcastAimObserversRpc(aimDir, ikWeight);
    }

    [ObserversRpc(BufferLast = true)]
    private void BroadcastAimObserversRpc(Vector3 aimDir, float ikWeight)
    {
        if (IsOwner) return;
        syncedAimDirection = aimDir;
        syncedIKWeight = ikWeight;
    }
}
