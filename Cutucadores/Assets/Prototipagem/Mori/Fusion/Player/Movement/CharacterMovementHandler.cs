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
        if (!Object.HasInputAuthority) Destroy(this);
    }
    public override void FixedUpdateNetwork()
    {
        networkCharacterDrillController.CalculateVelocity();
        if(GetInput(out NetworkInputData networkInputData))
        {
            networkCharacterDrillController.Move(networkInputData.movementInput.y);
            networkCharacterDrillController.Rotate(networkInputData.movementInput.x);
        }
    }
}
