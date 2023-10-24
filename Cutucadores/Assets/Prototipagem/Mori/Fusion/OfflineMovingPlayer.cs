using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class OfflineMovingPlayer : MonoBehaviour
{
    private Vector2 movementInput;
    private Rigidbody rb;

    public CharacterData characterData;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    private void FixedUpdate()
    {
        
        Move(movementInput.y);
        Rotate(movementInput.x);
    }
    private void Move(float YDirection)
    {
        float deltaTime = Time.deltaTime;
        rb.velocity = transform.forward * YDirection*10;


    }
    private void Rotate(float XDirection)
    {
        transform.Rotate(0, XDirection * Time.deltaTime * characterData.rotationSpeed*10, 0);
    }
}
