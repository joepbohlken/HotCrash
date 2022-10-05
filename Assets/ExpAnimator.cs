using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject pants;

    public void playPants()
    {
        pants.SetActive(true);
    }
}
