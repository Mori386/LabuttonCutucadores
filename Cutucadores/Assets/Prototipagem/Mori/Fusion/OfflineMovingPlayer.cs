using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
    }
    private void Move(float YDirection)
    {

    }
    private void Rotate(float XDirection)
    {

    }
}
