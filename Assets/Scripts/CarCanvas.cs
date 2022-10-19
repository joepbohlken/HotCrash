using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarCanvas : MonoBehaviour
{
    [Header("Camera")]
    public Camera cameraToFollow;

    [Header("HP bar")]
    public GameObject bars;
    public TextMeshProUGUI hpText;

    // Update is called once per frame
    private void Update()
    {
        if(cameraToFollow)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cameraToFollow.transform.position);
        }
    }
}
