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
public class NetworkCharacterDrillController : NetworkBehaviour
{
    public InGameCharacterData characterData;

    public float activeSpeedMultiplier = 1f;
    readonly private float speedBoostMultiplier = 2f;
    readonly private float speedBoostDuration = 10f;

    [Networked]
    [HideInInspector]
    public Vector2 Velocity { get; set; }

    public Rigidbody rb { get; private set; }

    [Space] public Transform visual;
    public Transform drillVisual;
    public TrailRenderer[] speedBoostTrail;

    protected void Awake()
    {
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
    float rotationDirection;
    public void RotateDrill()
    {
        drillVisual.Rotate(2.5f + rotationDirection * Velocity.magnitude * 2f, 0, 0, Space.Self);
    }
    public void CalculateVelocity()
    {
        Velocity = new Vector2(rb.velocity.x, rb.velocity.z);
    }
    public virtual void Move(float direction)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 moveForce = transform.forward * direction * 50 * characterData.maxSpeed * deltaTime * activeSpeedMultiplier;
        rb.AddForce(moveForce, ForceMode.Acceleration);
        rotationDirection = direction;
    }
    public virtual void Knockback(Vector3 contactPoint, bool considerWeight)
    {
        Vector3 directionOfKnockback;
        directionOfKnockback = transform.position - contactPoint;
        directionOfKnockback = new Vector3(directionOfKnockback.x, 0, directionOfKnockback.z);
        directionOfKnockback.Normalize();
        if (considerWeight)
        {
            directionOfKnockback = directionOfKnockback * (0.5f + ((100 - characterData.weight) / 100) * 0.25f) * 50;
        }
        else
        {
            directionOfKnockback = directionOfKnockback * 0.75f * 50;
        }
        rb.AddForce(directionOfKnockback, ForceMode.VelocityChange);
    }

    public void StartSpeedBoost()
    {
        if (speedBoostCoroutine == null)
        {
            speedBoostCoroutine = StartCoroutine(SpeedBoost());
        }
        else
        {
            StopCoroutine(speedBoostCoroutine);
            speedBoostCoroutine = StartCoroutine(SpeedBoost());
        }
    }
    public void SetActiveStateSpeedBoostVisual(bool state)
    {
        for(int i =0;i<speedBoostTrail.Length;i++)
        {
            speedBoostTrail[i].enabled = state;
        }
    }
    public void DefineTrailTime(float time)
    {
        for (int i = 0; i < speedBoostTrail.Length; i++)
        {
            speedBoostTrail[i].time = time;
        }
    }
    public Coroutine speedBoostCoroutine;
    public IEnumerator SpeedBoost()
    {
        yield return new WaitForSeconds(0.5f);
        SetActiveStateSpeedBoostVisual(true);
        activeSpeedMultiplier = speedBoostMultiplier;
        yield return new WaitForSeconds(speedBoostDuration);
        float timer = 0;
        float trailTime = speedBoostTrail[0].time;
        while (timer < 0.5f)
        {
            DefineTrailTime(Mathf.Lerp(trailTime, 0, timer / 0.5f));
            activeSpeedMultiplier = Mathf.Lerp(speedBoostMultiplier, 1, timer / 0.5f);
            timer += Time.deltaTime;
            yield return null;
        }
        activeSpeedMultiplier = 1;
        SetActiveStateSpeedBoostVisual(false);
        DefineTrailTime(trailTime);
        speedBoostCoroutine = null;
    }
    
    public virtual void Rotate(float direction)
    {
        //rb.AddTorque(transform.up * direction * rotationSpeed * Runner.DeltaTime, ForceMode.Force);
        transform.Rotate(0, direction * Runner.DeltaTime * characterData.rotationSpeed * 10f, 0);
    }
}
