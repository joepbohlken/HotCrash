using UnityEngine;
using static CarStateController;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter))]
public class DeformablePart : MonoBehaviour
{
    [Tooltip("The total damage that is allowed to be dealt to this mesh before it breaks off of the car.")]
    [SerializeField] private float maxAllowedDamage = 50f;
    [HideInInspector] public float MaxAllowedDamage { get { return maxAllowedDamage; } }

    [Space(12)]

    public bool detachable = false;
    [Tooltip("If true will create a hinge with the specified properties on first contact hit.")]
    public bool isHinge = false;
    [HideInInspector] public Vector3 hingeAnchor;
    [HideInInspector] public Vector3 hingeAxis;
    [HideInInspector] public float hingeMinLimit;
    [HideInInspector] public float hingeMaxLimit;

    [HideInInspector] public List<VitalType> vitalTypes = new List<VitalType>();
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private HingeJoint hinge;
    private CarDeformation carDeformation;
    private CarStateController carStateController;

    private bool hingeCreated = false;
    private bool isDestroyed = false;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        // Instantiate a 'clone' of the mesh, so it does not affect all other objects using the same mesh
        meshFilter.mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh.MarkDynamic();

        carDeformation = GetComponentInParent<CarDeformation>();

        if (vitalTypes.Count > 0) carStateController = GetComponentInParent<CarStateController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Forward the collision event to the CarDeformation script of the car
        // As the car won't receive these events anymore, because the hinge adds a rigidbody to this object
        if (hingeCreated && carDeformation)
        {
            carDeformation.OnCollision(collision);
        }
    }

    ///<summary>Applies damage to the part based on the given parameters.
    ///Return true if the part has been detached or already destroyed, else returns false.</summary>
    public bool ApplyDamage(int i, Collision collision, float minVelocity, float deformRadius, float deformStrength, Rigidbody body)
    {
        if (isDestroyed) return true;

        float damage = (collision.relativeVelocity.magnitude - minVelocity);
        maxAllowedDamage -= damage;
        // Update the car vitals on the UI
        if (carStateController) carStateController.AddVitalDamage(vitalTypes, damage);

        // Create a hinge on first collision
        if (isHinge && !hingeCreated) CreateHinge(body);

        maxAllowedDamage -= (collision.relativeVelocity.magnitude - minVelocity);
        if (maxAllowedDamage <= 0f && detachable)
        {
            // Get the direction of the collision in local space of the hit mesh
            Vector3 hitDirection = meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);

            DetachPart(hitDirection, -collision.relativeVelocity.magnitude);
            return true;
        }
        else
        {
            return false;
        }
    }

    ///<summary>Deforms the mesh with the given parameters.</summary>
    public void DeformPart(int i, Collision collision, float deformRadius, float deformStrength)
    {
        // Get the direction of the collision in local space of the hit mesh
        Vector3 hitDirection = meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);
        // Get the impact position of the collision in local space of the hit mesh
        Vector3 impactPoint = meshCollider.transform.InverseTransformPoint(collision.GetContact(i).point);
        Vector3[] vertices = meshFilter.mesh.vertices;


        for (int j = 0; j < vertices.Length; j++)
        {
            float distance = (impactPoint - vertices[j]).magnitude;
            if (distance <= deformRadius)
            {
                // Reposition the vertice
                vertices[j] += hitDirection * (deformRadius - distance) * deformStrength;
            }
        }

        // Apply modifications to the mesh
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    private void DetachPart(Vector3 hitDirection, float force)
    {
        isDestroyed = true;
        // Destroy the hinge if present
        if (isHinge) Destroy(hinge);
        // Add a force to the part in the direction of the collision
        transform.SetParent(null, true);
        Rigidbody myRigidbody = GetComponent<Rigidbody>();
        if (myRigidbody == null) myRigidbody = gameObject.AddComponent<Rigidbody>();
        myRigidbody.AddForce(hitDirection.normalized * force, ForceMode.Impulse);
    }

    private void CreateHinge(Rigidbody body)
    {
        hingeCreated = true;

        hinge = gameObject.AddComponent<HingeJoint>();
        hinge.connectedBody = body;
        hinge.anchor = hingeAnchor;
        hinge.axis = hingeAxis;
        hinge.useLimits = true;

        JointLimits limits = new JointLimits();
        limits.min = hingeMinLimit;
        limits.max = hingeMaxLimit;
        limits.bounciness = 0.4f;
        hinge.limits = limits;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DeformablePart))]
public class HingeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = target as DeformablePart;

        if (myScript.isHinge)
        {
            GUILayout.Label("Hinge Properties:", EditorStyles.boldLabel);
            myScript.hingeAnchor = EditorGUILayout.Vector3Field("Anchor", myScript.hingeAnchor);
            myScript.hingeAxis = EditorGUILayout.Vector3Field("Axis", myScript.hingeAxis);
            myScript.hingeMinLimit = EditorGUILayout.FloatField("Min Limit", myScript.hingeMinLimit);
            myScript.hingeMaxLimit = EditorGUILayout.FloatField("Max Limit", myScript.hingeMaxLimit);
        }
    }
}
#endif
