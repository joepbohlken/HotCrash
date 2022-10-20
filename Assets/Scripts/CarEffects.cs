using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private ArcadeCar arcadeCar;
    private bool isDrifting = false;
    private bool tireMarksFlag;

    public TrailRenderer[] tireMarks;

    private CarSound carSound;

    // Start is called before the first frame update
    void Start()
    {
        arcadeCar = GetComponent<ArcadeCar>();
        carSound = GetComponent<CarSound>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckDrift();
    }

    private void CheckDrift()
    {
        float actualSpeed = arcadeCar.speed * 3.6f;

        if (arcadeCar.isHandBrakeNow && !isDrifting && actualSpeed >= 25f)
        {
            isDrifting = true;
            carSound.PlayDriftSound();
            StartEmitter();
        }
        else if ((!arcadeCar.isHandBrakeNow && isDrifting) || actualSpeed <= 25f)
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
        foreach(TrailRenderer T in tireMarks)
        {
            T.emitting = false;
        }

        tireMarksFlag = false;
    }
}
