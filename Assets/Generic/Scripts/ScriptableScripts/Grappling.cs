using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Grappling")]
public class Grappling : Ability
{
    private Rigidbody rb;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask grappleable;
    [SerializeField] private LineRenderer lr;

    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleDelayTime;

    private Vector3 grapplePoint;

    [SerializeField] private float grapplingCd;
    private float grapplingCdTimer;

    private bool grappling;

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            grapplePoint = hit.point;

            ExecuteGrapple(grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            StopGrapple(grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple(float grappleDelayTime)
    {
        
    }

    private void StopGrapple(float grappleDelayTime)
    {
        grappling = false;

        grapplingCdTimer = grapplingCd;
    }
}
    
