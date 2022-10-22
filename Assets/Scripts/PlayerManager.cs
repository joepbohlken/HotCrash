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
    public PlayerSlot[] playerSlots = new PlayerSlot[4];
    public PlayerDevice[] playerDevices;
    public Color[] colors;

    private PlayerInputManager playerInputManager;
    private Canvas canvas;

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
        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
        canvas = GetComponentInChildren<Canvas>();
    }

    private void Update()
    {
        
    }

    // On player connect
    public void OnPlayerJoin(PlayerInput input)
    {
        Debug.Log("Player joined!");

        PlayerController player = input.GetComponent<PlayerController>();
        player.playerIndex = input.playerIndex;
        player.input = input;
        player.playerManager = this;

        // Add player to correct slot
        playerSlots[input.playerIndex].player = player;

        // Update UI
        UpdateSlot(playerSlots[input.playerIndex], true);
    }

    // On player disconnect
    public void OnPlayerLeave(PlayerInput input)
    {
        Debug.Log("Player left!");

        PlayerController player = input.GetComponent<PlayerController>();

        // Remove player from correct slot
        playerSlots[input.playerIndex].player = player;

        // Update UI
        UpdateSlot(playerSlots[input.playerIndex], false);
    }

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

    private void OpenMenu()
    {

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

    }

    public void DeviceLost(PlayerController player)
    {
        Destroy(player.gameObject);
    }
}
