using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class OfflineMovingPlayer : MonoBehaviour
{
    private Vector2 movementInput;
    private Rigidbody rb;

    public InGameCharacterData characterData;
    public float activeSpeedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
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
        float deltaTime = Time.fixedDeltaTime;
        Vector3 moveForce = transform.forward * YDirection * 50 * characterData.maxSpeed * deltaTime * activeSpeedMultiplier;
        rb.AddForce(moveForce, ForceMode.Acceleration);
    }
    private void Rotate(float XDirection)
    {
        rb.rotation = transform.rotation * Quaternion.Euler(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed * 10f, 0);
        //rb.MoveRotation(Quaternion.Euler(rb.rotation.eulerAngles + new Vector3(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed * 10, 0)));
        //transform.Rotate(0, XDirection * Time.fixedDeltaTime * characterData.rotationSpeed*10, 0);
    }
}
