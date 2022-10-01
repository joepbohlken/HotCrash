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
    public string upshiftButton;
    public string downshiftButton;

    void Start()
    {
        carController = GetComponent<CarController>();
    }

    void Update()
    {
        // Get single-frame input presses
        if (!string.IsNullOrEmpty(upshiftButton))
        {
            if (Input.GetButtonDown(upshiftButton))
            {
                //carController.PressUpshift();
            }
        }

        if (!string.IsNullOrEmpty(downshiftButton))
        {
            if (Input.GetButtonDown(downshiftButton))
            {
                //carController.PressDownshift();
            }
        }
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

        if (!string.IsNullOrEmpty(upshiftButton))
        {
            carController.SetUpshift(Input.GetAxis(upshiftButton));
        }

        if (!string.IsNullOrEmpty(downshiftButton))
        {
            carController.SetDownshift(Input.GetAxis(downshiftButton));
        }
    }
}
