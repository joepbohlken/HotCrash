using System.Collections;
using UnityEngine;

public class MineObject : MonoBehaviour
{
    [SerializeField] private Renderer highlight;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Space(12)]
    [SerializeField] private float explosionRadius = 10;
    [SerializeField] private float explosionStrength = 10000f;
    [SerializeField] private float explosionDamage = 50f;

    private Transform owner;
    private Transform cars;
    private bool isSetUp = false;
    private bool isTriggered = false;

    public void SetUpMine(Transform owner, Transform cars, bool isBot, AbilityController abilityController)
    {
        this.owner = owner;
        this.cars = cars;
        if (isBot) highlight.enabled = false;
        else
        {
            highlight.gameObject.layer = LayerMask.NameToLayer("Player " + (abilityController.playerIndex + 1));
            highlight.material.SetColor("_EmissionColor", abilityController.playerColor);
        }
        isSetUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSetUp || other.transform.parent == owner || isTriggered) return;

        isTriggered = true;

        for (int i = 0; i < cars.childCount; i++)
        {
            Transform car = cars.GetChild(i);
            float distance = (car.position - transform.position).magnitude;

            if (distance <= explosionRadius)
            {
                car.GetComponent<Rigidbody>().AddExplosionForce(explosionStrength, transform.position, explosionRadius, 100f, ForceMode.Impulse);
                car.GetComponent<CarHealth>().AddCarDamage(owner.gameObject, HitLocation.BOTTOM, explosionDamage);
            }
        }

        StartCoroutine(Explosion());
    }

    private IEnumerator Explosion()
    {
        GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        highlight.enabled = false;

        yield return new WaitForSeconds(2f);
        Destroy(explosion);
        Destroy(gameObject);
    }
}
