using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotCrash : MonoBehaviour
{
    [SerializeField]
    private GameObject button;

    public void enableButton()
    {
        button.SetActive(true);
    }
}
