using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum HitLocation
{
    FRONT,
    BACK,
    LEFT,
    RIGHT,
    TOP,
    BOTTOM,
    NONE
}

[RequireComponent(typeof(MeshFilter))]
public class DeformablePart : MonoBehaviour
{
    public MeshCollider meshCollider;
    [Tooltip("The side which is affected when impacted. Set to NONE in case it handles multiple sides like the body.")]
    public HitLocation carSide = HitLocation.NONE;

    [Space(12)]
    [Tooltip("If true will create a hinge with the specified properties on first contact hit.")]
    public bool isHinge = false;
    [Tooltip("The total damage that is allowed to be dealt to this mesh before it breaks off of the car.")]
    [HideInInspector] public float health = 50f;
    private float currentHealth;
    [Range(0, 1)]
    [HideInInspector] public float hingeCreationThreshold = 1;
    [HideInInspector] public Vector3 hingeAnchor;
    [HideInInspector] public Vector3 hingeAxis;
    [HideInInspector] public float hingeMinLimit;
    [HideInInspector] public float hingeMaxLimit;
    private bool justCreatedHinge = false;

    private MeshFilter meshFilter;
    private HingeJoint hinge;
    private CarDeformation carDeformation;
    private CarHealth carHealth;

    private Vector3[] originalMeshVertices;
    private bool hingeCreated = false;
    private bool isDestroyed = false;
    private HitLocation impactLocation = HitLocation.NONE;

    // UI style for debug render
    private static GUIStyle style = new GUIStyle();

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        // Instantiate a 'clone' of the mesh, so it does not affect all other objects using the same mesh
        meshFilter.mesh = (Mesh)Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh.MarkDynamic();

        carDeformation = GetComponentInParent<CarDeformation>();
        carHealth = GetComponentInParent<CarHealth>();

        style.normal.textColor = Color.red;
        style.fontSize = 24;

        // Store original mesh vertices
        originalMeshVertices = meshFilter.mesh.vertices;

