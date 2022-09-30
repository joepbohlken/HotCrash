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
    private float compression;
    [Range(0, 1)]
    public float compressionRatio = 0;
    [Min(0)]
    public float springStrength;
    [Min(0)]
    public float springDamping;

    private float relaxRate;
    private float suspensionForce;

    // Start is called before the first frame update
    private void Start()
    {
        carRigidbody = GetComponentInParent<Rigidbody>();

        compression = travelDist * compressionRatio;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        GetRaycastInfo();

        CalculateSuspension();
    }

    private void GetRaycastInfo()
    {

    }

    private void CalculateSuspension()
    {
        if (!wheel.averageOutput.hasHit)
        {
            if(compression > 0)
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

        carRigidbody.AddForceAtPosition(transform.up * suspensionForce, transform.position);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        var p1 = transform.position;
        var p2 = transform.position - transform.up * restLength;
        Handles.DrawLine(p1, p2);

        Handles.color = Color.red;
        Handles.DrawLine(transform.position, transform.position + transform.up * suspensionForce / 100f, 5f);
    }
#endif
}
