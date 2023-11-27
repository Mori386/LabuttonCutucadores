using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkVisualHandler : NetworkBehaviour
{
    NetworkMecanimAnimator mecanimAnimator;
    Animator animator;
    NetworkCharacterDrillController characterDrillController;
    ExplosionHandler explosionHandler;

    public Transform drillVisual;

    [Header("-----Power Up-----"), Space]
    [SerializeField] private ParticleSystem[] powerUpSpeedParticleSystem;
    [SerializeField] private LineRenderer powerUpSpeedLineRenderer;
    [SerializeField] private float maxRadius = 5f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private int pointsCount = 10;
    [SerializeField] private float startWidth = 2f;

    private void Awake()
    {
        mecanimAnimator = GetComponent<NetworkMecanimAnimator>();
        characterDrillController = GetComponent<NetworkCharacterDrillController>();
        explosionHandler = GetComponentInChildren<ExplosionHandler>();
        animator = mecanimAnimator.Animator;
        powerUpSpeedLineRenderer.positionCount = pointsCount + 1;
        powerUpSpeedLineRenderer.widthMultiplier = 0;

    }
    public void OndDeath()
    {
        explosionHandler.Explode();
    }
    public override void Spawned()
    {
        base.Spawned();
        LoadCharacterVisual();
        StartCoroutine(RotateDrillCoroutine());
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestLoadVisual(int playerID,RpcInfo info = default)
    {
        LoadCharacterVisual();
    }
    public void LoadCharacterVisual()
    {
        if(Object.HasInputAuthority)Instantiate(BetweenScenesPlayerInfos.Instance.GetDataFromPlayerID(0).visualPrefab, characterDrillController.visual);
    }
    [Networked]
    [HideInInspector] public float rotationDirection { get; set; }
    public void RotateDrill()
    {
        //drillVisual.Rotate((2.5f + rotationDirection * characterDrillController.Velocity.magnitude / 40f), 0, 0, Space.Self);
    }
    public void PlayPowerUpVfx()
    {
        for (int i = 0; i < powerUpSpeedParticleSystem.Length; i++)
        {
            powerUpSpeedParticleSystem[i].Play();
        }
        PlayBlastWave();
    }
    private void PlayBlastWave()
    {
        StartCoroutine(BlastWave());
    }
    private IEnumerator BlastWave()
    {
        float currentRadius = 0;
        while (currentRadius < maxRadius)
        {
            currentRadius += Time.deltaTime * speed;
            Draw(currentRadius);
            yield return null;
        }
    }
    private void Draw(float radius)
    {
        float angleBetweenPoints = 360f / pointsCount;
        for (int i = 0; i <= pointsCount; i++)
        {
            float angle = i * angleBetweenPoints * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
            Vector3 position = direction * radius;
            powerUpSpeedLineRenderer.SetPosition(i, position);
        }
        powerUpSpeedLineRenderer.widthMultiplier = Mathf.Lerp(0, startWidth, 1 - radius / maxRadius);
    }
    public IEnumerator RotateDrillCoroutine()
    {
        while (true)
        {
            RotateDrill();
            yield return new WaitForFixedUpdate();
        }
    }
}
