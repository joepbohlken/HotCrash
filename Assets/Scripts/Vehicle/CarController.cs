using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class C
{
    //CarParams constants
    public const float MPHMult = 2.23693629f;
    public const float KPHMult = 3.6f;
}
public enum CarDriveType
{
    FrontWheelDrive,
    RearWheelDrive,
    FourWheelDrive
}

public enum SpeedType
{
    MPH,
    KPH
}

[Serializable]
public class CarConfig
{
    [Header("Steer Settings")]
    public float maxSteerAngle = 35;
    public float steerAngleChangeSpeed = 240;
    public AnimationCurve steerAngleLimiter = AnimationCurve.Linear(0.0f, 1f, 200.0f, 0.3f);
    [Tooltip("0 is raw physics , 1 the car will grip in the direction it is facing")]
    [Range(0, 1)]
    public float steerHelper = 0.65f;
    [Tooltip("0 is no traction control, 1 is full interference")]
    [Range(0, 1)]
    public float tractionControl = 0;

    [Header("Drive Settings")]
    public CarDriveType carDriveType = CarDriveType.RearWheelDrive;
    public float fullTorqueOverAllWheels = 5000;
    public float reverseTorque = 1000;
    public SpeedType speedType = SpeedType.KPH;
    public float topSpeed = 200;
    public float topReverseSpeed = 70;
    public int nrOfGears = 5;
    public float revRangeBoundary = 1f;

    [Header("Braking Settings")]
    public float maxHandbrakeTorque = 20000;
    public float brakeTorque = 20000;

    [Header("Drift Settings")]
    public float driftSidewaysFriction = 1.5f;

    [Header("Aerial Settings")]
    public AnimationCurve torqueCurve = AnimationCurve.Linear(0.0f, 0.0f, 5.0f, 1.0f);
    public AnimationCurve aerialDragCurve = AnimationCurve.Linear(0.0f, 0.0f, 5.0f, 1.0f);

    [Header("Other Settings")]
    public float downForce = 5f;
    public float slipLimit = 0.3f;
    public float flipDuration = .8f;
}

public class CarController : MonoBehaviour
{
    [Header("Wheels")]
    public Wheel frontLeftWheel;
    public Wheel frontRightWheel;
    public Wheel rearLeftWheel;
    public Wheel rearRightWheel;

    [Header("General Settings")]
    public Transform COM;
    public bool isBot = false;
    public CarConfig carConfig;

    [Header("Debug Settings")]
    public bool useSinglePlayerInputs = false;

    // ---------------
    // Public variables
    public PlayerController player { get; set; }
    public Rigidbody rb { get; private set; }
    public CarHealth health { get; private set; }
    public bool driveable { get; set; } = true;
    public bool targetable { get; set; }
    public bool isDestroyed { get; set; } = false;
    public float currentSpeed { get; private set; } // magnitude of vehicle
    public float convertedCurrentSpeed { get { return currentSpeed * (carConfig.speedType == SpeedType.KPH ? C.KPHMult : C.MPHMult); } }
    public bool isGrounded { get; private set; } = false;

    // ---------------
    // Private variables
    private CarAI ai;
    private LevelManager levelManager;
    private ParticleSystem ps;
    private Wheel[] wheels;

    private float currentTorque;
    private float currentSteerAngle;
    private int currentGear;
    private float currentGearFactor;
    private float revs;
    private Vector3 originalCenterOfMass;
    private float oldRotation;
    private float oldDrag;
    private float oldExtremumSlip;
    private float oldStiffness;
    private float carAngle;
    private float driftReleased;

    // ---------------
    // Input variables & update method
    public float verticalInput { get; set; }
    public float horizontalInput { get; set; }
    public float rollInput { get; set; }
    public bool handBrakeInput { get; set; }
    public bool unflipCarInput { get; set; }

    // ---------------
    // Ability variables
    public bool isTargetable { get; set; } = true;
    public List<CarCanvas> carCanvasRefs { get; set; } = new();


    // Update control variables
    public void UpdateControls(float horizontal, float vertical, float roll, bool handBrake, bool unflip)
    {
        verticalInput = vertical;
        horizontalInput = horizontal;
        rollInput = handBrake ? horizontal : roll;
        handBrakeInput = handBrake;
        unflipCarInput = unflip;
    }
    // ---------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<CarHealth>();
        ps = GetComponentInChildren<ParticleSystem>();
        levelManager = FindObjectOfType<LevelManager>();

