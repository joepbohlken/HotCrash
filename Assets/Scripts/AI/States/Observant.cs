using System.Collections.Generic;
using UnityEngine;

public class Observant : BaseState
{
    protected enum DetectResult { None, Left, Right, Reverse }

    private class DetectRay
    {
        public Vector3 start;
        public float angle;
        public DetectResult side;
    }

    protected DetectResult currentDetectResult = DetectResult.None;

    private List<DetectRay> detectRays;
    private float flipDebounce = 5f;
    private LayerMask detectLayers;

    public Observant(CarController controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        if (detectRays != null && detectRays.Count > 0) return;

        // Set up detect rays
        Vector3 boxSize = carAI.boxSize;
        detectRays = new List<DetectRay>()
        {
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = 0f, side = DetectResult.Left },
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = -45f, side = DetectResult.Left },
            new DetectRay() { start = new Vector3(-boxSize.x / 2f + 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = 45f, side = DetectResult.Right },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = -45f, side = DetectResult.Left },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = 0f, side = DetectResult.Right },
            new DetectRay() { start = new Vector3(boxSize.x / 2f - 0.1f, boxSize.y, boxSize.z / 2f - 0.1f), angle = 45f, side = DetectResult.Right }
        };

        detectLayers = LayerMask.NameToLayer("CarBox") | LayerMask.NameToLayer("CarMesh");
        detectLayers = ~detectLayers;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Handle aerial movement
        if (!controller.isGrounded)
        {
            Vector3 direction = (Vector3.down + carAI.mainRb.velocity.normalized).normalized;
            RaycastHit hit;
            if (Physics.Raycast(carAI.transform.position, direction, out hit, 100f))
            {
                Vector3 crossDifference = Vector3.Cross(hit.normal, carAI.transform.up);
                controller.verticalInput = Mathf.Abs(crossDifference.x) > 0.15f ? Mathf.Sign(crossDifference.x) : 0f;
                controller.rollInput = Mathf.Abs(crossDifference.z) > 0.15f ? Mathf.Sign(crossDifference.z) : 0f;
                return;
            }
        }

        // Handle unflip
        flipDebounce -= Time.deltaTime;
        if (flipDebounce <= 0f && !controller.isGrounded && carAI.mainRb.velocity.magnitude < 1f)
        {
            flipDebounce = 5f;
            controller.unflipCarInput = true;
        }

        // Handle abilities
        carAI.useAbilityCooldown -= Time.deltaTime;
        if (carAI.useAbilityCooldown <= 0f)
        {
            carAI.useAbilityCooldown = Random.Range(3f, 10f);
            abilityController.UseAbility();
        }

        // Initialize detect rays
        float currentSpeed = Mathf.Clamp(Vector3.Dot(carAI.transform.forward, carAI.mainRb.velocity), 10f, 999f);

        float closestHit = 999f;
        currentDetectResult = DetectResult.None;

        foreach (DetectRay detectRay in detectRays)
        {
            RaycastHit hit;
            if (Physics.Raycast(carAI.transform.TransformPoint(detectRay.start), Quaternion.AngleAxis(detectRay.angle, carAI.transform.up) * carAI.transform.forward, out hit, currentSpeed / 2f))
            {
                float hitSurfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (hit.distance < closestHit && hitSurfaceAngle > 30f)
                {
                    closestHit = hit.distance;
                    currentDetectResult = detectRay.side;
                }
            }

#if UNITY_EDITOR
            if (carAI.debugging) Debug.DrawRay(carAI.transform.TransformPoint(detectRay.start), Quaternion.AngleAxis(detectRay.angle, carAI.transform.up) * carAI.transform.forward * (currentSpeed / 2f), detectRay.side == DetectResult.Left ? Color.white : Color.red);
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
