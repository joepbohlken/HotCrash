using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Abilities/Transparent")]
public class InvisibilityAbility : Ability
{
    [Space(12)]
    [Tooltip("Duration in seconds of the invisibility.")]
    [SerializeField] private float duration = 2f;
    [Tooltip("Determines how opaque the car becomes.")]
    [SerializeField] private float opacity = 0.25f;

    private List<Renderer> carRenderers;
    private Dictionary<Renderer, List<Tuple<Color, int>>> originalValues;
    private float durationLeft;
    private bool abilityEnded = false;
    private bool isSelfActivated = false;

    public override void Obtained()
    {
        base.Obtained();

        carRenderers = carController.GetComponentsInChildren<Renderer>().ToList();
        durationLeft = duration;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isActivated)
        {
            durationLeft -= Time.deltaTime;
            if (durationLeft <= 0 && !abilityEnded)
            {
                abilityEnded = true;
                AbilityEnded(false);
            }
        }
    }

    public override void Activated()
    {
        base.Activated();

        if (isSelfActivated) return;
        isSelfActivated = true;

        originalValues = new();

        foreach (Renderer rend in carRenderers)
        {
            originalValues.Add(rend, new List<Tuple<Color, int>>());
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

                originalValues[rend].Add(new Tuple<Color, int>(originalColor, renderQueue));

                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Color tempColor = mat.color;
                tempColor.a = opacity;
                mat.color = tempColor;
                mat.renderQueue = 3000;
            }
        }
    }

    public override void CarDestroyed()
    {
        base.CarDestroyed();

        if (!abilityEnded && isSelfActivated)
        {
            abilityEnded = true;
            AbilityEnded(true);
        }
    }

    private void AbilityEnded(bool isDestroyed)
    {
        foreach (KeyValuePair<Renderer, List<Tuple<Color, int>>> rendValuesPair in originalValues)
        {
            for (int i = 0; i < rendValuesPair.Value.Count; i++)
            {
                rendValuesPair.Key.materials[i].color = rendValuesPair.Value[i].Item1;
                rendValuesPair.Key.materials[i].renderQueue = rendValuesPair.Value[i].Item2;
            }
            rendValuesPair.Key.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        if (!isDestroyed) abilityController.AbilityEnded();
    }
}
