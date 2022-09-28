using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CarStateController : MonoBehaviour
{
    public enum VitalType { None, Front, Back, Left, Right };

    [System.Serializable]
    protected class Vitals
    {
        public VitalType vitalType;
        public Image image;
        [Tooltip("The parts that belong to the selected vital type.")]
        public List<DeformablePart> parts;
        [HideInInspector] public float maxHealth = 0f;
        [HideInInspector] public float currentHealth;
    }

    [Tooltip("The collider component of the engine.")]
    [SerializeField] private Collider engine;
    [Tooltip("The total damage the engine can withstand before the car stops working.")]
    [SerializeField] private float engineHealth = 200f;
    [Space(12)]
    [Tooltip("Used to visualize the vitals of the car on the UI.")]
    [SerializeField] private List<Vitals> vitals;

    [Space(12)]
    [Tooltip("If true will use the minVelocity property from the CarDeformation script on this object.")]
    public bool useCarDeformation = true;
    [Tooltip("Minimum velocity required on impact to damage the engine.")]
    [HideInInspector] public float minVelocity = 3f;

    private CarDeformation carDeformation;
    private bool isDestroyed = false;

    private void Awake()
    {
        if (useCarDeformation)
        {
            carDeformation = GetComponent<CarDeformation>();
            if (carDeformation == null) Debug.LogError("There is no CarDeformation script on " + name + ". Add one or remove the CarStateController script from this object.");
        }

        // Set up vitals and vital parts
        foreach(Vitals vital in vitals)
        {
            if (vital.vitalType == VitalType.None) continue;

            foreach (DeformablePart deformablePart in vital.parts)
            {
                deformablePart.vitalType = vital.vitalType;
                vital.maxHealth += deformablePart.MaxAllowedDamage;
            }
            vital.currentHealth = vital.maxHealth;
        }
    }

    public void AddVitalDamage(VitalType _vitalType, float damage)
    {
        // Get and update the correct vital information
        Vitals vital = vitals.FirstOrDefault(v => v.vitalType == _vitalType);
        vital.currentHealth = Mathf.Clamp(vital.currentHealth - damage, 0, vital.maxHealth);

        // Change the color of the vital image
        float progress = 1f - (1f / vital.maxHealth * vital.currentHealth);
        float threshold = 0.5f;

        if (progress <= threshold)
        {
            vital.image.color = Color.Lerp(Color.white, Color.yellow, progress / threshold);
        }
        else
        {
            vital.image.color = Color.Lerp(Color.yellow, Color.red, (progress - threshold)/(1 - threshold));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        float _minVelocity = useCarDeformation ? carDeformation.MinVelocity : minVelocity;

        // Check if the velocity on impact is greater than the required mininum velocity
        if (collision.relativeVelocity.magnitude >= _minVelocity)
        {
            // Check if the collision was made with the engine
            if (collision.GetContact(0).thisCollider == engine)
            {
                // Decrease engine health
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
