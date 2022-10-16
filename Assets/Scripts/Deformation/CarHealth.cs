using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

[System.Serializable]
public class Vitals
{
    public HitLocation vitalType;
    public Image image;
    public float health = 100f;
    [HideInInspector] public float currentHealth;
}

public class CarHealth : MonoBehaviour
{
    [Tooltip("The base health of the car.")]
    [SerializeField] private float health = 1000f;
    [Space(12)]
    [Tooltip("Used to visualize the vitals of the car on the UI.")]
    [SerializeField] private List<Vitals> vitals;

    private CarDeformation carDeformation;
    private float currentHealth;
    private bool isDestroyed = false;

    private void Awake()
    {
        carDeformation = GetComponent<CarDeformation>();

        currentHealth = health;
    }

    public void AddVitalDamage(HitLocation hitLocation, float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, health);

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
        if (health <= 0 && !isDestroyed)
        {
            isDestroyed = true;
        }
    }
}
