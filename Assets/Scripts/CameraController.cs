using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraObject;

    [Header("Camera Properties")]
    [SerializeField] private float startAngle = 10f;
    [SerializeField] private Vector3 startOffset;
    [SerializeField] private float fullSpeedAngle = 10f;
    [SerializeField] private Vector3 fullSpeedOffset;
    [SerializeField] private float fullSpeedThreshold = 40f;
    [SerializeField] private bool useVelocityInAir = false;

    [Space(12)]
    public LayerMask raycastLayerMask;
    public Transform target;

    [HideInInspector]
    public float x;
    [HideInInspector]
    public float y;

    private CarController carController;
    private float angleY = 0f;
    private bool hasCarTouchedGroundAtLeastOnce = false;

    private void OnValidate()
    {
        ResetPosition();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        ResetPosition();
    }

    private void LateUpdate()
    {
        if (!carController || !target) return;
        if (carController.isGrounded) hasCarTouchedGroundAtLeastOnce = true;

        float currentAngle = Mathf.Lerp(startAngle, fullSpeedAngle, 1f / fullSpeedThreshold * carController.currentSpeed);
        Vector3 currentOffset = Vector3.Lerp(startOffset, fullSpeedOffset, 1f / fullSpeedThreshold * carController.currentSpeed);

        Vector3 newPosition = currentOffset;

        // Handle camera collision
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.TransformPoint(newPosition), out hit, raycastLayerMask))
        {
            newPosition.z = -hit.distance;
        }

        cameraObject.localPosition = Vector3.Lerp(cameraObject.localPosition, newPosition, 0.35f);

        if (y == -1f)
        {
            angleY = 180;
        }
        else
        {
            float speed = 750f;
            if (x == 1f) angleY = Mathf.Clamp(angleY + speed * Time.deltaTime, 0f, 90f);
            else if (x == -1f) angleY = Mathf.Clamp(angleY - speed * Time.deltaTime, -90f, 0f);
            else if (Mathf.Abs(angleY) > 5f && angleY != 180)
            {
                angleY += speed * Time.deltaTime * Mathf.Sign(-angleY);
            }
            else angleY = 0;
        }

        float newAngle;
        if ((carController.isGrounded || !hasCarTouchedGroundAtLeastOnce) || carController.isDestroyed || !useVelocityInAir) newAngle = angleY + target.eulerAngles.y;
        else newAngle = angleY + Quaternion.LookRotation(carController.rb.velocity.normalized, Vector3.up).eulerAngles.y;
        Vector3 cameraRotation = new Vector3(currentAngle, newAngle, 0);
        transform.eulerAngles = cameraRotation;

        transform.position = target.position;
    }

    public void ResetPosition()
    {
        if (target == null || cameraObject == null)
        {
            return;
        }

        cameraObject.localPosition = startOffset;

        transform.position = target.position;
        Vector3 cameraRotation = new Vector3(startAngle, angleY + target.eulerAngles.y, 0);
        transform.eulerAngles = cameraRotation;
    }

    public void SetCar(CarController carController)
    {
        this.carController = carController;
    }
}