        // Set center of mass
        originalCenterOfMass = rb.centerOfMass;
        rb.centerOfMass = COM.localPosition;

        oldDrag = rb.drag;

        //Copy wheels in public property
        wheels = new Wheel[4] {
            frontLeftWheel,
            frontRightWheel,
            rearLeftWheel,
            rearRightWheel
        };

        currentTorque = carConfig.fullTorqueOverAllWheels - (carConfig.tractionControl * carConfig.fullTorqueOverAllWheels);
        oldExtremumSlip = wheels[2].wheelCollider.sidewaysFriction.extremumSlip;
        oldStiffness = wheels[2].wheelCollider.sidewaysFriction.stiffness;

        if (health)
            health.onDestroyed.AddListener(OnDestroyed);
    }

    private void OnDisable()
    {
        if (health)
            health.onDestroyed.AddListener(OnDestroyed);
    }

    private void Start()
    {
        if (isBot)
        {
            ai = gameObject.AddComponent<CarAI>();
            ai.boxSize = new Vector3(2, 1f, 5);
            ai.InitializeAI();
        }

        if (PlayerManager.main != null)
        {
            useSinglePlayerInputs = false;
        }
    }

    private void Update()
    {
        if (ps)
        {
            ps.transform.localRotation = Quaternion.LookRotation(ps.transform.parent.InverseTransformDirection(Vector3.up), ps.transform.up);
        }

        if (isDestroyed)
        {
            return;
        }

        if (gameObject.transform.position.y < -10f)
        {
            isDestroyed = true;
            OnDestroyed();
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].UpdateVisual();
        }

        if (useSinglePlayerInputs && !isBot)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float roll = Input.GetAxis("Roll");
            bool handBrake = Input.GetKey(KeyCode.LeftShift);
            bool unflip = Input.GetKeyDown(KeyCode.Q);

            UpdateControls(horizontal, vertical, roll, handBrake, unflip);
        }

        float targetSteerAngle = horizontalInput * carConfig.maxSteerAngle;
        float steerAngleMultiplier = carConfig.steerAngleLimiter.Evaluate(convertedCurrentSpeed / 100f);

        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, steerAngleMultiplier * targetSteerAngle, Time.deltaTime * carConfig.steerAngleChangeSpeed);

        carAngle = Vector3.Dot(transform.up, Vector3.down);
    }

    private void FixedUpdate()
    {
        // Set current speed
        currentSpeed = rb.velocity.magnitude;

        if (isDestroyed || !driveable)
        {
            return;
        }

        isGrounded = CheckIfGrounded();

        HandleMove();
    }

    private bool CheckIfGrounded()
    {
        bool result = true;

        foreach (Wheel wheel in wheels)
        {
            if (!wheel.isGrounded)
            {
                result = false;
                break;
            }
        }

        return result;
    }

    private void GearChanging()
    {
        float f = Mathf.Abs(convertedCurrentSpeed / carConfig.topSpeed);
        float upgearlimit = (1 / (float)carConfig.nrOfGears) * (currentGear + 1);
        float downgearlimit = (1 / (float)carConfig.nrOfGears) * currentGear;

        if (currentGear > 0 && f < downgearlimit)
        {
            currentGear--;
        }

        if (f > upgearlimit && (currentGear < (carConfig.nrOfGears - 1)))
        {
            currentGear++;
        }
    }

    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    private void CalculateGearFactor()
    {
        float f = (1 / (float)carConfig.nrOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * currentGear, f * (currentGear + 1), Mathf.Abs(convertedCurrentSpeed / carConfig.topSpeed));
        currentGearFactor = Mathf.Lerp(currentGearFactor, targetGearFactor, Time.deltaTime * 5f);
    }

    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = currentGear / (float)carConfig.nrOfGears;
        var revsRangeMin = ULerp(0f, carConfig.revRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(carConfig.revRangeBoundary, 1f, gearNumFactor);
        revs = ULerp(revsRangeMin, revsRangeMax, currentGearFactor);
    }

    private void HandleMove()
    {
        ApplySteer();
        ApplyDrive();
        CapSpeed();

        ApplyDrift();

        if (!isGrounded)
        {
            ApplyAerialMovement();
            rb.drag = carConfig.aerialDragCurve.Evaluate(convertedCurrentSpeed / 100f);
        }
        else
        {
            rb.drag = oldDrag;
        }

        if (!isGrounded && carAngle > .85f && unflipCarInput)
        {
            StartCoroutine(FlipCar());
        }

        CalculateRevs();
        GearChanging();

        AddDownForce();
        TractionControl();
    }

    private void ApplyAerialMovement()
    {
        float dt = Time.fixedDeltaTime;

        float rollRotation = rollInput * carConfig.torqueCurve.Evaluate(rb.angularVelocity.z) * dt;
        float pitchRotation = horizontalInput * carConfig.torqueCurve.Evaluate(rb.angularVelocity.x) / 2 * dt;
        float yawRotation = verticalInput * carConfig.torqueCurve.Evaluate(rb.angularVelocity.y) * dt;


        rb.AddTorque(transform.forward * rollRotation, ForceMode.VelocityChange);
        rb.AddTorque(transform.right * yawRotation, ForceMode.VelocityChange);
        rb.AddTorque(transform.up * pitchRotation, ForceMode.VelocityChange);
    }

    private IEnumerator FlipCar()
    {
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + Vector3.up * 1.5f;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, 180);

        bool reachedTargetPosition = false;

        while (timeElapsed < carConfig.flipDuration)
        {
            if (!reachedTargetPosition)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / (carConfig.flipDuration / 1.5f));
                reachedTargetPosition = transform.position == targetPosition;
            }

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / carConfig.flipDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void OnDestroyed()
    {
        isDestroyed = true;
        rb.centerOfMass = originalCenterOfMass;

        if (GameManager.main != null)
        {
            GameManager.main.OnCarDeath(gameObject, health.lastCollider);
        }

        Transform canvasContainer = gameObject.transform.Find("UI");
        for (int i = 0; i < canvasContainer.childCount; i++)
        {
            Destroy(canvasContainer.GetChild(i).gameObject);
        }

        ps.Play();

        foreach (Wheel wheel in wheels)
        {
            PopOffWheel(wheel);
        }
    }

    private void PopOffWheel(Wheel wheel)
    {
        Transform wheelContainer = levelManager != null ? levelManager.wheelContainer : null;

        wheel.wheelView.SetParent(wheelContainer);
        wheel.wheelCollider.gameObject.SetActive(false);

        Rigidbody wheelRb = wheel.wheelView.gameObject.AddComponent<Rigidbody>();
        MeshCollider wheelMesh = wheel.wheelView.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<MeshCollider>();

        wheelRb.mass = 50;
        wheelRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        wheelMesh.convex = true;

        wheelRb.AddForce((UnityEngine.Random.onUnitSphere.normalized * 10) + rb.velocity, ForceMode.VelocityChange);
    }

    private void CapSpeed()
    {
        float speed = rb.velocity.magnitude;
        switch (carConfig.speedType)
        {
            case SpeedType.MPH:
                speed *= C.MPHMult;
                if (speed > carConfig.topSpeed)
                    rb.velocity = (carConfig.topSpeed / C.MPHMult) * rb.velocity.normalized;
                //if (speed > -carConfig.topSpeed)
                //    rb.velocity = (carConfig.topSpeed / C.MPHMult) * rb.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= C.KPHMult;
                if (speed > carConfig.topSpeed)
                    rb.velocity = (carConfig.topSpeed / C.KPHMult) * rb.velocity.normalized;
                //if (speed < -carConfig.topReverseSpeed)
                //   rb.velocity = (carConfig.topReverseSpeed / C.KPHMult) * rb.velocity.normalized;
                break;
        }
    }

    private void ApplyBrake()
    {
        for (int i = 0; i < 4; i++)
        {
            wheels[i].wheelCollider.brakeTorque = carConfig.brakeTorque * Mathf.Abs(verticalInput);
        }
    }

    private void ApplyDrive()
    {
        bool isAccel = verticalInput > 0;
        bool isReverse = verticalInput < 0;
        bool velocityIsForwards = Vector3.Angle(transform.forward, rb.velocity) < 50f;

        if (isAccel)
        {
            if (convertedCurrentSpeed > 2 && !velocityIsForwards)
            {
                // Apply braking torque to slow down
                ApplyBrake();
            }
            else
            {
                float thrustTorque = carConfig.carDriveType == CarDriveType.FourWheelDrive ? verticalInput * (currentTorque / 4f) : verticalInput * (currentTorque / 2f);

                // Apply motor torque and remove brake torque
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].wheelCollider.brakeTorque = 0f;

                    switch (carConfig.carDriveType)
                    {
                        case CarDriveType.FourWheelDrive:
                            wheels[i].wheelCollider.motorTorque = thrustTorque;
                            break;

                        case CarDriveType.FrontWheelDrive:
                            if (0 <= i || i <= 1)
                                wheels[i].wheelCollider.motorTorque = thrustTorque;
                            else
                                wheels[i].wheelCollider.motorTorque = 0f;
                            break;

                        case CarDriveType.RearWheelDrive:
                            if (2 <= i || i <= 3)
                                wheels[i].wheelCollider.motorTorque = thrustTorque;
                            else
                                wheels[i].wheelCollider.motorTorque = 0f;
                            break;
                    }
                }
            }
        }
        else if (isReverse)
        {
            if (convertedCurrentSpeed > 2 && velocityIsForwards)
            {
                // Apply braking torque to slow down
                ApplyBrake();
            }
            else
            {
                // Apply reverse acceleration
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].wheelCollider.brakeTorque = 0f;
                    wheels[i].wheelCollider.motorTorque = -carConfig.reverseTorque * Mathf.Abs(verticalInput);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                wheels[i].wheelCollider.brakeTorque = carConfig.brakeTorque * .05f;
                wheels[i].wheelCollider.motorTorque = 0;
            }
        }
    }

    private void ApplySteer()
    {
        wheels[0].wheelCollider.steerAngle = currentSteerAngle;
        wheels[1].wheelCollider.steerAngle = currentSteerAngle;

        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            wheels[i].wheelCollider.GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - oldRotation) * carConfig.steerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rb.velocity = velRotation * rb.velocity;
        }
        oldRotation = transform.eulerAngles.y;
    }

    private void ApplyDrift()
    {
        WheelFrictionCurve leftWheelCurve = wheels[2].wheelCollider.sidewaysFriction;
        WheelFrictionCurve rightWheelCurve = wheels[3].wheelCollider.sidewaysFriction;

        if (handBrakeInput)
        {
            driftReleased = 0;
            var hbTorque = (handBrakeInput ? 1 : 0) * carConfig.maxHandbrakeTorque;
            wheels[2].wheelCollider.brakeTorque = hbTorque;
            wheels[3].wheelCollider.brakeTorque = hbTorque;

            leftWheelCurve.extremumSlip = carConfig.driftSidewaysFriction;
            rightWheelCurve.extremumSlip = carConfig.driftSidewaysFriction;
        }
        else
        {
            driftReleased += Time.fixedDeltaTime;
            leftWheelCurve.extremumSlip = oldExtremumSlip;
            rightWheelCurve.extremumSlip = oldExtremumSlip;

            if (driftReleased < .25f)
            {
                leftWheelCurve.stiffness = 2;
                rightWheelCurve.stiffness = 2;
            }
            else
            {
                leftWheelCurve.stiffness = oldStiffness;
                rightWheelCurve.stiffness = oldStiffness;
            }
        }

        wheels[2].wheelCollider.sidewaysFriction = leftWheelCurve;
        wheels[3].wheelCollider.sidewaysFriction = rightWheelCurve;
    }

    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        rb.AddForce(-transform.up * carConfig.downForce * rb.velocity.magnitude);
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;
        switch (carConfig.carDriveType)
        {
            case CarDriveType.FourWheelDrive:
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].wheelCollider.GetGroundHit(out wheelHit);

                    AdjustTorque(wheelHit.forwardSlip);
                }
                break;

            case CarDriveType.RearWheelDrive:
                wheels[2].wheelCollider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheels[3].wheelCollider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;

            case CarDriveType.FrontWheelDrive:
                wheels[0].wheelCollider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheels[1].wheelCollider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;
        }
    }

    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= carConfig.slipLimit && currentTorque >= 0)
        {
            currentTorque -= 10 * carConfig.tractionControl;
        }
        else
        {
            currentTorque += 10 * carConfig.tractionControl;
            if (currentTorque > carConfig.fullTorqueOverAllWheels)
            {
                currentTorque = carConfig.fullTorqueOverAllWheels;
            }
        }
    }
}