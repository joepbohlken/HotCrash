using System.Collections.Generic;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Tooltip("The time in seconds before the player gets a new ability after having used the previous one.")]
    [SerializeField] private float abilityCooldown = 5f;
    [Tooltip("The list of abilities of which a random one will be given to the player everytime the cooldown ends.")]
    [SerializeField] private List<Ability> availableAbilities;

    private ArcadeCar carController;
    [HideInInspector] public HUD hud;
    private Ability currentAbility;
    private float currentCooldown;
    private bool abilityActivated = true;
    private bool handledDestroyed = false;

    private void Start()
    {
        carController = GetComponent<ArcadeCar>();
        currentCooldown = abilityCooldown;

        if (!carController.isBot && hud) hud.StartCountdown(currentCooldown);
    }

    private void Update()
    {
        if (carController.carHealth.isDestroyed)
        {
            if (!handledDestroyed)
            {
                handledDestroyed = true;
                if (currentAbility) currentAbility.CarDestroyed();
            }
            return;
        }

        if (currentAbility)
        {
            currentAbility.LogicUpdate();
        }
        else
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                currentCooldown = abilityCooldown;

                // Give new random ability
                currentAbility = Instantiate(availableAbilities[Random.Range(0, availableAbilities.Count)]);
                currentAbility.Initialize(this, carController);
                currentAbility.Obtained();
                abilityActivated = false;

                if (!carController.isBot && hud) hud.SetInfo(currentAbility);
            }
        }
    }

    public void UseAbility()
    {
        if (currentAbility && !carController.carHealth.isDestroyed && !abilityActivated)
        {
            abilityActivated = true;
            currentAbility.Activated();
        }
    }

    public void AbilityEnded()
    {
        Destroy(currentAbility);
        currentAbility = null;

        if (!carController.isBot && hud) hud.StartCountdown(currentCooldown);
    }
}
