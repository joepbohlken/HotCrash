using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
    private float cd = 0;

    [SerializeField]
    private Ability Ability;
    [SerializeField]
    private bool consumableAbilities;

    [HideInInspector]
    public UnityEvent OnAbilityComplete = new UnityEvent();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cd <= 0)
        {
            Ability.Use();
            StartCoroutine(ActivateAfterDelay(Ability.AbilityDuration));
        }

        cd -= Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<AbilityBlock>())
        {
            AbilityBlock block = other.gameObject.GetComponent<AbilityBlock>();
            Ability = block.GetRandomAbility();

            Ability.PickedUp(gameObject);
        }
    }

    IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnAbilityComplete.Invoke();

        if (consumableAbilities)
        {
            Ability = null;
        }
        else
        {
            cd = Ability.AbilityCooldown;
        }
    }
}
