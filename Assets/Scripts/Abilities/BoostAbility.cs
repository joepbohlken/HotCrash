using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boost")]
public class BoostAbility : Ability
{

    public override void Use()
    {
        Debug.Log("Boost Ability");
    } 
}
