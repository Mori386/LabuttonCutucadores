using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputReceiver : MonoBehaviour
{
    private PlayerControl playerControl;
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
        PlayerInputActions playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        playerInputActions.PlayerGameplayInputMap.MovementInput.performed += OnMovementInput;
        playerInputActions.PlayerGameplayInputMap.RotationInput.performed += OnRotationInput;
        playerControl = GetComponent<PlayerControl>();

        for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
        {
            StartCoroutine(playerControl.SendTransformInfo(Multiplayer.Host.clients.Keys.ElementAt(i), 0, playerControl.transform));
        }
    }
}

