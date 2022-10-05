using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter))]
public class DeformablePart : MonoBehaviour
{
    [Tooltip("The total damage that is allowed to be dealt to this mesh before it breaks off of the car.")]
    [SerializeField] private float maxAllowedDamage = 50f;
    
    [Space(12)]
    public bool detachable = false;
    public bool isHinge = false;
    [HideInInspector] public Vector3 hingeAnchor;
    [HideInInspector] public Vector3 hingeAxis;
    [HideInInspector] public float hingeMinLimit;
    [HideInInspector] public float hingeMaxLimit;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private HingeJoint hinge;
    private CarDeformation carDeformation;

    private bool hingeCreated = false;
    private bool isDestroyed = false;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh.MarkDynamic();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hingeCreated && carDeformation)
        {
            carDeformation.OnCollision(collision);
        }
    }

    public bool ApplyDamage(int i, Collision collision, float minVelocity, float deformRadius, float deformStrength)
    {
        if (isDestroyed) return true;

        Vector3 hitDirection = meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);

        maxAllowedDamage -= (collision.relativeVelocity.magnitude - minVelocity);
        if (maxAllowedDamage <= 0f && detachable)
        {
            DetachPart(hitDirection, -collision.relativeVelocity.magnitude);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DeformPart(int i, Collision collision, float deformRadius, float deformStrength, Rigidbody body)
    {
        Vector3 hitDirection = meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);
        Vector3 impactPoint = meshCollider.transform.InverseTransformPoint(collision.GetContact(i).point);
        Vector3[] vertices = meshFilter.mesh.vertices;

        for (int j = 0; j < vertices.Length; j++)
        {
            float distance = (impactPoint - vertices[j]).magnitude;
            if (distance <= deformRadius)
            {
                vertices[j] += hitDirection * (deformRadius - distance) * deformStrength;
            }
        }

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;

        if (isHinge && !hingeCreated) CreateHinge(body);
    }

    private void DetachPart(Vector3 hitDirection, float force)
    {
        isDestroyed = true;
        if (isHinge) Destroy(hinge);
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

        carDeformation = GetComponentInParent<CarDeformation>();
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
