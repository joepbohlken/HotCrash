using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarDeformation : MonoBehaviour
{
    [Header("Deformation Properties")]
    [Tooltip("Minimum velocity required on impact to deform car meshes.")]
    [SerializeField] private float minVelocity = 3f;
    [HideInInspector] public float MinVelocity { get { return minVelocity; } }
    [Tooltip("Max distance of a mesh vertice from the impact point to become deformed.")]
    [SerializeField] private float deformRadius = 0.5f;
    [Tooltip("Strength of the mesh deformation.")]
    [SerializeField] private float deformStrength = 1f;

    [Header("Performance Properties")]
    [Tooltip("Minimum time in seconds between deformations.")]
    [SerializeField] private float collisionDebounce = 0.1f;
    [Tooltip("Maximum collision points to use when deforming.")]
    [SerializeField] private int maxCollisionPoints = 2;

    private Rigidbody myRigidbody;
    private float currentDebounce = 0f;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        currentDebounce = Mathf.Max(0, currentDebounce - Time.fixedDeltaTime);
    }

    public void OnCollision(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= minVelocity && collision.gameObject.CompareTag("Car"))
        {
            // Debounce between deformations to improve performance
            bool canDeform = currentDebounce <= 0f;
            if (canDeform) currentDebounce = collisionDebounce;

            // Go through each contact point of the collision
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Limit the contact points to improve performance
                if (i == maxCollisionPoints) break;

                // Check if the hit object can be deformed (does it contain the DeformablePart script)
                DeformablePart hitPart = collision.GetContact(i).thisCollider.GetComponent<DeformablePart>();
                if (hitPart != null)
                {
                    // Apply damage and only deform if the part has not been destroyed
                    bool partDestroyed = hitPart.ApplyDamage(i, collision, minVelocity, deformRadius, deformStrength, myRigidbody);
                    if (!partDestroyed && canDeform)
                    {
                        hitPart.DeformPart(i, collision, deformRadius, deformStrength);
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision(collision);
    }
}