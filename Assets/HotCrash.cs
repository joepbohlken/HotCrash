using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotCrash : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator animator;
    [SerializeField]
    private GameObject button;

    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void enableButton()
    {
        button.SetActive(true);
    }
}
