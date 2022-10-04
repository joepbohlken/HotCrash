using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    private CarController carController;
    private Rigidbody carRigidbody;
    private DriveForce targetDrive;
    public Wheel wheel;

    // Variables for inverting certain values on opposite sides of the vehicle
    [NonSerialized]
    public bool flippedSide;
    [NonSerialized]
    public float flippedSideFactor;
    [NonSerialized]
    public Quaternion initialRotation;

    [Header("Spring")]
    public float restLength;
    public float travelDist;
    [Range(0, 1)]
    public float compressionRatio = 0;
    [Min(0)]
    public float springStrength;
    [Min(0)]
    public float springDamping;

    [Header("Brakes and Steering")]
    public float brakeForce;
    public float ebrakeForce;
    [Range(-180, 180)]
    public float steerRangeMin;
    [Range(-180, 180)]
    public float steerRangeMax;
    [Tooltip("How much the wheel is steered")]
    public float steerFactor = 1;
    [Range(-1, 1)]
    public float steerAngle;
    [NonSerialized]
    public float steerDegrees;
    [Tooltip("Effect of Ackermann steering geometry")]
    public float ackermannFactor;

    [NonSerialized]
    public float compression;
    private float relaxRate;
    private float suspensionForce;

    private void Start()
    {
        carController = GetComponentInParent<CarController>();
        carRigidbody = GetComponentInParent<Rigidbody>();
        targetDrive = GetComponent<DriveForce>();
        compression = travelDist * compressionRatio;

        flippedSide = transform.position.x < carController.transform.position.x;
        flippedSideFactor = flippedSide ? -1 : 1;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // Set steer angle for the wheel
        steerDegrees = Mathf.Abs(steerAngle) * (steerAngle > 0 ? steerRangeMax : steerRangeMin);
    }

    private void FixedUpdate()
    {
        //Debug.Log("rpm: " + targetDrive.rpm + " | torque: " + targetDrive.torque);

        CalculateSuspension();

        carRigidbody.AddForceAtPosition(transform.forward * targetDrive.rpm / 1000, wheel.transform.position);
    }

    private void CalculateSuspension()
    {
        if (!wheel.isGrounded)
        {
            if (compression > 0)
            {
                relaxRate += Time.fixedDeltaTime;
                compression = Mathf.Lerp(compression, 0, relaxRate);
                compressionRatio = Mathf.Clamp01(compression / travelDist);
            }

            return;
        }

        relaxRate = 0;
        compression = restLength - Vector3.Distance(wheel.transform.position, transform.position);
        compressionRatio = Mathf.Clamp01(compression / travelDist);

        float velocity = Vector3.Dot(transform.up, carRigidbody.GetPointVelocity(wheel.transform.position));

        suspensionForce = (compression * springStrength) - (velocity * springDamping);

        carRigidbody.AddForceAtPosition(transform.up * suspensionForce, wheel.transform.position);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            //Handles.color = Color.magenta;
            //Handles.DrawWireDisc(transform.position + -transform.up * (restLength - travelDist * compressionRatio), transform.right, wheel.radius);

            float susForce = Mathf.Clamp(suspensionForce / 10, -10, 10);
            Handles.color = Color.green;
            Handles.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.up * (susForce == 0 ? .2f : susForce), 5f);

            Handles.color = Color.red;
            Handles.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.right * .2f, 5f);

            Handles.color = Color.blue;
            Handles.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.forward * .2f, 5f);
        }
    }
#endif
}
