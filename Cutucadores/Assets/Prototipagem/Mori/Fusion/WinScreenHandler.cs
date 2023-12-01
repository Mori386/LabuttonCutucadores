using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenHandler : NetworkBehaviour
{
    public static WinScreenHandler Instance;
    [SerializeField] private GameObject winScreenParent;
    [SerializeField]private Light mineradorLight, escavadoraLight, paiEFilhaLight, vovoLight;
    [SerializeField] private Camera winScreenCamera;
    [SerializeField] private Animator mineradorAnim, escavadoraAnim, paiAnim, filhaAnim, vovoAnim;
    private readonly float winnerLightTemperature = 4000;
    private readonly float loserLightTemperature = 20000;
    private void Awake()
    {
        Instance = this;
        winScreenParent.SetActive(false);
    }
    public void DefineWinner()
    {

    }
    public void DefineLoser()
    {

    }
    public void StartWinScreen()
    {

    }
}
