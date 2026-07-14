using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.VFX;

public class HPHandler : NetworkBehaviour
{
    //variavel em byte que ao ser mudada chama funcao
    [Networked(OnChanged = nameof(OnScoreChanged))]
    byte Kills { get; set; }

    public bool hasShield;
    //public GameObject shieldVisual;
    public ParticleSystem shieldVisualEffect;
    public ParticleSystem nudgeVisualEffect;
    [SerializeField] private GameObject shieldGO;
    private Material shieldMat;
    [SerializeField] private ParticleSystem shieldBreakParticle;

    public NetworkVisualHandler networkVisualHandler;
    public NetworkCharacterDrillController drillController;

    public bool isInvulnerable;
    [Networked] public TickTimer InvulnerabilityTimer { get; set; }
    private void Awake()
    {
        networkVisualHandler = GetComponent<NetworkVisualHandler>();
        drillController = GetComponent<NetworkCharacterDrillController>();
        shieldMat = shieldGO.GetComponent<MeshRenderer>().material;
    }
    public override void Spawned()
    {
        base.Spawned();
        HPBarHandler.Instance.LoadPlayerInfos();
        ChangeShieldState(true);
        Kills = 0;
        RPC_UpdateRankingUI();
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_OnHitTaken(HPHandler attacker, Vector3 hitPosition)
    {
        StartCoroutine(CheckForInvulnerability(attacker, hitPosition));
    }

    public IEnumerator CheckForInvulnerability(HPHandler attacker, Vector3 hitPosition)
    {
        transform.root.GetComponent<CollisionHandler>().StartFallChecker(attacker);
        attacker.transform.root.GetComponent<CollisionHandler>().StartFallChecker(transform.root.GetComponent<HPHandler>());
        if (isInvulnerable)
        {
            Debug.Log($"HPHandler - IsInvulnerable:{isInvulnerable}");
            yield break;
        }
        bool died = false;
        if (hasShield)
        {
            Debug.Log($"HPHandler - HasShield:{hasShield}, deactivating shield");
            ChangeShieldState(false, hitPosition);
        }
        else
        {
            died = true;
            Debug.Log($"HPHandler - Taking damage");
            attacker.IncreaseScore(1);
            
            drillController.Die();
        }
        isInvulnerable = true;
        Debug.Log($"HPHandler - Starting InvulnerabilityTimer");
        InvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, died ? 2.5f : .5f);
        if (died)
        {
           
            while (InvulnerabilityTimer.RemainingTime(Runner) >= .5f)
            {
                yield return null;
            }
            drillController.Respawn();
            ChangeShieldState(true);
        }
        //Cria um timer na rede para check de tempo de invulnerabilidade
        while (!InvulnerabilityTimer.Expired(Runner))
        {
            yield return null;
        }
        InvulnerabilityTimer = TickTimer.None;
        isInvulnerable = false;
    }

    public void ChangeShieldState(bool shieldState, Vector3 hitPosition = default)
    {
        if (hasShield == shieldState)
            return;
        if (shieldState)
        {
            //colocar aqui efeito de ganhar/recuperar shield
            //shieldVisual.SetActive(true);
        }
        else
        {
            //colocar aqui implementação do efeito de perder shield
            //shieldVisual.SetActive(false);
            StartCoroutine(ShieldVisual(hitPosition));
            shieldVisualEffect.Play();
        }
        hasShield = shieldState;
    }

    private IEnumerator ShieldVisual(Vector3 hitPosition)
    {
        shieldGO.SetActive(true);
        shieldMat.SetFloat("_Lifetime", 0);
        shieldGO.transform.LookAt(hitPosition);
        shieldBreakParticle.transform.LookAt(hitPosition);
        for (float elapsedTime = 0; elapsedTime <= 1; elapsedTime += Time.deltaTime)
        {
            shieldMat.SetFloat("_Lifetime", elapsedTime);
            yield return null;
        }
        shieldGO.SetActive(false);
        shieldBreakParticle.Play();
    }

    public void IncreaseScore(byte amount)
    {
        Kills += amount;
        nudgeVisualEffect.Play();
    }

    public void DecreaseScore(byte amount) 
    {
        if (Kills > 0)
            Kills -= amount;
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_UpdateRankingUI()
    {
        HPBarHandler.Instance.UpdateScore(Object.InputAuthority, Kills);
    }
    static void OnScoreChanged(Changed<HPHandler> changed)
    {
        changed.Behaviour.RPC_UpdateRankingUI();
    }
    public void OnHPLower()
    {
        //Se for ele mesmo que tomou dano da shake na camera
        if (Object.HasInputAuthority)
        {
            GameManager.Instance.ShakeCamera(GameManager.Instance.onBodyHitCameraShakeAmplitude);
        }
    }
}
