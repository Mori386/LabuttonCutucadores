using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerControl;

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
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                playerControl.connectedAdress = Multiplayer.Host.clients.Keys.ElementAt(i);
                playerControl.StartCoroutine(playerControl.SendInfoLoop());
            }
        }
        else
        {
            playerControl.connectedAdress = Multiplayer.Client.HostIP;
            playerControl.StartCoroutine(playerControl.SendInfoLoop());
        }
    }
    private void FixedUpdate()
    {
        Vector3 roundPos = new Vector3(Mathf.Round(transform.position.x * 1000) / 1000, Mathf.Round(transform.position.y * 1000) / 1000, 0);
        Vector2 roundRot = new Vector2(Mathf.Round(transform.rotation.z * 1000) / 1000, Mathf.Round(transform.rotation.w * 1000) / 1000);
        playerControl.positionToGo = playerControl.playerID.ToString() + roundPos.x + "Y" + roundPos.y + "Z" + roundRot.x + "W" + roundRot.y;
    }
}


