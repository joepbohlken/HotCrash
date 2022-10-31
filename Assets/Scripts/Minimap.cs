using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    private Dictionary<Transform, GameObject> carsAndIndicators = new Dictionary<Transform, GameObject>();

    //map props 166 - 245
    //minimap props 80 - 123

    [SerializeField]
    private GameObject carsParent;

    public Color[] indicatorColors;
    public GameObject carIndicatorPrefab;

    void Start()
    {
    }

    void Update()
    {
        if (carsAndIndicators.Count < 1)
        {
            foreach (Transform tf in carsParent.transform)
            {
                GameObject indicator = (Instantiate(carIndicatorPrefab));

                Color indicatorColor = indicatorColors[tf.name.Contains("Player") ? carsAndIndicators.Count : 4];

                indicator.transform.SetParent(transform);
                indicator.GetComponent<Image>().color = indicatorColor;

                carsAndIndicators.Add(tf, indicator);
            }
        }

        updatePostitions();
    }

    private void updatePostitions()
    {
        foreach (KeyValuePair<Transform, GameObject> pair in carsAndIndicators)
        {
            RectTransform rt = pair.Value.GetComponent<RectTransform>();
            Transform tf = pair.Key;

            rt.localPosition = new Vector3(
            /* X */    Mathf.Lerp(-86, 86, Mathf.Sign(tf.position.x / 166) == 1 ? Mathf.Lerp(0.5f, 1, tf.position.x / 166) : Mathf.Lerp(0.5f, 0f, Mathf.Abs(tf.position.x / 166))),
            /* Y */    Mathf.Lerp(-130, 130, Mathf.Sign(tf.position.z / 245) == 1 ? Mathf.Lerp(0.5f, 1, tf.position.z / 245) : Mathf.Lerp(0.5f, 0f, Mathf.Abs(tf.position.z / 245))));

            if(tf.name.Contains("Player")) Debug.Log(tf.rotation.y);
            rt.localRotation = Quaternion.Euler(0, 0, -tf.rotation.eulerAngles.y + 90);
        }
    }
}
