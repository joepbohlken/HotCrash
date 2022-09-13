using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boost")]
public class Ability : ScriptableObject
{
    [SerializeField]
    private float Duration;
    public virtual void Use()
    {
        Debug.Log(this.name + ": Ability not implemented");
    }
}
