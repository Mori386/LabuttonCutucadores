using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using TMPro;
using Unity.VisualScripting;

[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
public class NetworkCharacterDrillController : NetworkTransform
{
    public InGameCharacterData characterData;

    [Networked]
    [HideInInspector]
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// Sets the default teleport interpolation velocity to be the CC's current velocity.
    /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToPosition"/>.
    /// </summary>
    protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;

    /// <summary>
    /// Sets the default teleport interpolation angular velocity to be the CC's rotation speed on the Z axis.
    /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToRotation"/>.
    /// </summary>
    protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, characterData.rotationSpeed);

    public Rigidbody rb { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        CacheRigidbody();
    }

    public override void Spawned()
    {
        base.Spawned();
        CacheRigidbody();
    }
    private void CacheRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }
    public void CalculateVelocity()
    {
        Velocity = new Vector2(rb.velocity.x,rb.velocity.z);
    }
    public virtual void Move(float direction)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 moveForce = transform.forward * direction * 50 * characterData.maxSpeed * deltaTime;
        rb.AddForce(moveForce, ForceMode.Acceleration);
    }

    public virtual void Rotate(float direction)
    {
        //rb.AddTorque(transform.up * direction * rotationSpeed * Runner.DeltaTime, ForceMode.Force);
        transform.Rotate(0, direction * Runner.DeltaTime * characterData.rotationSpeed * 10f, 0);
    }
}
