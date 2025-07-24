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
        UpdateRankingUI();
    }

    [Rpc(RpcSources.All, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_OnHitTaken()
    {
        if (isInvulnerable)
        {
            Debug.LogError($"HPHandler - IsInvulnerable:{isInvulnerable}");
            return;
        }
        if (hasShield)
        {
            Debug.LogError($"HPHandler - HasShield:{hasShield}, deactivating shield");
            ChangeShieldState(false);
            StartCoroutine(CheckForInvulnerability(false));
            return;
        }
        Debug.LogError($"HPHandler - Taking damage");
        drillController.Die();
        StartCoroutine(CheckForInvulnerability(true));
    }

    public bool ChangeShieldState(bool shieldState)
    {
        if (hasShield == shieldState)
            return false;
        if (shieldState)
        {
            //colocar aqui efeito de ganhar/recuperar shield
        }
        else
        {
            //colocar aqui implementação do efeito de perder shield
        }
        hasShield = shieldState;
        return true;
    }

    public IEnumerator CheckForInvulnerability(bool died)
    {
        isInvulnerable = true;
        Debug.LogError($"HPHandler - Starting InvulnerabilityTimer");
        InvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, died ? 2.5f : .5f);
        if (died)
        {
            ManageColliders(false);
            killsText.gameObject.SetActive(false);
            while (InvulnerabilityTimer.RemainingTime(Runner) >= .5f)
            {
                yield return null;
            }
            drillController.Respawn();
            ManageColliders(true);
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

    public void ManageColliders(bool active)
    {
        foreach (var collider in colliders) 
            collider.SetActive(active);
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

    public void UpdateRankingUI()
    {
        HPBarHandler.Instance.UpdateScore(Object.InputAuthority, Kills);
        killsText.text = Kills.ToString();
    }
    static void OnScoreChanged(Changed<HPHandler> changed)
    {
        changed.Behaviour.UpdateRankingUI();
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
