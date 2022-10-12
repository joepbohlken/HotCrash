using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Abilities/Fist")]
public class FistAbility : Ability
{
    public override void Use()
    {
        FistPush.fistShoot = true;
    }
}
