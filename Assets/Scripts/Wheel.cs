using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public Transform wheel;

    [Header("Wheel")]
    public float wheelRadius;

    public bool wheelFrontLeft;
    public bool wheelFrontRight;
    public bool wheelRearLeft;
    public bool wheelRearRight;


    [Header("Suspension")]
    [Tooltip("Represents the strength of the spring. The higher the value, the more weight it can hold.")]
    public float springStrength;
    [Tooltip("Represents the spring damper. The higher the value, the more it wants to slow down the spring movement.")]
    public float springDamper;
    [Tooltip("Represents the resting distance of the spring, meaning how long it is in its neutral position.")]
    public float suspensionRestDist;
    [Tooltip("Represents the max travel distance the spring can go, meaning how long the offset is from the resting position.")]
    public float springTravel;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;
    private float springVelocity;
    private float springForce;
    private float damperForce;

    [HideInInspector]
    public float compressionRatio;

    private Vector3 suspensionForce;

    [Header("Steering")]
    public float steerAngle;
    public float steerTime;

    private float wheelAngle;

    [Header("Friction")]
    [Tooltip(
        "Multiplier for the wheels forward friction." +
        " Values below 1 will cause the wheel to slip (like ice)" +
        " Values above 1 will cause the wheel to have high traction (and thus higher speed)"
    )]
    public float forwardGrip = 1;
    [Tooltip(
        "Multiplier for the wheels sideways friction." +
        " Values below 1 will cause the wheel to drift" +
        " Values above 1 will cause sharp turns"
    )]
    public float sidewaysGrip = 1;
    [Tooltip(
        "Multiplier for the wheels sideways friction. " +
        " Values below 1 cause the wheel to skid" +
        " Values above 1 will cause the wheel to take sharper turns"
    )]
    public float rollingFriction = .5f;

    public RaycastHit hit;
    private bool rayDidHit;

    public bool isGrounded;

    private void Start()
    {
        minLength = suspensionRestDist - springTravel;
        maxLength = suspensionRestDist + springTravel;
    }

    private void FixedUpdate()
    {
        CalculateSteering();
        CalculateSuspension();
        CalculateFriction();
    }

    private void CalculateSteering()
    {
        rayDidHit = Physics.Raycast(transform.position, -transform.up, out hit, maxLength + wheelRadius);
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        wheel.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    private void CalculateSuspension()
    {
        compressionRatio = Mathf.Clamp01((-hit.point.y - wheelRadius) / suspensionRestDist);
        Debug.Log(compressionRatio);

        if (!rayDidHit)
        {
            wheel.position = transform.position - transform.up * maxLength;
            return;
        }

        isGrounded = true;
        lastLength = springLength;
        springLength = Mathf.Clamp(hit.distance - wheelRadius, minLength, maxLength);
        springForce = springStrength * (suspensionRestDist - springLength);
        springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

        suspensionForce = springForce * transform.up;
        damperForce = springDamper * springVelocity;

        suspensionForce = (springForce + damperForce) * transform.up;

        carRigidbody.AddForceAtPosition(suspensionForce, hit.point);

        wheel.position = hit.point + transform.up * wheelRadius;
    }

    private void CalculateFriction()
    {
        if(!rayDidHit)
        {
            return;
        }

        Vector3 velocity = carRigidbody.GetPointVelocity(hit.point);

        // Contact basis (can be different from wheel basis)
        Vector3 normal = hit.normal;// Hit.normal;
        Vector3 side = transform.right;
        Vector3 forward = transform.forward;

        // Apply less force if the vehicle is tilted
        var angle = Vector3.Angle(normal, transform.up);
        float multiplier = Mathf.Cos(angle * Mathf.Deg2Rad);

        // Calculate sliding velocity (velocity without normal force)
        Vector3 sideVel = Vector3.Dot(velocity, side) * side * multiplier;
        Vector3 forwardVel = Vector3.Dot(velocity, forward) * forward * multiplier;
        Vector3 velocity2D = sideVel + forwardVel;

        // contact forward normal
        Vector3 momentum = velocity2D;// * rigidbody.mass;

        Vector3 latForce = Vector3.Dot(-momentum, side) * side * sidewaysGrip;
        Vector3 longForce = Vector3.Dot(-momentum, forward) * forward * forwardGrip;
        Vector3 frictionForce = Vector3.zero;

        frictionForce += longForce;
        frictionForce += latForce;

        // Apply rolling friction
        longForce *= 1 - rollingFriction;

        // Apply brake force
        var brakeForceMag = 0 / wheelRadius;
        brakeForceMag = Mathf.Clamp(brakeForceMag, 0, longForce.magnitude);
        longForce -= longForce.normalized * brakeForceMag;
        frictionForce -= longForce;

        carRigidbody.AddForceAtPosition(frictionForce * carRigidbody.mass, hit.point); //Hit.point
        carRigidbody.AddForceAtPosition(forward * Input.GetAxis("Vertical") * 500 / wheelRadius * forwardGrip, hit.point);//Hit.point
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, -transform.up * springLength, Color.blue);

        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position + (-transform.up * suspensionRestDist), transform.right, wheelRadius);
    }
#endif
}
