using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    protected GameObject Car;

    public string a_Name;

    [SerializeField]
    protected float Duration;
    [SerializeField]
    protected float Cooldown;
    public virtual void Use()
    {
        Debug.Log("Ability not implemented OR No ability active");
    }

    public virtual void OnPickup()
    {
        Car.GetComponent<AbilityController>().OnAbilityComplete.AddListener(OnAbilityEnded);
    }
    public virtual void OnAbilityEnded()
    {
    }

    public void PickedUp(GameObject player)
    {
        SetCar(player);
        OnPickup();
    }

    private void SetCar(GameObject player)
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
    public float AbilityDuration
    {
        get
        {
            return Duration;
        }
    }
}
