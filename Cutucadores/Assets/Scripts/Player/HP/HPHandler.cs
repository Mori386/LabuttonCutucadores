using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class HPHandler : NetworkBehaviour
{
    //variavel em byte que ao ser mudada chama funcao
    [Networked(OnChanged = nameof(OnScoreChanged))]
    byte Kills { get; set; }

    public bool hasShield;

    public TextMeshPro killsText;

    public NetworkVisualHandler networkVisualHandler;
    public NetworkCharacterDrillController drillController;

    [SerializeField] private GameObject[] colliders;

    public bool isInvulnerable;
    [Networked] public TickTimer InvulnerabilityTimer { get; set; }
    private void Awake()
    {
        networkVisualHandler = GetComponent<NetworkVisualHandler>();
        drillController = GetComponent<NetworkCharacterDrillController>();
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
    public void RPC_OnHitTaken()
    {
        StartCoroutine(CheckForInvulnerability());
    }

    public IEnumerator CheckForInvulnerability()
    {
        if (isInvulnerable)
        {
            Debug.Log($"HPHandler - IsInvulnerable:{isInvulnerable}");
            yield break;
        }
        bool died = false;
        if (hasShield)
        {
            Debug.Log($"HPHandler - HasShield:{hasShield}, deactivating shield");
            ChangeShieldState(false);
        }
        else
        {
            died = true;
            Debug.Log($"HPHandler - Taking damage");
            drillController.Die();
        }
        isInvulnerable = true;
        Debug.Log($"HPHandler - Starting InvulnerabilityTimer");
        InvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, died ? 2.5f : .5f);
        if (died)
        {
            killsText.gameObject.SetActive(false);
            while (InvulnerabilityTimer.RemainingTime(Runner) >= .5f)
            {
                yield return null;
            }
            drillController.Respawn();
            killsText.gameObject.SetActive(true);
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

    public void ChangeShieldState(bool shieldState)
    {
        if (hasShield == shieldState)
            return;
        if (shieldState)
        {
            //colocar aqui efeito de ganhar/recuperar shield
        }
        else
        {
            //colocar aqui implementação do efeito de perder shield
        }
        hasShield = shieldState;
    }

    public void IncreaseScore(byte amount)
    {
        Kills += amount;
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
        killsText.text = Kills.ToString();
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
