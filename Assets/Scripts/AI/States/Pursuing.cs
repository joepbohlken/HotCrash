using UnityEngine;

public class Pursuing : Observant
{
    private Rigidbody targetRb;
    private Transform whitelistedTarget;
    private float targetTime = 0f;

    public Pursuing(CarController controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        whitelistedTarget = null;
        targetTime = 0f;

        // Set acceleration direction
        //float playerSideFB = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
        controller.verticalInput = 1f;

        // Set steering direction
        controller.horizontalInput = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        // Assign target
        float closestCar = 999f;
        CarController newTarget = null;

        foreach (CarController car in carAI.cars)
        {
            if (car.transform == carAI.transform || !car.gameObject.activeSelf || car.transform == whitelistedTarget || car.isDestroyed) continue;

            bool isWithinView = Vector3.Angle(carAI.transform.forward, (car.transform.position - carAI.transform.position).normalized) <= 45f;

            float distance = (car.transform.position - carAI.transform.position).magnitude;
            if (isWithinView && distance < closestCar)
            {
                closestCar = distance;
                newTarget = car;
            }
        }

        if (newTarget != null && newTarget.rb == targetRb)
        {
            targetTime += Time.deltaTime;

            if (targetTime >= 10f)
            {
                whitelistedTarget = targetRb.transform;
                targetRb = null;
            }
        }
        else
        {
            if (newTarget == null)
            {
                targetRb = null;
            }
            else if (targetRb == null || newTarget.transform != targetRb.transform)
            {
                targetRb = newTarget.rb;
            }
            targetTime = 0f;
        }

        // Set acceleration direction
        if (controller.isGrounded) controller.verticalInput = 1f;

        // Set steering direction
        if (targetRb != null && controller.isGrounded)
        {
            float predictValue = Mathf.Clamp(targetRb.velocity.magnitude / 3f, 0f, (targetRb.transform.position - carAI.transform.position).magnitude);
            float playerSideLR = Vector3.SignedAngle(carAI.transform.forward, (targetRb.transform.position + targetRb.transform.forward * predictValue - carAI.transform.position).normalized, Vector3.up);
            if (Mathf.Abs(playerSideLR) > 5f)
            {
                controller.horizontalInput = Mathf.Sign(playerSideLR);
            }
            else
            {
                controller.horizontalInput = 0f;
            }
        }

        // Transition
        if (currentDetectResult == DetectResult.Left || currentDetectResult == DetectResult.Right) carAI.ChangeState(carAI.avoiding);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
