using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector;
    CharacterMovementHandler characterMovementHandler;
    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!characterMovementHandler.Object.HasInputAuthority) return;
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");
    }


    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();
        networkInputData.movementInput = moveInputVector;
        return networkInputData;
    }
}
