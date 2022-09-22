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
        GameObject go = Instantiate(ShieldObject, Car.transform.position, Car.transform.rotation, Car.transform);
        Destroy(go, Duration);
        Debug.Log("Shield Ability " + Car.name);
    }
}
