using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraObject;

    [Header("Camera Properties")]
    [SerializeField] private float angle = 10f;
    [SerializeField] private Vector3 offset;
    public Transform target;

    private Vector3 currentRotation = Vector3.zero;

    [HideInInspector]
    public float x;
    [HideInInspector]
    public float y;

    private float angleY = 0;

    private void OnValidate()
    {
        ResetPosition();
    }

    private void Start()
    {
        currentRotation = transform.localEulerAngles;
        Cursor.lockState = CursorLockMode.Locked;

        ResetPosition();
    }

    private void LateUpdate()
    {
        Vector3 newPosition = offset;

        // Handle camera collision
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.TransformPoint(newPosition), out hit))
        {
            newPosition.z = -hit.distance;
        }

        cameraObject.localPosition = Vector3.Lerp(cameraObject.localPosition, newPosition, 0.35f);

        // Set camera rotation

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
        Vector3 cameraRotation = new Vector3(angle, angleY + target.eulerAngles.y, 0);
        transform.eulerAngles = cameraRotation;

        transform.position = target.position;
    }

    public void ResetPosition()
    {
        if (target == null || cameraObject == null)
        {
            return;
        }

        cameraObject.localPosition = offset;

        transform.position = target.position;
        Vector3 cameraRotation = new Vector3(angle, angleY + target.eulerAngles.y, 0);
        transform.eulerAngles = cameraRotation;
    }
}
