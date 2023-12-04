using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CharacterData;

public class WinScreenHandler : NetworkBehaviour
{
    Camera mainCamera;
    public static WinScreenHandler Instance;
    [SerializeField] private CanvasGroup fadeInEffect;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private CanvasGroup winnerTextCanvasGroup;
    [SerializeField] private AudioSource music;
    [SerializeField] private GameObject winScreenParent;
    [SerializeField] private Light mineradorLight, escavadoraLight, paiEFilhaLight, vovoLight;
    [SerializeField] private Camera winScreenCamera;
    [SerializeField] private Animator mineradorAnim, escavadoraAnim, paiAnim, filhaAnim, vovoAnim;
    private int mineradorWin, escavadoraWin, paiEFilhaWin, vovoWin; // 1 = win/ 0 = not present/ -1 = lose
    private readonly float winnerLightTemperature = 4000;
    private readonly float loserLightTemperature = 20000;
    private void Awake()
    {
        Instance = this;
        winScreenParent.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
    }
    public override void Spawned()
    {
        base.Spawned();
        if (mainCamera == null) mainCamera = Camera.main;
    }
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
    public Animator[] GetCharacterAnim(Character character)
    {
        Animator[] returnAnimator;
        switch (character)
        {
            default:
            case Character.Escavador:
                returnAnimator = new Animator[1];
                returnAnimator[0] = escavadoraAnim;
                return returnAnimator;

            case Character.Minerador:
                returnAnimator = new Animator[1];
                returnAnimator[0] = mineradorAnim;
                return returnAnimator;

            case Character.PaiEFilha:
                returnAnimator = new Animator[2];
                returnAnimator[0] = paiAnim;
                returnAnimator[1] = filhaAnim;
                return returnAnimator;

            case Character.Vovo:
                returnAnimator = new Animator[1];
                returnAnimator[0] = vovoAnim;
                return returnAnimator;
        }
    }

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
        GetCharacterLight(character).colorTemperature = loserLightTemperature;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_StartWinScreen(string textToApper)
    {
        StartCoroutine(WinScreenAnimations(textToApper));
    }
    public IEnumerator WinScreenAnimations(string textToApper)
    {
        float timer = 0f;
        float duration = 1;
        float gameplayVolumeStartValue = GameManager.Instance.gameplayMusic.volume;
        while (timer<duration)
        {
            GameManager.Instance.gameplayMusic.volume = Mathf.Lerp(gameplayVolumeStartValue,0,timer/duration);
            fadeInEffect.alpha = timer/duration;
            timer += Time.deltaTime;
            yield return null;
        }
        GameManager.Instance.gameplayMusic.volume = 0;
        fadeInEffect.alpha = 1;
        winnerTextCanvasGroup.alpha = 1;
        winnerText.text = textToApper;
        music.Play();
        EnableCharacter();
        winScreenParent.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        timer = 0f;
        duration = 0.5f;
        while (timer < duration)
        {
            fadeInEffect.alpha = 1-(timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        PlayCharacterAnimations();
        yield return new WaitForSeconds(4f);
        timer = 0f;
        duration = 1f;
        while (timer < duration)
        {
            fadeInEffect.alpha = timer / duration;
            timer += Time.deltaTime;
            yield return null;
        }
        fadeInEffect.alpha = 1f;
        var sacrificialGo = new GameObject("Sacrificial Lamb");
        Runner.Shutdown();


        DontDestroyOnLoad(sacrificialGo);

        foreach (var root in sacrificialGo.scene.GetRootGameObjects())
            Destroy(root);
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
    }
    public void EnableCharacter()
    {
        if (mineradorWin == 0)
        {
            mineradorAnim.gameObject.SetActive(false);
            mineradorLight.gameObject.SetActive(false);
        }
        if (escavadoraWin == 0)
        {
            escavadoraAnim.gameObject.SetActive(false);
            escavadoraLight.gameObject.SetActive(false);
        }
        if(paiEFilhaWin == 0)
        {
            paiAnim.gameObject.SetActive(false);
            filhaAnim.gameObject.SetActive(false);

            paiEFilhaLight.gameObject.SetActive(false);
        }
        if (vovoWin == 0)
        {
            vovoAnim.gameObject.SetActive(false);
            vovoLight.gameObject.SetActive(false);
        }
    }
    public void PlayCharacterAnimations()
    {
        //Minerador
        if (mineradorWin >= 1)
        {
            mineradorAnim.SetTrigger("isWin");
        }
        else
        {
            mineradorAnim.SetTrigger("isLose");
        }

        //Escavadora
        if (escavadoraWin >= 1)
        {
            escavadoraAnim.SetTrigger("isWin");
        }
        else
        {
            escavadoraAnim.SetTrigger("isLose");
        }

        //Pai e filha
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
        
        
        //Vovo
        if (vovoWin >= 1)
        {
            vovoAnim.SetTrigger("isWin");
        }
        else
        {
            vovoAnim.SetTrigger("isLose");
        }
    }
}
