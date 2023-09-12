using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerInputPrefab;
    [SerializeField] private GameObject playerNetworkPrefab;
    [Space, SerializeField] private Transform[] playerSpawnpoint;

    private PlayerControl[] players;
    void Start()
    {
        if (Multiplayer.isHost)
        {
            PlayerControl playerControl =  SpawnPlayer(playerInputPrefab, 0, Multiplayer.Host.myNickname).GetComponent<PlayerControl>();
            playerControl.playerID = 0;
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                playerControl = SpawnPlayer(playerNetworkPrefab, player.id + 1, player.name).GetComponent<PlayerControl>();
                playerControl.playerID = player.id + 1;
            }
        }
        else
        {
            for (int i = 0; i < Multiplayer.Client.players.Length; i++)
            {
                Player player = Multiplayer.Client.players[i];
                if (player != null)
                {
                    if(player.id != Multiplayer.Client.myID)
                    {
                        SpawnPlayer(playerNetworkPrefab, player.id, player.name).GetComponent<PlayerControl>().playerID = player.id;
                    }
                    else
                    {
                        SpawnPlayer(playerInputPrefab, player.id, player.name).GetComponent<PlayerControl>().playerID = player.id;
                    }
                }
            }
        }
    }
    public GameObject SpawnPlayer(GameObject playerTypePrefab, int posIndex, string nickname)
    {
        GameObject playerSpawned = Instantiate(playerTypePrefab, playerSpawnpoint[posIndex].position, playerTypePrefab.transform.rotation);
        playerSpawned.GetComponentInChildren<TextMeshPro>().text = nickname;
        return playerSpawned;
    }
}
