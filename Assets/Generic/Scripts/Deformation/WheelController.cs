using UnityEditor;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] private Transform wheelObj;

    [Header("Wheel Properties")]
    [SerializeField] private float radius = 0.5f;

    [Header("Suspension Properties")]
    [SerializeField] private float strength = 100f;
    [SerializeField] private float damping = 10f;

    private Rigidbody mainRb;
    private float force;

    private void Start()
    {
        mainRb = GetComponentInParent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float offset = 0f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, radius))
        {
            // Calculate suspension
            offset = radius - hit.distance;
            float velocity = Vector3.Dot(transform.up, mainRb.GetPointVelocity(transform.position));
            force = (offset * strength) - (velocity * damping);

            mainRb.AddForceAtPosition(transform.up * force, transform.position);
        }

        // Update wheel object position
        Vector3 curPos = wheelObj.transform.localPosition;
        wheelObj.transform.localPosition = new Vector3(curPos.x, offset, curPos.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, transform.right, radius);
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(transform.position + Vector3.down * radius, transform.right, 0.05f);

        Handles.color = Color.red;
        Handles.DrawLine(transform.position, transform.position + transform.up * force / 100f, 5f);
    }
#endif
}
