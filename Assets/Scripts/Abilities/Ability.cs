using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite abilityIcon;

    protected AbilityController abilityController;
    protected ArcadeCar carController;
    protected bool isActivated { get; private set; } = false;
    protected bool isCarDestroyed { get; private set; } = false;

    public void Initialize(AbilityController abilityController, ArcadeCar carController)
    {
        this.abilityController = abilityController;
        this.carController = carController;
    }

    public virtual void Obtained() { }

    public virtual void LogicUpdate() { }

    ///<summary>Call 'abilityController.AbilityEnded()' if the ability has ended, this will destroy the ability instance.</summary>
    public virtual void Activated()
    {
        isActivated = true;
    }

    public virtual void CarDestroyed()
    {
        isCarDestroyed = true;
    }
}
