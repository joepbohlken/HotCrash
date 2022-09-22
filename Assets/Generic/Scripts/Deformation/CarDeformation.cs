using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarDeformation : MonoBehaviour
{
    [Tooltip("Minimum velocity required on impact to deform car meshes.")]
    [SerializeField] private float minVelocity = 3f;
    [Tooltip("Max distance of a mesh vertice from the impact point to become deformed.")]
    [SerializeField] private float deformRadius = 0.5f;
    [Tooltip("Strength of the mesh deformation.")]
    [SerializeField] private float deformStrength = 1f;

    private Rigidbody myRigidbody;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    public void OnCollision(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= minVelocity)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
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
