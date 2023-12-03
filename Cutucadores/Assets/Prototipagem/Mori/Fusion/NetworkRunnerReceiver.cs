using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRunnerReceiver : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject stampPlayerPrefab;
    public GameObject networkBetweenScenesManager;

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void Rpc_DefineCarimbo([RpcTarget] PlayerRef player, GameObject carimbo)
    {
        CursorController.Instance.carimbo = carimbo;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (runner.SessionInfo.PlayerCount >= 5) runner.Disconnect(player);
            if (runner.LocalPlayer == player)
            {
                runner.Spawn(networkBetweenScenesManager, networkBetweenScenesManager.transform.position, networkBetweenScenesManager.transform.rotation);
            }
            NetworkObject NObject = runner.Spawn(stampPlayerPrefab, stampPlayerPrefab.transform.position, stampPlayerPrefab.transform.rotation, player);
            if (runner.LocalPlayer == player) CursorController.Instance.carimbo = NObject.gameObject;
            else Rpc_DefineCarimbo(player, NObject.gameObject);
            // (runner.LocalPlayer == player) BetweenScenesPlayerInfos.Instance.idSelf = player.PlayerId;
        }
        else Debug.Log("OnPlayerJoined");
        if (runner.LocalPlayer == player)
        {
            NetworkBetweenScenesManager.Instance.userIDToPlayerData.Add(runner.UserId, new PlayerData());
            NetworkBetweenScenesManager.Instance.selfUserID = runner.UserId;
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

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
