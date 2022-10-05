using System;
using System.Collections.Generic;
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
        Dictionary<Renderer, List<Color>> originalValues = new();

        foreach (Renderer rend in CarRenderers)
        {
            foreach(Material mat in rend.materials)
            {
                originalValues.TryAdd(rend, new List<Color>());
                originalValues[rend].Add(mat.color);

                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color tempColor = rend.material.color;
                tempColor.a = opacity;
                mat.color = tempColor;
                mat.renderQueue = 3000;
            }
        }
        InvisibilityTimer.invisTimer.StartCoroutine(InvisibilityTimer.invisTimer.BecomeVisible(Duration, originalValues));
    }
}
