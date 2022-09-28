using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CarStateController : MonoBehaviour
{
    [SerializeField] private Collider engine;
    [SerializeField] private float engineHealth = 200f;

    [Tooltip("If true will use the minVelocity property from the CarDeformation script on this object.")]
    public bool useCarDeformation = true;
    [Tooltip("Minimum velocity required on impact to damage the engine.")]
    [HideInInspector] public float minVelocity = 3f;

    private CarDeformation carDeformation;
    private bool isDestroyed = false;

    private void Start()
    {
        if (useCarDeformation)
        {
            carDeformation = GetComponent<CarDeformation>();
            if (carDeformation == null) Debug.LogError("There is no CarDeformation script on " + name + ". Add one or remove the CarStateController script from this object.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        float _minVelocity = useCarDeformation ? carDeformation.MinVelocity : minVelocity;

        if (collision.relativeVelocity.magnitude >= _minVelocity)
        {
            if (collision.GetContact(0).thisCollider == engine)
            {
                engineHealth -= (collision.relativeVelocity.magnitude - _minVelocity);

                if (engineHealth <= 0)
                {
                    isDestroyed = true;

                    // Temporarily
                    Destroy(GetComponent<Vehicle>());
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CarStateController))]
public class UseDeformationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = target as CarStateController;

        if (!myScript.useCarDeformation)
        {
            myScript.minVelocity = EditorGUILayout.FloatField("Min Velocity", myScript.minVelocity);
        }
    }
}
#endif
