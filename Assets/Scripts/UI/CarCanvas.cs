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

    private Transform masterParent;
    public CanvasGroup canvasGroup { get; set; }

    private void Start()
    {
        masterParent = transform.parent.parent;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (masterParent)
        {
            masterParent.localPosition = masterParent.parent.InverseTransformPoint(masterParent.parent.position + Vector3.up * 2f);
            masterParent.eulerAngles = new Vector3(0, 0, 0);
        }

        if(cameraToFollow)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cameraToFollow.transform.position);
        }
    }
}
