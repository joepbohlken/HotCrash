using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrapplePull : MonoBehaviour
{
    [SerializeField] public Vector3 hookPoint;
    [SerializeField] public Transform grappleGun;
    private Transform gunTip;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform car;

    public static bool isGrappling;
    private bool hookSet = false;
    private SpringJoint joint;
    private Steering steering;
    private Ackermann ackermann;
    public Transform hitPlayer = null;
    public List<Transform> visibleTargets = new List<Transform>();

    [SerializeField]
    List<GameObject> players;
    [SerializeField]
    List<Transform> playerCars;

    //Ability Params
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] float maxSpeed;
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float range;
    [SerializeField] float playerTargetAngle;

    // Start is called before the first frame update
    void Start()
    {
        grappleGun = gameObject.transform;
        rb = grappleGun.GetComponent<Rigidbody>();
        gunTip = grappleGun.transform.Find("GunTip");
        steering = grappleGun.GetComponent<Steering>();
        ackermann = grappleGun.GetComponent<Ackermann>();
        car = rb.GetComponent<Transform>();
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        foreach (GameObject player in players)
        {
            playerCars.Add(player.transform);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isGrappling)
        {
            if (!hookSet)
            {
                ShootHook();
                SetJoint();
                hookSet = true;
                steering.rate = 0;
            }
            if (hookSet)
            {
                steering.m_CurrAngle = 0;
                Rotate();
                Pull();
            }
        }
    }

    public void Pull()
    {
        Vector3 difference = gunTip.position - hookPoint;

        if (Vector3.Distance(gunTip.position, hookPoint) > 4 && isGrappling)
        {
            if (rb.velocity.magnitude < maxSpeed) rb.AddForce((hookPoint - gunTip.position).normalized * speed, ForceMode.VelocityChange);
            if(hitPlayer != null)
            {
                hookPoint = hitPlayer.transform.position;
            }
        }
        else
        {
            isGrappling = false;
            Destroy(joint);
            hookSet = false;
            steering.rate = 90;
            hitPlayer = null;
        }
    }

    public void Rotate()
    {
        ackermann.SetAngle(0);
        Quaternion targetDirection = Quaternion.LookRotation(hookPoint - car.position);

        float step = rotationSpeed * Time.deltaTime/2;

        car.rotation = Quaternion.RotateTowards(grappleGun.rotation, targetDirection, step);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public void ShootHook()
    {
        FindVisibleTargets();
        RaycastHit hit;
        //Speciaal point voor grapplinghook
        if (visibleTargets.Count > 0)
        {
            Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, grappleLayer);
            hookPoint = hit.point;
            hitPlayer = GetClosestPlayer(playerCars);
        }
        else if (Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, grappleLayer))
        {
            hookPoint = hit.point;
        }
        else
        {
            hookPoint = gunTip.position + grappleGun.forward * range;
        }
    }

    public void FindVisibleTargets()
    {
        visibleTargets.Clear();

        for (int i = 0; i < playerCars.Count; i++)
        {
            Transform target = playerCars[i].transform;
            Vector3 dirToTarget = (target.position - gunTip.position).normalized;
            if (Vector3.Angle(gunTip.forward, dirToTarget) < playerTargetAngle / 2)
            { 
                float dstToTarget = Vector3.Distance(gunTip.position, target.position);

                if (!Physics.Raycast(gunTip.position, dirToTarget, dstToTarget, objectLayer))
                {
                    if(dstToTarget <= range)
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
    }

    public void SetJoint()
    {
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
    }

    public Transform GetClosestPlayer(List<Transform> playerCars)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        foreach (Transform t in playerCars)
        {
            float dist = Vector3.Distance(t.position, hookPoint);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }
}
