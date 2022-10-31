using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player input controls
    private Vector3 movementInput = Vector3.zero;
    private Vector2 cameraInput = Vector2.zero;
    private bool abilityInput = false;
    private bool driftingInput = false;
    private bool flipInput = false;
    [HideInInspector]
    public bool disconnect = false;
    [HideInInspector]
    public bool startGame = false;

    [HideInInspector]
    public bool isReady = false;

    [HideInInspector]
    public ArcadeCar car;
    [HideInInspector]
    public AbilityController abilityController;
    [HideInInspector]
    public CameraController cameraFollow;

    private PlayerInput input;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        isReady = car && cameraFollow && abilityController;
        if (!isReady)
        {
            return;
        }

        UpdateCarInputs();
        UpdateAbilityInputs();
        UpdateCameraInputs();
    }

    private void OnDisable()
    {
        input.actions = null;
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
        if (abilityInput)
            abilityController.UseAbility();
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
        driftingInput = ctx.ReadValue<float>() > 0.1f;
    }

    public void OnAbility(InputAction.CallbackContext ctx)
    {
        abilityInput = ctx.ReadValue<float>() > 0.1f;
    }

    public void OnFlip(InputAction.CallbackContext ctx)
    {
        flipInput = ctx.ReadValue<float>() > 0.1f;
    }

    public void OnDisconnect(InputAction.CallbackContext ctx)
    {
        disconnect = ctx.ReadValue<float>() > 0.1f;
    }

    public void OnStartGame(InputAction.CallbackContext ctx)
    {
        startGame = ctx.ReadValue<float>() > 0.1f;
    }
}
