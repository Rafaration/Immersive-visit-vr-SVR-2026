using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject painelDoMenu;
    public InputActionReference abrirMenu;


    private void Awake()
    {
        if (abrirMenu != null && abrirMenu.action != null)
        {
            abrirMenu.action.Enable();
            abrirMenu.action.performed += ToggleMenu;
        }
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        if (abrirMenu != null && abrirMenu.action != null)
        {
            abrirMenu.action.Disable();
            abrirMenu.action.performed -= ToggleMenu;
        }
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    { 
        painelDoMenu.SetActive(!painelDoMenu.activeSelf);
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                abrirMenu.action.Disable();
                abrirMenu.action.performed -= ToggleMenu;
                break;
            case InputDeviceChange.Reconnected:
                abrirMenu.action.Enable();
                abrirMenu.action.performed += ToggleMenu;
                break;
        }
    }

}
