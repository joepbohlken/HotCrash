using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
    private UnityEvent OnAbilityComplete = new UnityEvent();
    private float cd = 0;

    [SerializeField]
    public Ability Ability;
    [SerializeField]
    private bool consumableAbilities;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cd <= 0 && Ability != null)
        {
            Ability.Use();
            StartCoroutine(ActivateAfterDelay(Ability.AbilityDuration));

            if (consumableAbilities)
            {
                Ability = null;
            }
            else
            {
                cd = Ability.AbilityCooldown + Ability.AbilityDuration;
            }
        }

        cd -= Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<AbilityBlock>())
        {
            if (Ability == null)
            {
                AbilityBlock block = other.gameObject.GetComponent<AbilityBlock>();
                Ability = block.GetRandomAbility();
                OnAbilityComplete.AddListener(Ability.OnAbilityEnded);
                Ability.PickedUp(gameObject);
            }
        }
    }

    IEnumerator ActivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnAbilityComplete.Invoke();
    }
}
