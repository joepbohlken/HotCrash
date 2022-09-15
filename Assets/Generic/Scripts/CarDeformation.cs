using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarDeformation : MonoBehaviour
{
    [System.Serializable]
    protected class DeformableMesh
    {
        public Mesh originalMesh;
        public MeshFilter meshFilter;
        [Tooltip("The total damage that is allowed to be dealt to this mesh before it breaks off of the car.")]
        public float maxAllowedDamage = 20;
        [HideInInspector] public MeshCollider meshCollider;
    }

    [Tooltip("Minimum velocity required on impact to deform car meshes.")]
    [SerializeField] private float minVelocity = 3f;
    [Tooltip("Strength of the mesh deformation.")]
    [SerializeField][Range(1, 1000)] private int deformStrength = 10;
    [SerializeField] private List<DeformableMesh> carMeshes = new List<DeformableMesh>();

    private Rigidbody myRigidbody;
    private float currentVelocity = 0f;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();

        foreach (DeformableMesh deformableMesh in carMeshes)
        {
            deformableMesh.meshFilter.mesh = Instantiate(deformableMesh.originalMesh);
            deformableMesh.meshCollider = deformableMesh.meshFilter.GetComponent<MeshCollider>();
        }
    }

    private void Update()
    {
        currentVelocity = myRigidbody.velocity.magnitude;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentVelocity >= minVelocity)
        {
            DeformableMesh hitMesh = carMeshes.FirstOrDefault(m => m.meshCollider == collision.GetContact(0).thisCollider);
            if (hitMesh == null) { return; }

            Vector3 otherPos = collision.GetContact(0).otherCollider.transform.position;
            otherPos.y = 0;
            Vector3 meshPos = hitMesh.meshCollider.transform.position;
            meshPos.y = 0;
            Vector3 hitDirection = (otherPos - meshPos).normalized;

            hitMesh.maxAllowedDamage -= (currentVelocity - minVelocity);
            if (hitMesh.maxAllowedDamage <= 0f)
            {
                hitMesh.meshCollider.transform.SetParent(null, true);
                Rigidbody meshRigidbody = hitMesh.meshCollider.gameObject.AddComponent<Rigidbody>();
                meshRigidbody.AddForce(hitDirection * 10, ForceMode.Impulse);
                carMeshes.Remove(hitMesh);
                return;
            }

            Vector3 impactPoint = hitMesh.meshCollider.transform.InverseTransformPoint(collision.GetContact(0).point);
            float strength = (currentVelocity - minVelocity) / 1000f * (float)deformStrength;

            Vector3[] vertices = hitMesh.meshFilter.mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (Vector3.Distance(impactPoint, vertices[i]) <= 0.2f)
                {
                    vertices[i] -= hitDirection * strength;
                }
            }

            hitMesh.meshFilter.mesh.SetVertices(vertices);
            hitMesh.meshCollider.sharedMesh = hitMesh.meshFilter.mesh;
        }
    }
}
