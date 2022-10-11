using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
    private UnityEvent OnAbilityComplete = new UnityEvent();
    private bool used;

    [SerializeField]
    private List<Ability> availableAbiliteis;
    [SerializeField]
    private float cooldownBetweenAbilities;
    [SerializeField]
    private bool consumableAbilities;

    public Ability Ability;

    private void Start()
    {
        GenerateAbility();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && used == false)
        {
            Ability.Use();
            used = true;
            StartCoroutine(ActivateAfterDelay(Ability.AbilityDuration));
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
        yield return new WaitForSeconds(cooldownBetweenAbilities);
        GenerateAbility();
    }

    private void GenerateAbility()
    {
        Ability = availableAbiliteis[Random.Range(0, availableAbiliteis.Count)];
        OnAbilityComplete.AddListener(Ability.OnAbilityEnded);
        Ability.Obtained(gameObject);
        used = false;
    }
}
