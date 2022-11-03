using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class SwitchCameras : MonoBehaviour
{
    public CinemachineVirtualCamera camera1;
    public CinemachineVirtualCamera camera2;

    public InputActionAsset playerInput;

    private void Update()
    {

        if (playerInput.FindAction("Camera").IsPressed())
        {

        }
    }
}
