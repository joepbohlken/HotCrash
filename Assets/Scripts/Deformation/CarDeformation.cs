using System.Collections.Generic;
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
    [Tooltip("Maximum distance a vertex can deform from its original position: x-axis being for the side, y-axis being the top side, z-axis being the front and back")]
    [SerializeField] private Vector3 maxTotalDeformDistance = new Vector3(.2f, .3f, .5f);

    [Header("Performance Properties")]
    [Tooltip("Minimum time in seconds between deformations.")]
    [SerializeField] private float collisionDebounce = 0.1f;
    [Tooltip("Maximum collision points to use when deforming.")]
    [SerializeField] private int maxCollisionPoints = 1;

    [Header("Debug")]
    public bool debugMode = true;

    private Rigidbody myRigidbody;
    private CarController carController;
    private float currentDebounce = 0f;
    private float currentCollisionAngle;

    private Vector3 hitOrigin;
    private Vector3 hitDirection;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
    }

    private void FixedUpdate()
    {
        currentDebounce = Mathf.Max(0, currentDebounce - Time.fixedDeltaTime);
    }

    public void OnCollision(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= minVelocity)
        {
            bool deformedMesh = false;

            // Debounce between deformations to improve performance
            bool canDeform = currentDebounce <= 0f;
            if (canDeform) currentDebounce = collisionDebounce;

            hitOrigin = Vector3.zero;
            hitDirection = Vector3.zero;

            // Go through each contact point of the collision
            for (int i = 0; i < collision.contactCount; i++)
            {
                // Limit the contact points to improve performance
                if (i == maxCollisionPoints) break;

                if (debugMode)
                {
                    hitOrigin += collision.GetContact(i).point;
                    hitDirection += collision.GetContact(i).normal;
                }

                currentCollisionAngle = Vector3.Dot(collision.GetContact(i).normal, transform.up);

                // Check if the hit object can be deformed (does it contain the DeformablePart script)
                DeformablePart hitPart = collision.GetContact(i).thisCollider.GetComponent<DeformablePart>();
                if (hitPart != null)
                {
                    bool hitBottom = currentCollisionAngle > 0.7f;

                    bool partDestroyed = false;
                    if ((collision.gameObject.CompareTag("Ground") && !hitBottom) || collision.gameObject.CompareTag("Car"))
                    {
                        // Apply damage and only deform if the part has not been destroyed
                        partDestroyed = hitPart.ApplyDamage(i, collision, minVelocity, myRigidbody);
                    }

                    if (!partDestroyed && canDeform && !hitBottom)
                    {
                        deformedMesh = true;
                        hitPart.DeformPart(i, collision, deformRadius, carController.isDestroyed ? Vector3.one * 10 : maxTotalDeformDistance, deformStrength);
                    }
                }
            }

            if (debugMode && deformedMesh)
            {
                hitOrigin /= collision.contactCount;
                hitDirection /= collision.contactCount;

                Debug.DrawRay(hitOrigin, hitDirection * .5f, Color.cyan, 10);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision(collision);
    }
}
