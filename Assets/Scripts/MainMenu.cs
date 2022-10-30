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


    private void OnEnable()
    {
        PlayerManager.main.onReady.AddListener(OnPlayersReady);
        PlayerManager.main.onCancel.AddListener(OnPlayersCancel);
    }

    private void OnDisable()
    {
        PlayerManager.main.onReady.RemoveListener(OnPlayersReady);
        PlayerManager.main.onCancel.RemoveListener(OnPlayersCancel);
    }

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
            slot.changeColor.SetActive(false);
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

    public void CheckStartGame()
    {
        bool everyoneReady = CheckEveryoneReady();

        if (everyoneReady)
        {
            GameManager.main.StartGame(carSlots);
        }
    }

    private bool CheckEveryoneReady()
    {
        bool result = true;
        int players = 0;

        foreach (CarSelectionSlot slot in carSlots)
        {
            if (!slot.ready && slot.player != null)
            {
                result = false;
            }

            if (slot.player != null)
            {
                players++;
            }
        }

        // Set ready = false when there are no players
        if (players == 0)
        {
            result = false;
        }

        return result;
    }

    private void OnPlayersReady()
    {
        PlayerManager playerManager = PlayerManager.main;

        // Assign Players to slots
        foreach (var (playerSlot, i) in playerManager.playerSlots.Select((value, i) => (value, i)))
        {
            if (playerSlot.player != null)
            {
                carSlots[i].player = playerSlot.player;
                playerSlot.player.carSelectionSlot = carSlots[i];
            }
        }

        foreach (var (slot, i) in carSlots.Select((value, i) => (value, i)))
        {
            if (i < playerCount)
            {
                PlayerDevice device = playerManager.playerDevices.FirstOrDefault(device => device.name == playerManager.playerSlots[i].player.input.currentControlScheme);

                slot.interactable = true;
                slot.changeColor.SetActive(true);
                slot.changeColorKey.sprite = device.changeColorBtn;
            }
            else
            {
                slot.interactable = false;
                slot.changeColor.SetActive(false);
            }
        }
    }

    private void OnPlayersCancel()
    {
        GoBackToHome();
    }


    public void StartParticleSystem()
    {
        particleSystem.Play();
    }

    public void PauseParticleSystem()
    {
        particleSystem.Pause(true);
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

    public void GoBackToHome()
    {
        foreach (CarSelectionSlot slot in carSlots)
        {
            slot.interactable = false;
            slot.ready = false;
        }

        animator.Play("HideCarSelection");
    }

    public void ShowSettings()
    {
        HideMultiplayerSelection();
    }

    private void ShowCarSelectionScreen()
    {
        HideMultiplayerSelection();

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

                Vector3 newPosition = carSlotParent.position + (direction * spawnDistance) + (carSlotParent.up * .38f);
                slot.transform.position = newPosition;
                slot.transform.rotation = Quaternion.LookRotation((carSlotParent.position - new Vector3(newPosition.x, carSlotParent.position.y, newPosition.z)), Vector3.up);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }

        animator.Play("ShowCarSelection");
    }

    public void UpdatePlayers()
    {
        GameManager.main.playersCount = playerCount;

        PlayerManager.main.ShowMenu(true);
    }
}
