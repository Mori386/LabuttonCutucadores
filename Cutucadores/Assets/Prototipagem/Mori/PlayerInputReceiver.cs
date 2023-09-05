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
using static UnityEditor.PlayerSettings;

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
        if (Multiplayer.isHost)
        {
            for(int i = 0; i<Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                StartCoroutine(SendTransformInfo(Multiplayer.Host.clients.Keys.ElementAt(i), 0, transform));
            }
        }
        else
        {
            StartCoroutine(SendTransformInfo(Multiplayer.Client.HostIP, Multiplayer.Client.myID, transform));
        }
    }

    public IEnumerator SendTransformInfo(string IPAdress, int playerID, Transform transform)
    {
        while (true)
        {
            Vector3 roundPos = new Vector3(Mathf.Round(transform.position.x * 1000) / 1000, Mathf.Round(transform.position.y * 1000) / 1000,0);
            Multiplayer.SendMessageToIP(IPAdress, playerID.ToString()+ roundPos.x+"Y"+ roundPos.y+"Z"+ Mathf.Round(transform.rotation.eulerAngles.z * 1000) / 1000);
            yield return null;
        }
    }
}
