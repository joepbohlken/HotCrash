using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    // Player input controls
    private Vector3 movementInput = Vector3.zero;
    private Vector2 cameraInput = Vector2.zero;
    private bool driftingInput = false;
    private bool flipInput = false;
    private bool abilityInput = false;
    private bool readyInput = false;
    private bool cancelInput = false;
    private bool disconnectInput = false;
    private bool changeColor = false;

    [HideInInspector]
    public PlayerInput input;
    [HideInInspector]
    public PlayerManager playerManager;

    [HideInInspector]
    public CarSelectionSlot carSelectionSlot;
    [HideInInspector]
    public ArcadeCar car;
    [HideInInspector]
    public AbilityController abilityController;
    [HideInInspector]
    public CameraController cameraFollow;
    [HideInInspector]
    public int playerIndex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (playerManager.menuOpen)
        {
            if (readyInput)
            {
                readyInput = false;
                playerManager.Ready();
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

        if (carSelectionSlot && carSelectionSlot.interactable)
            UpdateCarSelection();
    }

    private void UpdateCarInputs()
    {
        car.v = movementInput.y;
        car.h = movementInput.x;
        car.qe = movementInput.z;
        car.flip = flipInput;
        car.handbrake = driftingInput;
    }

    private void UpdateAbilityInputs()
    {
        abilityController.useAbility = abilityInput;
    }

    private void UpdateCameraInputs()
    {
        cameraFollow.x = cameraInput.x;
        cameraFollow.y = cameraInput.y;
    }

    private void UpdateCarSelection()
    {
        if (movementInput.x > 0.8f)
        {
            carSelectionSlot.NextCarLeft();
        }

        if (movementInput.x < -0.8f)
        {
            carSelectionSlot.NextCarRight();
        }

        if (changeColor)
        {
            carSelectionSlot.ChangeColor();
            changeColor = false;
        }

        carSelectionSlot.h = cameraInput.x;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector3>();
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
