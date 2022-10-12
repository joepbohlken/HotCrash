using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform cameraObject;

    [Header("Camera Properties")]
    [SerializeField] private float mouseSensitivity = 6f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;

    private bool isReady = false;
    private Vector3 currentRotation = Vector3.zero;

    private void OnValidate()
    {
        ResetPosition();
    }

    private void Start()
    {
        currentRotation = transform.localEulerAngles;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnApplicationFocus(bool focus)
    {
        isReady = focus;
    }

    private void LateUpdate()
    {
        if(!isReady)
        {
            return;
        }

        Vector3 newPosition = offset;

        // Handle camera collision
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.TransformPoint(newPosition), out hit))
        {
            newPosition.z = -hit.distance;
        }

        cameraObject.localPosition = Vector3.Lerp(cameraObject.localPosition, newPosition, 0.35f);

        // Set camera rotation
        Vector2 mouseAxis = new Vector2(-Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mouseAxis.x != 0 || mouseAxis.y != 0)
        {
            currentRotation -= new Vector3(mouseAxis.y * mouseSensitivity, mouseAxis.x * mouseSensitivity, 0);
            currentRotation.x = Mathf.Clamp(currentRotation.x, -50, 60);

            Quaternion cameraQuaternion = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
            transform.rotation = cameraQuaternion;
        }

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
        Quaternion cameraQuaternion = Quaternion.Euler(0, target.parent.localEulerAngles.y, 0);
        transform.rotation = cameraQuaternion;
    }
}