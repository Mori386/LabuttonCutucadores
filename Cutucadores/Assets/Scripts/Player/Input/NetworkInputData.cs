using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public NetworkInputData(Vector2 MovementInput)
    {
        movementInput = MovementInput;
    }
}
