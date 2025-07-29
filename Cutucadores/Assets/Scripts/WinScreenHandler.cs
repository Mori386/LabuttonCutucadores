using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CharacterData;

public class WinScreenHandler : NetworkBehaviour
{
    //Singleton
    public static WinScreenHandler Instance;

    //Components
    Camera mainCamera;
    [SerializeField] private AudioSource music;
    [SerializeField] private Light mineradorLight, escavadoraLight, paiEFilhaLight, vovoLight;
    [SerializeField] private Camera winScreenCamera;
    [SerializeField] private Animator mineradorAnim, escavadoraAnim, paiAnim, filhaAnim, vovoAnim;
    //Ui Components
    [SerializeField] private GameObject winScreenParent;
    [SerializeField] private GameObject CanvasWin;
    [SerializeField] private CanvasGroup fadeInEffect;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private CanvasGroup winnerTextCanvasGroup;
    [SerializeField] private GameObject hostButtonsGO;
    [SerializeField] private GameObject waitingForHostText;

    private int mineradorWin, escavadoraWin, paiEFilhaWin, vovoWin; // 1 = win/ 0 = not present/ -1 = lose

    private readonly float winnerLightTemperature = 4000;
    private readonly float loserLightTemperature = 20000;
    private bool shouldGoToCharacterSelection = false;
    public bool gameEnded = false;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(this);
        winScreenParent.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
    }
    public override void Spawned()
    {
        base.Spawned();
        if (mainCamera == null) mainCamera = Camera.main;
    }
    #region Define Winner and Loser

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_DefineWinner(Character character)
    {
        switch (character)
        {
            default:
            case Character.Escavador:
                escavadoraWin = 1;
                break;
            case Character.Minerador:
                mineradorWin = 1;
                break;
            case Character.PaiEFilha:
                paiEFilhaWin = 1;
                break;
            case Character.Vovo:
                vovoWin = 1;
                break;
        }
        GetCharacterLight(character).colorTemperature = winnerLightTemperature;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_DefineLoser(Character character)
    {
        switch (character)
        {
            default:
            case Character.Escavador:
                escavadoraWin = -1;
                break;
            case Character.Minerador:
                mineradorWin = -1;
                break;
            case Character.PaiEFilha:
                paiEFilhaWin = -1;
                break;
            case Character.Vovo:
                vovoWin = -1;
                break;
        }
        //GetCharacterLight(character).colorTemperature = loserLightTemperature;
    }
    #endregion

    #region Set Characters and Lights
    public Light GetCharacterLight(Character character)
    {
        switch (character)
        {
            default:
            case Character.Escavador:
                return escavadoraLight;
            case Character.Minerador:
                return mineradorLight;
            case Character.PaiEFilha:
                return paiEFilhaLight;
            case Character.Vovo:
                return vovoLight;
        }
    }
    public void EnableCharacter()
    {
        //All characters start enabled and whover isnt in the match are set active false
        if (mineradorWin == 0)
        {
            mineradorAnim.gameObject.SetActive(false);
            //mineradorLight.gameObject.SetActive(false);
        }
        if (escavadoraWin == 0)
        {
            escavadoraAnim.gameObject.SetActive(false);
            //escavadoraLight.gameObject.SetActive(false);
        }
        if (paiEFilhaWin == 0)
        {
            paiAnim.gameObject.SetActive(false);
            filhaAnim.gameObject.SetActive(false);

            //paiEFilhaLight.gameObject.SetActive(false);
        }
        if (vovoWin == 0)
        {
            vovoAnim.gameObject.SetActive(false);
            //vovoLight.gameObject.SetActive(false);
        }

    }

    public void MostrarSomenteVencedor()
    {
        // Minerador
        mineradorAnim.gameObject.SetActive(mineradorWin == 1);

        // Escavadora
        escavadoraAnim.gameObject.SetActive(escavadoraWin == 1);

        // Pai e Filha
        paiAnim.gameObject.SetActive(paiEFilhaWin == 1);
        filhaAnim.gameObject.SetActive(paiEFilhaWin == 1);

        // Vovô
        vovoAnim.gameObject.SetActive(vovoWin == 1);
    }
    #endregion

    #region Set Animations
    public void PlayCharacterAnimations()
    {
        #region Minerador
        if (mineradorWin >= 1)
        {
            mineradorAnim.SetTrigger("isWin");
        }
        else
        {
            mineradorAnim.SetTrigger("isLose");
        }
        #endregion

        #region Escavadora
        if (escavadoraWin >= 1)
        {
            escavadoraAnim.SetTrigger("isWin");
        }
        else
        {
            escavadoraAnim.SetTrigger("isLose");
        }
        #endregion

        #region Pai e filha
        if (paiEFilhaWin >= 1)
        {
            paiAnim.SetTrigger("isWin");
            filhaAnim.SetTrigger("isWin");
        }
        else
        {
            paiAnim.SetTrigger("isLose");
            filhaAnim.SetTrigger("isLose");
        }
        #endregion

        #region Vovo
        if (vovoWin >= 1)
        {
            vovoAnim.SetTrigger("isWin");
        }
        else
        {
            vovoAnim.SetTrigger("isLose");
        }
        #endregion
    }
    #endregion

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StartWinScreen(string textToApper)
    {
        StartCoroutine(WinScreenAnimations(textToApper));
    }
    public IEnumerator WinScreenAnimations(string textToApper)
    {
        if (gameEnded)
            yield break;
        gameEnded = true;
        HPBarHandler.Instance.ManageUIToEndGame();
        PauseUI.Instance.OpenOrCloseUI(false);
        float timer = 0f;
        float duration = 1;
        float gameplayVolumeStartValue = GameManager.Instance.gameplayMusic.volume;
        //Fade out music and fade in winscreen
        while (timer<duration)
        {
            GameManager.Instance.gameplayMusic.volume = Mathf.Lerp(gameplayVolumeStartValue,0,timer/duration);
            fadeInEffect.alpha = timer/duration;
            timer += Time.deltaTime;
            yield return null;
        }
        GameManager.Instance.gameplayMusic.volume = 0;
        fadeInEffect.alpha = 1;

        //Setup components ready for fade out
        winnerTextCanvasGroup.alpha = 1;
        winnerText.text = textToApper;
        music.Play();
        MostrarSomenteVencedor();
        winScreenParent.SetActive(true);
        CanvasWin.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        //Fade out timer
        timer = 0f;
        duration = 0.5f;
        while (timer < duration)
        {
            fadeInEffect.alpha = 1-(timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        //Play animations wait and fade out
        PlayCharacterAnimations();
       /* yield return new WaitForSeconds(4f);
        timer = 0f;
        duration = 1f;
        while (timer < duration)
        {
            fadeInEffect.alpha = timer / duration;
            timer += Time.deltaTime;
            yield return null;
        }
        fadeInEffect.alpha = 1f;*/

        if (shouldGoToCharacterSelection)
            RPC_ReturnToCharacterSelection();

        if (Runner.IsServer)
            hostButtonsGO.SetActive(true);
        else
            waitingForHostText.SetActive(true);
    }

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
