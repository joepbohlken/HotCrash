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
    public List<MeshCollider> carList;
    public GameObject carContainer;
    public LayerMask groundMask;

    public float range;
    public float angle = 60;

    private bool readytoThrow;
    private MeshCollider targetCollider;

    public override void Obtained()
    {
        base.Obtained();

        readytoThrow = true;
        car = abilityController.transform;
        gunTip = car.Find("GunTip");
        carContainer = GameObject.Find("Cars");
        foreach(Transform t in carContainer.transform)
        {
            carList.Add(t.GetComponentInChildren<MeshCollider>());
        }
        //if (!carController.isBot) indicator.carCamera = abilityController.playerCamera;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        targetBox();

        if (abilityController.hud)
        {
            if (targetCollider) abilityController.hud.TargetingReticle(targetCollider);
            else abilityController.hud.DisableTargeting();
        }
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
        GameObject closestCar = GetClosestCar();
        if (closestCar)
        {
            if (!carController.isBot) Debug.Log("Hey hallo!");
            targetCollider = closestCar.transform.Find("Body").GetComponent<MeshCollider>();
        }
    }

    private void FieldOfViewCheck()
    {
        foreach(MeshCollider col in carList)
        {
            Transform target = col.transform;
            Vector3 directionToTarget = (target.position - gunTip.position).normalized;

            if (Vector3.Angle(gunTip.forward, directionToTarget) < angle / 2)
            {
                if (Physics.Raycast(gunTip.position, directionToTarget, range, groundMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }

        closestTarget = GetClosestPlayer();
        if(abilityController.hud != null)
        {
            if (visibleTargets.Count() == 0)
            {
                targetCollider = null;
            }
            else
            {
                targetCollider = closestTarget.GetComponentInChildren<MeshCollider>();
            }
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

    private GameObject GetClosestCar()
    {
        GameObject closestCar = null;
        float closestDistance = 999f;

        for (int i = 0; i < carController.transform.parent.childCount; i++)
        {
            Transform car = carController.transform.parent.GetChild(i);
            if (car == carController.transform) continue;

            bool isWithinView = Vector3.Angle(carController.transform.forward, (car.transform.position - carController.transform.position).normalized) <= angle;

            float distance = (car.transform.position - carController.transform.position).magnitude;
            if (isWithinView && distance < closestDistance)
            {
                Vector3 direction = (carController.transform.position - car.transform.position).normalized;
                if (Physics.Raycast(gunTip.position, direction, range, groundMask))
                {
                    closestDistance = distance;
                    closestCar = car.gameObject;
                }  
            }
        }

        return closestCar;
    }
}
