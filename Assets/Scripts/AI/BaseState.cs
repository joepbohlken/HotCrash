public class BaseState
{
    protected ArcadeCar controller;
    protected CarAI carAI;

    public BaseState(ArcadeCar controller, CarAI carAI)
    {
        this.controller = controller;
        this.carAI = carAI;
    }

    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void Exit() { }
}
