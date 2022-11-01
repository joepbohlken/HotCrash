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
    private float flipDebounce = 5f;

    public Observant(ArcadeCar controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        if (detectRays != null && detectRays.Count > 0) return;

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

        // Handle aerial movement
        if (controller.allWheelIsOnAir)
        {
            Vector3 direction = (Vector3.down + carAI.mainRb.velocity.normalized).normalized;
            RaycastHit hit;
            if (Physics.Raycast(carAI.transform.position, direction, out hit, 100f))
            {
                Vector3 crossDifference = Vector3.Cross(hit.normal, carAI.transform.up);
                controller.v = Mathf.Abs(crossDifference.x) > 0.15f ? Mathf.Sign(crossDifference.x) : 0f;
                // Might need to be changed to 'controller.h' in the future
                controller.qe = Mathf.Abs(crossDifference.z) > 0.15f ? Mathf.Sign(crossDifference.z) : 0f;
                return;
            }
        }

        // Handle unflip
        flipDebounce -= Time.deltaTime;
        if (flipDebounce <= 0f && controller.isTouchingGround && carAI.mainRb.velocity.magnitude < 1f)
        {
            flipDebounce = 5f;
            controller.flip = true;
        }

        // Initialize detect rays
        float currentSpeed = Mathf.Clamp(Vector3.Dot(carAI.transform.forward, carAI.mainRb.velocity), 10f, 999f);

        float closestHit = 999f;
        currentDetectResult = DetectResult.None;

        foreach (DetectRay detectRay in detectRays)
        {
            RaycastHit hit;
            if (Physics.Raycast(carAI.transform.TransformPoint(detectRay.start), detectRay.direction * carAI.transform.forward, out hit, currentSpeed / 2f, controller.raycastLayerMask))
            {
                float hitSurfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (hit.distance < closestHit && hitSurfaceAngle > 30f)
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
