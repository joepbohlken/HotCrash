using System.Collections.Generic;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [Tooltip("The time in seconds before the player gets a new ability after having used the previous one.")]
    [SerializeField] private float abilityCooldown = 5f;
    [Tooltip("The list of abilities of which a random one will be given to the player everytime the cooldown ends.")]
    [SerializeField] private List<Ability> availableAbilities;

    private CarController carController;
    [HideInInspector] public HUD hud;
    [HideInInspector] public Camera playerCamera;
    [HideInInspector] public Transform abilityContainer;
    private Ability currentAbility;
    private float currentCooldown;
    private bool handledDestroyed = false;
    private bool carBecameDriveable = false;

    public int playerIndex
    {
        get { return carController.player.playerIndex; }
    }

    public Color playerColor
    {
        get { return carController.player.playerColor; }
    }

    private void Start()
    {
        carController = GetComponent<CarController>();
        abilityContainer = GameObject.Find("AbilityContainer").transform;
        currentCooldown = abilityCooldown;
    }

    private void Update()
    {
        if (carController.isDestroyed)
        {
            if (!handledDestroyed)
            {
                handledDestroyed = true;
                if (currentAbility) currentAbility.CarDestroyed();
            }
            return;
        }

        if (carController.driveable && !carBecameDriveable)
        {
            carBecameDriveable = true;
            if (!carController.isBot && hud) hud.StartCountdown(currentCooldown);
        }

        if (currentAbility)
        {
            currentAbility.LogicUpdate();
        }
        else if (availableAbilities.Count > 0 && carController.driveable)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                currentCooldown = abilityCooldown;

                // Give new random ability
                currentAbility = Instantiate(availableAbilities[Random.Range(0, availableAbilities.Count)]);
                currentAbility.Initialize(this, carController);
                currentAbility.Obtained();

                if (!carController.isBot && hud) hud.SetInfo(currentAbility);
            }
        }
    }

    public void UseAbility()
    {
        if (currentAbility && !carController.isDestroyed)
        {
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
