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
    private bool isShooting, isGrappling;
    private Vector3 hookPoint;
    [Range(0f, 1f)] public float positionStrength = 1f;
    [Range(0f, 1f)] public float rotationStrength = 1f;

    [SerializeField]
    private GameObject grapplingHook;
    [SerializeField]
    private GameObject carPos;
    [SerializeField]
    private LayerMask grappleLayer;
    [SerializeField]
    private float maxGrappleDistance;

    [SerializeField]
    private float grappleSpeed = 10;

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

        grappleGun = Car.transform.Find("Grapple Gun");
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

        isGrappling = true;
        lr.enabled = true;

        isShooting = false;
    }
}
