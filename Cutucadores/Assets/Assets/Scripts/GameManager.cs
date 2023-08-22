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
            for(int i = 0; i < Multiplayer.clientsIndex.Count; i++)
            {
                SpawnPlayer(Multiplayer.clientsIndex.Values.ElementAt(i));
            }
        }
        else
        {

        }
    }
    public GameObject SpawnPlayer(int posIndex)
    {
        return Instantiate(playerPrefab, playerSpawnpoint[posIndex].position, playerPrefab.transform.rotation);
    }
}
