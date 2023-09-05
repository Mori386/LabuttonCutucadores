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
    void Start()
    {
        if (Multiplayer.isHost)
        {
            SpawnPlayer(playerInputPrefab, 0, Multiplayer.Host.myNickname);
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                SpawnPlayer(playerNetworkPrefab, player.id + 1, player.name);
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
                        SpawnPlayer(playerNetworkPrefab, player.id, player.name);
                    }
                    else
                    {
                        SpawnPlayer(playerInputPrefab, player.id, player.name);
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
