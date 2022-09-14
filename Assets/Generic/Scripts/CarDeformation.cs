using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDeformation : MonoBehaviour
{
    [SerializeField] private Mesh originalMesh;
    [SerializeField] private MeshFilter meshToDeform;

    private bool canGetDamaged = true;

    private void Start()
    {
        meshToDeform.mesh = Instantiate(originalMesh);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canGetDamaged) { return; }
        canGetDamaged = false;

        Vector3 impactPoint = meshToDeform.transform.InverseTransformPoint(collision.GetContact(0).point);
        Vector3[] vertices = meshToDeform.mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (Vector3.Distance(impactPoint, vertices[i]) <= 1f)
            {
                vertices[i] -= vertices[i]/10;
            }
        }

        meshToDeform.mesh.SetVertices(vertices);
        meshToDeform.GetComponent<MeshCollider>().sharedMesh = meshToDeform.mesh;

        StartCoroutine(DamageDebounce());
    }

    private IEnumerator DamageDebounce()
    {
        yield return new WaitForSeconds(0.1f);
        canGetDamaged = true;
    }
}
