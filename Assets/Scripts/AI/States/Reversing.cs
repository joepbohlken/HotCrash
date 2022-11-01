using UnityEngine;

public class Reversing : BaseState
{
    private float reverseTime;

    public Reversing(CarController controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        reverseTime = Random.Range(1f, 2f);

        // Set acceleration direction
        controller.verticalInput = -1f;

        // Set steering direction
        controller.horizontalInput = 0f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        reverseTime -= Time.deltaTime;
        // Transition
        if (reverseTime <= 0f) carAI.ChangeState(carAI.avoiding);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
