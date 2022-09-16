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
    [Tooltip("Max distance of a mesh vertice from the impact point to become deformed.")]
    [SerializeField] private float deformRadius = 0.5f;
    [Tooltip("Strength of the mesh deformation.")]
    [SerializeField] private float deformStrength = 1f;
    [SerializeField] private List<DeformableMesh> carMeshes = new List<DeformableMesh>();

    private void Start()
    {
        foreach (DeformableMesh deformableMesh in carMeshes)
        {
            deformableMesh.meshFilter.mesh = Instantiate(deformableMesh.originalMesh);
            deformableMesh.meshFilter.mesh.MarkDynamic();
            deformableMesh.meshCollider = deformableMesh.meshFilter.GetComponent<MeshCollider>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= minVelocity)
        {
            DeformableMesh hitMesh = carMeshes.FirstOrDefault(m => m.meshCollider == collision.GetContact(0).thisCollider);
            if (hitMesh == null) { return; }

            Vector3 hitDirection = hitMesh.meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);

            hitMesh.maxAllowedDamage -= (collision.relativeVelocity.magnitude - minVelocity);
            if (hitMesh.maxAllowedDamage <= 0f)
            {
                hitMesh.meshCollider.transform.SetParent(null, true);
                hitMesh.meshCollider.gameObject.AddComponent<Rigidbody>().AddForce(hitDirection.normalized * 10, ForceMode.Impulse);
                carMeshes.Remove(hitMesh);
                return;
            }

            Vector3 impactPoint = hitMesh.meshCollider.transform.InverseTransformPoint(collision.GetContact(0).point);
            Vector3[] vertices = hitMesh.meshFilter.mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = (impactPoint - vertices[i]).magnitude;
                if (distance <= deformRadius)
                {
                    vertices[i] += hitDirection * (deformRadius - distance) * deformStrength;
                }
            }

            hitMesh.meshFilter.mesh.vertices = vertices;
            hitMesh.meshFilter.mesh.RecalculateNormals();
            hitMesh.meshFilter.mesh.RecalculateBounds();
            hitMesh.meshCollider.sharedMesh = hitMesh.meshFilter.mesh;
        }
    }
}
