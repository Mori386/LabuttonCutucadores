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
        if (characterMovementHandler.Object.HasInputAuthority) StartCoroutine(GetInputCoroutine());
    }
    public IEnumerator GetInputCoroutine()
    {
        while(true)
        {
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");
            yield return null;
        }
    }


    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData(moveInputVector);
        return networkInputData;
    }
}
