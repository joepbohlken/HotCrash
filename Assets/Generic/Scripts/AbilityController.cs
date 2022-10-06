using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cd <= 0 && Ability != null)
        {
            Ability.Use();
            if (consumableAbilities)
            {
                Ability = ScriptableObject.CreateInstance<Ability>();
            }
            else
            {
                cd = Ability.AbilityCooldown;
            }
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
}
