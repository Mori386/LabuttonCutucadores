using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer playerPrefab;

    CharacterInputHandler characterInputHandler;
    void Start()
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log("OnPlayerJoined we are server, spawning player");
            runner.Spawn(playerPrefab, GameManager.Instance.playerSpawnpoints[runner.SessionInfo.PlayerCount-1].position, Quaternion.identity, player);
        }
        else Debug.Log("OnPlayerJoined");
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if(characterInputHandler == null)
        {
            if(NetworkPlayer.Local != null) characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
        }
        else
        {
            input.Set(characterInputHandler.GetNetworkInput());
        }
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}
