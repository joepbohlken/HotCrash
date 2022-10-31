using UnityEngine;

public class MineObject : MonoBehaviour
{
    [SerializeField] private Renderer highlight;

    [Space(12)]
    [SerializeField] private float explosionRadius = 10;
    [SerializeField] private float explosionStrength = 10000f;

    private Transform owner;
    private Transform cars;
    private bool isSetUp = false;

    public void SetUpMine(Transform owner, Transform cars, bool isBot, Color highlightColor, int playerIndex)
    {
        this.owner = owner;
        this.cars = cars;
        if (isBot) highlight.enabled = false;
        else
        {
            highlight.gameObject.layer = LayerMask.NameToLayer("Player " + playerIndex);
            highlight.material.SetColor("_EmissionColor", highlightColor);
        }
        isSetUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSetUp || other.transform.parent == owner) return;

        for (int i = 0; i < cars.childCount; i++)
        {
            Transform car = cars.GetChild(i);
            float distance = (car.position - transform.position).magnitude;

            if (distance <= explosionRadius)
            {
                car.GetComponent<Rigidbody>().AddExplosionForce(explosionStrength, transform.position, explosionRadius, 100f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
