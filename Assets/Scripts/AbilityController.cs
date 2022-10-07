using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityController : MonoBehaviour
{
    private float cd = 0;
    [NonSerialized] public static AbilityController controller;

    [SerializeField]
    public Ability Ability;
    [SerializeField]
    private bool consumableAbilities;


    private void Start()
    {
        AbilityController.controller = this;
        //Ability = ScriptableObject.CreateInstance<Ability>();
    }
    [HideInInspector]
    public UnityEvent OnAbilityComplete = new UnityEvent();

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
