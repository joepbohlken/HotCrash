using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Mines")]
public class MineAbility : Ability
{
    [Space(12)]
    [SerializeField] private GameObject minePrefab;
    [Tooltip("The max distance the car can be from the ground in order to place a mine.")]
    [SerializeField] private float maxPlaceDistance = 3f;
    [Tooltip("The amount of mines that can be placed.")]
    [SerializeField] private int amount = 3;

    private int placedMines = 0;
    private bool abilityEnded = false;

    public override void Obtained()
    {
        base.Obtained();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void Activated()
    {
        base.Activated();

        if (placedMines < amount)
        {
            RaycastHit hit;
            if (Physics.Raycast(carController.transform.position, Vector3.down, out hit, maxPlaceDistance))
            {
                placedMines++;

                MineObject mine = Instantiate(minePrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)).GetComponent<MineObject>();
                mine.SetUpMine(carController.transform, carController.transform.parent, carController.isBot, Color.red, 1);

                if (placedMines >= amount && !abilityEnded)
                {
                    abilityEnded = true;
                    abilityController.AbilityEnded();
                }
            }
        }
    }

    public override void CarDestroyed()
    {
        base.CarDestroyed();
    }
}
