using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    public float carHeight = .5f;
    public float suspensionRestDist = .25f;
    public float springStrength = 100f;
    public float springDamper = 10f;

    public Rigidbody carRigidBody;

    // Update is called once per frame
    void Update()
    {
        /* Debug  */
        Vector3 raycastDebugDirection = transform.TransformDirection(Vector3.down) * carHeight;
        Debug.DrawRay(transform.position, raycastDebugDirection, Color.green);
        /* Debug */

        Transform tireTransform = transform;

        RaycastHit tireRay;
        bool rayDidHit = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out tireRay, carHeight, Physics.AllLayers);

        Debug.Log(rayDidHit);
        if(rayDidHit)
        {
            Vector3 springDir = tireTransform.up;
            Vector3 tireWordVel = carRigidBody.GetPointVelocity(tireTransform.position);

            float offset = suspensionRestDist - tireRay.distance;
            float vel = Vector3.Dot(springDir, tireWordVel);

            float force = (offset * springStrength) - (vel * springDamper);

            carRigidBody.AddForceAtPosition(springDir * force, tireTransform.position);
        }
    }
}
