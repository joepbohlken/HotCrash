using System.Collections.Generic;
using UnityEngine;

public class AbilityBlock : MonoBehaviour
{
    [SerializeField]
    private List<Ability> abilities;
    public Ability GetRandomAbility()
    {
        Destroy(gameObject);
        return abilities[Random.Range(0, abilities.Count)];
    }
}
