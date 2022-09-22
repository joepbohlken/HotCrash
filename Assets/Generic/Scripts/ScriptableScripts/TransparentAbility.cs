using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Transparent")]
public class TransparentAbility : Ability
{
    private List<Renderer> CarRenderers;
    [SerializeField]
    private float opacity;

    public override void OnPickup()
    {
        Car.GetComponentsInChildren(CarRenderers);
    }

    public override void Use()
    {
        Dictionary<Renderer, List<Material>> originalValues = new();

        foreach (Renderer rend in CarRenderers)
        {
            foreach(Material mat in rend.materials)
            {
                originalValues[rend].Add(mat);

                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color tempColor = rend.material.color;
                tempColor.a = opacity;
                mat.color = tempColor;
                mat.renderQueue = 3000;
            }
        }
    }
}
