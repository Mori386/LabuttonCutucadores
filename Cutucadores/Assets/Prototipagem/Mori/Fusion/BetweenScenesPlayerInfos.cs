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
        playerIDToPlayerData.Add(0, new PlayerData
        {
            character = Character.Escavador,
            username = "Player0"
        });
        playerIDToPlayerData.Add(1, new PlayerData
        {
            character = Character.Minerador,
            username = "Player1"
        });
        playerIDToPlayerData.Add(2, new PlayerData
        {
            character = Character.PaiEFilha,
            username = "Player2"
        });
        playerIDToPlayerData.Add(3, new PlayerData
        {
            character = Character.Vovo,
            username = "Player3"
        });
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }
    public override void Spawned()
    {
        base.Spawned();
    }
    [SerializeField] private CharacterData escavadorCharData, mineradorCharData, PaiEFilhaCharData, VovoCharData;

    [Networked]
    [Capacity(4)] // Sets the fixed capacity of the collection
    public NetworkLinkedList<int> playerIDLinkedList { get; }

    public Dictionary<int, PlayerData> playerIDToPlayerData = new Dictionary<int, PlayerData>();

    [Networked]
    [Capacity(4)]
    public NetworkDictionary<string, int> userIDToPlayerOrder { get; }

    public CharacterData GetDataFromPlayerID(int playerID)
    {
        if (playerIDToPlayerData.TryGetValue(playerID, out PlayerData playerData))
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
}
