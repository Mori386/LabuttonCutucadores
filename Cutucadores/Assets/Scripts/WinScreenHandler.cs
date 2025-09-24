using System.Collections;
using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.SceneManagement;
using static CharacterData;

public class WinScreenHandler : NetworkBehaviour
{
    public static WinScreenHandler Instance;

    [Header("Thumbs")]
    [SerializeField] private GameObject MineradorThumb;
    [SerializeField] private GameObject EscavadorThumb;
    [SerializeField] private GameObject PaiEFilhaThumb;
    [SerializeField] private GameObject VovoThumb;

    [Header("UI")]
    [SerializeField] private GameObject winScreenParent;
    [SerializeField] private GameObject CanvasWin;
    [SerializeField] private CanvasGroup fadeInEffect;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private CanvasGroup winnerTextCanvasGroup;
    [SerializeField] private AudioSource music;
    [SerializeField] private GameObject hostButtonsGO;
    [SerializeField] private GameObject waitingForHostText;

    private int mineradorWin, escavadoraWin, paiEFilhaWin, vovoWin; // 1 = win / 0 = nao jogou / -1 = perdeu
    private bool shouldGoToCharacterSelection = false;
    public bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        winScreenParent.SetActive(false);
    }

    #region RPCs Winner/Loser
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_DefineWinner(Character character)
    {
        switch (character)
        {
            case Character.Minerador: mineradorWin = 1; break;
            case Character.Escavador: escavadoraWin = 1; break;
            case Character.PaiEFilha: paiEFilhaWin = 1; break;
            case Character.Vovo: vovoWin = 1; break;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_DefineLoser(Character character)
    {
        switch (character)
        {
            case Character.Minerador: mineradorWin = -1; break;
            case Character.Escavador: escavadoraWin = -1; break;
            case Character.PaiEFilha: paiEFilhaWin = -1; break;
            case Character.Vovo: vovoWin = -1; break;
        }
    }
    #endregion

    #region Mostrar vencedor
    private void MostrarThumb(int status, GameObject thumb)
    {
        if (status == 1) 
        {
            thumb.SetActive(true);
            StartThumbDance(thumb);
        }
        else 
        {
            thumb.SetActive(false);
            StopThumbDance(thumb);
        }
    }

    public void MostrarSomenteVencedor()
    {
        MostrarThumb(mineradorWin, MineradorThumb);
        MostrarThumb(escavadoraWin, EscavadorThumb);
        MostrarThumb(paiEFilhaWin, PaiEFilhaThumb);
        MostrarThumb(vovoWin, VovoThumb);
    }
    #endregion

    #region Thumb Animation
    private void StartThumbDance(GameObject thumb)
    {
        StopThumbDance(thumb);
        StartCoroutine(ThumbDance(thumb.transform));
    }

    private void StopThumbDance(GameObject thumb)
    {
        thumb.transform.localScale = Vector3.one;
        thumb.transform.rotation = Quaternion.identity; 
        StopAllCoroutines();
    }

    private IEnumerator ThumbDance(Transform t)
    {
        Vector3 baseScale = Vector3.one;
        float bounceSpeed = 4f;
        float bounceAmount = 0.15f;

        float rotSpeed = 3f;
        float rotAmount = 15f;

        while (true)
        {
            
            float scale = 1 + Mathf.Sin(Time.time * bounceSpeed) * bounceAmount;
            float rot = Mathf.Sin(Time.time * rotSpeed) * rotAmount;

            t.localScale = baseScale * scale;
            t.rotation = Quaternion.Euler(0, 0, rot);

            yield return null;
        }
    }
    #endregion

    #region WinScreen Flow
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StartWinScreen(string textToApper)
    {
        StartCoroutine(WinScreenAnimations(textToApper));
    }

    public IEnumerator WinScreenAnimations(string textToApper)
    {
        if (gameEnded) yield break;
        gameEnded = true;

        float timer = 0f;
        float duration = 1f;

        // fade
        while (timer < duration)
        {
            fadeInEffect.alpha = timer / duration;
            timer += Time.deltaTime;
            yield return null;
        }
        fadeInEffect.alpha = 1;

        winnerTextCanvasGroup.alpha = 1;
        winnerText.text = textToApper;
        music.Play();

        MostrarSomenteVencedor();

        winScreenParent.SetActive(true);
        CanvasWin.SetActive(true);

        // fade out
        timer = 0f;
        duration = 0.5f;
        while (timer < duration)
        {
            fadeInEffect.alpha = 1 - (timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeInEffect.alpha = 0;

        if (shouldGoToCharacterSelection)
            RPC_ReturnToCharacterSelection();

        if (Runner.IsServer)
            hostButtonsGO.SetActive(true);
        else
            waitingForHostText.SetActive(true);
    }
    #endregion

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ReplayMatch()
    {
        NetworkBetweenScenesManager.Instance.RPC_PostGameReset(false);
        NetworkBetweenScenesManager.Instance.LoadSceneToHost(SceneManager.GetActiveScene().buildIndex);
        winScreenParent.SetActive(false);
        CanvasWin.SetActive(false);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable, InvokeLocal = true)]
    public void RPC_ReturnToCharacterSelection()
    {
        NetworkBetweenScenesManager.Instance.RPC_PostGameReset(true);
        NetworkBetweenScenesManager.Instance.LoadSceneToHost(0);
        winScreenParent.SetActive(false);
        CanvasWin.SetActive(false);
    }

    public void ShouldGoToCharacterSelection()
    {
        shouldGoToCharacterSelection = true;
        if (hostButtonsGO.activeInHierarchy || waitingForHostText.activeInHierarchy)
            RPC_ReturnToCharacterSelection();
    }
}
