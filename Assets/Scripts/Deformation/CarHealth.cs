using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using TMPro;

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
    public GameObject healthBar;
    public TextMeshProUGUI healthText;
    [Tooltip("Y - Desired damage multiplier. X - Health ratio")]
    public AnimationCurve damageMultiplierCurve;
    [Space(6)]
    [Tooltip("Used to visualize the vitals of the car on the UI.")]
    [SerializeField] private List<Vitals> vitals;

    private ArcadeCar arcadeCar;
    private Image[] bars;
    private float currentHealth;
    [HideInInspector]
    public bool isDestroyed = false;

    private void Awake()
    {
        arcadeCar = GetComponent<ArcadeCar>();
        bars = healthBar.GetComponentsInChildren<Image>();

        // Set current health of each vital
        foreach (Vitals vital in vitals)
        {
            vital.currentHealth = vital.health;
        }

        // Set base health
        currentHealth = health;
    }

    public void AddVitalDamage(HitLocation hitLocation, float damage)
    {
        if(hitLocation == HitLocation.NONE) return;

        // Damage the correct side
        Vitals vital = vitals.FirstOrDefault(v => v.vitalType == hitLocation);
        float multiplier = 1;

        if (vital != null)
        {
            vital.currentHealth = Mathf.Clamp(vital.currentHealth - damage, 0, vital.health);
            vital.image.color = GetVitalColor(vital.health, vital.currentHealth);

            multiplier = damageMultiplierCurve.Evaluate(vital.currentHealth / vital.health);
        }

        // Damage base health
        currentHealth = Mathf.Clamp(currentHealth - (damage * multiplier), 0, health);

        // Update health bar
        float healthPercentage = Mathf.Round(Mathf.Clamp01(currentHealth / health) * 100);
        healthText.text = healthPercentage.ToString();

        float barPercentage = 100 / bars.Length;
        foreach (var (bar, i) in bars.Select((value, i) => (value, i)))
        {
            float minPercentage = barPercentage * i;
            float maxPercentage = barPercentage * (i + 1);

            float fillAmount = 1;

            if (healthPercentage < maxPercentage)
            {
                float offset = Mathf.Clamp(maxPercentage - healthPercentage, 0, barPercentage);
                fillAmount = Mathf.Clamp01((barPercentage - offset) / barPercentage);
            }

            bar.fillAmount = fillAmount;
        }

        CheckHealth();
    }

    private Color GetVitalColor(float max, float current)
    {
        float progress = 1 - (current / max);

        if (progress == 1)
        {
            return new Color(0, 0, 0, 0.5f);
        }

        return Color.Lerp(Color.white, new Color(0.9058824f, 0.2980392f, 0.2352941f, 1), progress);
    }

    private void CheckHealth()
    {
        if (currentHealth <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            arcadeCar.GetComponent<Rigidbody>().centerOfMass = arcadeCar.originalCenterOfMass;
            arcadeCar.DestroyCar();
        }
    }
}
