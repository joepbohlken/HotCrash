using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    private CarController carController;
    public bool ignition;
    public float power = 1;

    [Tooltip("Throttle curve, x-axis = input, y-axis = output")]
    public AnimationCurve inputCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    protected float actualInput; // Input after applying the input curve

    protected AudioSource snd;

    [Header("Engine Audio")]

    public float minPitch;
    public float maxPitch;
    [System.NonSerialized]
    public float targetPitch;
    protected float pitchFactor;
    protected float airPitch;

    [Header("Damage")]
    [Range(0, 1)]
    public float strength = 1;
    [System.NonSerialized]
    public float health = 1;

    [Header("Performance")]

    [Tooltip("X-axis = RPM in thousands, y-axis = torque.  The rightmost key represents the maximum RPM")]
    public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0, 0, 8, 1);

    [Range(0, 0.99f)]
    [Tooltip("How quickly the engine adjusts its RPMs")]
    public float inertia;

    [Tooltip("Can the engine turn backwards?")]
    public bool canReverse;
    DriveForce targetDrive;
    [System.NonSerialized]
    public float maxRPM;

    public DriveForce[] outputDrives;

    [Tooltip("Exponent for torque output on each wheel")]
    public float driveDividePower = 3;
    float actualAccel;

    [Header("Transmission")]

    public Transmission transmission;
    [System.NonSerialized]
    public bool shifting;

    [Tooltip("Increase sound pitch between shifts")]
    public bool pitchIncreaseBetweenShift;

    public void Start()
    {
        carController = transform.GetComponentInParent<CarController>();

        targetDrive = GetComponent<DriveForce>();
        // Get maximum possible RPM
        GetMaxRPM();
    }

    public void FixedUpdate()
    {
        health = Mathf.Clamp01(health);

        // Calculate proper input
        actualAccel = Mathf.Lerp(carController.reversing && carController.accelInput <= 0 ? carController.brakeInput : carController.accelInput, Mathf.Max(carController.accelInput, carController.burnout), carController.burnout);
        float accelGet = canReverse ? actualAccel : Mathf.Clamp01(actualAccel);
        actualInput = inputCurve.Evaluate(Mathf.Abs(accelGet)) * Mathf.Sign(accelGet);
        targetDrive.curve = torqueCurve;

        if (ignition)
        {
            // Set RPM
            targetDrive.rpm = Mathf.Lerp(targetDrive.rpm, actualInput * maxRPM * 1000, (1 - inertia) * Time.timeScale);
            // Set torque
            if (targetDrive.feedbackRPM > targetDrive.rpm)
            {
                targetDrive.torque = 0;
            }
            else
            {
                targetDrive.torque = torqueCurve.Evaluate(targetDrive.feedbackRPM * 0.001f) * Mathf.Lerp(targetDrive.torque, power * Mathf.Abs(System.Math.Sign(actualInput)), (1 - inertia) * Time.timeScale) * health;
            }

            // Send RPM and torque through drivetrain
            if (outputDrives.Length > 0)
            {
                float torqueFactor = Mathf.Pow(1f / outputDrives.Length, driveDividePower);
                float tempRPM = 0;

                foreach (DriveForce curOutput in outputDrives)
                {
                    tempRPM += curOutput.feedbackRPM;
                    curOutput.SetDrive(targetDrive, torqueFactor);
                }

                targetDrive.feedbackRPM = tempRPM / outputDrives.Length;
            }

            if (transmission)
            {
                shifting = transmission.shiftTime > 0;
            }
            else
            {
                shifting = false;
            }
        }
        else
        {
            // If turned off, set RPM and torque to 0 and distribute it through drivetrain
            targetDrive.rpm = 0;
            targetDrive.torque = 0;
            targetDrive.feedbackRPM = 0;
            shifting = false;

            if (outputDrives.Length > 0)
            {
                foreach (DriveForce curOutput in outputDrives)
                {
                    curOutput.SetDrive(targetDrive);
                }
            }
        }
    }

    // Calculates the max RPM and propagates its effects
    public void GetMaxRPM()
    {
        maxRPM = torqueCurve.keys[torqueCurve.length - 1].time;

        if (outputDrives.Length > 0)
        {
            foreach (DriveForce curOutput in outputDrives)
            {
                curOutput.curve = targetDrive.curve;

                if (curOutput.GetComponent<Transmission>())
                {
                    curOutput.GetComponent<Transmission>().ResetMaxRPM();
                }
            }
        }
    }
}
