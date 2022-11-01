using UnityEngine;

public class Idle : Observant
{
    private float randomOffset;

    public Idle(CarController controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        randomOffset = Random.Range(0f, 999f);

        // Set acceleration direction
        controller.verticalInput = 1f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        // Smooth random number between -1 and 1 using Perlin noise
        float steerValue = Mathf.PerlinNoise(Time.time + randomOffset, 0f) * 2f - 1f;

        // Set acceleration & steering direction
        if (controller.isGrounded)
        {
            controller.verticalInput = 1f;
            controller.horizontalInput = Mathf.Abs(steerValue) > 0.3f ? Mathf.Sign(steerValue) : 0f;
        }
        

        carAI.idleTime -= Time.deltaTime;
        if (carAI.idleTime <= 0f) carAI.currentDrivingMode = CarAI.DrivingMode.Pursue;

        // Transition
        if (carAI.currentDrivingMode == CarAI.DrivingMode.Pursue) carAI.ChangeState(carAI.pursuing);
        else if (currentDetectResult == DetectResult.Left || currentDetectResult == DetectResult.Right) carAI.ChangeState(carAI.avoiding);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
