public class Avoiding : Observant
{
    public Avoiding(ArcadeCar controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        // Set acceleration direction
        controller.v = 1f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        // Set steering direction
        controller.h = currentDetectResult == DetectResult.Left ? 1f : -1f;

        // Transition
        if (currentDetectResult == DetectResult.None) carAI.ChangeState(carAI.pursuing);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
