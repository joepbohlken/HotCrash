using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

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
    [Tooltip("The health points of the engine. These get added to the total health points of the car, along with the health points of all deformable parts.")]
    [SerializeField] private float engineHealth = 200f;
    [Tooltip("The vital image of the engine on the UI.")]
    [SerializeField] private Image engineImage;
    [Space(12)]
    [Tooltip("Used to visualize the vitals of the car on the UI.")]
    [SerializeField] private List<Vitals> vitals;
    [Space(12)]

    public UnityEvent onCarDestroyed;

    [Space(12)]
    [Tooltip("If true will use the minVelocity property from the CarDeformation script on this object.")]
    public bool useCarDeformation = true;
    [Tooltip("Minimum velocity required on impact to damage the engine.")]
    [HideInInspector] public float minVelocity = 3f;

    private CarDeformation carDeformation;
    private float maxTotalHealth = 0f;
    private float currentTotalHealth = 0f;
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
            // Ignore vital type None, it's main purpose is to prevent any unwanted bugs
            if (vital.vitalType == VitalType.None) continue;

            // Set up every deformable part that is assigned to this vital type
            foreach (DeformablePart deformablePart in vital.parts)
            {
                deformablePart.vitalTypes.Add(vital.vitalType);
                // Add the 'health' of this part to the corresponding vital type total health
                vital.maxHealth += deformablePart.MaxAllowedDamage;
                // Add the 'health' of this part to the total health of the car, but only if it has not been added yet
                if (deformablePart.vitalTypes.Count == 1) maxTotalHealth += deformablePart.MaxAllowedDamage;
            }
            vital.currentHealth = vital.maxHealth;
        }

        maxTotalHealth += engineHealth;
        currentTotalHealth = maxTotalHealth;
    }

    public void AddVitalDamage(List<VitalType> vitalTypes, float damage)
    {
        // Go through each vital type the part belongs to
        foreach (VitalType vitalType in vitalTypes)
        {
            // Get and update the correct vital information
            Vitals vital = vitals.FirstOrDefault(v => v.vitalType == vitalType);
            vital.currentHealth = Mathf.Clamp(vital.currentHealth - damage, 0, vital.maxHealth);

            vital.image.color = GetVitalColor(vital.maxHealth, vital.currentHealth);
        }

        // Update the total health
        currentTotalHealth = Mathf.Clamp(currentTotalHealth - damage, 0, maxTotalHealth);
        engineImage.color = GetVitalColor(maxTotalHealth, currentTotalHealth);

        CheckHealth();
    }

    private Color GetVitalColor(float max, float current)
    {
        float progress = 1f - (1f / max * current);
        float threshold = 0.5f;

        if (progress <= threshold)
        {
            return Color.Lerp(Color.white, Color.yellow, progress / threshold);
        }
        else
        {
            return Color.Lerp(Color.yellow, Color.red, (progress - threshold) / (1 - threshold));
        }
    }

    private void CheckHealth()
    {
        if (currentTotalHealth <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            onCarDestroyed.Invoke();

            // Temporarily
            foreach (Wheel wheel in GetComponentsInChildren<Wheel>())
            {
                wheel.enabled = false;
            }
            // Disabling AntiRoll prevents the car from becoming crazy
            GetComponent<AntiRoll>().enabled = false;
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
                currentTotalHealth -= (collision.relativeVelocity.magnitude - _minVelocity) * 2;

                CheckHealth();
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
