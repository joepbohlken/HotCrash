using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private CarController car;
    private bool isDrifting = false;
    private bool tireMarksFlag;

    public TrailRenderer[] tireMarks;

    private CarSound carSound = null;

    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<CarController>();
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

    private void CheckDrift()
    {
        float actualSpeed = car.currentSpeed * (car.carConfig.speedType == SpeedType.KPH ? C.KPHMult : C.MPHMult);

        if (car.handBrakeInput && !isDrifting && actualSpeed >= 25f)
        {
            isDrifting = true;
            carSound.PlayDriftSound();
            StartEmitter();
        }
        else if ((!car.handBrakeInput && isDrifting) || actualSpeed <= 25f)
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

        foreach (TrailRenderer T in tireMarks)
        {
            T.emitting = true;
        }

        tireMarksFlag = true;
    }

    private void StopEmitter()
    {
        if (!tireMarksFlag)
        {
            return;
        }
        foreach (TrailRenderer T in tireMarks)
        {
            T.emitting = false;
        }

        tireMarksFlag = false;
    }
}
