using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    NetworkCharacterDrillController networkCharacterDrillController;
    private void Awake()
    {
        networkCharacterDrillController = GetComponent<NetworkCharacterDrillController>();
    }
    void Start()
    {
        
    }
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData networkInputData))
        {
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y;
            moveDirection.Normalize();

            networkCharacterDrillController.Move(moveDirection);
            networkCharacterDrillController.Rotate(networkInputData.movementInput.x);
        }
    }
}
