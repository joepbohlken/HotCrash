using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
    [SerializeField] private WheelCollider wheelR;
    [SerializeField] private WheelCollider wheelL;
    [SerializeField] private float AntiRoll = 5000.0f;
    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        WheelHit hit = new WheelHit();
        float travelL = 1.0f;
        float travelR = 1.0f;

        var groundedL = wheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;

        var groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

        var antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            rigidbody.AddForceAtPosition(wheelL.transform.up * -antiRollForce,
                   wheelL.transform.position);
            rigidbody.AddForceAtPosition(wheelL.transform.up * -antiRollForce,
                   wheelL.transform.position);
        if (groundedR)
            rigidbody.AddForceAtPosition(wheelR.transform.up * antiRollForce,
                   wheelR.transform.position);
    }

}
