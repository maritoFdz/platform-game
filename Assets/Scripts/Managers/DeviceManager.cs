using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceManager : MonoBehaviour
{
    public static DeviceManager instance;

    public bool UsingGamepad {  get; private set; }
    public Gamepad ActiveGamepad => UsingGamepad ? Gamepad.current : null;
    public event Action<bool> OnDeviceChanged;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        UsingGamepad = Gamepad.current != null;
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object control, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;

        InputDevice device = (control as InputAction)?.activeControl?.device;
        if (device == null) return;

        bool isPad = device is Gamepad;
        if (isPad == UsingGamepad) return;

        UsingGamepad = isPad;
        OnDeviceChanged?.Invoke(UsingGamepad); // mouseherramienta que nos servira mas tarde
    }
}
