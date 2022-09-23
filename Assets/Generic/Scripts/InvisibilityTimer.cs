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
    public IEnumerator BecomeVisible(float duration, Dictionary<Renderer, List<Color>> originalValues)
    {
        yield return new WaitForSeconds(duration);

        foreach (KeyValuePair<Renderer, List<Color>> obj in originalValues)
        {
            int index = 0;
            foreach(Material mat in obj.Key.materials)
            {
                mat.color = obj.Value[index];
                mat.renderQueue = 2500;
                index++;
            }

            obj.Key.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
