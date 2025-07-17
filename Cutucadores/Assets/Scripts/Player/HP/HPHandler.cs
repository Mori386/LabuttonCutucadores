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

    [Networked] public bool HasShield { get; set; }

    public TextMeshPro killsText;

    public NetworkVisualHandler networkVisualHandler;
    public NetworkCharacterDrillController drillController;

    [Networked] public bool IsInvulnerable { get; set; }
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
    public void OnHitTaken()
    {
        if (IsInvulnerable)
            return;
        if (HasShield)
        {
            ChangeShieldState(false);
            StartCoroutine(CheckForInvulnerability(false));
            return;
        }
        drillController.Die();
        StartCoroutine(CheckForInvulnerability(true));
    }

    public bool ChangeShieldState(bool shieldState)
    {
        if (HasShield == shieldState)
            return false;
        if (shieldState)
        {
            Debug.LogWarning($"Ganhou escudo");
            //colocar aqui efeito de ganhar/recuperar shield
        }
        else
        {
            Debug.LogWarning($"Perdeu escudo");
            //colocar aqui implementação do efeito de perder shield
        }
        HasShield = shieldState;
        return true;
    }

    public IEnumerator CheckForInvulnerability(bool died)
    {
        IsInvulnerable = true;
        InvulnerabilityTimer = TickTimer.CreateFromSeconds(Runner, died ? 1.5f : .5f);
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
        IsInvulnerable = false;
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
