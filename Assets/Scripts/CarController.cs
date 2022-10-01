using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rigidbody;

    [NonSerialized]
    public float accelInput;
    [NonSerialized]
    public float brakeInput;
    [NonSerialized]
    public float steerInput;
    [NonSerialized]
    public float handbrakeInput;
    [NonSerialized]
    public bool upshiftPressed;
    [NonSerialized]
    public bool downshiftPressed;
    [NonSerialized]
    public float upshiftHold;
    [NonSerialized]
    public float downshiftHold;

    [NonSerialized]
    public float velMag;

    private bool stopUpshift;
    private bool stopDownShift;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Shift single frame pressing logic
        if (stopUpshift)
        {
            upshiftPressed = false;
            stopUpshift = false;
        }

        if (stopDownShift)
        {
            downshiftPressed = false;
            stopDownShift = false;
        }

        if (upshiftPressed)
        {
            stopUpshift = true;
        }

        if (downshiftPressed)
        {
            stopDownShift = true;
        }
    }

    private void FixedUpdate()
    {
        velMag = rigidbody.velocity.magnitude;
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

    // Do upshift input
    public void PressUpshift()
    {
        upshiftPressed = true;
    }

    // Do downshift input
    public void PressDownshift()
    {
        downshiftPressed = true;
    }

    // Set held upshift input
    public void SetUpshift(float f)
    {
        upshiftHold = f;
    }

    // Set held downshift input
    public void SetDownshift(float f)
    {
        downshiftHold = f;
    }
}
