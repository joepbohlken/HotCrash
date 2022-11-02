using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private CarHealth carHealth;
    private CarController car;
    private bool isDrifting = false;
    private bool tireMarksFlag;
    public TrailRenderer rearLeftRenderer;
    public TrailRenderer rearRightRenderer;


    private CarSound carSound = null;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<CarController>();
        carHealth = GetComponent<CarHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if (carSound == null) return;
        CheckDrift();
    }

    public void SetSound(CarSound carSound)
    {
        this.carSound = carSound;
    }
    public void UpdateTrailPosition(Vector3 locationLeft, Vector3 locationRight)
    {
        rearLeftRenderer.transform.position = locationLeft;
        rearRightRenderer.transform.position = locationRight;
    }

    private void CheckDrift()
    {
        float actualSpeed = car.currentSpeed * (car.carConfig.speedType == SpeedType.KPH ? C.KPHMult : C.MPHMult);

        if (car.handBrakeInput && !isDrifting && actualSpeed >= 25f && !car.isDestroyed)
        {
            isDrifting = true;
            carSound.PlayDriftSound();
            StartEmitter();
        }
        else if ((!car.handBrakeInput && isDrifting) || actualSpeed <= 25f || car.isDestroyed)
        {
            isDrifting = false;
            carSound.StopDriftSound();
            StopEmitter();
        }
    }

    private void StartEmitter()
    {
        if (tireMarksFlag)
        {
            return;
        }

        rearLeftRenderer.emitting = true;
        rearRightRenderer.emitting = true;

        tireMarksFlag = true;
    }

    private void StopEmitter()
    {
        if (!tireMarksFlag)
        {
            return;
        }

        rearLeftRenderer.emitting = false;
        rearRightRenderer.emitting = false;

        tireMarksFlag = false;
    }
}
