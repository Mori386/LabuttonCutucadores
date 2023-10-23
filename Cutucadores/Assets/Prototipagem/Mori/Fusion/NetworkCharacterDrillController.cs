using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
public class NetworkCharacterDrillController : NetworkTransform
{
    [Header("Character Controller Settings")]
    public float acceleration = 10.0f;
    public float braking = 1.0f;
    public float maxSpeed = 2.0f;
    public float rotationSpeed = 15.0f;

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
    protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, rotationSpeed);

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
    public virtual void Move(Vector3 direction)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 previousPos = transform.position;
        Vector3 moveVelocity = Velocity;

        direction = direction.normalized;
        if(direction == Vector3.zero)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.AddForce(direction * maxSpeed * 100f * deltaTime, ForceMode.Force);
        }

        Velocity = (transform.position - previousPos) * Runner.Simulation.Config.TickRate;
    }

    public virtual void Rotate(float direction)
    {
        //rb.AddTorque(transform.up * direction * rotationSpeed * Runner.DeltaTime, ForceMode.Force);
        transform.Rotate(0, direction * Runner.DeltaTime * rotationSpeed*10f, 0);
    }
}
