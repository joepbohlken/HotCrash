using System.Collections.Generic;
using UnityEngine;

public class Observant : BaseState
{
    protected enum DetectResult { None, Left, Right, Reverse }

    private class DetectRay
    {
        public Vector3 start;
        public Quaternion direction;
        public DetectResult side;
    }

    protected DetectResult currentDetectResult = DetectResult.None;

    private List<DetectRay> detectRays;

    public Observant(ArcadeCar controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        // Set up detect rays
        Vector3 boxSize = carAI.boxSize;
        detectRays = new List<DetectRay>()
        {
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(0f, Vector3.up), side = DetectResult.Left },
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(-45f, Vector3.up), side = DetectResult.Left },
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(45f, Vector3.up), side = DetectResult.Right },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(-45f, Vector3.up), side = DetectResult.Left },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(0f, Vector3.up), side = DetectResult.Right },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), direction = Quaternion.AngleAxis(45f, Vector3.up), side = DetectResult.Right }
        };
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Initialize detect rays
        float currentSpeed = Mathf.Clamp(Vector3.Dot(carAI.transform.forward, carAI.mainRb.velocity), 10f, 999f);

        float closestHit = 999f;
        currentDetectResult = DetectResult.None;

        foreach (DetectRay detectRay in detectRays)
        {
            RaycastHit hit;
            if (Physics.Raycast(carAI.transform.TransformPoint(detectRay.start), detectRay.direction * carAI.transform.forward, out hit, currentSpeed / 2f))
            {
                if (hit.distance < closestHit)
                {
                    closestHit = hit.distance;
                    currentDetectResult = detectRay.side;
                }
            }

#if UNITY_EDITOR
            if (carAI.debugging) Debug.DrawRay(carAI.transform.TransformPoint(detectRay.start), detectRay.direction * carAI.transform.forward * (currentSpeed / 2f), detectRay.side == DetectResult.Left ? Color.white : Color.red);
#endif
        }

        if (closestHit < 1f || carAI.hitOpponent)
        {
            if (carAI.hitOpponent && carAI.currentDrivingMode != CarAI.DrivingMode.Idle)
            {
                float idleValue = Random.Range(0f, 1f);

                if (idleValue > carAI.aggression)
                {
                    carAI.currentDrivingMode = CarAI.DrivingMode.Idle;
                    carAI.idleTime = Random.Range(5f, 10f);
                }
            }
            currentDetectResult = DetectResult.Reverse;
            carAI.hitOpponent = false;
        }

        if (carAI.gotHit && carAI.currentDrivingMode == CarAI.DrivingMode.Idle)
        {
            carAI.currentDrivingMode = CarAI.DrivingMode.Pursue;
            carAI.gotHit = false;
        }

        // Transition
        if (currentDetectResult == DetectResult.Reverse)
        {
            carAI.ChangeState(carAI.reversing);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
