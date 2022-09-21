using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public Wheel[] wheels;

    [Header("Car specs")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;

    [Header("Inputs")]
    public float steerInput;

    public float ackermannAngleLeft;
    public float ackermannAngleRight;

    void Start()
    {

    }

    void Update()
    {
        steerInput = Input.GetAxis("Horizontal");

        if (steerInput > 0) // is turning right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else if (steerInput < 0) // is turning left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach (Wheel wheel in wheels)
        {
            if (wheel.wheelFrontLeft)
            {
                wheel.steerAngle = ackermannAngleLeft;
            }
            if (wheel.wheelFrontRight)
            {
                wheel.steerAngle = ackermannAngleRight;
            }
        }
    }
}
