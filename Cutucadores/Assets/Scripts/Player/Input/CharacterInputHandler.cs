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
        StartCoroutine(Delay());
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
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(.5f);
        EnableCharacter();
    }
    public void EnableCharacter()
    {
        //Para nao contar inputs desnecessarios ele contabiliza so se tiver input authority
        if (characterMovementHandler.Object.HasInputAuthority && InputRegisterCoroutine == null)
        {
            //Debug.Log($"{NetworkBetweenScenesManager.Instance.userIDToPlayerData[characterMovementHandler.Runner.UserId].username} Starting input coroutine");
            InputRegisterCoroutine = StartCoroutine(GetInputCoroutine());
        }
    }
    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData(moveInputVector);
        return networkInputData;
    }
}
