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
        networkCharacterDrillController.CalculateVelocity();
        if(GetInput(out NetworkInputData networkInputData))
        {
            networkCharacterDrillController.RotateDrill();
            networkCharacterDrillController.Move(networkInputData.movementInput.y);
            networkCharacterDrillController.Rotate(networkInputData.movementInput.x);
        }
    }
}