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
    public IEnumerator BecomeVisible(float duration, Dictionary<Renderer, List<Tuple<Color, int>>> originalValues)
    {
        yield return new WaitForSeconds(duration);

        foreach (KeyValuePair<Renderer, List<Tuple<Color, int>>> obj in originalValues)
        {
            for(int i = 0; i < obj.Value.Count; i++)
            {
                obj.Key.materials[i].color = obj.Value[i].Item1;
                obj.Key.materials[i].renderQueue = obj.Value[i].Item2;
            }
            obj.Key.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
