using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player input controls
    private Vector3 movementInput = Vector3.zero;
    private Vector2 cameraInput = Vector2.zero;
    private bool driftingInput = false;
    private bool flipInput = false;
    private bool abilityInput = false;

    [HideInInspector]
    public PlayerInput input;
    [HideInInspector]
    public PlayerManager playerManager;

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

    private void OnDisable()
    {
        input.actions = null;
    }


    private void Update()
    {
        bool isReady = car && cameraFollow && abilityController;
        if (!isReady)
        {
            return;
        }

        UpdateCarInputs();
        UpdateAbilityInputs();
        UpdateCameraInputs();
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

    public void OnReady(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() > 0.5f)
        {
            playerManager.Ready();
        }
    }
    public void OnDisconnect(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() > 0.5f)
        {
            playerManager.DisconnectPlayer(this);
        }
    }

    public void OnDeviceLost()
    {
        Debug.Log("Device lost!");
        playerManager.DeviceLost(this);
    }
}
