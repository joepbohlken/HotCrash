using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public bool showStartAnim = true;
    public Animator canvasAnimator;

    [Header("Car Selection")]
    public Transform carSlotParent;
    public GameObject[] availableCars;
    public Material[] availableColors;

    private float maxSpawnAngle = 100f;
    private float maxSpawnRadius = 15f;

    private Animator animator;
    private ParticleSystem particleSystem;
    private CarSelectionSlot[] carSlots;

    private bool multiplayerMenuOpen = false;
    private int playerCount = 1;

    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();

        // Add all the car slots
        carSlots = carSlotParent.GetComponentsInChildren<CarSelectionSlot>();

        foreach (CarSelectionSlot slot in carSlots)
        {
            slot.menu = this;
            slot.gameObject.SetActive(false);
        }
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

        ShowCarSelectionScreen();
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

    private void ShowCarSelectionScreen()
    {
        animator.Play("ShowCarSelection");

        float actualSpawnAngle = maxSpawnAngle - (10f * (playerCount - 1));
        float angleOffset = actualSpawnAngle / 3;
        float initalSpawnAngle = (-angleOffset / 2) * (playerCount - 1);
        float spawnDistance = maxSpawnRadius - (2 * (4 - playerCount));

        foreach (var (slot, i) in carSlots.Select((value, i) => (value, i)))
        {
            if (i < playerCount)
            {
                slot.gameObject.SetActive(true);

                // Reset car and color
                slot.ResetSlot();

                // Position and rotate each slot corectly depending on player count
                Vector3 direction = Quaternion.AngleAxis(initalSpawnAngle + (angleOffset * i), Vector3.up) * carSlotParent.forward;

                Vector3 newPosition = carSlotParent.position + (direction * spawnDistance) + (carSlotParent.up * .32f);
                slot.transform.position = newPosition;
                slot.transform.rotation = Quaternion.LookRotation((carSlotParent.position - newPosition), Vector3.up);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}
