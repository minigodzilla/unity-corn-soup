#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public UnityEvent clickEvent;
    public UnityEvent anyButtonEvent;

    private static InputManager _instance;
    public static InputManager Instance {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<InputManager>();
            }

            return _instance;
        }
    }

#if ENABLE_INPUT_SYSTEM
    InputAction click;
    InputAction anyButton;
    // Start is called before the first frame update
    void Start()
    {
        click = new InputAction("Pick", binding: "<Joystick>/button3");
        click.AddBinding("<Mouse>/leftButton");
        click.performed += Click;
        
        click.Enable();

        anyButton = new InputAction("AnyButton", binding: "<Joystick>/button2");
        anyButton.AddBinding("<Joystick>/button3");
        anyButton.AddBinding("<Joystick>/button4");
        anyButton.AddBinding("<Joystick>/button5");
        anyButton.AddBinding("<Joystick>/button6");
        anyButton.AddBinding("<Joystick>/button7");
        anyButton.AddBinding("<Joystick>/button8");
        anyButton.AddBinding("<Joystick>/button9");
        anyButton.AddBinding("<Joystick>/button10");
        anyButton.AddBinding("<Joystick>/button11");
        anyButton.AddBinding("<Joystick>/button12");
        anyButton.performed += AnyButton;
        
        anyButton.Enable();
    }
#endif
    public void Click(InputAction.CallbackContext context) {
        clickEvent.Invoke();
    }
    public void AnyButton(InputAction.CallbackContext context) {
        anyButtonEvent.Invoke();
    }
}
