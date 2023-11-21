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
        EnableCharacter();
    }
    public Coroutine InputRegisterCoroutine;
    public IEnumerator GetInputCoroutine()
    {
        while(true)
        {
            moveInputVector.x = Input.GetAxis("Horizontal");
            moveInputVector.y = Input.GetAxis("Vertical");
            yield return null;
        }
    }

    private void OnDisable()
    {
        if(InputRegisterCoroutine != null)
        {
            StopCoroutine(InputRegisterCoroutine);
            InputRegisterCoroutine = null;
            moveInputVector = Vector2.zero;
        }
    }
    private void OnEnable()
    {
        EnableCharacter();
    }
    public void EnableCharacter()
    {
        if (characterMovementHandler.Object.HasInputAuthority && InputRegisterCoroutine == null) InputRegisterCoroutine = StartCoroutine(GetInputCoroutine());
    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData(moveInputVector);
        return networkInputData;
    }
}
