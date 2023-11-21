using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using TMPro;

[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
public class NetworkCharacterDrillController : NetworkTransform
{
    public InGameCharacterData characterData;
    public float activeSpeedMultiplier = 1f;
    readonly private float speedBoostMultiplier = 1.5f;
    readonly private float speedBoostDuration = 10f;
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
    [Space] public Transform visual;
    public TrailRenderer[] speedBoostTrail;
    [HideInInspector] public CharacterInputHandler characterInputHandler;
    [HideInInspector] public HPHandler hpHandler;
    [HideInInspector] public NetworkVisualHandler visualHandler;

    [HideInInspector] public Collider[] playerColliders;
    protected override void Awake()
    {
        base.Awake();
        CacheInfos();
    }
    public override void Spawned()
    {
        base.Spawned();
        CacheInfos();
        GameManager.Instance.playersControllers.Add(this);
    }
    private void CacheInfos()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (visualHandler == null) visualHandler = GetComponent<NetworkVisualHandler>();
        if (characterInputHandler == null) characterInputHandler = GetComponent<CharacterInputHandler>();
        Collider[] collidersFoundInThisObject = GetComponents<Collider>();
        Collider[] collidersFoundInChild = GetComponentsInChildren<Collider>();
        playerColliders = new Collider[collidersFoundInThisObject.Length + collidersFoundInChild.Length];
        int arrayLocation =0;
        for (int i = 0; i < collidersFoundInThisObject.Length; i++)
        {
            playerColliders[arrayLocation] = collidersFoundInThisObject[i];
            arrayLocation++;
        }

        for (int i = 0; i < collidersFoundInChild.Length; i++)
        {
            playerColliders[arrayLocation] = collidersFoundInChild[i];
            arrayLocation++;
        }
        if (hpHandler == null) hpHandler = GetComponent<HPHandler>();

    }
    public void CalculateVelocity()
    {
        Velocity = new Vector2(rb.velocity.x, rb.velocity.z) * Runner.Simulation.Config.TickRate;
    }
    public virtual void Move(float direction)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 moveForce = transform.forward * direction * 50 * characterData.maxSpeed * deltaTime * activeSpeedMultiplier;
        rb.AddForce(moveForce, ForceMode.Acceleration);
        visualHandler.rotationDirection = direction;
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
        for (int i = 0; i < speedBoostTrail.Length; i++)
        {
            speedBoostTrail[i].enabled = state;
        }
    }
    public virtual void DefineTrailTime(float time)
    {
        for (int i = 0; i < speedBoostTrail.Length; i++)
        {
            speedBoostTrail[i].time = time;
        }
    }
    public Coroutine speedBoostCoroutine;
    public IEnumerator SpeedBoost()
    {
        visualHandler.PlayPowerUpVfx();
        yield return new WaitForSeconds(0.1f);
        if (Object.HasInputAuthority) GameManager.Instance.ShakeCamera(20f);
        rb.AddForce(transform.forward * 20f, ForceMode.VelocityChange);
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
    bool isFalling;
    public virtual void FallInHole(Vector3 holePosition)
    {
        if (!isFalling)
        {
            isFalling = true;
            ToggleCharacterInput(false);
            rb.velocity = Vector3.zero;
            ToggleCharacterCollider(false);
            StartCoroutine(FallIntoHole(holePosition));
        }
    }
    public IEnumerator FallIntoHole(Vector3 holePosition)
    {
        //Para fazer a rotacao dele virando rotacionar ele baseado em um ponto embaixo dele no momento com a diferenca de altura sendo a distancia dele do buraco
        Vector3 startPos = transform.position;
        transform.position = holePosition;
        Quaternion originalRotation = transform.rotation;
        float timer = 0;
        Vector3 rotationDirection = Vector3.zero;
        rotationDirection.x = Random.Range(0, 1) * 2 - 1;
        rotationDirection.y = Random.Range(0, 1) * 2 - 1;
        rotationDirection.z = Random.Range(0, 1) * 2 - 1;
        while (timer < 1f)
        {
            transform.Rotate(rotationDirection * 3f);
            transform.position += new Vector3(0, -0.5f, 0);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        hpHandler.OnTakeDamage(1);
        if (!hpHandler.isDead)
        {
            Vector3 deltaPos = startPos - holePosition;
            deltaPos.y = 0;
            deltaPos.Normalize();
            ToggleCharacterVisual(false);
            yield return new WaitForSeconds(0.5f);
            transform.position = startPos + deltaPos * 10f;
            transform.LookAt(startPos + deltaPos * 11f);
            ToggleCharacterVisual(true);
            ToggleCharacterInput(true);
            ToggleCharacterCollider(true);
        }
        else
        {
            transform.rotation = originalRotation;
        }
        isFalling = false;

    }

    public void Die()
    {
        visualHandler.OndDeath();
        ToggleCharacterCollider(false);
        ToggleCharacterVisual(false);
        ToggleCharacterInput(false);
        GameManager.Instance.CheckIfThereIsWinner();
    }
    public void ToggleCharacterInput(bool state)
    {
        characterInputHandler.enabled = state;
    }
    public void ToggleCharacterCollider(bool state)
    {
        for (int i = 0; i < playerColliders.Length; i++)
        {
            playerColliders[i].enabled = state;
        }
    }
    public void ToggleCharacterVisual(bool state)
    {
        visual.gameObject.SetActive(state);
    }
    public virtual void Rotate(float direction)
    {
        //rb.AddTorque(transform.up * direction * characterData.rotationSpeed * Runner.DeltaTime*30f, ForceMode.Acceleration);
        rb.rotation = transform.rotation * Quaternion.Euler(0, direction * Runner.DeltaTime * characterData.rotationSpeed * (0.85f + activeSpeedMultiplier * 0.15f) * 10f, 0);
    }
}