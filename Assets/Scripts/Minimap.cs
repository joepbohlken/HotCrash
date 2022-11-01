using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    private Dictionary<Transform, GameObject> carsAndIndicators = new Dictionary<Transform, GameObject>();

    private int minimapSizeX = 84;
    private int minimapSizeY = 138;

    [SerializeField]
    private Color[] indicatorColors;
    [SerializeField]
    private GameObject carIndicatorPrefab;
    [SerializeField]
    private int mapMinX, mapMaxX, mapMinZ, mapMaxZ;

    public GameObject carsParent;

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
        updateIndicatorPostitions();
    }

    private void updateIndicatorPostitions()
    {
        foreach (KeyValuePair<Transform, GameObject> pair in carsAndIndicators)
        {
            RectTransform imageRectTransform = pair.Value.GetComponent<RectTransform>();
            Transform carTransform = pair.Key;

            imageRectTransform.localPosition = new Vector3(
            /* X */    Mathf.Lerp(-minimapSizeX, minimapSizeX, Mathf.Sign(carTransform.position.x) == 1 ? Mathf.Lerp(0.5f, 1, carTransform.position.x / mapMaxX) : Mathf.Lerp(0.5f, 0f, carTransform.position.x / mapMinX)),
            /* Y */    Mathf.Lerp(-minimapSizeY, minimapSizeY, Mathf.Sign(carTransform.position.z) == 1 ? Mathf.Lerp(0.5f, 1, carTransform.position.z / mapMaxZ) : Mathf.Lerp(0.5f, 0f, carTransform.position.z / mapMinZ)));

            imageRectTransform.localRotation = Quaternion.Euler(0, 0, -carTransform.rotation.eulerAngles.y + 90);
        }
    }
}
