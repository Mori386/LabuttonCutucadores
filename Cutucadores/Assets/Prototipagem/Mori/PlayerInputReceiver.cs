using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReceiver : MonoBehaviour
{
    private PlayerControl playerControl;
    private PlayerInput playerInput;
    public void OnMovementInput(InputAction.CallbackContext context)
    {
        float moveInput = context.ReadValue<float>();
        playerControl.moveDirection = moveInput;
        playerControl.OnMoveInputReceive();
    }
    public void OnRotationInput(InputAction.CallbackContext context)
    {
        float rotateInput = context.ReadValue<float>();
        playerControl.rotationDirection = rotateInput;
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        playerInputActions.PlayerGameplayInputMap.MovementInput.performed += OnMovementInput;
        playerInputActions.PlayerGameplayInputMap.RotationInput.performed += OnRotationInput;
        playerControl = GetComponent<PlayerControl>();
    }

}
