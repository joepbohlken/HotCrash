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

    private Vector3 currentRotation = Vector3.zero;

    private void OnValidate()
    {
        ResetPosition();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        Vector2 mouseAxis = new Vector2(-Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mouseAxis.x != 0 || mouseAxis.y != 0)
        {
            currentRotation -= new Vector3(mouseAxis.x * mouseSensitivity, mouseAxis.y * mouseSensitivity, 0);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -50, 60);
            Debug.Log(currentRotation);

            Quaternion cameraQuaternion = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
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
