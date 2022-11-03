public class Avoiding : Observant
{
    public Avoiding(CarController controller, CarAI carAI) : base(controller, carAI) { }

    public override void Enter()
    {
        base.Enter();

        // Set acceleration direction
        controller.verticalInput = 1f;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (carAI.currentState != this) return;

        // Set steering direction
        controller.horizontalInput = currentDetectResult == DetectResult.Left ? 1f : -1f;

        // Transition
        if (currentDetectResult == DetectResult.None) carAI.ChangeState(carAI.currentDrivingMode == CarAI.DrivingMode.Pursue ? carAI.pursuing : carAI.idle);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
