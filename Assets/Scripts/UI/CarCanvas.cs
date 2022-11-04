using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarCanvas : MonoBehaviour
{
    [Header("Camera")]
    public Camera cameraToFollow;

    [Header("HP bar")]
    public GameObject bars;
    public TextMeshProUGUI hpText;

    [Header("TargetBox")]
    public Image topLeftBorder;
    public Image bottomLeftBorder;
    public Image topRightBorder;
    public Image bottomRightBorder;

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

    public void EnableTargeting(MeshCollider targetCollider)
    {
        if (cameraToFollow == null) return;

        topLeftBorder.GetComponent<RectTransform>().position = new Vector2(masterParent.localPosition.x, masterParent.localPosition.y);
        topRightBorder.GetComponent<RectTransform>().position = new Vector2(masterParent.localPosition.x, masterParent.localPosition.y);
        bottomLeftBorder.GetComponent<RectTransform>().position = new Vector2(masterParent.localPosition.x, masterParent.localPosition.y);
        bottomRightBorder.GetComponent<RectTransform>().position = new Vector2(masterParent.localPosition.x, masterParent.localPosition.y);

        topLeftBorder.enabled = true;
        topRightBorder.enabled = true;
        bottomLeftBorder.enabled = true;
        bottomRightBorder.enabled = true;
    }

    public void DisableTargeting()
    {
        topLeftBorder.enabled = false;
        topRightBorder.enabled = false;
        bottomLeftBorder.enabled = false;
        bottomRightBorder.enabled = false;
    }
}
