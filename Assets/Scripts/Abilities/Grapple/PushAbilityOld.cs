using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/PushOld")]
public class PushAbilityOld : Ability
{
    [SerializeField] public Vector3 hookPoint;
    [NonSerialized] public Transform gunTip;
    [SerializeField] private Transform car;

    public static bool fistShoot;
    private bool hookSet = false;
    private SpringJoint joint;
    private Vector3 dirToPlayer;
    public Transform hitPlayer = null;
    public List<Transform> visibleTargets = new List<Transform>();
    private Spring spring;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;

    [SerializeField]
    List<GameObject> players;
    [SerializeField]
    List<Transform> playerCars;

    [SerializeField] float range = 10f;
    [SerializeField] float playerTargetAngle = 60f;
    [SerializeField] float hookSetTime = 0.25f;
    [SerializeField] float pushForce = 35f;

    public bool allowMultipleTargets = false;

    public List<MeshCollider> targets;

    public override void Obtained()
    {
        base.Obtained();

        car = abilityController.transform;
        gunTip = car.Find("Guntip");
        players.AddRange(GameObject.FindGameObjectsWithTag("Car"));
        foreach (GameObject player in players)
        {
            playerCars.Add(player.transform);
        }
        lr = car.GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        targetBox();
        DrawRope();
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
                if (hitPlayer != null)
                {
                    CarPush();
                }
                FistHit();
                
            }
        }
    }

    public override void Activated()
    {
        base.Activated();

        fistShoot = true;
    }

    public override void CarDestroyed()
    {
        base.CarDestroyed();

        AbilityEnded(true);
    }

    private void AbilityEnded(bool isDestroyed)
    {
        if (!isDestroyed) abilityController.AbilityEnded();
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
        AbilityEnded(false);
    }

    public void ShootHook()
    {
        RaycastHit hit;
        if (visibleTargets.Count > 0)
        {
            Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, LayerMask.NameToLayer("Ignore Raycast"));
            hookPoint = hit.point;
            hitPlayer = GetClosestPlayer();
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
        targets.Clear();
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
                        targets.Add(cTarget);
                        visibleTargets.Add(target);
                    }
                }
                else if (Physics.Raycast(gunTip.position + Vector3.up, gunTip.TransformDirection(Vector3.forward), out hit, range, LayerMask.NameToLayer("Ignore Raycast")))
                {
                    hookPoint = hit.point;
                    hitPlayer = GetClosestPlayer();

                    if (dstToTarget <= range + 2)
                    {
                        targets.Add(cTarget);
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

    public Transform GetClosestPlayer()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        foreach (Transform t in visibleTargets)
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

    

    public int quality = 100;
    public float damper = 14;
    public float strength = 800;
    public float velocity = 30;
    public float waveCount = 3;
    public float waveHeight = 1;
    public AnimationCurve affectCurve;

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!IsGrappling())
        {
            currentGrapplePosition = gunTip.position;
            spring.Reset();
            if (lr.positionCount > 0)
                lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grapplePoint;

        if (hitPlayer != null)
        {
            grapplePoint = hitPlayer.transform.position;
        }
        else
        {
            grapplePoint = hookPoint;
        }

        var gunTipPosition = gunTip.position;
        var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

        for (var i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
                         affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
