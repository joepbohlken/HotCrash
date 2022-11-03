using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    // Player input controls
    private float movementInputX = 0;
    private float movementInputY = 0;
    private float movementInputZ = 0;
    private Vector2 cameraInput = Vector2.zero;
    private bool driftingInput = false;
    private bool flipInput = false;
    private bool abilityInput = false;
    private bool readyInput = false;
    private bool cancelInput = false;
    private bool disconnectInput = false;
    private bool changeColor = false;

    // Public variables
    public PlayerInput input { get; set; }
    public PlayerManager playerManager { get; set; }
    public CarSelectionSlot carSelectionSlot { get; set; }
    public CarController car { get; set; }
    public AbilityController abilityController { get; set; }
    public CameraController cameraFollow { get; set; }
    public int playerIndex { get; set; }
    public string deviceType { get; set; }
    public Color playerColor { get; set; }

    private bool justJoined = true;
    private float time = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        time += Time.unscaledDeltaTime;
        if (time > .1f)
        {
            justJoined = false;
        }

        if (playerManager.menuOpen)
        {
            if (readyInput)
            {
                readyInput = false;
                playerManager.Ready(justJoined);
            }
            if (cancelInput && playerIndex == 0)
            {
                cancelInput = false;
                playerManager.Cancel();
            }
            if (disconnectInput)
            {
                disconnectInput = false;
                playerManager.DisconnectPlayer(this);
            }
        }

        if (car)
            UpdateCarInputs();

        if (abilityController)
            UpdateAbilityInputs();

        if (cameraFollow)
            UpdateCameraInputs();

        if (carSelectionSlot)
            UpdateCarSelection();

        // Confirm return to main menu
        if (GameManager.main != null && GameManager.main.leaderboardOpen && playerIndex == 0 && readyInput)
        {
            readyInput = false;
            GameManager.main.ReturnToMainMenu();
        }
    }

    private void UpdateCarInputs()
    {
        car.UpdateControls(movementInputX, movementInputY, movementInputZ, driftingInput, flipInput);
    }

    private void UpdateAbilityInputs()
    {
        if (abilityInput)
            abilityController.UseAbility();
    }

    private void UpdateCameraInputs()
    {
        cameraFollow.x = cameraInput.x;
        cameraFollow.y = cameraInput.y;
    }

    private void UpdateCarSelection()
    {
        if (carSelectionSlot.interactable || carSelectionSlot.ready)
            carSelectionSlot.h = cameraInput.x;

        if (cancelInput)
        {
            cancelInput = false;

            if (carSelectionSlot.ready)
                carSelectionSlot.UnReady();
            else if (carSelectionSlot.interactable)
                carSelectionSlot.menu.GoBackToHome();
        }

        if (carSelectionSlot.interactable)
        {
            if (movementInputX > 0.8f)
            {
                carSelectionSlot.NextCarLeft();
            }

            if (movementInputX < -0.8f)
            {
                carSelectionSlot.NextCarRight();
            }

            if (changeColor)
            {
                carSelectionSlot.ChangeColor();
                changeColor = false;
            }

            if (readyInput)
            {
                readyInput = false;
                carSelectionSlot.Ready();
            }
        }
    }

    public void OnMoveX(InputAction.CallbackContext ctx)
    {
        movementInputX = ctx.ReadValue<float>();
    }
    public void OnMoveY(InputAction.CallbackContext ctx)
    {
        movementInputY = ctx.ReadValue<float>();
    }
    public void OnMoveZ(InputAction.CallbackContext ctx)
    {
        movementInputZ = ctx.ReadValue<float>();
    }

    public void OnCameraMove(InputAction.CallbackContext ctx)
    {
        cameraInput = ctx.ReadValue<Vector2>();
    }

    public void OnDrift(InputAction.CallbackContext ctx)
    {
        driftingInput = ctx.ReadValue<float>() > 0.5f;
    }

    public void OnFlip(InputAction.CallbackContext ctx)
    {
        flipInput = ctx.ReadValue<float>() > 0.5f;
    }

    public void OnAbility(InputAction.CallbackContext ctx)
    {
        abilityInput = ctx.ReadValue<float>() > 0.5f;
    }
    public void OnChangeColor(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is PressInteraction)
        {
            changeColor = ctx.performed;
        }
    }

    public void OnReady(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is PressInteraction)
        {
            readyInput = ctx.performed;
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is PressInteraction)
        {
            cancelInput = ctx.performed;
        }
    }

    public void OnDisconnect(InputAction.CallbackContext ctx)
    {
        if (ctx.interaction is PressInteraction)
        {
            disconnectInput = ctx.performed;
        }
    }

    public void OnDeviceLost()
    {
        Debug.Log("Device lost!");
        playerManager.DeviceLost(this);
    }
}
