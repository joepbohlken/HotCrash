using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Push")]
public class PushAbility : Ability
{
    private Transform gunTip;
    public GameObject glove;
    public LayerMask groundMask;
    private GameObject closestCar;

    public float range;
    public float angle = 60;

    private bool readytoThrow;
    private MeshCollider targetCollider;

    public override void Obtained()
    {
        base.Obtained();

        readytoThrow = true;
        gunTip = abilityController.transform.Find("GunTip");
        //if (!carController.isBot) indicator.carCamera = abilityController.playerCamera;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        closestCar = null;
        closestCar = GetClosestCar();
        if (closestCar)
        {
            targetCollider = closestCar.transform.Find("Body").GetComponent<MeshCollider>();
        }

        if (abilityController.hud)
        {
            if (closestCar) abilityController.hud.TargetingTextEnable();
            else abilityController.hud.TargetingTextDisable();
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
        if (abilityController.hud) abilityController.hud.TargetingTextDisable();
        if (!isDestroyed) abilityController.AbilityEnded();
    }

    private void Throw()
    {
        readytoThrow = false;
        if (closestCar != null)
        {
            Vector3 dirToTarget = (closestCar.transform.position + Vector3.up - gunTip.position).normalized;

            Quaternion throwRotation = Quaternion.Euler(dirToTarget.x, 90, 90);

            GameObject projectile = Instantiate(glove, gunTip.position, throwRotation);

            GloveAddon projectileScript = projectile.GetComponentInChildren<GloveAddon>();
            projectileScript.target = closestCar.transform;
        }
        AbilityEnded(false);
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
            if (isWithinView && distance < closestDistance && distance < range)
            {
                Vector3 direction = (car.transform.position - carController.transform.position).normalized;
                if (!Physics.Raycast(carController.transform.position, direction, distance, groundMask))
                {
                    closestDistance = distance;
                    closestCar = car.gameObject;
                }
            }
        }

        return closestCar;
    }
}
