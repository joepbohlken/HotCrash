using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/GrapplingHook")]
public class GrapplingHookAbility : Ability
{
    private Transform grapplePoint;
    private LineRenderer lr;
    private bool isShooting, isGrappling;
    private Vector3 hookPoint;

    [SerializeField]
    private GameObject grapplingHook;
    [SerializeField]
    private GameObject carPos;
    [SerializeField]
    private LayerMask grappleLayer;
    [SerializeField]
    private float maxGrappleDistance;

    public override void Use()
    {
        ShootHook();
    }

    private void ShootHook()
    {

        //if (isShooting || isGrappling)
        //{
        //    return;
        //}

        grapplePoint = Car.transform.Find("Visuals/Body/GrapplePoint");
        lr = grapplePoint.GetComponent<LineRenderer>();

        isShooting = true;
        RaycastHit hit; 
        //Speciaal point voor grapplinghook
        if (Physics.Raycast(grapplePoint.position + Vector3.up, grapplePoint.TransformDirection(Vector3.forward), out hit, maxGrappleDistance, grappleLayer))
        {
            Debug.Log("Grapple: " + grapplePoint.position.z + " Car: " + Car.transform.position.z + 1);
            hookPoint = hit.point;

            isGrappling = true;

            lr.SetPosition(0, grapplePoint.position);
            lr.SetPosition(1, hookPoint);
        }

        lr.enabled = true;

        isShooting = false;
    }
}
