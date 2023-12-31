using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using System.Net;
using System.Text;
using System.Threading;
using System;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public AnimatorController orangeAnim, greenAnim;
    public static GameManager Instance { private set; get; }

    public CinemachineVirtualCamera VirtualCamera;

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
            VirtualCamera.LookAt = playerControl.transform;
            VirtualCamera.Follow = playerControl.transform;
            playerControl.playerType = PlayerControl.PlayerTypes.Input;
            playerControl.animator.runtimeAnimatorController = GetAnimatorBasedOnID(playerControl.playerID);
            for (int i = 0; i < Multiplayer.Host.clients.Count; i++)
            {
                Player player = Multiplayer.Host.clients.Values.ElementAt(i);
                playerControl = SpawnPlayer(playerNetworkPrefab, player.id + 1, player.name).GetComponent<PlayerControl>();
                playerControl.playerID = player.id + 1;
                players[i + 1] = playerControl;
                playerControl.playerType = PlayerControl.PlayerTypes.Network;
                playerControl.animator.runtimeAnimatorController = GetAnimatorBasedOnID(playerControl.playerID);
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
                        playerControl.animator.runtimeAnimatorController = GetAnimatorBasedOnID(playerControl.playerID);
                        playerControl.playerType = PlayerControl.PlayerTypes.Network;
                    }
                    else
                    {
                        PlayerControl playerControl = SpawnPlayer(playerInputPrefab, player.id, player.name).GetComponent<PlayerControl>();
                        playerControl.playerID = player.id;
                        players[player.id] = playerControl;
                        myID = player.id;
                        playerControl.animator.runtimeAnimatorController = GetAnimatorBasedOnID(playerControl.playerID);
                        VirtualCamera.LookAt = playerControl.transform;
                        VirtualCamera.Follow = playerControl.transform;
                        playerControl.playerType = PlayerControl.PlayerTypes.Input;
                    }
                }
            }
        }
    }
    public AnimatorController GetAnimatorBasedOnID(int id)
    {
        switch (id)
        {
            default:
            case 0:
            case 2:
            case 3:
                return orangeAnim;
            case 1:
                return greenAnim;
        }
    }
    public PlayerControl GetPlayerControl(GameObject playerObject)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (playerObject.GetInstanceID().Equals(players[i].gameObject.GetInstanceID()))
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

    public  void ReturnToMenu(float delay)
    {
        StartCoroutine(WaitDelayForReturnToMenu(delay));
    }
    public  IEnumerator WaitDelayForReturnToMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Menu");
    }
}
