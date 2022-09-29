using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Abilities/GrapplingHook")]
public class GrapplingHookAbility : Ability
{
    private Transform grappleGun;
    private LineRenderer lr;
    private bool isShooting;
    private Vector3 hookPoint;

    [SerializeField]
    private LayerMask grappleLayer;
    [SerializeField]
    private float maxGrappleDistance;

    [SerializeField]
    private float grappleSpeed = 10;

    public override void Use()
    {
        if(!isShooting && !GrapplePull.isGrappling)
        ShootHook();
    }

    private void ShootHook()
    {

        //if (isShooting || isGrappling)
        //{
        //    return;
        //}

        grappleGun = Car.transform.Find("CarGrapple");
        lr = grappleGun.GetComponent<LineRenderer>();

        isShooting = true;
        RaycastHit hit; 
        //Speciaal point voor grapplinghook
        if (Physics.Raycast(grappleGun.position + Vector3.up, grappleGun.TransformDirection(Vector3.forward), out hit, maxGrappleDistance, grappleLayer))
        {
            Debug.Log("Grapple: " + grappleGun.position.z + " Car: " + Car.transform.position.z + 1);
            hookPoint = hit.point;
        }
        else
        {
            hookPoint = grappleGun.position + grappleGun.forward * maxGrappleDistance;
        }

        lr.SetPosition(0, grappleGun.position);
        lr.SetPosition(1, hookPoint);

        GrapplePull.isGrappling = true;
        lr.enabled = true;

        isShooting = false;
    }
}