        if (isHinge)
        {
            currentHealth = health;
        }
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
    public bool ApplyDamage(int i, Collision collision, float minVelocity, Rigidbody body)
    {
        if (isDestroyed) return true;

        justCreatedHinge = false;

        HitLocation hitLocation = carSide;
        if (carSide == HitLocation.NONE)
            hitLocation = CheckImpactLocation(collision.GetContact(i).normal, collision.GetContact(i).thisCollider);

        bool isAttacker = CheckAttacker(collision.GetContact(i));
        float damage = (collision.relativeVelocity.magnitude - minVelocity) / collision.contactCount;
        HitLocation opponentImpactedSide = CheckImpactLocation(collision.GetContact(i).normal, collision.GetContact(i).otherCollider);

        Vitals opponentVital = null;
        if (opponentImpactedSide != HitLocation.BOTTOM && opponentImpactedSide != HitLocation.TOP && opponentImpactedSide != HitLocation.NONE && !collision.GetContact(i).otherCollider.CompareTag("Ground"))
        {
            opponentVital = collision.GetContact(i).otherCollider.transform.GetComponentInParent<CarHealth>().vitals.Find(v => v.vitalType == opponentImpactedSide);
        }

        // Update the car vitals on the UI
        CarController carOpponent = collision.GetContact(i).otherCollider.CompareTag("Ground") ? null : collision.GetContact(i).otherCollider.GetComponentInParent<CarController>();
        if (carHealth) carHealth.AddCarDamage(carOpponent, hitLocation, opponentVital, damage, isAttacker);

        // Update hinge health
        if (isHinge) currentHealth -= damage;

        // Create hinge when currentHealth is under the threshold
        bool createHinge = (currentHealth / health) <= hingeCreationThreshold;
        if (isHinge && createHinge && !hingeCreated) CreateHinge(body);

        // Detach hinge when it loses all its hp
        if (hingeCreated && currentHealth < 0 && !justCreatedHinge)
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
    public void DeformPart(int i, Collision collision, float deformRadius, Vector3 maxTotalDeformDist, float deformStrength)
    {
        // Get the direction of the collision in local space of the hit mesh
        Vector3 hitDirection = meshCollider.transform.InverseTransformDirection(collision.relativeVelocity * 0.02f);
        // Get the impact position of the collision in local space of the hit mesh
        Vector3 impactPoint = meshCollider.transform.InverseTransformPoint(collision.GetContact(i).point);
        Vector3[] vertices = meshFilter.mesh.vertices;

        // Default max deformation set to very high 
        float currentMaxDeformDist = 1000;

        // Check impact location in case its the body, else use the dedicated hitLocation
        if (carSide == HitLocation.NONE)
            impactLocation = CheckImpactLocation(collision.GetContact(i).normal, collision.GetContact(i).thisCollider);
        else
            impactLocation = carSide;

        switch (impactLocation)
        {
            case HitLocation.FRONT:
                currentMaxDeformDist = maxTotalDeformDist.z;
                break;
            case HitLocation.BACK:
                currentMaxDeformDist = maxTotalDeformDist.z;
                break;
            case HitLocation.LEFT:
                currentMaxDeformDist = maxTotalDeformDist.x;
                break;
            case HitLocation.RIGHT:
                currentMaxDeformDist = maxTotalDeformDist.x;
                break;
            case HitLocation.TOP:
                currentMaxDeformDist = maxTotalDeformDist.y;
                break;
            case HitLocation.BOTTOM:
                return;
            case HitLocation.NONE:
                return;
        }

        for (int j = 0; j < vertices.Length; j++)
        {
            float distance = (impactPoint - vertices[j]).magnitude;
            if (distance <= deformRadius)
            {
                // Reposition the vertice
                Vector3 movement = hitDirection * (deformRadius - distance) * deformStrength;

                float offset = (vertices[j] - originalMeshVertices[j]).magnitude;

                if (offset < currentMaxDeformDist)
                {
                    vertices[j] += movement;
                }
                else if (carDeformation.debugMode)
                {
                    Debug.Log("Maxmimum deformation reached!");
                }
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
        justCreatedHinge = true;

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

    private bool CheckAttacker(ContactPoint contactPoint)
    {
        float angle = Vector3.Dot(contactPoint.thisCollider.transform.forward, contactPoint.normal);
        if (angle < -.95f)
        {
            return true;
        }

        return false;
    }

    private HitLocation CheckImpactLocation(Vector3 collisionNormal, Collider collider)
    {
        float right = Vector3.Dot(collisionNormal, collider.transform.right);
        float up = Vector3.Dot(collisionNormal, collider.transform.up);
        float forward = Vector3.Dot(collisionNormal, collider.transform.forward);

        if (-0.7 > up)
        {
            return HitLocation.TOP;
        }

        if (0.7 < up)
        {
            return HitLocation.BOTTOM;
        }

        if ((-0.7 > forward) && (-0.7 < up && up < 0.7))
        {
            return HitLocation.FRONT;
        }

        if ((0.7 < forward) && (-0.7 < up && up < 0.7))
        {
            return HitLocation.BACK;
        }

        if ((0.7 < right) && (-0.7 < up && up < 0.7))
        {
            return HitLocation.LEFT;
        }

        if ((-0.7 > right) && (-0.7 < up && up < 0.7))
        {
            return HitLocation.RIGHT;
        }

        return HitLocation.NONE;
    }

    private void OnGUI()
    {
        if (!carDeformation.debugMode)
        {
            return;
        }

        GUI.Label(new Rect(30.0f, 20.0f, 150, 130), "Last hit location: " + impactLocation, style);
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
            myScript.health = EditorGUILayout.FloatField("Health", myScript.health);
            myScript.hingeCreationThreshold = EditorGUILayout.FloatField("Hinge Creation Threshold", myScript.hingeCreationThreshold);
            myScript.hingeAnchor = EditorGUILayout.Vector3Field("Anchor", myScript.hingeAnchor);
            myScript.hingeAxis = EditorGUILayout.Vector3Field("Axis", myScript.hingeAxis);
            myScript.hingeMinLimit = EditorGUILayout.FloatField("Min Limit", myScript.hingeMinLimit);
            myScript.hingeMaxLimit = EditorGUILayout.FloatField("Max Limit", myScript.hingeMaxLimit);
        }
    }
}
#endif
