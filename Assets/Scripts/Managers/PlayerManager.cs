using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    public GameObject bottom;
    public Image disconnectBtn;
}

[Serializable]
public class PlayerDevice
{
    public string name;
    public Sprite sprite;
    public Sprite outline;
    public Sprite changeColorBtn;
    public Sprite disconnectBtn;
}

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent onReady;
    [HideInInspector]
    public UnityEvent onCancel;

    public static PlayerManager main;

    public PlayerSlot[] playerSlots = new PlayerSlot[4];
    public PlayerDevice[] playerDevices;
    public Color[] colors;

    public GameObject backgroundBlur;
    public GameObject mainPanel;

    private PlayerInputManager playerInputManager;
    private Animator animator;

    [HideInInspector]
    public List<PlayerController> players = new List<PlayerController>();
    [HideInInspector]
    public bool menuOpen = false;
    [HideInInspector]
    public int playerCount = 0;

    private bool ready = false;
    private bool cancelled = false;

    [HideInInspector]
    public bool canCancel = true;

    private bool gameManagerAvailable = false;

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
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        gameManagerAvailable = GameManager.main != null;
        int currentPlayerCount = GetCurrentPlayerCount();

        if (gameManagerAvailable)
        {
            playerCount = GameManager.main.playersCount;
        }

        // Only show required amount of player slots (i.e. 3 players = only 3 slots shown)s
        UpdateVisibleSlots();

        // If number of players isn't matching show menu 
        if (playerCount != currentPlayerCount && !menuOpen)
        {
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
        player.playerColor = colors[playerIndex];
        player.input = input;
        player.playerManager = this;

        // Add player to correct slot
        playerSlots[playerIndex].player = player;
        players.Add(player);

        // Disable player joining on max players reached or first player joined on startup
        if (playerCount == GetCurrentPlayerCount() || (playerIndex == 0 && !menuOpen))
        {
            playerInputManager.DisableJoining();

            if (gameManagerAvailable)
            {
                GameManager.main.playersCount = GetCurrentPlayerCount();
            }
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
        playerSlots[input.playerIndex].player = null;
        players.Remove(player);

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

        // Activate/deactivate corner piece
        slot.bottom.gameObject.SetActive(joined);

        if (joined)
        {
            PlayerDevice device = playerDevices.FirstOrDefault(device => device.name == input.currentControlScheme);
            slot.device.sprite = device.sprite;
            slot.deviceOutline.sprite = device.outline;

            slot.disconnectBtn.sprite = device.disconnectBtn;
            bool isKeyboard = input.currentControlScheme == "Keyboard";
            slot.disconnectBtn.rectTransform.localScale = new Vector3(isKeyboard ? 1.2f : 1f, isKeyboard ? .6f : 1f, 1f);
            slot.disconnectBtn.rectTransform.anchoredPosition = new Vector2(isKeyboard ? -30f : -25f, 0f);
        }
    }

    private void UpdateVisibleSlots()
    {
        if (playerCount < 1)
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
    public void ShowMenu(bool show)
    {
        menuOpen = show;

        animator.Play(show ? "ShowPlayerMenu" : "HidePlayerMenu");

        // Enable/disable player joining
        if (!show)
            playerInputManager.DisableJoining();
    }

    public void FinishedOpening()
    {
        ready = false;
        cancelled = false;

        int currentPlayerCount = GetCurrentPlayerCount();

        if (!gameManagerAvailable)
        {
            playerInputManager.EnableJoining();
            return;
        }

        if (currentPlayerCount < GameManager.main.playersCount)
        {
            playerInputManager.EnableJoining();
        }
        else
        {
            // Disonnect excess players
            for (int i = 0; i < players.Count; i++)
            {
                if (i >= GameManager.main.playersCount)
                {
                    RemovePlayer(players[i]);
                }
            }
        }
    }

    public void FinishedClosing()
    {
        if (cancelled)
        {
            onCancel.Invoke();
        }

        if (ready)
        {
            onReady.Invoke();
        }
    }

    // Remove player from device lost
    public void DisconnectPlayer(PlayerController player)
    {
        if (menuOpen)
        {
            RemovePlayer(player);
        }
    }

    // Disconnect all players
    private void DisconnectAll()
    {
        foreach (PlayerSlot slot in playerSlots)
        {
            PlayerController player = slot.player;
            slot.player = null;

            RemovePlayer(player);
        }
    }

    public void Ready(bool justJoined = false)
    {
        if ((!gameManagerAvailable || GetCurrentPlayerCount() == playerCount) && !justJoined)
        {
            ShowMenu(false);
            ready = true;

            playerCount = GetCurrentPlayerCount();
        }
    }

    public void Cancel()
    {
        if (canCancel)
        {
            ShowMenu(false);
            cancelled = true;
        }
    }


    // Remove player from device lost
    public void DeviceLost(PlayerController player)
    {
        RemovePlayer(player);
    }

    private void RemovePlayer(PlayerController player)
    {
        player.input.actions = null;
        Destroy(player.gameObject);
    }
}
