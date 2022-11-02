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
    [Tooltip("How fast the car switches from visible to invisible and vice versa.")]
    [SerializeField] private float transitionSpeed = 2f;
    [Range(0, 1)]
    [Tooltip("0 = 0% damage reduction, 0.5 = 50% damage reduction, 1 = 100% damage reduction.")]
    [SerializeField] private float damageReduction = 0.5f;

    private List<Renderer> carRenderers;
    private Dictionary<Renderer, List<Tuple<Color, int>>> originalValues;
    private Dictionary<Renderer, LayerMask> originalLayers = new();
    private float durationLeft;
    private bool abilityEnded = false;
    private bool abilitySelfEnded = false;
    private bool isSelfActivated = false;

    private float currentAlpha = 1f;
    private List<(Material, float)> materials = new();

    public override void Obtained()
    {
        base.Obtained();

        carRenderers = carController.GetComponentsInChildren<Renderer>().ToList();
        durationLeft = duration;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (abilitySelfEnded) return;

        if (isActivated)
        {
            durationLeft -= Time.deltaTime;
            if (durationLeft <= 0 && !abilityEnded)
            {
                abilityEnded = true;
                currentAlpha = 0f;
            }
        }

        if (abilityEnded)
        {
            currentAlpha += Time.deltaTime * transitionSpeed;

            foreach (ValueTuple<Material, float> mat in materials)
            {
                Color tempColor = mat.Item1.color;
                tempColor.a = Mathf.Clamp(currentAlpha, mat.Item2, 1f);
                mat.Item1.color = tempColor;
            }

            if (currentAlpha >= 1f && !abilitySelfEnded)
            {
                abilitySelfEnded = true;
                AbilityEnded(false);
            }
        }

        if (!abilityEnded && isSelfActivated && currentAlpha > -0.1f)
        {
            currentAlpha -= Time.deltaTime * transitionSpeed;

            foreach(ValueTuple<Material, float> mat in materials)
            {
                Color tempColor = mat.Item1.color;
                tempColor.a = Mathf.Clamp(currentAlpha, mat.Item2, 1f);
                mat.Item1.color = tempColor;
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
            if (!carController.isBot)
            {
                originalLayers.Add(rend, rend.gameObject.layer);
                rend.gameObject.layer = LayerMask.NameToLayer("Player " + (abilityController.playerIndex + 1));
            }
            
            foreach(CarCanvas carCanvas in carController.carCanvasRefs)
            {
                carCanvas.canvasGroup.alpha = 0f;
            }

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
                mat.renderQueue = 3000;
                materials.Add((mat, carController.isBot ? 0f : opacity));
            }
        }

        carController.health.damageModifier = damageReduction;
        carController.isTargetable = false;
    }

    public override void CarDestroyed()
    {
        base.CarDestroyed();

        if (!abilityEnded && isSelfActivated)
        {
            abilityEnded = true;
            abilitySelfEnded = true;
            AbilityEnded(true);
        }
    }

    private void AbilityEnded(bool isDestroyed)
    {
        carController.health.damageModifier = 1f;
        carController.isTargetable = true;

        foreach (KeyValuePair<Renderer, List<Tuple<Color, int>>> rendValuesPair in originalValues)
        {
            if (!carController.isBot) rendValuesPair.Key.gameObject.layer = originalLayers[rendValuesPair.Key];

            foreach (CarCanvas carCanvas in carController.carCanvasRefs)
            {
                carCanvas.canvasGroup.alpha = 1f;
            }

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
