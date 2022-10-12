using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class FistPush : MonoBehaviour
{
    [SerializeField] public Vector3 hookPoint;
    //[NonSerialized] 
    public Transform gunTip;
    [SerializeField] private Transform car;
    [SerializeField] private AbilityController abilityController;

    public static bool fistShoot;
    private bool hookSet = false;
    private SpringJoint joint;
    private Vector3 dirToPlayer;
    private TargetIndicator targetIndicator;
    public Transform hitPlayer = null;
    public List<Transform> visibleTargets = new List<Transform>();

    [SerializeField]
    List<GameObject> players;
    [SerializeField]
    List<Transform> playerCars;

    [SerializeField] float range = 10f;
    [SerializeField] float playerTargetAngle = 60f;
    [SerializeField] float hookSetTime = 0.25f;
    [SerializeField] float pushForce = 100000f;

    // Start is called before the first frame update
    void Start()
    {
        car = gameObject.transform;
        gunTip = car.Find("GunTip");
        abilityController = car.GetComponent<AbilityController>();
        targetIndicator = car.GetComponent<TargetIndicator>();
        players.AddRange(GameObject.FindGameObjectsWithTag("Car"));
        foreach (GameObject player in players)
        {
            playerCars.Add(player.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(abilityController.Ability != null && abilityController.Ability.a_Name == "Fist")
        {
            targetBox();
            if (fistShoot)
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
                    if(hitPlayer != null)
                    {
                        CarPush();
                    }
                    FistHit();
                }
            }
        }
    }

    public void CarPush()
    {
        Rigidbody hitRb = hitPlayer.GetComponent<Rigidbody>();
        hitRb.AddForce(dirToPlayer * pushForce, ForceMode.VelocityChange);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public void FistHit()
    {
        fistShoot = false;
        Destroy(joint);
        hookSet = false;
        hitPlayer = null;
    }

    public void ShootHook()
    {
        RaycastHit hit;
        if (visibleTargets.Count > 0)
        {
            Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, LayerMask.NameToLayer("Ignore Raycast"));
            hookPoint = hit.point;
            hitPlayer = GetClosestPlayer(playerCars);
            dirToPlayer = (hitPlayer.position - gunTip.position).normalized;
        }
        else
        {
            hookPoint = gunTip.position + car.forward * 5;
        }
    }

    public void targetBox()
    {
        RaycastHit hit;
        visibleTargets.Clear();
        targetIndicator.targets.Clear();
        for (int i = 0; i < playerCars.Count; i++)
        {
            Transform target = playerCars[i].transform;
            MeshCollider cTarget = players[i].GetComponentInChildren<MeshCollider>();
            Vector3 dirToTarget = (target.position - gunTip.position).normalized;
            if (Vector3.Angle(gunTip.forward, dirToTarget) < playerTargetAngle / 2)
            {
                float dstToTarget = Vector3.Distance(gunTip.position, target.position);

                if (!Physics.Raycast(gunTip.position, dirToTarget, dstToTarget, 0))
                {
                    if (dstToTarget <= range)
                    {
                        targetIndicator.targets.Add(cTarget);
                        visibleTargets.Add(target);
                    }
                }
                else if(Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, LayerMask.NameToLayer("Ignore Raycast")))
                {
                    hookPoint = hit.point;
                    hitPlayer = GetClosestPlayer(playerCars);

                    if (dstToTarget <= range + 2)
                    {
                        targetIndicator.targets.Add(cTarget);
                        visibleTargets.Add(hitPlayer);
                    }
                }
            }
        }
    }

    public void SetJoint()
    {
        joint = car.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hookPoint;

        float distanceFromPoint = Vector3.Distance(car.position, hookPoint);

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
