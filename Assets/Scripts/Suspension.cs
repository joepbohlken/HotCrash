using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    private Rigidbody carRigidbody;
    public Wheel wheel;

    [Header("Spring")]
    public float restLength;
    public float travelDist;
    [Range(0, 1)]
    public float compressionRatio = 0;
    [Min(0)]
    public float springStrength;
    [Min(0)]
    public float springDamping;

    [NonSerialized]
    public float compression;
    private float relaxRate;
    private float suspensionForce;

    private void Start()
    {
        carRigidbody = GetComponentInParent<Rigidbody>();
        compression = travelDist * compressionRatio;
    }


    private void FixedUpdate()
    {
        CalculateSuspension();
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

        float velocity = Vector3.Dot(transform.up, carRigidbody.GetPointVelocity(transform.position));

        suspensionForce = (compression * springStrength) - (velocity * springDamping);

        carRigidbody.AddForceAtPosition(transform.up * suspensionForce, wheel.transform.position);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Handles.color = Color.magenta;
            Handles.DrawWireDisc(transform.position + -transform.up * (restLength - travelDist * compressionRatio), transform.right, wheel.radius);
        }

        Handles.color = Color.green;
        Handles.DrawLine(transform.position, transform.position + transform.up * Mathf.Clamp(suspensionForce / 10, -10, 10), 5f);
    }
#endif
}
