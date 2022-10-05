using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilityTimer : MonoBehaviour
{
    public static InvisibilityTimer invisTimer;
    // Start is called before the first frame update
    void Start()
    {
        invisTimer = this;
    }
    public IEnumerator BecomeVisible(float duration, Dictionary<Renderer, Tuple<Color, int>> originalValues)
    {
        yield return new WaitForSeconds(duration);

        foreach (KeyValuePair<Renderer, Tuple<Color, int>> obj in originalValues)
        {
            obj.Key.material.color = obj.Value.Item1;
            obj.Key.material.renderQueue = obj.Value.Item2;
            obj.Key.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
