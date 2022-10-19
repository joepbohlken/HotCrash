using UnityEngine;

public class Pursuing : Observant
{
    private Transform target;
    private Transform whitelistedTarget;
    private float targetTime = 0f;

    public Pursuing(ArcadeCar controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        whitelistedTarget = null;
        targetTime = 0f;

        // Set acceleration direction
        //float playerSideFB = Vector3.Dot(transform.forward, (target.position - transform.position).normalized);
        controller.v = 1f;

        // Set steering direction
        controller.h = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        // Assign target
        float closestCar = 999f;
        Transform newTarget = null;

        for (int i = 0; i < carAI.transform.parent.childCount; i++)
        {
            Transform car = carAI.transform.parent.GetChild(i);
            if (car == carAI.transform || !car.gameObject.activeSelf || car == whitelistedTarget) continue;

            bool isWithinView = Vector3.Angle(carAI.transform.forward, (car.position - carAI.transform.position).normalized) <= 45f;

            float distance = (car.position - carAI.transform.position).magnitude;
            if (isWithinView && distance < closestCar)
            {
                closestCar = distance;
                newTarget = car;
            }
        }

        if (newTarget == target && newTarget != null)
        {
            targetTime += Time.deltaTime;

            if (targetTime >= 10f)
            {
                whitelistedTarget = target;
                target = null;
            }
        }
        else
        {
            target = newTarget;
            targetTime = 0f;
        }

        // Set steering direction
        if (target != null)
        {
            float predictValue = Mathf.Clamp(carAI.mainRb.velocity.magnitude / 3f, 0f, (target.position - carAI.transform.position).magnitude);
            float playerSideLR = Vector3.SignedAngle(carAI.transform.forward, (target.position + target.forward * predictValue - carAI.transform.position).normalized, Vector3.up);
            if (Mathf.Abs(playerSideLR) > 5f)
            {
                controller.h = Mathf.Sign(playerSideLR);
            }
            else
            {
                controller.h = 0f;
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
