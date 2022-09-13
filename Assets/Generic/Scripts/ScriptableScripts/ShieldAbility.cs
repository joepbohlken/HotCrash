using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shield")]
public class ShieldAbility : Ability
{
    [SerializeField]
    private GameObject ShieldObject;

    public override void Use()
    {
        
        //Instantiate(ShieldObject, )
        Debug.Log("Shield Ability " + Car.name);
    }
}
