using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public bool showStartAnim = true;
    public Animator canvasAnimator;

    private Animator animator;
    private ParticleSystem particleSystem;

    private bool multiplayerMenuOpen = false;
    private int playerCount = 1;

    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        if (showStartAnim)
        {
            animator.Play("MainMenuStart");
        }
        else
        {
            float time = .8f;
            particleSystem.Simulate(time, true, true);
            particleSystem.Pause();
            particleSystem.time = time;
        }
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
        particleSystem.Pause(true);
        Debug.Log(particleSystem.time);
    }

    public void StartGame(int playerCount)
    {
        this.playerCount = playerCount;

        ShowCarSelectionScreen(playerCount);
    }

    public void ShowMultiplayerSelection()
    {
        if (!multiplayerMenuOpen)
        {
            multiplayerMenuOpen = true;
            canvasAnimator.Play("ShowMultiplayerSelect");
        }
    }
    public void HideMultiplayerSelection()
    {
        if (multiplayerMenuOpen)
        {
            multiplayerMenuOpen = false;
            canvasAnimator.Play("HideMultiplayerSelect");
        }
    }

    public void ShowSettings()
    {
        HideMultiplayerSelection();
    }

    private void ShowCarSelectionScreen(int players = 1)
    {
        animator.Play("ShowCarSelection");
    }
}
