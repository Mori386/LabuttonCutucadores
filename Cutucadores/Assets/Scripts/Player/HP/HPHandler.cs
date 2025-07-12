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

    //variavel em bool que ao ser mudada chama funcao
    //[Networked(OnChanged = nameof(OnStateChanged))]
    //public bool isDead { get; set; }

    //bool isInitialized = false;

    public TextMeshPro killsText;

    public NetworkVisualHandler networkVisualHandler;
    public NetworkCharacterDrillController drillController;

    [Networked] public bool isInvulnerable { get; set; }
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
        Kills = 0;
        UpdateRankingUI();
    }
    public void OnHitTaken()
    {
        if (!isInvulnerable)
        {
            drillController.Die();
            isInvulnerable = true;
            StartCoroutine(CheckForInvulnerability());
        }
    }

    public IEnumerator CheckForInvulnerability()
    {
        InvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
        while (InvulnerabilityTimer.RemainingTime(Runner) >= 0.2f)
        {
            yield return null;
        }
        drillController.Respawn();
        //Cria um timer na rede para check de tempo de invulnerabilidade
        while (!InvulnerabilityTimer.Expired(Runner))
        {
            yield return null;
        }
        InvulnerabilityTimer = TickTimer.None;
        isInvulnerable = false;
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
