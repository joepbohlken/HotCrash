using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car1 : MonoBehaviour

{

    private Animator animator;
    [SerializeField]
    private GameObject explosion;
    [SerializeField]
    private GameObject title;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playExplosion()
    {
        explosion.SetActive(true);
        title.SetActive(true);
    }
}
