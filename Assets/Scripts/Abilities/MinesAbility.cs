using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Mines")]
public class MinesAbility : Ability
{
    /*
    [SerializeField]
    private GameObject MineObject;
    [SerializeField]
    private int amountOfMines;

    private int mineCounter = 1;

    public override void Use()
    {
        if (amountOfMines == mineCounter)
        {
            Cooldown = 10;
            mineCounter = 1;
        }
        else
        {
            Cooldown = 0;
            mineCounter++;
        }

        Vector3 minePosition = Car.transform.position + Vector3.up + Car.transform.forward.normalized * -2;
        Instantiate(MineObject, minePosition, Quaternion.identity);
    }
    */
}
