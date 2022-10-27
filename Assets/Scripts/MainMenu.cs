using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private Animator animator;
    private ParticleSystem particleSystem;


    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        animator.Play("Start");
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void StartParticleSystem()
    {
        particleSystem.Play();
    }

    public void PauseParticleSystem()
    {
        particleSystem.Pause();
    }
}
