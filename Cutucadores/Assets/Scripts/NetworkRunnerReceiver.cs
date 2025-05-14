using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerReceiver : MonoBehaviour, INetworkRunnerCallbacks
{
    //Sigleston
    public static NetworkRunnerReceiver Instance;

    //Stamp is deprecated, stamp estava bloqueando a visao do jogador, entao foi retirado, usado agora so para rastrear mouse do player 
    public GameObject stampPlayerPrefab;
    public GameObject networkBetweenScenesManager;

    CharacterInputHandler characterInputHandler;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
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
            NetworkBetweenScenesManager.Instance.RPC_CheckForPlayerReady();
        }
        else Debug.Log("OnPlayerJoined");
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (NetworkBetweenScenesManager.Instance != null && NetworkBetweenScenesManager.Instance.isInGameplay)
        {
            //Debug.Log($"OnInput {NetworkBetweenScenesManager.Instance.userIDToPlayerData[runner.UserId].username}");
            if (characterInputHandler == null)
            {
                if (NetworkPlayer.Local != null) characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
            }
            else
            {
                input.Set(characterInputHandler.GetNetworkInput());
            }
        }
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerLeft: {player}");
        NetworkBetweenScenesManager.Instance.UnlockCharacter(player);
        NetworkBetweenScenesManager.Instance.RemoveUserID(player);
        if (HPBarHandler.Instance != null)
        {
            HPBarHandler.Instance.UpdateHp(player, 0);
            HPBarHandler.Instance.UpdateState(player, true);
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (NetworkBetweenScenesManager.Instance == null) 
            return;
        //Fix for not unloading previous scene to go back to menu, probably better to change it for something better
        var sacrificialGo = new GameObject("Sacrificial Lamb");
        NetworkBetweenScenesManager.Instance.Runner.Shutdown();

        DontDestroyOnLoad(sacrificialGo);

        foreach (var root in sacrificialGo.scene.GetRootGameObjects())
            Destroy(root);
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
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
        Debug.Log($"OnSessionListUpdated ({sessionList})");
        if (CursorController.Instance != null)
        {
            CursorController.Instance.ReloadRoomList(sessionList);
        }
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
