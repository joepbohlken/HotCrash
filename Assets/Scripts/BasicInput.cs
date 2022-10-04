using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInput : MonoBehaviour
{
    CarController carController;
    public string accelAxis;
    public string brakeAxis;
    public string steerAxis;
    public string handbrakeAxis;

    void Start()
    {
        carController = GetComponent<CarController>();
    }

    void FixedUpdate()
    {
        // Get constant inputs
        if (!string.IsNullOrEmpty(accelAxis))
        {
            carController.SetAccel(Input.GetAxis(accelAxis));
        }

        if (!string.IsNullOrEmpty(brakeAxis))
        {
            carController.SetBrake(Input.GetAxis(brakeAxis));
        }

        if (!string.IsNullOrEmpty(steerAxis))
        {
            carController.SetSteer(Input.GetAxis(steerAxis));
        }

        if (!string.IsNullOrEmpty(handbrakeAxis))
        {
            carController.SetHandbrake(Input.GetAxis(handbrakeAxis));
        }
    }
}
