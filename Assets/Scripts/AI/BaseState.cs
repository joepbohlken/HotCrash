public class BaseState
{
    protected CarController controller;
    protected CarAI carAI;

    public BaseState(CarController controller, CarAI carAI)
    {
        this.controller = controller;
        this.carAI = carAI;
    }

    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void Exit() { }
}
