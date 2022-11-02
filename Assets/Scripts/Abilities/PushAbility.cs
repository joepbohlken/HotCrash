using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Cinemachine.CinemachineTargetGroup;
using static UnityEditor.Experimental.GraphView.Port;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Abilities/Push")]
public class PushAbility : Ability
{
    public Transform car;
    public Transform gunTip;
    public Transform closestTarget;
    public GameObject glove;
    public List<Transform> visibleTargets = new List<Transform>();
    public LayerMask playerMask;
    public TargetIndicator indicator;
    public List<MeshCollider> playerList;

    public float range = 30;
    public float angle = 60;

    private bool readytoThrow;

    public override void Obtained()
    {
        base.Obtained();

        readytoThrow = true;
        car = abilityController.transform;
        gunTip = car.Find("GunTip");
        indicator = car.GetComponent<TargetIndicator>();
        indicator.targetCamera = abilityController.playerCamera;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        targetBox();
    }

    public override void Activated()
    {
        base.Activated();

        if (readytoThrow)
        {
            Throw();
        }
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

    private void Throw()
    {
        readytoThrow = false;
        if (closestTarget != null)
        {
            Vector3 dirToTarget = (closestTarget.position + Vector3.up - gunTip.position).normalized;

            Quaternion throwRotation = Quaternion.Euler(dirToTarget.x, 90, 90);

            GameObject projectile = Instantiate(glove, gunTip.position, throwRotation);

            GloveAddon projectileScript = projectile.GetComponentInChildren<GloveAddon>();
            projectileScript.target = closestTarget;
        }
        AbilityEnded(false);
    }    

    public void targetBox()
    {
        visibleTargets.Clear();
        closestTarget = null;
        FieldOfViewCheck();
        closestTarget = GetClosestPlayer();
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(gunTip.position, range, playerMask);

        if (rangeChecks.Length != 0)
        {
            for (int i = 0; i < rangeChecks.Length; i++)
            {
                string tag = rangeChecks[i].GetComponentInParent<Transform>().parent.tag;
                if (tag == "Car")
                {
                    Transform target = rangeChecks[i].transform;
                    Vector3 directionToTarget = (target.position - gunTip.position).normalized;

                    if (Vector3.Angle(gunTip.forward, directionToTarget) < angle / 2)
                    {
                        float distanceToTarget = Vector3.Distance(gunTip.position, target.position);

                        if (!Physics.Raycast(gunTip.position, directionToTarget, distanceToTarget, LayerMask.NameToLayer("Ground")))
                        {
                            indicator.target = rangeChecks[i];
                            visibleTargets.Add(target);
                        }
                    }
                }
            }
        }
        else
        {
            indicator.target = null;
        }
    }

    public Transform GetClosestPlayer()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        foreach (Transform t in visibleTargets)
        {
            float dist = Vector3.Distance(t.position, gunTip.position);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }
}
