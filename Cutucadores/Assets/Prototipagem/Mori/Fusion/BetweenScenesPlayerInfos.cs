using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterData;

public class BetweenScenesPlayerInfos : MonoBehaviour
{
    [HideInInspector] public int mapSelected;
    static public BetweenScenesPlayerInfos Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }
    [SerializeField] public CharacterData escavadorCharData, mineradorCharData, PaiEFilhaCharData, VovoCharData;

}
