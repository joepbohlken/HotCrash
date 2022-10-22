using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class PlayerSlot
{
    public string name;
    [HideInInspector]
    public PlayerController player;

    [Header("UI")]
    public Image border;
    public TextMeshProUGUI unusedPlayerNr;
    public Image device;
    public Image deviceOutline;
    public GameObject corner;
}

[Serializable]
public class PlayerDevice
{
    public string name;
    public Sprite sprite;
    public Sprite outline;
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager main;

    public PlayerSlot[] playerSlots = new PlayerSlot[4];
    public PlayerDevice[] playerDevices;
    public Color[] colors;

    public GameObject backgroundBlur;
    public GameObject mainPanel;

    private PlayerInputManager playerInputManager;
    private bool menuOpen = false;
    private int playerCount = 0;

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoin;
        playerInputManager.onPlayerLeft += OnPlayerLeave;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoin;
        playerInputManager.onPlayerLeft -= OnPlayerLeave;
    }

    private void Awake()
    {
        if (main != null) Destroy(gameObject);
        main = this;

        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void Update()
    {
        playerCount = GameManager.main.playersCount;

        int currentPlayerCount = GetCurrentPlayerCount();

        // If number of players isn't matching show menu 
        if (playerCount != currentPlayerCount && !menuOpen)
        {
            // Only show required amount of player slots (i.e. 3 players = only 3 slots shown)s
            UpdateVisibleSlots();

            ShowMenu(true);
        }
    }

    private int GetCurrentPlayerCount()
    {
        int currentPlayersCount = 0;
        foreach (PlayerSlot slot in playerSlots)
        {
            if (slot.player != null)
            {
                currentPlayersCount++;
            }
        }

        return currentPlayersCount;
    }

    // On player connect
    public void OnPlayerJoin(PlayerInput input)
    {
        Debug.Log("Player joined!");

        int playerIndex = input.playerIndex;

        PlayerController player = input.GetComponent<PlayerController>();
        player.playerIndex = playerIndex;
        player.input = input;
        player.playerManager = this;

        // Add player to correct slot
        playerSlots[playerIndex].player = player;

        // Disable player joining on max players reached
        if (playerCount == GetCurrentPlayerCount())
        {
            Debug.Log("Max players reached!");
            playerInputManager.DisableJoining();
        }

        // Update UI
        UpdateSlot(playerSlots[playerIndex], true);
    }

    // On player disconnect
    public void OnPlayerLeave(PlayerInput input)
    {
        Debug.Log("Player left!");

        PlayerController player = input.GetComponent<PlayerController>();

        // Remove player from correct slot
        playerSlots[input.playerIndex].player = player;

        // Enable player joining on max players reached and menu already open
        if (playerCount == GetCurrentPlayerCount() && menuOpen)
        {
            playerInputManager.EnableJoining();
        }

        // Update UI
        UpdateSlot(playerSlots[input.playerIndex], false);
    }

    // Update the current player slot
    private void UpdateSlot(PlayerSlot slot, bool joined)
    {
        PlayerInput input = null;
        if (joined)
        {
            input = slot.player.input;
        }

        // Set default box color and slot nr
        slot.border.color = colors[joined ? input.playerIndex : 4];
        slot.unusedPlayerNr.gameObject.SetActive(!joined);

        // Set device icon
        slot.device.gameObject.SetActive(joined);
        slot.deviceOutline.gameObject.SetActive(joined);

        if (joined)
        {
            PlayerDevice device = playerDevices.FirstOrDefault(device => device.name == input.currentControlScheme);

            slot.device.sprite = device.sprite;
            slot.deviceOutline.sprite = device.outline;
        }

        // Activate/deactivate corner piece
        slot.corner.gameObject.SetActive(joined);
    }

    private void UpdateVisibleSlots()
    {
        if(playerCount < 1)
        {
            return;
        }

        float startingPosX = -200 * (playerCount - 1);

        // Calculate slot positioning and hide slots that aren't needed
        for (int i = 0; i < playerSlots.Length; i++)
        {
            // Hide slots that aren't needed
            if (i >= playerCount)
            {
                playerSlots[i].border.gameObject.SetActive(false);
            }
            else
            {
                RectTransform rect = playerSlots[i].border.GetComponent<RectTransform>();

                rect.anchoredPosition = new Vector2(startingPosX + (400 * i), 0);

                playerSlots[i].border.gameObject.SetActive(true);
            }
        }
    }

    // Open/close the player menu
    private void ShowMenu(bool show)
    {
        menuOpen = show;

        backgroundBlur.SetActive(show);
        mainPanel.SetActive(show);

        // Enable/disable player joining
        if (show)
            playerInputManager.EnableJoining();
        else
            playerInputManager.DisableJoining();
    }

    // Animate player menu
    private IEnumerator FadeMenu(bool fadeIn)
    {
        return null;
    }

    // Disconnect all players
    private void DisconnectAll()
    {
        foreach (PlayerSlot slot in playerSlots)
        {
            PlayerController player = slot.player;
            slot.player = null;

            Destroy(player.gameObject);
        }
    }

    public void Ready()
    {
        ShowMenu(false);
    }

    // Remove player on device lost
    public void DeviceLost(PlayerController player)
    {
        Destroy(player.gameObject);
    }
}
