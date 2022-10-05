using System.Collections.Generic;
using UnityEngine;
using System;

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
        Dictionary<Renderer, Tuple<Color, int>> originalValues = new();

        foreach (Renderer rend in CarRenderers)
        {
            foreach (Material mat in rend.materials)
            {
                Color originalColor = new Color
                {
                    a = mat.color.a,
                    r = mat.color.r,
                    g = mat.color.g,
                    b = mat.color.b,
                };
                int renderQueue = mat.renderQueue;

                originalValues.TryAdd(rend, new Tuple<Color, int>(originalColor, renderQueue));

                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color tempColor = mat.color;
                tempColor.a = opacity;
                mat.color = tempColor;
                mat.renderQueue = 3000;
            }
        }
        InvisibilityTimer.invisTimer.StartCoroutine(InvisibilityTimer.invisTimer.BecomeVisible(Duration, originalValues));
    }
}
