using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public GameObject followTarget;
    public Camera cameraObject;

    [Header("Camera Settings")]
    public float cameraSmoothSpeed = 1f;
    public float rotationSpeed = 110f;
    public float minimumPivot = -89f;
    public float maximumPivot = 89f;
    public LayerMask cameraCollisionLayers;
    public LayerMask aimCollisionLayers;

    [Header("Camera Values")]
    Vector3 cameraVelocity;
    Vector3 cameraObjectPosition;
    float horizontalLookAngle;  //left and right angle
    float verticalLookAngle;    //up and down angle
    float horizontalLookInput;
    float verticalLookInput;
    float cameraCollisionRadius = 0.2f;
    float defaultCameraZPosition;
    float targetCameraZPosition;

    //over the shoulder offset.
    //Will add the ability to toggle between left and right shoulder.

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        cameraObject = GetComponentInChildren<Camera>();
        cameraObjectPosition = cameraObject.transform.localPosition;
        defaultCameraZPosition = cameraObject.transform.localPosition.z;
    }

    private void Update()
    {
        GetCameraInput();
    }

    private void LateUpdate()
    {
        if (followTarget != null)
        {
            FollowTarget();
            RotateAroundTarget();
            HandleCameraCollisions();
        }
    }

    void GetCameraInput()
    {
        verticalLookInput = PlayerInputManager.Instance.lookInput.y;
        horizontalLookInput = PlayerInputManager.Instance.lookInput.x;
    }

    void FollowTarget()
    {
        Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, followTarget.transform.position, ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
        transform.position = targetCameraPosition;
    }

    void RotateAroundTarget()
    {
        horizontalLookAngle += (horizontalLookInput * rotationSpeed) * Time.deltaTime;
        verticalLookAngle -= (verticalLookInput * rotationSpeed) * Time.deltaTime;

        verticalLookAngle = Mathf.Clamp(verticalLookAngle, minimumPivot, maximumPivot);

        Vector3 cameraRotation = Vector3.zero;
        Quaternion targetRotation;

        cameraRotation.y = horizontalLookAngle;
        cameraRotation.x = verticalLookAngle;
        targetRotation = Quaternion.Euler(cameraRotation);
        transform.rotation = targetRotation;
    }

    void HandleCameraCollisions()
    {
        targetCameraZPosition = defaultCameraZPosition;

        RaycastHit hit;
        Vector3 direction = cameraObject.transform.position - transform.position;
        direction.Normalize();

        if (Physics.SphereCast(transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCameraZPosition), cameraCollisionLayers))
        {
            float distanceFromHitObject = Vector3.Distance(transform.position, hit.point);
            targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);
        }

        if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius)
        {
            targetCameraZPosition = 0f;
        }

        cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
        cameraObject.transform.localPosition = cameraObjectPosition;
    }

    public Vector3 GetAimTargetPoint()
    {
        Ray ray = cameraObject.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f, aimCollisionLayers)) // 100 units max
        {
            targetPoint = hit.point;
        }
        else
        {
            // if nothing hit, aim at some point far away along the ray
            targetPoint = ray.origin + ray.direction * 100f;
        }

        return targetPoint;
    }
}