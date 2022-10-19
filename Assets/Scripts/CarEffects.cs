using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private ArcadeCar arcadeCar;
    private bool isDrifting = false;
    private bool tireMarksFlag;

    public TrailRenderer[] tireMarks;

    // Start is called before the first frame update
    void Start()
    {
        arcadeCar = GetComponent<ArcadeCar>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckDrift();
    }

    private void CheckDrift()
    {
        if (arcadeCar.isHandBrakeNow && !isDrifting)
        {
            Debug.Log("dikke poep");
            isDrifting = true;
            StartEmitter();
        }
        else if (!arcadeCar.isHandBrakeNow && isDrifting)
        {
            isDrifting = false;
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
