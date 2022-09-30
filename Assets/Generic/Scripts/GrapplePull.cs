using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GrapplePull : MonoBehaviour
{
    [SerializeField] private LineRenderer lr;
    [SerializeField] public Vector3 hookPoint;
    [SerializeField] private Transform grappleGun;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform car;

    [SerializeField] float maxSpeed;
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;

    public static bool isGrappling = false;
    private bool hookSet = false;
    private SpringJoint joint;


    // Start is called before the first frame update
    void Start()
    {
        grappleGun = gameObject.transform;
        lr = grappleGun.GetComponent<LineRenderer>();
        rb = grappleGun.GetComponentInParent<Rigidbody>();
        lr.SetPosition(1, grappleGun.position);
        lr.enabled = false;
        car = rb.GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (lr.enabled)
        {
            if(!hookSet)
            {
                hookPoint = lr.GetPosition(1);
                joint = grappleGun.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = hookPoint;

                float distanceFromPoint = Vector3.Distance(grappleGun.position, hookPoint);

                joint.maxDistance = distanceFromPoint * 0.8f;
                joint.minDistance = distanceFromPoint * 0.25f;

                //Adjust these values to fit your game.
                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 4.5f;
                hookSet = true;
            }
            Rotate();
            Pull();
        }
    }

    public void Pull()
    {
        Vector3 difference = grappleGun.position - hookPoint;

        if (Vector3.Distance(grappleGun.position, hookPoint) > 1 && isGrappling)
        {
            if (rb.velocity.magnitude < maxSpeed) rb.AddForce((hookPoint - transform.position).normalized * speed);
        }
        else
        {
            isGrappling = false;
            lr.enabled = false;
            Destroy(joint);
            hookSet = false;
        }
    }

    public void Rotate()
    {
        Vector3 targetDirection = hookPoint - car.position;

        float step = rotationSpeed * Time.deltaTime;

        Vector3 newDirection = Vector3.RotateTowards(car.forward, targetDirection, step, 0.0f);

        car.rotation = Quaternion.LookRotation(newDirection);
    }
}
