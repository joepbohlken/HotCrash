using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering : MonoBehaviour
{
    private CarController carController;
    public float steerRate = .15f;
    private float steerAmount;

    [Tooltip("Curve for limiting steer range based on speed, x-axis = speed, y-axis = multiplier")]
    public AnimationCurve steerCurve = AnimationCurve.Linear(0, 1, 30, 0.1f);
    public bool limitSteer = true;

    [Tooltip("Horizontal stretch of the steer curve")]
    public float steerCurveStretch = 1;
    public bool applyInReverse = true; // Limit steering in reverse?
    public Suspension[] steeredWheels;

    [Header("Visual")]
    public bool rotate;
    public float maxDegreesRotation;
    public float rotationOffset;
    private float steerRot;

    // Start is called before the first frame update
    private void Start()
    {
        carController = transform.GetComponentInParent<CarController>();
        steerRot = rotationOffset;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float rbSpeed = carController.localVelocity.z / steerCurveStretch;
        float steerLimit = limitSteer ? steerCurve.Evaluate(applyInReverse ? Mathf.Abs(rbSpeed) : rbSpeed) : 1;
        steerAmount = carController.steerInput * steerLimit;

        // Set steer angles in wheels
        foreach (Suspension suspension in steeredWheels)
        {
            suspension.steerAngle = Mathf.Lerp(suspension.steerAngle, steerAmount * suspension.steerFactor, steerRate * Time.timeScale);
        }
    }

    private void Update()
    {
        // Visual steering wheel rotation
        if (rotate)
        {
            steerRot = Mathf.Lerp(steerRot, steerAmount * maxDegreesRotation + rotationOffset, steerRate * Time.timeScale);
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, steerRot);
        }
    }
}
