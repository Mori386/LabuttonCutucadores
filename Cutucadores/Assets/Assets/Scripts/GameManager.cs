using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]private GameObject playerInputPrefab;
    [SerializeField]private GameObject playerNetworkPrefab;
    [Space,SerializeField] private Transform[] playerSpawnpoint;
    void Start()
    {
        if (Multiplayer.isHost)
        {
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Debug.Log(Multiplayer.Host.clients.Values.ElementAt(i).name);
            }
        }
        else
        {
            for (int i = 0; i < Multiplayer.Client.players.Length; i++)
            {
                Debug.Log(Multiplayer.Client.players[i].name);
            }
        }
        //if(Multiplayer.isHost)
        //{
        //    SpawnPlayer(0);
        //    for (int i = 0; i < Multiplayer.Host.Count; i++)
        //    {
        //        SpawnPlayer(Multiplayer.clientsIndex.Values.ElementAt(i)+1);
        //    }
        //}
        //else
        //{
        //    SpawnPlayer(0);
        //    for (int i = 0; i < Multiplayer.clientOnlyPlayersNames.Count; i++)
        //    {
        //        SpawnPlayer(Multiplayer.clientOnlyPlayersNames.Keys.ElementAt(i)+1);
        //    }
        //}
    }
    //public GameObject SpawnPlayer(GameObject playerType,int posIndex)
    //{
    //    return Instantiate(playerPrefab, playerSpawnpoint[posIndex].position, playerPrefab.transform.rotation);
    //}
}
