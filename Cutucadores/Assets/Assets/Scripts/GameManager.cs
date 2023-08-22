using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject playerPrefab;
    [Space,SerializeField] private Transform[] playerSpawnpoint;
    void Start()
    {
        if(Multiplayer.isHost)
        {
            SpawnPlayer(0);
            for (int i = 0; i < Multiplayer.clientsIndex.Count; i++)
            {
                SpawnPlayer(Multiplayer.clientsIndex.Values.ElementAt(i+1));
            }
        }
        else
        {
            SpawnPlayer(0);
            for (int i = 0; i < Multiplayer.clientOnlyPlayersNames.Count; i++)
            {
                SpawnPlayer(Multiplayer.clientOnlyPlayersNames.Keys.ElementAt(i + 1));
            }
        }
    }
    public GameObject SpawnPlayer(int posIndex)
    {
        return Instantiate(playerPrefab, playerSpawnpoint[posIndex].position, playerPrefab.transform.rotation);
    }
}
