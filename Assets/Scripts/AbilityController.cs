using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
    private UnityEvent OnAbilityComplete = new UnityEvent();

    [SerializeField]
    private List<Ability> availableAbilities;
    [SerializeField]
    private float cooldownBetweenAbilities;
    [SerializeField]
    private bool consumableAbilities;

    private CarHealth carHealth;
    private ArcadeCar carController;
    [HideInInspector]
    public Ability ability;
    [HideInInspector]
    public HUD hud;
    private bool used;

    [HideInInspector]
    public bool useAbility = false;

    private void Start()
    {
        carHealth = GetComponent<CarHealth>();
        carController = GetComponent<ArcadeCar>();

        if(!carController.isBot)
        {
            StartCoroutine(GiveAbility());
        }
    }

    void Update()
    {
        if (useAbility && used == false && !carController.isBot && !carHealth.isDestroyed)
        {
            ability.Use();
            used = true;
            StartCoroutine(ActivateAfterDelay(ability.AbilityDuration));
        }
    }

    IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnAbilityComplete.Invoke();
        StartCoroutine(GiveAbility());
    }

    IEnumerator GiveAbility()
    {
        hud.StartCountdown(cooldownBetweenAbilities);
        yield return new WaitForSeconds(cooldownBetweenAbilities);
        GenerateAbility();
        hud.SetInfo(ability);
    }

    private void GenerateAbility()
    {
        ability = availableAbilities[Random.Range(0, availableAbilities.Count)];
        OnAbilityComplete.AddListener(ability.OnAbilityEnded);
        ability.Obtained(gameObject);
        used = false;
    }
}
