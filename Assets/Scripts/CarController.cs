using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rigidbody;

    [NonSerialized]
    public Vector3 localVelocity;

    [NonSerialized]
    public float accelInput;
    [NonSerialized]
    public float brakeInput;
    [NonSerialized]
    public float steerInput;
    [NonSerialized]
    public float handbrakeInput;

    [Tooltip("Automatically hold ebrake if it's pressed while parked")]
    public bool holdEbrakePark;

    public float burnoutThreshold = 0.9f;
    [System.NonSerialized]
    public float burnout;
    public float burnoutSpin = 5;
    [Range(0, 0.9f)]
    public float burnoutSmoothness = 0.5f;
    public Engine engine;

    [NonSerialized]
    public float velMag;

    [NonSerialized]
    public bool reversing;

    public Wheel[] wheels;
    [NonSerialized]
    public int groundedWheels; // Number of wheels grounded

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        velMag = rigidbody.velocity.magnitude;

        GetGroundedWheels();
    }

    void GetGroundedWheels()
    {
        groundedWheels = 0;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].isGrounded)
            {
                groundedWheels++;
            }
        }
    }

    // Set accel input
    public void SetAccel(float f)
    {
        f = Mathf.Clamp(f, -1, 1);
        accelInput = f;
    }

    // Set brake input
    public void SetBrake(float f)
    {
        brakeInput = Mathf.Clamp(f, -1, 1);
    }

    // Set steer input
    public void SetSteer(float f)
    {
        steerInput = Mathf.Clamp(f, -1, 1);
    }

    // Set handbrake input
    public void SetHandbrake(float f)
    {
        if ((f > 0 || handbrakeInput > 0) && velMag < 1 && accelInput == 0 && brakeInput == 0)
        {
            handbrakeInput = 1;
        }
        else
        {
            handbrakeInput = Mathf.Clamp01(f);
        }
    }
}
