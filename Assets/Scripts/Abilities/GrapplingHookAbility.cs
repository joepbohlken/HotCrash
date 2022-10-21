using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Abilities/GrapplingHook")]
public class GrapplingHookAbility : Ability
{
    [SerializeField]
    private LayerMask grappleLayer;
    [SerializeField]
    private float maxGrappleDistance;

    public override void Use()
    {
        if(!GrapplePull.isGrappling)
        ShootHook();
    }

    private void ShootHook()
    {
        GrapplePull.isGrappling = true;
    }
}
