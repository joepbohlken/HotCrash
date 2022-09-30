using Unity.Collections;
using Unity.Jobs;
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

    [Header("Raycast Properties")]
    [Range(1, 30)]
    [Tooltip("Number of rays being fired from the center of the wheel towards the ground. Set uneven number for one of those rays to be straight down.")]
    [SerializeField] private int rayCount = 9;
    [Range(0, 180)]
    [Tooltip("The angle on which the rays will be spread across.")]
    [SerializeField] private int discAngle = 135;

    private Rigidbody mainRb;
    private float force;
    private NativeArray<RaycastCommand> raycastCommands;
    private NativeArray<RaycastHit> raycastHits;

    private Vector3 tempPos;

    private void OnDestroy()
    {
        try { raycastCommands.Dispose(); raycastHits.Dispose(); } catch { }
    }

    private void OnDisable()
    {
        try { raycastCommands.Dispose(); raycastHits.Dispose(); } catch { }
    }

    private void Start()
    {
        mainRb = GetComponentInParent<Rigidbody>();

        // Initialize raycast commands
        if (rayCount > 0)
        {
            raycastCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.Persistent);
            raycastHits = new NativeArray<RaycastHit>(rayCount, Allocator.Persistent);

            float startAngle = rayCount == 1 ? 0 : -discAngle / 2f;
            float stepAngle = rayCount == 1 ? 0 : (float)discAngle / (float)(rayCount - 1);
            for (int i = 0; i < rayCount; i++)
            {
                //Debug.DrawRay(transform.position, Quaternion.AngleAxis(startAngle + stepAngle * i, -transform.right) * -transform.up * radius, Color.blue);
                raycastCommands[i] = new RaycastCommand(transform.position, Quaternion.AngleAxis(startAngle + stepAngle * i, -transform.right) * -transform.up, radius);
                Debug.Log(raycastCommands[i].direction);
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < rayCount; i++)
        {
            RaycastCommand tempCom = raycastCommands[i];
            tempCom.from = transform.position;
            raycastCommands[i] = tempCom;
        }

        // Cast rays
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 10);
        handle.Complete();

        float shortestDist = 999f;
        RaycastHit? shortestHit = null;
        for (int i = 0; i < rayCount; i++)
        {
            //Debug.DrawLine(raycastCommands[i].from, raycastHits[i].point, Color.black);

            if (raycastHits[i].distance < shortestDist)
            {
                shortestDist = raycastHits[i].distance;
                shortestHit = raycastHits[i];
            }
        }
        tempPos = shortestHit.Value.point;

        float offset = 0f;

        if (shortestHit != null)
        {
            // Calculate suspension
            offset = radius - shortestDist;
            float velocity = Vector3.Dot(transform.up, mainRb.GetPointVelocity(transform.position));
            force = (offset * strength) - (velocity * damping);

            mainRb.AddForceAtPosition(shortestHit.Value.normal * force, transform.position);
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

        Handles.color = Color.black;
        for (int i = 0; i < raycastHits.Length; i++)
        {
            Handles.DrawSolidDisc(raycastHits[i].point, transform.right, 0.03f);
        }
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(tempPos, transform.right, 0.04f);
    }
#endif
}
