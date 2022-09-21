using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Transparent")]
public class TransparentAbility : Ability
{
    [SerializeField]
    private float visableDistance;
    public override void Use()
    {
        base.Use();
    }
}
