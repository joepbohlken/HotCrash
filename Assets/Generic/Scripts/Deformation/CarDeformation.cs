using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarDeformation : MonoBehaviour
{
    [Header("Deformation Properties")]
    [Tooltip("Minimum velocity required on impact to deform car meshes.")]
    [SerializeField] private float minVelocity = 3f;
    [Tooltip("Max distance of a mesh vertice from the impact point to become deformed.")]
    [SerializeField] private float deformRadius = 0.5f;
    [Tooltip("Strength of the mesh deformation.")]
    [SerializeField] private float deformStrength = 1f;

    [Header("Performance Properties")]
    [Tooltip("Minimum time in seconds between collisions.")]
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
        if (currentDebounce <= 0f && collision.relativeVelocity.magnitude >= minVelocity)
        {
            currentDebounce = collisionDebounce;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (i == maxCollisionPoints) break;

                DeformablePart hitPart = collision.GetContact(i).thisCollider.GetComponent<DeformablePart>();
                if (hitPart != null) hitPart.ApplyDamage(i, collision, minVelocity, deformRadius, deformStrength, myRigidbody);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision(collision);
    }
}
