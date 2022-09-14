using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    protected GameObject Car;

    [SerializeField]
    protected float Duration;
    [SerializeField]
    protected float Cooldown;
    public virtual void Use()
    {
        Debug.Log(this.name + ": Ability not implemented");
    }
    public void SetCar(GameObject player)
    {
        Car = player;
    }

    public float AbilityCooldown
    {
        get
        {
            return Cooldown;
        }
    }
}
