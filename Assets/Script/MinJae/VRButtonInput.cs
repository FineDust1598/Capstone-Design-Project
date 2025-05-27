using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRButtonInput : MonoBehaviour

{
    private VRInputAction inputActions;

    private void Awake()
    {
        inputActions = new VRInputAction();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.XRIRIghtHand.AButton.started += OnAButtonDown;
        inputActions.XRIRIghtHand.AButton.canceled += OnAButtonUp;
    }

    private void OnDisable()
    {
        inputActions.XRIRIghtHand.AButton.started -= OnAButtonDown;
        inputActions.XRIRIghtHand.AButton.canceled -= OnAButtonUp;

        inputActions.Disable();
    }

    private void OnAButtonDown(InputAction.CallbackContext context)
    {
        Debug.Log("A 버튼 Down");
    }

    private void OnAButtonUp(InputAction.CallbackContext context)
    {
        Debug.Log("A 버튼 Up");
    }
}
