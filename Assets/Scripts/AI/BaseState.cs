public class BaseState
{
    public CarController controller;
    protected CarAI carAI;
    protected AbilityController abilityController;

    public BaseState(CarController controller, CarAI carAI)
    {
        this.controller = controller;
        this.carAI = carAI;
        abilityController = controller.GetComponent<AbilityController>();
    }

    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void Exit() { }
}
