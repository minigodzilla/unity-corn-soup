#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookWithMouse : MonoBehaviour
{
    const float k_MouseSensitivityMultiplier = 0.01f;

    public float mouseSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        LockPressed();

        InputManager.Instance.clickEvent.AddListener(LockPressed);
    }

    // Update is called once per frame
    void Update()
    {
        bool unlockPressed = false, lockPressed = false;

#if ENABLE_INPUT_SYSTEM
        float mouseX = 0, mouseY = 0;

        if (Mouse.current != null)
        {
            var delta = Mouse.current.delta.ReadValue() / 15.0f;
            mouseX += delta.x;
            mouseY += delta.y;
            lockPressed = Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame;
        }
        if (Joystick.current != null)
        {
            var value = Joystick.current.stick.ReadValue() * 4;
            mouseX += value.x;
            mouseY += -value.y;
        }
        if (Gamepad.current != null)
        {
            var value = Gamepad.current.rightStick.ReadValue() * 2;
            mouseX += value.x;
            mouseY += value.y;
        }
        if (Keyboard.current != null)
        {
            unlockPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        }

        mouseX *= mouseSensitivity * k_MouseSensitivityMultiplier;
        mouseY *= mouseSensitivity * k_MouseSensitivityMultiplier;
#else
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * k_MouseSensitivityMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * k_MouseSensitivityMultiplier;

        unlockPressed = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (unlockPressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    public static void LockPressed() {
        if (!UserInterfaceController.Instance.titleVisible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
