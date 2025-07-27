using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    
    //Componentes
    public Rigidbody rb { get; private set; }
    [Space] public Transform visual;
    public TrailRenderer[] speedBoostTrail;
    [HideInInspector] public CharacterInputHandler characterInputHandler;
    [HideInInspector] public HPHandler hpHandler;
    [HideInInspector] public NetworkVisualHandler visualHandler;
    [HideInInspector] public AudioPlayerHandler audioHandler;

    [HideInInspector] public Collider[] playerColliders;
    [SerializeField] private List<Transform> otherPlayersTransforms = new();
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
    /// <summary>
    /// Armazena as informaçoes do jogador
    /// </summary>
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
        if (audioHandler == null) audioHandler = GetComponent<AudioPlayerHandler>();
        StartCoroutine(GetDrillControllers());
    }

    private IEnumerator GetDrillControllers()
    {
        yield return new WaitForSeconds(.3f);
        Transform thisPlayer = transform.GetChild(1);
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player != thisPlayer)
                otherPlayersTransforms.Add(player.transform);
        }
    }

    #region Movement
    public virtual void Move(float direction)
    {
        float deltaTime = Runner.DeltaTime;
        Vector3 moveForce = transform.forward * direction * 50 * characterData.maxSpeed * deltaTime * activeSpeedMultiplier;
        rb.AddForce(moveForce, ForceMode.Acceleration);
        visualHandler.rotationDirection = direction;
        audioHandler.wheelVolume = Mathf.Abs(direction);
    }
    public virtual void Rotate(float direction)
    {
        rb.rotation = transform.rotation * Quaternion.Euler(0, direction * Runner.DeltaTime * characterData.rotationSpeed * (0.85f + activeSpeedMultiplier * 0.15f) * 10f, 0);
        visualHandler.RotateWheel(direction);
    }
    public void CalculateVelocity()
    {
        Velocity = new Vector2(rb.velocity.x, rb.velocity.z) * Runner.Simulation.Config.TickRate;
    }
    public virtual void Knockback(Vector3 contactPoint, bool considerWeight)
    {
        Vector3 directionOfKnockback;
        directionOfKnockback = transform.position - contactPoint;
        directionOfKnockback = new Vector3(directionOfKnockback.x, 0, directionOfKnockback.z);
        directionOfKnockback.Normalize();
        if (considerWeight)
        {
            directionOfKnockback = (0.5f + ((100 - characterData.weight) / 100) * 0.25f) * 100 * directionOfKnockback;
        }
        else
        {
            directionOfKnockback = 0.75f * 100 * directionOfKnockback;
        }
        rb.AddForce(directionOfKnockback, ForceMode.VelocityChange);
    }

    #region SpeedBoost
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
    #endregion
    #endregion

    #region Fall in hole
    bool isFalling;
    public virtual void FallInHole(Vector3 holePosition)
    {
        if (!isFalling)
        {
            isFalling = true;
            RPC_ToggleCharacterInput(false);
            rb.velocity = Vector3.zero;
            RPC_ToggleCharacterCollider(false);
            StartCoroutine(FallIntoHole(holePosition));
        }
    }
    public IEnumerator FallIntoHole(Vector3 holePosition)
    {
        Vector3 startPos = transform.position;
        transform.position = holePosition;
        Quaternion originalRotation = transform.rotation;

        //Cria uma rotacao aleatoria
        Vector3 rotationDirection = Vector3.zero;
        rotationDirection.x = Random.Range(0, 1) * 2 - 1;
        rotationDirection.y = Random.Range(0, 1) * 2 - 1;
        rotationDirection.z = Random.Range(0, 1) * 2 - 1;
        
        //Cair
        float timer = 0;
        while (timer < 1f)
        {
            transform.Rotate(rotationDirection * 3f);
            transform.position += new Vector3(0, -0.5f, 0);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Vector3 deltaPos = startPos - holePosition;
        deltaPos.y = 0;
        deltaPos.Normalize();
        RPC_ToggleCharacterVisual(false);
        yield return new WaitForSeconds(0.5f);
        transform.position = startPos + deltaPos * 10f;
        transform.LookAt(startPos + deltaPos * 11f);
        RPC_ToggleCharacterVisual(true);
        RPC_ToggleCharacterInput(true);
        RPC_ToggleCharacterCollider(true);

        isFalling = false;
    }

    #endregion

    #region Death/Damage
    public void Die()
    {
        visualHandler.OnDeath();
        RPC_ToggleCharacterInput(false);
        RPC_ToggleCharacterVisual(false);
        RPC_ToggleCharacterCollider(false);
    }

    public void Respawn()
    {
        Dictionary<int, float> spawnpointSafeDistance = new();
        for (int spawnpointCount = 0; spawnpointCount < GameManager.Instance.playerSpawnpoints.Length; spawnpointCount++)
        {
            spawnpointSafeDistance.Add(spawnpointCount, 0);
            foreach (Transform player in otherPlayersTransforms)
            {
                float safeDistance = Vector3.Distance(GameManager.Instance.playerSpawnpoints[spawnpointCount].position, player.position);
                if (spawnpointSafeDistance[spawnpointCount] > safeDistance || spawnpointSafeDistance[spawnpointCount] == 0)
                    spawnpointSafeDistance[spawnpointCount] = safeDistance;
            }
        }
        var mostSafeSpawnpoints = spawnpointSafeDistance.Where(kvp => kvp.Value >= GameManager.Instance.safeZoneSize).ToList();
        if (mostSafeSpawnpoints.Count > 0)
        {
            int randomPoint = Random.Range(0, mostSafeSpawnpoints.Count);
            transform.SetPositionAndRotation(GameManager.Instance.playerSpawnpoints[mostSafeSpawnpoints[randomPoint].Key].position, GameManager.Instance.playerSpawnpoints[mostSafeSpawnpoints[randomPoint].Key].rotation);
        }
        else
        {
            int mostSafeSpawnpoint = spawnpointSafeDistance.OrderBy(kvp => kvp.Value).Last().Key;
            transform.SetPositionAndRotation(GameManager.Instance.playerSpawnpoints[mostSafeSpawnpoint].position, GameManager.Instance.playerSpawnpoints[mostSafeSpawnpoint].rotation);
        }
        StartCoroutine(ToggleCharacterDelay());
    }

    private IEnumerator ToggleCharacterDelay()
    {
        yield return new WaitForSeconds(1f);
        RPC_ToggleCharacterInput(true);
        RPC_ToggleCharacterCollider(true);
        RPC_ToggleCharacterVisual(true);
    }
    #endregion

    #region Toggles
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ToggleCharacterInput(bool state)
    {
        characterInputHandler.enabled = state;
    }
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ToggleCharacterCollider(bool state)
    {
        for (int i = 0; i < playerColliders.Length; i++)
        {
            playerColliders[i].enabled = state;
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ToggleCharacterVisual(bool state)
    {
        visual.gameObject.SetActive(state);
    }
    #endregion
}