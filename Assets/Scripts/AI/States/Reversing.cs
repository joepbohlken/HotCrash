using UnityEngine;

public class Reversing : BaseState
{
    private float reverseTime;

    public Reversing(ArcadeCar controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        reverseTime = Random.Range(0.5f, 1f);

        // Set acceleration direction
        controller.v = -1f;

        // Set steering direction
        controller.h = 0f;
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
