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
    [SerializeField] private AbilityController abilityController;

    public static bool isGrappling;
    private bool hookSet = false;
    private SpringJoint joint;
    private TargetIndicator targetIndicator;
    private ArcadeCar arcadeCar;
    public Transform hitPlayer = null;
    public List<Transform> visibleTargets = new List<Transform>();

    [SerializeField]
    List<GameObject> players;
    [SerializeField]
    List<Transform> playerCars;

    //Ability Params
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] float speed = 3f;
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] float range = 30f;
    [SerializeField] float playerTargetAngle = 33f;
    [SerializeField] float hookSetTime = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        objectLayer = LayerMask.GetMask("Default");
        grappleGun = gameObject.transform;
        arcadeCar = grappleGun.GetComponent<ArcadeCar>();
        rb = grappleGun.GetComponent<Rigidbody>();
        gunTip = grappleGun.transform.Find("GunTip");
        abilityController = grappleGun.GetComponent<AbilityController>();
        car = rb.GetComponent<Transform>();
        targetIndicator = grappleGun.GetComponent<TargetIndicator>();
        players.AddRange(GameObject.FindGameObjectsWithTag("OpponentCar"));
        foreach (GameObject player in players)
        {
            playerCars.Add(player.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(abilityController.ability != null && abilityController.ability.a_Name == "Grapple")
        {
            targetBox();
            if (isGrappling)
            {
                hookSetTime += Time.deltaTime;
                if (!hookSet)
                {
                    ShootHook();
                    SetJoint();
                    hookSet = true;
                    hookSetTime = 0;
                }
                if (hookSet && hookSetTime > 0.25f)
                {
                    //arcadeCar.steerAngleLimit = 0;
                    //steering.m_CurrAngle = 0;
                    Rotate();
                    Pull();
                }
            }
        }
    }

    public void Pull()
    {
        Vector3 difference = gunTip.position - hookPoint;

        if (Vector3.Distance(gunTip.position, hookPoint) > 1)
        {
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce((hookPoint - gunTip.position).normalized * speed, ForceMode.VelocityChange);
            }
            if(hitPlayer != null)
            {
                hookPoint = hitPlayer.transform.position;
            }
        }
        else
        {
            StopGrapple();
        }

        if(hitPlayer != null)
        {
            if (Vector3.Distance(gunTip.position, hitPlayer.position) < 4)
            {
                StopGrapple();
            }
        }
    }

    public void Rotate()
    {
        //ackermann.SetAngle(0);
        Quaternion targetDirection = Quaternion.LookRotation(hookPoint - car.position);

        float step = rotationSpeed * Time.deltaTime/2;

        car.rotation = Quaternion.RotateTowards(grappleGun.rotation, targetDirection, step);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public void StopGrapple()
    {
        isGrappling = false;
        Destroy(joint);
        hookSet = false;
        //steering.rate = 90;
        hitPlayer = null;
    }

    public void ShootHook()
    {
        FindVisibleTargets();
        RaycastHit hit;
        //Speciaal point voor grapplinghook
        if (visibleTargets.Count > 0)
        {
            Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, (1 << LayerMask.NameToLayer("Grapple") | (1 << LayerMask.NameToLayer("Ignore Raycast"))));
            hookPoint = hit.point;
            hitPlayer = GetClosestPlayer(playerCars);
        }
        else if (Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, (1 << LayerMask.NameToLayer("Grapple") | (1 << LayerMask.NameToLayer("Default")))))
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

    public void targetBox()
    {
        targetIndicator.targets.Clear();
        for (int i = 0; i < playerCars.Count; i++)
        {
            Transform target = playerCars[i].transform;
            MeshCollider cTarget = players[i].GetComponentInChildren<MeshCollider>();
            Vector3 dirToTarget = (target.position - gunTip.position).normalized;
            if (Vector3.Angle(gunTip.forward, dirToTarget) < playerTargetAngle / 2)
            {
                float dstToTarget = Vector3.Distance(gunTip.position, target.position);

                if (!Physics.Raycast(gunTip.position, dirToTarget, dstToTarget, objectLayer))
                {
                    if (dstToTarget <= range)
                    {
                        targetIndicator.targets.Add(cTarget);
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
