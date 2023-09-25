using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { private set; get; }

    [SerializeField] private GameObject playerInputPrefab;
    [SerializeField] private GameObject playerNetworkPrefab;
    [Space] public Transform[] playerSpawnpoint;

    [System.NonSerialized] public PlayerControl[] players;
    [System.NonSerialized] public int myID;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (Multiplayer.isHost)
        {
            players = new PlayerControl[Multiplayer.Host.clients.Count + 1];
            PlayerControl playerControl = SpawnPlayer(playerInputPrefab, 0, Multiplayer.Host.myNickname).GetComponent<PlayerControl>();
            playerControl.playerID = 0;
            myID = 0;
            players[0] = playerControl;
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                playerControl = SpawnPlayer(playerNetworkPrefab, player.id + 1, player.name).GetComponent<PlayerControl>();
                playerControl.playerID = player.id + 1;
                players[i+1] = playerControl;
            }
        }
        else
        {
            int playerCount = 0;
            for (int i = 0; i < Multiplayer.Client.players.Length; i++)
            {
                if (Multiplayer.Client.players[i] != null) playerCount++;
                else break;
            }
            players = new PlayerControl[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                Player player = Multiplayer.Client.players[i];
                if (player != null)
                {
                    if (player.id != Multiplayer.Client.myID)
                    {
                        PlayerControl playerControl = SpawnPlayer(playerNetworkPrefab, player.id, player.name).GetComponent<PlayerControl>();
                        playerControl.playerID = player.id;
                        players[player.id] = playerControl;
                    }
                    else
                    {
                        PlayerControl playerControl = SpawnPlayer(playerInputPrefab, player.id, player.name).GetComponent<PlayerControl>();
                        playerControl.playerID = player.id;
                        players[player.id] = playerControl;
                        myID = player.id;
                    }
                }
            }
        }
    }

    public PlayerControl GetPlayerControl(GameObject playerObject)
    {
        for(int i = 0; i< players.Length;i++)
        {
            if(playerObject.GetInstanceID().Equals(players[i].gameObject.GetInstanceID()))
            {
                return players[i];
            }
        }
        return null;
    }
    public GameObject SpawnPlayer(GameObject playerTypePrefab, int posIndex, string nickname)
    {
        GameObject playerSpawned = Instantiate(playerTypePrefab, playerSpawnpoint[posIndex].position, playerTypePrefab.transform.rotation);
        playerSpawned.GetComponentInChildren<TextMeshPro>().text = nickname;
        return playerSpawned;
    }
}
