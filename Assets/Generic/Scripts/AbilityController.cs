using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    private float cd = 0;

    [SerializeField]
    private Ability Ability;
    [SerializeField]
    private bool consumableAbilities;


    private void Start()
    {
        //Ability = ScriptableObject.CreateInstance<Ability>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cd <= 0)
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