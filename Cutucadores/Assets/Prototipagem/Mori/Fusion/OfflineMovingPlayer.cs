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
    private void Start()
    {
        rb.inertiaTensor = rb.inertiaTensor;

        rb.inertiaTensorRotation = rb.inertiaTensorRotation;
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
        float fixedDeltaTime = Time.fixedDeltaTime;
        rb.AddForce(transform.forward * YDirection * 100 * fixedDeltaTime * characterData.maxSpeed,ForceMode.Force);


    }
    private void Rotate(float XDirection)
    {
        rb.AddTorque(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed * 10, 0,ForceMode.Force);
        //rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles + new Vector3(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed * 10, 0)));
        //transform.Rotate(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed*10, 0);
    }
}
