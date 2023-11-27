using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterData;

public class BetweenScenesPlayerInfos : NetworkBehaviour
{
    static public BetweenScenesPlayerInfos Instance;
    private void Awake()
    {
        playerIDToCharacter.Add(0, Character.Escavador);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }
    [SerializeField] private CharacterData escavadorCharData, mineradorCharData, PaiEFilhaCharData, VovoCharData;

    public Dictionary<int, Character> playerIDToCharacter = new Dictionary<int, Character>();

    public CharacterData GetDataFromPlayerID(int playerID)
    {
        if (playerIDToCharacter.TryGetValue(playerID, out Character character))
        {
            switch (character)
            {
                case Character.Escavador:
                default:
                    return escavadorCharData;
                case Character.Minerador:
                    return mineradorCharData;
                case Character.PaiEFilha:
                    return PaiEFilhaCharData;
                case Character.Vovo:
                    return VovoCharData;
            }
        }
        else
        {
            Debug.LogError("Error in search to find " + playerID + " inCharacterID");
            return null;
        }
    }
}
