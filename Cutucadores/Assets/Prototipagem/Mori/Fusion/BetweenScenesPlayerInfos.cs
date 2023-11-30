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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }
    [SerializeField] private CharacterData escavadorCharData, mineradorCharData, PaiEFilhaCharData, VovoCharData;

    [Networked]
    [Capacity(4)] // Sets the fixed capacity of the collection
    NetworkArray<int> playerIDArray { get; }

    [Networked, Capacity(4)] public NetworkDictionary<int, PlayerData> playerIDToPlayerData => default;

    public CharacterData GetDataFromPlayerID(int playerID)
    {
        if (playerIDToPlayerData.TryGet(playerID, out PlayerData playerData))
        {
            switch (playerData.character)
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
public struct PlayerData : INetworkStruct
{
    public PlayerRef playerRef;
    public NetworkString<_16> username;
    public Character character;
    public NetworkBool characterLocked;
}
